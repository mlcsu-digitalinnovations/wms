using AngleSharp.Dom;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.AccessKeys;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Common.Helpers.Constants;
using static WmsHub.Business.Enums.ReferralStatus;
using static WmsHub.Business.Models.ReferralSearch;
using Analytics = WmsHub.Business.Entities.Analytics;
using Ethnicity = WmsHub.Business.Enums.Ethnicity;
using GeneralReferral = WmsHub.Business.Entities.GeneralReferral;
using IReferral = WmsHub.Business.Models.IReferral;
using IStaffRole = WmsHub.Business.Models.ReferralService.IStaffRole;
using PharmacyReferral = WmsHub.Business.Entities.PharmacyReferral;
using Provider = WmsHub.Business.Models.Provider;
using ReferralStatusReason = WmsHub.Business.Models.ReferralStatusReason.ReferralStatusReason;
using Referral = WmsHub.Business.Models.Referral;
using SelfReferral = WmsHub.Business.Entities.SelfReferral;
using StaffRole = WmsHub.Business.Models.ReferralService.StaffRole;
using WmsHub.Business.Services.Interfaces;

[assembly: InternalsVisibleTo("WmsHub.Business.Tests")]

namespace WmsHub.Business.Services;

public class ReferralService
  : ServiceBase<Entities.Referral>, IReferralService
{
  private readonly ICsvExportService _csvExportService;
  private readonly IDeprivationService _deprivationService;
  private readonly GpDocumentProxyOptions _gpDocumentProxyOptions;
  private readonly HttpClient _httpClient;
  private readonly ILinkIdService _linkIdService;
  private readonly ILogger _log;
  private readonly IMapper _mapper;
  private readonly IOdsOrganisationService _odsOrganisationService;
  private readonly IPatientTriageService _patientTriageService;
  private readonly IPostcodesIoService _postcodeService;
  private readonly IProviderService _providerService;
  private readonly ReferralTimelineOptions _referralTimelineOptions;

  public ReferralService(
    DatabaseContext context,
    IMapper mapper,
    IProviderService providerService,
    IDeprivationService deprivationService,
    ILinkIdService linkIdService,
    IPostcodesIoService postcodeService,
    IPatientTriageService patientTriageService,
    IOdsOrganisationService odsOrganisationService,
    IOptions<GpDocumentProxyOptions> gpDocumentProxyOptions,
    IOptions<ReferralTimelineOptions> referralTimelineOptions,
    HttpClient httpClient,
    ILogger log)
    : base(context)
  {
    _mapper = mapper;
    _deprivationService = deprivationService;

    _linkIdService = linkIdService;
    _postcodeService = postcodeService;
    _providerService = providerService;
    _patientTriageService = patientTriageService;
    _odsOrganisationService = odsOrganisationService;
    _gpDocumentProxyOptions = gpDocumentProxyOptions.Value;
    _httpClient = httpClient;
    _log = log;
    _referralTimelineOptions = referralTimelineOptions.Value;
  }

  public ReferralService(
    DatabaseContext context,
    ILinkIdService linkIdService,
    IMapper mapper,
    IProviderService providerService,
    ICsvExportService csvExportService,
    IPatientTriageService patientTriageService)
    : base(context)
  {
    _mapper = mapper;
    _linkIdService = linkIdService;
    _providerService = providerService;
    _csvExportService = csvExportService;
    _patientTriageService = patientTriageService;
  }

  public virtual async Task<IReferral> CreateReferral(
    IReferralCreate referralCreate)
  {
    if (referralCreate == null)
    {
      throw new ArgumentNullException(nameof(referralCreate));
    }

    referralCreate.FixPhoneNumberFields();

    CanCreateGpReferral canCreateResult = await CanCreateGpReferral(
      referralCreate);

    if (canCreateResult.IsUpdatingCancelledReferral)
    {
      // A special case where the referral has been removed by the GP
      // practice from the eRS worklist and then re-added at a later date
      // In this case the status needs to be updated back to
      // RejectedToEReferrals and an update performed.
      Entities.Referral existingReferral = _context.Referrals
        .Find(canCreateResult.ExistingReferralId);

      if (existingReferral == null)
      {
        throw new ReferralNotFoundException(
          canCreateResult.ExistingReferralId);
      }

      existingReferral.Status = RejectedToEreferrals.ToString();
      existingReferral.StatusReason =
        "UBRN was removed and re-add by GP practice.";
      UpdateModified(existingReferral);

      if (existingReferral.DateOfReferral.HasValue
        && existingReferral.DateOfReferral.Value != default)
      {
        referralCreate.DateOfReferral = existingReferral.DateOfReferral;
      }

      await _context.SaveChangesAsync();

      ReferralUpdate referralUpdate =
        _mapper.Map<ReferralUpdate>(referralCreate);

      return await UpdateGpReferral(referralUpdate);
    }

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(referralCreate);

    referralEntity.IsActive = true;
    referralEntity.NumberOfContacts = 0;
    referralEntity.CreatedDate = DateTimeOffset.Now;

    await ValidateReferral(referralCreate, referralEntity);

    await UpdateDeprivation(referralEntity);

    referralEntity.ReferralSource = ReferralSource.GpReferral.ToString();

    UpdateModified(referralEntity);

    if (canCreateResult.Status == ReferralStatus.Exception)
    {
      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason =
        $"{canCreateResult.StatusReason} {referralEntity.StatusReason}"
        .Trim();
    }

    _context.Referrals.Add(referralEntity);

    await _context.SaveChangesAsync();

    try
    {
      await UpdateGpReferralUbrnAsync(referralEntity.Id);
    }
    catch (Exception ex)
    {
      try
      {
        _log.Error(ex.Message);
        referralEntity.Status = ReferralStatus.Exception.ToString();
        referralEntity.StatusReason = "Error adding Ubrn to Referral.";
        referralEntity.IsActive = false;
        UpdateModified(referralEntity);
        await _context.SaveChangesAsync();
      }
      catch
      {
        _log.Error("Deactivation failed for Referral {ReferralId} with null Ubrn.",
          referralEntity.Id);
      }
      finally
      {
        throw new ReferralUpdateException("An error occurred creating this referral and it was " +
        "not saved. Please try again later.");
      }
    }

    Referral referralModel =
      _mapper.Map<Entities.Referral, Referral>(referralEntity);

    UpdateCri(referralModel, referralEntity);

    return referralModel;
  }

  public virtual async Task<IReferral> CreateException(
    IReferralExceptionCreate referralExceptionCreate)
  {
    if (referralExceptionCreate == null)
    {
      throw new ArgumentNullException(nameof(referralExceptionCreate));
    }

    if (referralExceptionCreate.ExceptionType == CreateReferralException.Undefined)
    {
      throw new ReferralCreateException("Invalid " +
        $"{nameof(CreateReferralException)} enum value of " +
        $"{referralExceptionCreate.ExceptionType}");
    }

    ValidateModelResult validationResult = Validators.ValidateModel(referralExceptionCreate);
    if (!validationResult.IsValid)
    {
      throw new ReferralCreateException(validationResult.GetErrorMessage());
    }

    Entities.Referral existingReferral = await _context.Referrals
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync(r => r.Ubrn == referralExceptionCreate.Ubrn);

    if (existingReferral != null)
    {
      // If there is an existing referral but it's status is CancelledByEreferrals allow an update.
      if (existingReferral.Status == CancelledByEreferrals.ToString())
      {
        ReferralExceptionUpdate update = new()
        {
          ExceptionType = referralExceptionCreate.ExceptionType,
          MostRecentAttachmentDate = referralExceptionCreate.MostRecentAttachmentDate,
          NhsNumberAttachment = referralExceptionCreate.NhsNumberAttachment,
          NhsNumberWorkList = referralExceptionCreate.NhsNumberWorkList,
          ReferralAttachmentId = referralExceptionCreate.ReferralAttachmentId,
          Ubrn = referralExceptionCreate.Ubrn,
        };

        return await UpdateReferralToStatusExceptionAsync(update);
      }
      else
      {
        throw new ReferralNotUniqueException(
          $"A referral with a Ubrn of {referralExceptionCreate.Ubrn} already exists.");
      }
    }

    Entities.Referral referralEntity = _mapper.Map<Entities.Referral>(referralExceptionCreate);

    referralEntity.IsActive = true;
    referralEntity.NumberOfContacts = 0;
    referralEntity.Status = ReferralStatus.Exception.ToString();
    referralEntity.CreatedDate = DateTimeOffset.Now;
    UpdateModified(referralEntity);

    List<string> validationErrors = new List<string>();

    switch (referralExceptionCreate.ExceptionType)
    {
      case CreateReferralException.NhsNumberMismatch:
        validationErrors.Add("The NHS number in the eRS work list " +
          $"'{referralExceptionCreate.NhsNumberWorkList}' does not match the NHS number " +
          $"'{referralExceptionCreate.NhsNumberAttachment}' in the attached referral letter.");
        break;
      case CreateReferralException.MissingAttachment:
        validationErrors.Add(WarningMessages.NO_ATTACHMENT);
        break;
      case CreateReferralException.InvalidAttachment:
        validationErrors.Add(WarningMessages.INVALID_FILE_TYPE);
        break;
    }

    referralEntity.DocumentVersion = referralExceptionCreate.DocumentVersion;
    referralEntity.ServiceId = referralExceptionCreate.ServiceId;
    referralEntity.SourceSystem = referralExceptionCreate.SourceSystem;
    referralEntity.StatusReason = string.Join(' ', validationErrors);

    _context.Referrals.Add(referralEntity);

    await _context.SaveChangesAsync();

    await UpdateGpReferralUbrnAsync(referralEntity.Id);

    Referral referralModel = _mapper.Map<Entities.Referral, Referral>(referralEntity);

    return referralModel;
  }

  public async Task<IReferral> TestCreateWithChatBotStatus(
    IReferralCreate referralCreate)
  {
    if (referralCreate == null)
    {
      throw new ArgumentNullException(nameof(referralCreate));
    }

    ValidateModelResult result = Validators.ValidateModel(referralCreate);
    if (!result.IsValid)
    {
      throw new ReferralInvalidCreationException("Unable to create test " +
        $"referral due to: {result.GetErrorMessage()}");
    }

    CanCreateGpReferral canCreateResult =
      await CanCreateGpReferral(referralCreate);

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(referralCreate);

    referralEntity.IsActive = true;
    if (canCreateResult.Status == ReferralStatus.Exception)
    {
      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason += canCreateResult.StatusReason;
    }
    else
    {
      referralEntity.Status = ReferralStatus.TextMessage2.ToString();
      referralEntity.StatusReason = "TestCreateWithChatBotStatus";
    }

    UpdateModified(referralEntity);

    // add text messages 48 and 96 hours in the past
    referralEntity.TextMessages = new List<Entities.TextMessage>
    {
      RandomEntityCreator.CreateRandomTextMessage(
        number: referralCreate.Mobile,
        sent: DateTimeOffset.Now.AddHours(-48),
        modifiedAt: DateTimeOffset.Now.AddHours(-48),
        modifiedByUserId: User.GetUserId(),
        outcome: "delivered",
        received: DateTimeOffset.Now.AddHours(-48).AddMinutes(5)),
      RandomEntityCreator.CreateRandomTextMessage(
        number: referralCreate.Mobile,
        sent: DateTimeOffset.Now.AddHours(-96),
        modifiedAt: DateTimeOffset.Now.AddHours(-96),
        modifiedByUserId: User.GetUserId(),
        outcome: "delivered",
        received: DateTimeOffset.Now.AddHours(-96).AddMinutes(5))
    };

    _context.Referrals.Add(referralEntity);

    await _context.SaveChangesAsync();

    Referral referralModel = _mapper.Map<Referral>(referralEntity);

    return referralModel;
  }

  private async Task<CanCreateGpReferral> CanCreateGpReferral(
    IReferralCreate referralCreate)
  {
    if (referralCreate is null)
    {
      throw new ArgumentNullException(nameof(referralCreate));
    }

    CanCreateGpReferral result = new();

    List<Entities.Referral> existingUbrns =
      await GetReferrals(r => r.Ubrn == referralCreate.Ubrn);

    if (existingUbrns.Count > 1)
    {
      Exception_MultipleUbrns(referralCreate, existingUbrns);
    }

    Entities.Referral existingUbrn = existingUbrns.SingleOrDefault();

    if (existingUbrn == null)
    {
      try
      {
        await CheckReferralCanBeCreatedWithNhsNumberAsync(
          referralCreate.NhsNumber);
      }
      catch (ReferralNotUniqueException ex)
      {
        result.Status = ReferralStatus.Exception;
        result.StatusReasons.Add(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        result.Status = ReferralStatus.Exception;
        result.StatusReasons.Add(ex.Message);
      }
    }
    else
    {
      string message = $"A referral with a Ubrn of {referralCreate.Ubrn} " +
        $"already exists.";

      if (existingUbrn.Status == CancelledByEreferrals.ToString())
      {
        result.ExistingReferralId = existingUbrn.Id;
      }
      else if (existingUbrn.Status == ReferralStatus.Exception.ToString()
        || existingUbrn.Status == RejectedToEreferrals.ToString())
      {
        throw new ReferralNotUniqueException(message);
      }
      else
      {
        if (existingUbrn.IsErsClosed == true)
        {
          message += " A triage outcome was previously sent to eRS.";
        }
        else
        {
          message += " A triage outcome was not previously sent to eRS.";
        }

        throw new ReferralInProgressException(message);
      }
    }

    return result;

    static void Exception_MultipleUbrns(
      IReferralCreate referralCreate,
      List<Entities.Referral> existingUbrns)
    {
      string[] duplicateIds = existingUbrns
        .Select(r => r.Id.ToString())
        .ToArray();

      throw new ReferralNotUniqueException(
        $"Found {existingUbrns.Count} existing referrals with UBRN " +
        $"{referralCreate.Ubrn}. There can be only one. Examine referrals " +
        $"with Id's of {string.Join(", ", duplicateIds)}.");
    }

    async Task<List<Entities.Referral>> GetReferrals(
      Expression<Func<Entities.Referral, bool>> predicate)
    {
      return await _context.Referrals
        .Where(r => r.IsActive)
        .Where(predicate)
        .Select(r => new Entities.Referral
        {
          Id = r.Id,
          IsErsClosed = r.IsErsClosed,
          ProviderId = r.ProviderId,
          Status = r.Status,
          Ubrn = r.Ubrn
        })
        .ToListAsync();
    }
  }

  public async Task CheckSelfReferralIsUniqueAsync(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentNullException(nameof(email));
    }

    List<Entities.Referral> existingReferrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Email == email)
      .ToListAsync();

    ReferralHelper.CheckMatchingReferralsIfReEntryIsAllowed(
      existingReferrals,
      "Email Address");
  }

  /// <summary>
  /// Checks if the referral can be created with a given NHS number.
  /// </summary>
  /// <remarks>
  /// A referral can be created if it's NHS number:
  /// does not match an existing referral, or it does match but the matched
  /// referral has been cancelled and a provider was not selected.
  /// </remarks>
  /// <param name="nhsNumber">The NHS number to check.</param>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="ReferralNotUniqueException"></exception>
  /// <exception cref="ReferralReEntryException"></exception>
  public virtual async Task CheckReferralCanBeCreatedWithNhsNumberAsync(
    string nhsNumber)
  {
    List<Entities.Referral> referrals = await _context.Referrals
      .Include(r => r.Provider)
      .Where(r => r.IsActive)
      .Where(r => r.NhsNumber == nhsNumber)
      .OrderByDescending(r => r.CreatedDate)
      .ToListAsync();

    ReferralHelper.CheckMatchingReferralsIfReEntryIsAllowed(referrals);

  }

  /// <inheritdoc/>
  public virtual async Task<List<ActiveReferralAndExceptionUbrn>>
    GetOpenErsGpReferralsThatAreNotCancelledByEreferals(
      string serviceId = null)
  {
    IQueryable<Entities.Referral> query = _context
      .Referrals
      .Where(r => r.IsActive);

    if (serviceId != null)
    {
      query = query.Where(r => r.ServiceId == serviceId);
    }

    List<ActiveReferralAndExceptionUbrn> ubrns = await query
      .Where(r => r.IsErsClosed != true)
      .Where(r => r.ReferralSource == ReferralSource.GpReferral.ToString())
      .Where(r => r.Status != ReferralStatus.CancelledByEreferrals.ToString())
      .Select(r => new ActiveReferralAndExceptionUbrn
      {
        CriLastUpdated = r.Cri.ClinicalInfoLastUpdated,
        MostRecentAttachmentDate = r.MostRecentAttachmentDate,
        ReferralAttachmentId = r.Status == RejectedToEreferrals.ToString()
          ? r.ReferralAttachmentId
          : null,
        ServiceId = r.ServiceId,
        ReferralStatus = r.Status,
        Ubrn = r.Ubrn
      })
      .ToListAsync();

    return ubrns;
  }

  /// <inheritdoc/>
  public async Task<bool> ElectiveCareReferralHasTextMessageWithLinkId(string linkId)
  {
    if (!RegexUtilities.IsValidLinkId(linkId))
    {
      throw new ArgumentException("Parameter is invalid.", nameof(linkId));
    }

    string[] validReferralStatuses =
    {
      ChatBotCall1.ToString(),
      ChatBotTransfer.ToString(),
      New.ToString(),
      RmcCall.ToString(),
      RmcDelayed.ToString(),
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      TextMessage3.ToString()
    };

    bool referralWithMatchingTextMessageExists = await _context.TextMessages
      .Where(t => t.IsActive)
      .Where(t => t.ServiceUserLinkId == linkId)
      .Where(t => t.Referral != null)
      .Where(t => t.Referral.IsActive)
      .Where(t => t.Referral.ReferralSource == ReferralSource.ElectiveCare.ToString())
      .Where(t => validReferralStatuses.Contains(t.Referral.Status))
      .AnyAsync();

    return referralWithMatchingTextMessageExists;
  }

  public async Task<IReferral> GetReferralByNhsNumber(string nhsNumber)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Provider == null)
      .Where(r => r.NhsNumber == nhsNumber)
      .AsNoTracking()
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(nhsNumber);
    }

    Referral referral = _mapper.Map<Referral>(referralEntity);
    UpdateCri(referral, referralEntity);

    if (int.TryParse(referral.TriagedCompletionLevel, out int triageLevel))
    {
      if (Enum.IsDefined(typeof(TriageLevel), triageLevel))
      {
        IEnumerable<Provider> providers = await _providerService
          .GetProvidersAsync(triageLevel);

        referral.Providers = providers.ToList();
      }
    }

    return referral;
  }

  public async Task<List<Guid>> GetReferralIdsByNhsNumber(string nhsNumber)
    => await _context.Referrals
    .Where(r => r.IsActive)
    .Where(r => r.NhsNumber == nhsNumber)
    .OrderByDescending(r => r.DateOfReferral)
    .AsNoTracking()
    .Select(s => s.Id)
    .ToListAsync();

  public async Task<IReferral> GetReferralWithTriagedProvidersById(Guid id)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Include(r => r.Calls.Where(c => c.IsActive))
      .Include(r => r.TextMessages.Where(c => c.IsActive))
      .Include(r => r.Provider)
      .Where(r => r.IsActive)
      .Where(r => r.Id == id)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    Referral referral = _mapper.Map<Referral>(referralEntity);
    UpdateCri(referral, referralEntity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      referral.Providers = new List<Provider>();
    }
    else
    {
      referral.Providers = providers.ToList();
    }

    return referral;
  }

  public async Task ExpireTextMessageDueToDobCheckAsync(
    string serviceUserLinkId)
  {
    if (string.IsNullOrWhiteSpace(serviceUserLinkId))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(serviceUserLinkId));
    }

    Entities.TextMessage textMessage = await _context
      .TextMessages
      .Where(t => t.IsActive)
      .Where(t => t.ServiceUserLinkId == serviceUserLinkId)
      .OrderBy(t => t.Sent)
      .LastOrDefaultAsync();

    if (textMessage == null)
    {
      throw new ReferralNotFoundException("Cannot find a TextMessage with " +
        $"a ServiceUserLinkId of '{serviceUserLinkId}'");
    }

    textMessage.Outcome = Constants.DATE_OF_BIRTH_EXPIRY;
    UpdateModified(textMessage);

    await _context.SaveChangesAsync();
  }

  public async Task<IReferral> GetServiceUserReferralAsync(
    string serviceUserLinkId)
  {
    if (serviceUserLinkId == null ||
      !serviceUserLinkId.All(char.IsLetterOrDigit))
    {
      string value = string.IsNullOrEmpty(serviceUserLinkId)
        ? "Null"
        : serviceUserLinkId;

      throw new ReferralNotFoundException(
        $"Invalid {nameof(serviceUserLinkId)} of [{value}].");
    }

    Entities.TextMessage textMessage = await _context
      .TextMessages
      .Include(t => t.Referral)
      .Where(t => t.IsActive)
      .Where(t => t.ServiceUserLinkId == serviceUserLinkId)
      .OrderBy(t => t.Sent)
      .LastOrDefaultAsync() ?? throw new ReferralNotFoundException("No match with text message id: " +
        (string.IsNullOrEmpty(serviceUserLinkId) ? "Null" : serviceUserLinkId));

    if (textMessage.Outcome == Constants.DO_NOT_CONTACT_EMAIL)
    {
      throw new TextMessageExpiredByEmailException("Text message token " +
        $"{serviceUserLinkId} expired by Email not submitted.");
    }

    if (textMessage.Referral.ProviderId != null)
    {
      throw new TextMessageExpiredByProviderSelectionException("Text " +
        $"message with token {serviceUserLinkId} had its provider selected" +
        $"on {textMessage.Referral.DateOfProviderSelection}.");
    }

    if (textMessage.Outcome == Constants.DATE_OF_BIRTH_EXPIRY)
    {
      throw new TextMessageExpiredByDoBCheckException("Text message with " +
        $"token {serviceUserLinkId} expired by Date of Birth check.");
    }

    string[] validStatuses = new[] {
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      ChatBotCall1.ToString(),
      ChatBotTransfer.ToString(),
      RmcCall.ToString(),
      RmcDelayed.ToString(),
      TextMessage3.ToString()
    };

    if (!validStatuses.Contains(textMessage.Referral.Status))
    {
      throw new ReferralInvalidStatusException(
        $"Invalid status of {textMessage.Referral.Status}, expecting a " +
        $"status of one of following: {string.Join(",", validStatuses)}.");
    }

    Referral referral = _mapper.Map<Referral>(textMessage.Referral);

    return referral;
  }

  public async Task<IReferral> GetReferralByTextMessageId(Guid id)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Include(r => r.TextMessages)
      .Where(r => r.TextMessages.Any(t => t.Id == id))
      .Where(r => r.IsActive)
      .AsNoTracking()
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (!referralEntity.TextMessages.First(t => t.Id == id).IsActive)
    {
      throw new TextMessageExpiredException(
        $"The text message with an id of {id} has expired.");
    }

    if (referralEntity.Status != TextMessage1.ToString()
      && referralEntity.Status != TextMessage2.ToString()
      && referralEntity.Status != TextMessage3.ToString())
    {
      throw new ReferralInvalidStatusException(
        $"Expected referral status of {TextMessage1}, {TextMessage2} or {TextMessage3} but status " +
        $"is {referralEntity.Status}.");
    }

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public virtual async Task<IEnumerable<ReferralSourceInfo>>
    GetReferralSourceInfo()
  {
    List<ReferralSourceInfo> referralSourceInfoList = await _context
      .Referrals.GroupBy(x => x.ReferralSource)
      .Select(x => new ReferralSourceInfo(
        x.Key,
        x.Count(
          x => x.IsActive
          && x.Status != Complete.ToString()
          && x.Status != CancelledByEreferrals.ToString()),
        x.Count(
          r => r.IsActive
          && r.Status == Complete.ToString()
          && r.Status != CancelledByEreferrals.ToString()),
        x.Count(
          r => r.IsActive
          && r.Status == CancelledByEreferrals.ToString())
        ))
      .ToListAsync();

    return referralSourceInfoList.OrderBy(x => x.Name);
  }

  public async Task<IReferralSearchResponse> Search(ReferralSearch search)
  {
    IQueryable<Entities.Referral> query = _context
      .Referrals
      .Where(r => r.IsActive);

    if (search.Postcode != null)
    {
      string[] variants = search.GetPostcodeVariations();
      query = query.Where(
        r => r.Postcode == variants[(int)Variation.Original] ||
        r.Postcode == variants[(int)Variation.Upper] ||
        r.Postcode == variants[(int)Variation.Lower] ||
        r.Postcode == variants[(int)Variation.Variant]);
    }

    if (search.TelephoneNumber != null)
    {
      query = query.Where(r => r.Telephone == search.TelephoneNumber);
    }

    if (search.MobileNumber != null)
    {
      query = query.Where(r => r.Mobile == search.MobileNumber);
    }

    if (search.EmailAddress != null)
    {
      string[] variants = search.GetEmailAddressVariations();
      query = query.Where(
        r => r.Email == variants[(int)Variation.Original] ||
        r.Email == variants[(int)Variation.Upper] ||
        r.Email == variants[(int)Variation.Lower] ||
        r.Email == variants[(int)Variation.Variant]);
    }

    if (search.Ubrn != null)
    {
      string ubrn = search.Ubrn.Replace(" ", "");
      query = query.Where(r => r.Ubrn == ubrn || r.ProviderUbrn == ubrn);
    }

    if (search.NhsNumber != null)
    {
      query = query
        .Where(r => r.NhsNumber == search.NhsNumber.Replace(" ", ""));
    }

    if (search.FamilyName != null)
    {
      string[] variants = search.GetFamilyNameVariations();
      query = query.Where(
        r => r.FamilyName == variants[(int)Variation.Original] ||
        r.FamilyName == variants[(int)Variation.Upper] ||
        r.FamilyName == variants[(int)Variation.Lower] ||
        r.FamilyName == variants[(int)Variation.Variant]);
    }

    if (search.GivenName != null)
    {
      string[] variants = search.GetGivenNameVariations();
      query = query.Where(
        r => r.GivenName == variants[(int)Variation.Original] ||
        r.GivenName == variants[(int)Variation.Upper] ||
        r.GivenName == variants[(int)Variation.Lower] ||
        r.GivenName == variants[(int)Variation.Variant]);
    }

    // Only include the status, delayed or vulnerable filters if there
    // are no search parameters.
    if (!search.HasSearchParameters)
    {
      if (search.Statuses != null)
      {
        if (search.Statuses.Length == 1)
        {
          query = query.Where(r => r.Status == search.Statuses[0]);
        }
        else if (search.Statuses.Length > 1)
        {
          query = query.Where(r => search.Statuses.Contains(r.Status));
        }
      }

      if (search.DelayedReferralsFilter == SearchFilter.Only)
      {
        query = query
          .Where(r => r.Audits.Any(a => a.DateToDelayUntil != null));
      }
      else if (search.DelayedReferralsFilter == SearchFilter.Exclude)
      {
        query = query
          .Where(r => r.Audits.All(a => a.DateToDelayUntil == null));
      }

      if (search.IsVulnerable != null)
      {
        if (search.IsVulnerable.Value)
        {
          query = query.Where(r => r.IsVulnerable.Value);
        }
        else
        {
          query = query.Where(r =>
            (!r.IsVulnerable.Value || r.IsVulnerable == null));
        }
      }
    }

    if (!string.IsNullOrWhiteSpace(search.ReferralSource))
    {
      query = query.Where(r => r.ReferralSource == search.ReferralSource);
    }

    int count = query.Count();

    var referralsWithAudits = await query
      .AsNoTracking()
      .AsSingleQuery()
      .Select(r => new
      {
        r.Id,
        r.DateOfBirth,
        r.DateOfReferral,
        r.DelayReason,
        r.DateToDelayUntil,
        r.FamilyName,
        r.GivenName,
        r.Status,
        r.StatusReason,
        r.Ubrn,
        r.ReferralSource,
        Audits = r.Audits
          .Where(a => a.Status == RmcDelayed.ToString())
          .Select(a => new
          {
            a.DelayReason,
            a.DateToDelayUntil
          })
          .OrderByDescending(a => a.DateToDelayUntil)
          .ToList()
      })
      .OrderBy(r => r.DateOfReferral)
      .ToListAsync();

    IEnumerable<Referral> referrals = referralsWithAudits
    .Select(r => new Referral
    {
      Id = r.Id,
      DelayUntil = r.DateToDelayUntil ??
        r.Audits.FirstOrDefault()?.DateToDelayUntil,
      DelayReason = r.DelayReason ??
        r.Audits.FirstOrDefault()?.DelayReason,
      NumberOfDelays = r.Audits.Count,
      DateOfBirth = r.DateOfBirth,
      DateOfReferral = r.DateOfReferral,
      FamilyName = r.FamilyName,
      GivenName = r.GivenName,
      Status = r.Status,
      StatusReason = r.StatusReason,
      Ubrn = r.Ubrn,
      ReferralSource = r.ReferralSource
    });

    referrals = referrals.BringToTop(
      r => r.Status == ChatBotTransfer.ToString());

    if ((search.Limit ?? 0) > 0)
    {
      referrals = referrals.Take(search.Limit.Value);
    }

    return new ReferralSearchResponse
    {
      Referrals = referrals,
      Count = count
    };
  }

  public async Task<byte[]> SendReferralLettersAsync(List<Guid> referrals,
    DateTimeOffset dateLettersExported)
  {

    if (!referrals.Any())
    {
      throw new ReferralLetterException(
        $"Referral letter list contains zero referrals");
    }

    IEnumerable<Entities.Referral> referralList = await _context
      .Referrals
      .Where(r => referrals.Contains(r.Id))
      .ToListAsync();

    foreach (Entities.Referral referral in referralList)
    {
      referral.Status = ReferralStatus.LetterSent.ToString();
      referral.DateLetterSent = dateLettersExported;
      referral.MethodOfContact = (int)MethodOfContact.Letter;
      referral.NumberOfContacts++;
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    IEnumerable<Referral> businessReferrals = _mapper
      .Map<IEnumerable<Entities.Referral>,
        IEnumerable<Referral>>(referralList);

    if (referralList != null)
    {
      return _csvExportService
        .Export<CsvExportAttribute>(businessReferrals);
    }

    return null;
  }

  public async Task<FileContentResult>
    CreateDischargeLettersAsync(List<Guid> referrals)
  {

    if ((referrals == null) || !referrals.Any())
    {
      throw new ReferralLetterException(
        $"Referral discharge list contains zero referrals");
    }

    IEnumerable<Entities.Referral> referralList = await _context
      .Referrals
      .Include(r => r.Provider)
      .Where(r => referrals.Contains(r.Id))
      .ToListAsync();

    IEnumerable<Referral> businessReferrals = _mapper
      .Map<IEnumerable<Entities.Referral>,
        IEnumerable<Referral>>(referralList);

    if (businessReferrals == null)
    {
      throw new ReferralLetterException("Failed to create file");
    }
    else
    {
      FileContentResult result = null;
      if (businessReferrals.Count() == 1)
      {
        result =
          GetSingleDischargeLetter(businessReferrals.First());
        await UpdateReferralToStatusCompleteAsync(
          businessReferrals.First().Id);
      }
      else
      {
        result = GetZippedDischargeLetters(businessReferrals);
        foreach (Referral r in businessReferrals)
        {
          await UpdateReferralToStatusCompleteAsync(r.Id);
        }
      }

      return result;
    }
  }

  public async Task<bool> UpdateReferralToStatusCompleteAsync(Guid id)
  {
    Entities.Referral entity =
      await _context.Referrals.SingleOrDefaultAsync(t => t.Id == id);

    if (entity == null)
    {
      return false;
    }

    entity.Status = ReferralStatus.Complete.ToString();
    UpdateModified(entity);

    return await _context.SaveChangesAsync() > 0;

  }

  public async Task<IReferral> UpdateConsentForFutureContactForEvaluation(
    Guid id,
    bool emailNotSupplied,
    bool consented,
    string emailAddress = null)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        referralEntity.Id, referralEntity.ProviderId);
    }

    if (consented && string.IsNullOrEmpty(emailAddress))
    {
      throw new ReferralContactEmailException(
        $"An email address is required when confirming contact " +
        "after program completion");
    }

    referralEntity.ConsentForFutureContactForEvaluation = consented;
    referralEntity.Email = emailAddress;
    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public async Task<IReferral> EmailAddressNotProvidedAsync(Guid id)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Include(r => r.TextMessages)
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(id);

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(referralEntity.Id, referralEntity.ProviderId);
    }

    referralEntity.ConsentForFutureContactForEvaluation = false;
    referralEntity.Email = Constants.DO_NOT_CONTACT_EMAIL;
    referralEntity.Status = ReferralStatus.Exception.ToString();
    referralEntity.StatusReason = "Service user did not want to provide an email address.";
    referralEntity.TextMessages.ForEach(t => t.Outcome = Constants.DO_NOT_CONTACT_EMAIL);
    UpdateModified(referralEntity);
    await _context.SaveChangesAsync();

    referralEntity.Status = ReferralStatus.FailedToContact.ToString();
    UpdateModified(referralEntity);
    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public async Task<IReferral> SetBmiTooLowAsync(Guid id)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException(
        $"{nameof(id)} cannot be empty.",
        nameof(id));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        referralEntity.Id, referralEntity.ProviderId);
    }

    var ethnicity = await _context
    .Ethnicities
    .Where(e => e.DisplayName == referralEntity.ServiceUserEthnicity)
    .Where(e => e.GroupName == referralEntity.ServiceUserEthnicityGroup)
    .Where(e => e.IsActive)
    .Select(e => new
    {
      Ethnicity = $"{e.GroupName}, {e.DisplayName}",
      e.MinimumBmi
    })
    .SingleOrDefaultAsync();

    if (ethnicity == null)
    {
      throw new EthnicityNotFoundException($"Unable to find an ethnicity " +
        $"with a Display Name of {referralEntity.ServiceUserEthnicity}.");
    }

    if (referralEntity.CalculatedBmiAtRegistration >= ethnicity.MinimumBmi)
    {
      throw new BmiTooLowException($"BMI of " +
        $"{referralEntity.CalculatedBmiAtRegistration} is not below the " +
        $"minimum of {ethnicity.MinimumBmi} for the selected " +
        $"ethnicity {ethnicity.Ethnicity}.");
    }

    referralEntity.Status = RejectedToEreferrals.ToString();
    referralEntity.StatusReason =
      $"BMI of {referralEntity.CalculatedBmiAtRegistration} is below " +
      $"the minimum of {ethnicity.MinimumBmi} for the selected " +
      $"ethnicity {ethnicity.Ethnicity}.";

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public async Task<IReferral> ConfirmProviderAsync(
    Guid referralId,
    Guid? providerId,
    bool isRmcCall = false,
    bool consentForContact = false,
    string email = null)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == referralId)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        referralEntity.Id, referralEntity.ProviderId);
    }

    if (providerId == null || !await _context.Providers
        .Where(p => p.IsActive)
        .AnyAsync(p => p.Id == providerId))
    {
      throw new ProviderNotFoundException(providerId);
    }

    referralEntity.ProviderId = providerId;
    referralEntity.DateOfProviderSelection = DateTimeOffset.Now;
    referralEntity.DateToDelayUntil = null;

    if (string.IsNullOrWhiteSpace(referralEntity.NhsNumber))
    {
      referralEntity.Status = ProviderAwaitingTrace.ToString();
    }
    else
    {
      referralEntity.Status = ProviderAwaitingStart.ToString();
    }

    if (isRmcCall)
    {
      referralEntity.ConsentForFutureContactForEvaluation =
        consentForContact;
      referralEntity.Email = email;
      referralEntity.MethodOfContact = (int)MethodOfContact.RmcCall;
      referralEntity.NumberOfContacts++;
    }

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public async Task<IReferral> ConfirmProviderAsync(Referral model)
  {

    return await ConfirmProviderAsync(
      model.Id,
      model.ProviderId,
      true,
      model.ConsentForFutureContactForEvaluation ?? false,
      model.Email);
  }

  public async Task<IReferral> UpdateServiceUserEthnicityGroupAsync(
    Guid id,
    string ethnicityGroup)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException(
        $"{nameof(id)} cannot be empty.",
        nameof(id));
    }

    if (string.IsNullOrWhiteSpace(ethnicityGroup))
    {
      throw new ArgumentException(
        $"{nameof(ethnicityGroup)} cannot be null or white space.",
        nameof(ethnicityGroup));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        referralEntity.Id, referralEntity.ProviderId);
    }

    var ethnicity = await _context
      .Ethnicities
      .Where(e => e.GroupName == ethnicityGroup)
      .Where(e => e.IsActive)
      .OrderBy(e => e.MinimumBmi)
      .Select(e => new { e.MinimumBmi })
      .FirstOrDefaultAsync();

    if (ethnicity == null)
    {
      throw new EthnicityNotFoundException("An ethnicity group with a " +
        $"display name of {ethnicityGroup} cannot be found.");
    }

    referralEntity.ServiceUserEthnicity = null;
    referralEntity.ServiceUserEthnicityGroup = ethnicityGroup;

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  /// <inheritdoc/>
  public async Task<Entities.Referral> GetReferralEntity(Guid id)
  {
    return await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(id);
  }

  /// <inheritdoc/>
  public virtual async Task<IReferral> UpdateStatusToRejectedToEreferralsAsync(
    Guid referralId, string statusReason)
  {
    Entities.Referral referralEntity = await GetReferralEntity(referralId);

    referralEntity.Status = RejectedToEreferrals.ToString();
    if (!string.IsNullOrWhiteSpace(statusReason))
    {
      referralEntity.StatusReason = statusReason;
    }

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  /// <inheritdoc/>
  public async Task<IReferral> RejectAfterProviderSelectionAsync(
    Guid id,
    string reason)
  {
    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(reason));
    }

    string[] validStatuses = new[]
    {
      ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
      ReferralStatus.ProviderRejected.ToString(),
      ReferralStatus.ProviderTerminated.ToString(),
    };

    Entities.Referral referralEntity = await GetReferralEntity(id);

    if (referralEntity.ReferralSource != ReferralSource.GpReferral.ToString())
    {
      throw new ReferralInvalidReferralSourceException(
        $"Referral {id} has an unexpected referral source of " +
        $"{referralEntity.ReferralSource}. The only valid referral source " +
        $"is {ReferralSource.GpReferral}.");
    }

    if (!validStatuses.Contains(referralEntity.Status))
    {
      throw new ReferralInvalidStatusException(
        $"Referral {id} has an unexpected status of " +
        $"{referralEntity.Status}. Valid statuses are: " +
        $"{string.Join(",", validStatuses)}.");
    }

    if (referralEntity.ProviderId == null)
    {
      throw new ReferralProviderSelectedException(
        $"Referral {id} has a status of {referralEntity.Status}, " +
        "and therefore should have a selected provider, but it does not.");
    }

    referralEntity.DateCompletedProgramme = DateTimeOffset.Now;
    referralEntity.ProgrammeOutcome =
      ProgrammeOutcome.RejectedAfterProviderSelection.ToString();
    referralEntity.Status = ReferralStatus.AwaitingDischarge.ToString();
    referralEntity.StatusReason = reason;

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    return _mapper.Map<Referral>(referralEntity);
  }

  /// <inheritdoc/>
  public async Task<IReferral> RejectBeforeProviderSelectionAsync(
    Guid id,
    string reason)
  {
    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(reason));
    }

    string[] validStatuses = new[]
    {
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.RmcCall.ToString(),
      ReferralStatus.RmcDelayed.ToString(),
      ReferralStatus.TextMessage3.ToString()
    };

    Entities.Referral referralEntity = await GetReferralEntity(id);

    if (referralEntity.ReferralSource != ReferralSource.GpReferral.ToString())
    {
      throw new ReferralInvalidReferralSourceException(
        $"Referral {id} has an unexpected referral source of " +
        $"{referralEntity.ReferralSource}. The only valid referral source " +
        $"is {ReferralSource.GpReferral}.");
    }

    if (!validStatuses.Contains(referralEntity.Status))
    {
      throw new ReferralInvalidStatusException(
        $"Referral {id} has an unexpected status of " +
        $"{referralEntity.Status}. Valid statuses are: " +
        $"{string.Join(",", validStatuses)}.");
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        $"Referral {id} has a status of {referralEntity.Status}, " +
        "and therefore shouldn't have a selected provider, but it has " +
        $"a provider id of {referralEntity.ProviderId}.");
    }

    referralEntity.DateCompletedProgramme = DateTimeOffset.Now;
    referralEntity.ProgrammeOutcome =
      ProgrammeOutcome.RejectedBeforeProviderSelection.ToString();
    referralEntity.Status = ReferralStatus.AwaitingDischarge.ToString();
    referralEntity.StatusReason = reason;

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    return _mapper.Map<Referral>(referralEntity);
  }

  public async Task<IReferral> UpdateStatusToRmcCallAsync(Guid referralId)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Include(r => r.ProviderSubmissions)
      .Where(r => r.Id == referralId)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    string[] validStatuses = new string[]
    {
      ReferralStatus.RmcDelayed.ToString(),
      ReferralStatus.Letter.ToString(),
      ReferralStatus.LetterSent.ToString(),
      ReferralStatus.FailedToContact.ToString(),
      ReferralStatus.ProviderRejected.ToString(),
      ReferralStatus.ProviderTerminated.ToString(),
      ReferralStatus.ProviderDeclinedByServiceUser.ToString()
    };

    if (!validStatuses.Contains(referralEntity.Status))
    {
      throw new ReferralInvalidStatusException("Unable to update status " +
        $"to {ReferralStatus.RmcCall} because the referral has a status " +
        $"of {referralEntity.Status} and not one of the required " +
        $"statuses: {string.Join(", ", validStatuses)}.");
    }

    referralEntity.Status = RmcCall.ToString();
    referralEntity.StatusReason = "Manually re-added to call list.";
    referralEntity.ProviderId = null;
    referralEntity.DateOfProviderSelection = null;
    referralEntity.DateOfProviderContactedServiceUser = null;
    referralEntity.DateLetterSent = null;
    referralEntity.DateStartedProgramme = null;
    referralEntity.DateCompletedProgramme = null;
    referralEntity.FirstRecordedWeight = null;
    referralEntity.FirstRecordedWeightDate = null;
    referralEntity.LastRecordedWeight = null;
    referralEntity.LastRecordedWeightDate = null;
    referralEntity.ProgrammeOutcome = null;

    if (referralEntity.ProviderSubmissions != null &&
      referralEntity.ProviderSubmissions.Any())
    {
      referralEntity.ProviderSubmissions.ForEach(ps => ps.IsActive = false);
    }

    if (referralEntity.Email == Constants.DO_NOT_CONTACT_EMAIL)
    {
      referralEntity.Email = null;
    }

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  public async Task<List<Models.ReferralAudit>>
    GetReferralAuditForServiceUserAsync(Guid id)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentNullException(nameof(id));
    }

    List<Models.ReferralAudit> audits = await _context.ReferralsAudit
      .Where(t => t.Id == id)
      .Select(ra => new Models.ReferralAudit()
      {
        AuditId = ra.AuditId,
        Address1 = ra.Address1,
        Address2 = ra.Address2,
        Address3 = ra.Address3,
        DateOfBirth = ra.DateOfBirth,
        DateOfReferral = ra.DateOfReferral,
        DateToDelayUntil = ra.DateToDelayUntil,
        DelayReason = ra.DelayReason,
        Email = ra.Email,
        FamilyName = ra.FamilyName,
        GivenName = ra.GivenName,
        Id = ra.Id,
        Mobile = ra.Mobile,
        ModifiedAt = ra.ModifiedAt,
        ModifiedByUserId = ra.ModifiedByUserId,
        NhsNumber = ra.NhsNumber,
        Postcode = ra.Postcode,
        ReferringGpPracticeName = ra.ReferringGpPracticeName,
        Status = ra.Status,
        StatusReason = ra.StatusReason,
        Telephone = ra.Telephone,
        Ubrn = ra.Ubrn,
        Username = ra.User.OwnerName
      })
      .OrderByDescending(t => t.ModifiedAt)
      .ToListAsync();

    audits
      .Where(r => r.ModifiedByUserId == Guid.Empty)
      .ToList()
      .ForEach(r => r.Username = Constants.WebUi.RMC_UNKNOWN);

    audits
      .Where(t => _context.TextMessages.Any(r => r.Id == t.ModifiedByUserId))
      .ToList()
      .ForEach(s => s.Username = Constants.WebUi.RMC_SERVICE_USER);

    audits.ForEach(a => a.Username ??= Constants.WebUi.RMC_AUTOMATED);

    if (audits == null)
    {
      throw new ReferralNotFoundException(
        $"Referral Audits not found for Id '{id}'.");
    }

    return audits;
  }

  public async Task<IReferral> UpdateStatusFromRmcCallToFailedToContactAsync(
    Guid referralId,
    string reason)
  {
    Entities.Referral referral = await _context
      .Referrals
      .Where(r => r.Id == referralId)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referral == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    if (referral.Status != RmcCall.ToString())
    {
      throw new ReferralInvalidStatusException("Unable to set status to " +
        $"{FailedToContact} because status is {referral.Status} when it " +
        $"must be {RmcCall}.");
    }

    referral.Status = FailedToContact.ToString();

    if (!string.IsNullOrWhiteSpace(reason))
    {
      referral.StatusReason = reason.SanitizeInput();
    }

    referral.MethodOfContact = (int)MethodOfContact.RmcCall;
    referral.NumberOfContacts++;
    UpdateModified(referral);

    await _context.SaveChangesAsync();

    return _mapper.Map<Referral>(referral);
  }

  public virtual string[] GetNhsNumbers(int? required)
  {
    required ??= 1;
    if (required.Value > 1000)
    {
      required = 1000;
    }

    Random random = new();
    string[] nhsNumbers = new string[required.Value];

    string[] existingNhsNumbers = _context.Referrals
      .Where(r => r.IsActive)
      .Select(r => r.NhsNumber)
      .ToArray();

    int i = 0;
    while (i < required)
    {
      string nhsNumber = Generators.GenerateNhsNumber(random);
      if (!existingNhsNumbers.Contains(nhsNumber))
      {
        nhsNumbers[i++] = nhsNumber;
      }
    }

    return nhsNumbers;
  }

  public async Task<IReferral> DelayReferralUntilAsync(
    Guid referralId,
    string reason,
    DateTimeOffset until)
  {
    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == referralId)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new DelayReferralException(
        "A provider has been selected for this referral.");
    }

    referralEntity.DateToDelayUntil = until;
    referralEntity.Status = RmcDelayed.ToString();
    referralEntity.MethodOfContact = (int)MethodOfContact.RmcCall;
    referralEntity.NumberOfContacts++;
    referralEntity.DelayReason = reason;
    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  /// <summary>
  /// Overrides the current referral and sets the status to either
  /// New or RmcCall;<br />
  /// If there is a previous referral which does not have a completed status
  /// then that referral IsActive is set to false;<br/>
  /// If a previous referral has it's IsActive set to false then the
  /// description of this will be added to the StatusReason, including the
  /// previous referral UBRN.
  /// </summary>
  /// <param name="id">Referral ID Guid</param>
  /// <param name="status">ReferralStatus of New or RmcCall</param>
  /// <param name="statusReason">If the status reason is due to an 
  /// attachment upload failure, then this can be caught using this reason.
  /// </param>
  /// <returns></returns>
  /// <exception cref="StatusOverrideException"></exception>
  public async Task<IReferral> ExceptionOverride(
    Guid id,
    ReferralStatus status,
    string statusReason)
  {
    // Get the referral
    Entities.Referral currentReferral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Id == id)
      .Where(r => r.Status == ReferralStatus.Exception.ToString())
      .SingleOrDefaultAsync();

    if (currentReferral == null)
    {
      throw new ReferralNotFoundException($"Referral is ID of {id} and" +
        $" status of 'Exception' was not found.");
    }

    // If NHS Number is null, Date Of Birth is DateTime.MinValue, 
    // DateOfReferral is DateTime.MinValue and StatusReason is 
    // Constants.WarningMessage.NO_ATTACHMENT then archive and return
    if (string.IsNullOrWhiteSpace(currentReferral.NhsNumber)
        && status == ReferralStatus.Cancelled
        && currentReferral.StatusReason == WarningMessages.NO_ATTACHMENT)
    {
      currentReferral.IsActive = false;
      currentReferral.StatusReason = $"Set IsActive = false as " +
        $"'{WarningMessages.NO_ATTACHMENT}'.";
      UpdateModified(currentReferral);
      await _context.SaveChangesAsync();

      return null;
    }

    if (status == ReferralStatus.Cancelled)
    {
      throw new StatusChangeException($"Status cannot be changes as " +
       $"{status} is not valid.");
    }

    if (string.IsNullOrWhiteSpace(currentReferral.NhsNumber))
    {
      // NHS Number is required to trace other referrals
      throw new StatusChangeException($"Status cannot be changes as " +
        $"{nameof(currentReferral.NhsNumber)} is not valid.");
    }

    // Use the NHS Number to get the previous referral not
    // in completed state
    Entities.Referral previousReferral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.NhsNumber == currentReferral.NhsNumber)
      .Where(r => r.Id != currentReferral.Id)
      .OrderByDescending(r => r.DateOfReferral)
      .FirstOrDefaultAsync();

    if (previousReferral != null)
    {
      string searchText = $"Referral cannot be created because there are" +
        $" in progress referrals with the same NHS number:" +
        $" {previousReferral.Ubrn}.";
      if (currentReferral.StatusReason == searchText)
      {
        previousReferral.Status = ReferralStatus.Complete.ToString();
        previousReferral.StatusReason =
          $"Status set to {ReferralStatus.Complete} manually by RMC.";
        UpdateModified(currentReferral);
        await _context.SaveChangesAsync();
      }
    }

    string statusChanged = "";

    if (currentReferral.IsVulnerable.HasValue
        && currentReferral.IsVulnerable == true)
    {
      status = ReferralStatus.RmcCall;
      statusChanged = "(referral IsVulnerable is true) ";
    }

    currentReferral.Status = status.ToString();
    currentReferral.StatusReason =
      $"Status {status} {statusChanged}manually by RMC with " +
      $"reason '{statusReason}'";
    UpdateModified(currentReferral);
    await _context.SaveChangesAsync();

    Referral referralModel = _mapper.Map<Referral>(currentReferral);

    return referralModel;
  }

  public virtual async Task<IReferral> UpdateGpReferral(
    IReferralUpdate referralUpdate)
  {
    if (referralUpdate is null)
    {
      throw new ArgumentNullException(nameof(referralUpdate));
    }

    List<Entities.Referral> existingReferrals = await _context.Referrals
       .Where(r => r.IsActive)
       .Where(r => r.Ubrn == referralUpdate.Ubrn)
       .ToListAsync();

    if (!existingReferrals.Any())
    {
      throw new ReferralNotFoundException("An active referral with UBRN " +
        $"of {referralUpdate.Ubrn} was not found.");
    }
    else if (existingReferrals.Count > 1)
    {
      throw new ReferralNotUniqueException(
        $"There are {existingReferrals.Count} active referrals with the " +
        $"same UBRN of {referralUpdate.Ubrn}.");
    }

    Entities.Referral existingReferral = existingReferrals.Single();

    // Referral being updated must have status of RejectedToEreferrals.
    if (existingReferral.Status != RejectedToEreferrals.ToString())
    {
      throw new ReferralInvalidStatusException("The referral with a UBRN " +
        $"of {referralUpdate.Ubrn} cannot be updated because its status " +
        $"is {existingReferral.Status} when it needs to be {RejectedToEreferrals}.");
    }

    List<Entities.Referral> existingNhsNumReferrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.NhsNumber == referralUpdate.NhsNumber)
      .Where(r => r.Ubrn != referralUpdate.Ubrn)
      .OrderByDescending(r => r.CreatedDate)
      .Select(r => new Entities.Referral
      {
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateStartedProgramme = r.DateStartedProgramme,
        ProviderId = r.ProviderId,
        Status = r.Status,
        Ubrn = r.Ubrn
      })
      .ToListAsync();

    try
    {

      ReferralHelper
        .CheckMatchingReferralsIfReEntryIsAllowed(existingNhsNumReferrals);

      existingReferral.StatusReason = null;
      string existingPostcode = existingReferral.Postcode;

      referralUpdate.FixPhoneNumberFields();

      if (existingReferral.DateOfReferral.HasValue
        && existingReferral.DateOfReferral.Value != default)
      {
        referralUpdate.DateOfReferral = existingReferral.DateOfReferral;
      }

      _mapper.Map((ReferralUpdate)referralUpdate, existingReferral);

      await ValidateReferral(referralUpdate, existingReferral);

      if (existingPostcode != referralUpdate.Postcode)
      {
        await UpdateDeprivation(existingReferral);
      }
    }
    catch (ReferralNotUniqueException ex)
    {
      existingReferral.Status = ReferralStatus.Exception.ToString();
      existingReferral.NhsNumber = referralUpdate.NhsNumber;
      existingReferral.StatusReason = ex.Message;
    }
    catch (InvalidOperationException ex)
    {
      existingReferral.Status = ReferralStatus.Exception.ToString();
      existingReferral.NhsNumber = referralUpdate.NhsNumber;
      existingReferral.StatusReason = ex.Message;
    }

    if (existingReferral.Ethnicity == null)
    {
      existingReferral.TriagedCompletionLevel = null;
      existingReferral.TriagedWeightedLevel = null;
    }

    existingReferral.IsErsClosed = false;

    UpdateModified(existingReferral);

    await _context.SaveChangesAsync();

    Referral referralModel = _mapper.Map<Referral>(existingReferral);

    return referralModel;
  }

  private FileContentResult GetSingleDischargeLetter(Referral referral)
  {
    byte[] documentBytes = DischargeLetterCreator
      .GenerateDischargeLetter(referral);

    return new FileContentResult(documentBytes.ToArray(), "application/pdf")
    { FileDownloadName = $"{referral.Ubrn}_DischargeLetter.pdf" };
  }

  private FileContentResult GetZippedDischargeLetters(IEnumerable<Referral>
    referrals)
  {
    if (referrals == null)
    {
      throw new ArgumentNullException();
    }

    using (MemoryStream compressedFileStream = new MemoryStream())
    {
      using (ZipArchive zipArchive =
        new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true))
      {
        foreach (Referral referral in referrals)
        {
          byte[] documentBytes =
            DischargeLetterCreator.GenerateDischargeLetter(referral);

          ZipArchiveEntry zipEntry =
            zipArchive.CreateEntry($"{referral.Ubrn}_DischargeLetter.pdf");

          using (MemoryStream documentStream =
            new MemoryStream(documentBytes))
          {
            using (Stream zipEntryStream = zipEntry.Open())
            {
              documentStream.CopyTo(zipEntryStream);
            }
          }
        }
      }

      return new FileContentResult(compressedFileStream.ToArray(),
        "application/zip")
      { FileDownloadName = "DischargedReferrals.zip" };
    }
  }

  private async Task ValidateReferral(
    IReferralCreate referralCreate,
    Entities.Referral existingReferral)
  {

    // remove the ethnicity if it doesn't match by triage name
    if (!string.IsNullOrWhiteSpace(referralCreate.Ethnicity))
    {
      if (!await _context.Ethnicities
        .AnyAsync(e => e.TriageName == referralCreate.Ethnicity))
      {
        existingReferral.Ethnicity = null;
        referralCreate.Ethnicity = null;
      }
    }

    ValidateModelResult validationResult = Validators
      .ValidateModel(referralCreate);

    if (referralCreate.DateOfBirth.HasValue
      && referralCreate.DateOfBmiAtRegistration.HasValue
      && referralCreate.DateOfBirth.Value.Date == referralCreate.DateOfBmiAtRegistration.Value.Date)
    {
      // Value set to null to prevent inclusion of PID in UDAL extract.
      referralCreate.DateOfBmiAtRegistration = null;
      existingReferral.DateOfBmiAtRegistration = null;
    }

    if (validationResult.IsValid)
    {
      existingReferral.Status = ReferralStatus.New.ToString();
      existingReferral.StatusReason = null;
      existingReferral.MethodOfContact = (int)MethodOfContact.NoContact;
      existingReferral.NumberOfContacts = 0;
    }
    else
    {
      existingReferral.Status = ReferralStatus.Exception.ToString();
      if (string.IsNullOrWhiteSpace(existingReferral.StatusReason))
      {
        existingReferral.StatusReason = validationResult.GetErrorMessage();
      }
      else
      {
        existingReferral.StatusReason =
          $"{validationResult.GetErrorMessage()} " +
          $"{existingReferral.StatusReason}";
      }
    }
  }

  private async Task ValidateReferral(
    IPharmacyReferralCreate referralModel,
    Entities.IReferral referralEntity)
  {
    ValidateModelResult result = Validators.ValidateModel(referralModel);

    if (!await _context.Ethnicities
      .AnyAsync(s => s.GroupName == referralModel.ServiceUserEthnicityGroup))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicityGroup);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.DisplayName == referralModel.ServiceUserEthnicity))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicity);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    ValidationResult validation = await BmiValidationAsync(referralEntity);
    if (validation != null)
    {
      result.Results.Add(validation);
      result.IsValid = false;
    }

    if (!result.IsValid)
    {
      throw new PharmacyReferralValidationException(result.Results);
    }
  }

  private async Task ValidateReferral(
    ISelfReferralCreate referralModel,
    Entities.IReferral referralEntity)
  {
    ValidateModelResult result = Validators.ValidateModel(referralModel);

    if (!await _context.StaffRoles
      .AnyAsync(s => s.DisplayName == referralModel.StaffRole))
    {
      string memberName = nameof(referralModel.StaffRole);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.GroupName == referralModel.ServiceUserEthnicityGroup))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicityGroup);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.DisplayName == referralModel.ServiceUserEthnicity))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicity);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    ValidationResult validation = await BmiValidationAsync(referralEntity);
    if (validation != null)
    {
      result.Results.Add(validation);
      result.IsValid = false;
    }

    if (result.IsValid)
    {
      referralEntity.Status = ReferralStatus.New.ToString();
      referralEntity.StatusReason = null;
    }
    else
    {
      throw new SelfReferralValidationException(result.Results);
    }
  }

  private async Task ValidateReferral(
    IGeneralReferralCreate referralModel,
    Entities.IReferral referralEntity)
  {
    ValidateModelResult result = Validators.ValidateModel(referralModel);

    if (!await _context.Ethnicities
      .AnyAsync(s => s.GroupName == referralModel.ServiceUserEthnicityGroup))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicityGroup);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.DisplayName == referralModel.ServiceUserEthnicity))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicity);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _postcodeService.IsEnglishPostcodeAsync(referralModel.Postcode))
    {
      string memberName = nameof(referralModel.Postcode);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field does not contain a valid English postcode.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    ValidationResult validation = await BmiValidationAsync(referralEntity);
    if (validation != null)
    {
      result.Results.Add(validation);
      result.IsValid = false;
    }

    if (!result.IsValid)
    {
      throw new GeneralReferralValidationException(result.Results);
    }
  }

  private async Task ValidateCancelGeneralReferral(
    Entities.IReferral referral)
  {
    ValidateModelResult result = new()
    {
      IsValid = true
    };

    if (!await _context.Ethnicities
      .AnyAsync(s => s.GroupName == referral.ServiceUserEthnicityGroup))
    {
      string memberName = nameof(referral.ServiceUserEthnicityGroup);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.DisplayName == referral.ServiceUserEthnicity))
    {
      string memberName = nameof(referral.ServiceUserEthnicity);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    // Bmi MUST be too low for it to be a reason for cancellation,
    // so BmiValidationAsync MUST return a non-null ValidationResult.
    ValidationResult validation = await BmiValidationAsync(referral);
    bool isBmiTooLow = validation != null;

    int totalReasonsForCancellation = (isBmiTooLow ? 1 : 0)
      + (referral.HasActiveEatingDisorder ?? false ? 1 : 0)
      + (referral.HasHadBariatricSurgery ?? false ? 1 : 0)
      + (referral.IsPregnant ?? false ? 1 : 0);

    if (totalReasonsForCancellation > 1)
    {
      result.Results.Add(new ValidationResult(
        $"Unable to determine cancellation reason. More than one of: " +
          $"HasActiveEatingDisorder, HasHadBariatricSurgery, IsPregnant or " +
          $"BMI too low is present. Only one of these must be provided.",
        new string[] { "Multiple Properties" }));
      result.IsValid = false;
    }
    else if (totalReasonsForCancellation < 1)
    {
      result.Results.Add(new ValidationResult(
        $"Unable to determine cancellation reason. Either " +
          $"HasActiveEatingDisorder, HasHadBariatricSurgery or IsPregnant " +
          $"must be true or the BMI must be too low for the ethnicity.",
        new string[] { "Multiple Properties" }));
      result.IsValid = false;
    }

    if (!result.IsValid)
    {
      throw new GeneralReferralValidationException(result.Results);
    }
  }

  private async Task ValidateReferral(
    IGeneralReferralUpdate referralModel,
    Entities.IReferral referralEntity)
  {
    ValidateModelResult result = Validators.ValidateModel(referralModel);

    if (!await _context.Ethnicities
      .AnyAsync(s => s.GroupName == referralModel.ServiceUserEthnicityGroup))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicityGroup);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    if (!await _context.Ethnicities
      .AnyAsync(s => s.DisplayName == referralModel.ServiceUserEthnicity))
    {
      string memberName = nameof(referralModel.ServiceUserEthnicity);
      result.Results.Add(new ValidationResult(
        $"The {memberName} field is invalid.",
        new string[] { $"{memberName}" }));
      result.IsValid = false;
    }

    ValidationResult validation = await BmiValidationAsync(referralEntity);
    if (validation != null)
    {
      result.Results.Add(validation);
      result.IsValid = false;
    }

    if (!result.IsValid)
    {
      throw new GeneralReferralValidationException(result.Results);
    }
  }

  public async Task<IReferral> UpdateServiceUserEthnicityAsync(
    Guid id,
    string ethnicityDisplayName)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException(
        $"{nameof(id)} cannot be empty.",
        nameof(id));
    }

    if (string.IsNullOrWhiteSpace(ethnicityDisplayName))
    {
      throw new ArgumentException(
        $"{nameof(ethnicityDisplayName)} cannot be null or white space.",
        nameof(ethnicityDisplayName));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        referralEntity.Id, referralEntity.ProviderId);
    }

    var ethnicity = await _context
      .Ethnicities
      .Where(e => e.DisplayName == ethnicityDisplayName)
      .Where(e => e.IsActive)
      .Select(e => new
      {
        e.DisplayName,
        e.GroupName,
        e.MinimumBmi,
        e.TriageName
      })
      .SingleOrDefaultAsync();

    if (ethnicity == null)
    {
      throw new EthnicityNotFoundException("The ethnicity with a display " +
        $"name of {ethnicityDisplayName} cannot be found.");
    }

    if (referralEntity.CalculatedBmiAtRegistration == null
      && referralEntity.ReferralSource !=
        ReferralSource.GpReferral.ToString())
    {
      referralEntity.CalculatedBmiAtRegistration = BmiHelper.CalculateBmi(
        referralEntity.WeightKg.Value,
        referralEntity.HeightCm.Value);
    }

    referralEntity.ServiceUserEthnicityGroup = ethnicity.GroupName;
    referralEntity.ServiceUserEthnicity = ethnicity.DisplayName;
    referralEntity.Ethnicity = ethnicity.TriageName;

    ValidationResult validation = await BmiValidationAsync(referralEntity);

    UpdateModified(referralEntity);

    if (validation != null)
    {
      await _context.SaveChangesAsync();

      throw new BmiTooLowException(validation.ErrorMessage);
    }

    UpdateTriage(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    return referral;
  }

  /// <summary>
  /// Updates the status of each referral that has a status of ChatBotTransfer
  /// to RmcCall if the last chat bot call was more than 48 hours ago.
  /// </summary>
  /// <returns></returns>
  public virtual async Task<string> PrepareRmcCallsAsync()
  {
    DateTimeOffset after =
      DateTimeOffset.Now.AddHours(-HOURS_BEFORE_NEXT_STAGE).Date;

    List<Entities.Referral> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == ChatBotTransfer.ToString())
      .Where(r => !r.Calls.Any(c => c.IsActive && (c.Sent.Date > after || c.Sent == default)))
      .ToListAsync();

    foreach (Entities.Referral referral in referrals)
    {
      referral.Status = RmcCall.ToString();
      referral.MethodOfContact = (int)MethodOfContact.RmcCall;
      referral.DateToDelayUntil = null;
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return $"Prepared {referrals.Count} referral(s) for an RMC call.";
  }

  /// <summary>
  /// Updates each active referral's Status to RmcCall, MethodOfContact 
  /// to RmcCall and DateToDelayUntil to null if the date component of 
  /// DateToDelayUntil is before Now's date component and the Status of the 
  /// referral is RmcDelayed.
  /// </summary>
  public async Task<string> PrepareDelayedCallsAsync()
  {
    List<Entities.Referral> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.DateToDelayUntil.HasValue &&
        r.DateToDelayUntil.Value.Date < DateTimeOffset.Now.Date)
      .Where(r => r.Status == RmcDelayed.ToString())
      .ToListAsync();

    foreach (Entities.Referral referral in referrals)
    {
      referral.Status = RmcCall.ToString();
      referral.MethodOfContact = (int)MethodOfContact.RmcCall;
      referral.DateToDelayUntil = null;
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return $"Prepared DelayedCalls - " +
      $"{referrals.Count} referral(s) set to 'RmcCall'.";
  }

  /// <summary>
  /// Updates the Status and ProgrammeOutcome of each referral to FailedToContact where the
  /// referral has a status of TextMessage3, ChatBotCall1, ChatBotTransfer, RmcCall, RmcDelayed and
  /// a date of first contact attempt over 42 days ago, then updates Status to Complete or 
  /// AwaitingDischarge depending on ReferralSource.
  /// </summary>
  /// <returns>String message detailing number of referrals processed.</returns>
  public async Task<string> PrepareFailedToContactAsync()
  {
    DateTimeOffset dateThreshold = DateTimeOffset.Now.AddDays(-MAX_DAYS_UNTIL_FAILEDTOCONTACT);

    string[] referralStatuses =
    [
      ChatBotCall1.ToString(),
      ChatBotTransfer.ToString(),
      TextMessage3.ToString(),
      RmcCall.ToString(),
      RmcDelayed.ToString()
    ];

    IQueryable<Entities.Referral> referralsWithStatus = _context.Referrals
      .Include(r => r.TextMessages)
      .Include(r => r.Calls)
      .Where(r => r.IsActive)
      .Where(r => referralStatuses.Contains(r.Status));

    IEnumerable<Entities.Referral> referralsWithTextMessage1BeforeThreshold = referralsWithStatus
      .Where(r => r.TextMessages
        .Where(t => t.IsActive)
        .Where(t => t.ReferralStatus == TextMessage1.ToString())
        .Where(t => t.Sent != default)
        .Where(t => t.Sent < dateThreshold)
        .Any());

    IEnumerable<Entities.Referral> referralsWithCallBeforeThreshold = referralsWithStatus
      .Where(r => r.Calls
        .Where(c => c.IsActive)
        .Where(c => c.Sent != default)
        .Where(c => c.Sent < dateThreshold)
        .Any());

    IEnumerable<Entities.Referral> referralsToProcess = referralsWithTextMessage1BeforeThreshold
      .Union(referralsWithCallBeforeThreshold);

    foreach (Entities.Referral referral in referralsToProcess)
    {
      referral.Status = FailedToContact.ToString();
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    List<Entities.Referral> failedToContactReferrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == FailedToContact.ToString())
      .ToListAsync();

    foreach (Entities.Referral referral in failedToContactReferrals)
    {
      referral.ProgrammeOutcome = ProgrammeOutcome.FailedToContact.ToString();
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    IEnumerable<Entities.Referral> referralsForDischarge = failedToContactReferrals
      .Where(r => r.ReferralSource == ReferralSource.GpReferral.ToString());

    foreach (Entities.Referral referral in referralsForDischarge)
    {
      referral.Status = AwaitingDischarge.ToString();
      referral.DateCompletedProgramme = DateTimeOffset.Now;
      UpdateModified(referral);
    }

    IEnumerable<Entities.Referral> nonGpReferrals = failedToContactReferrals
      .Except(referralsForDischarge);

    foreach (Entities.Referral referral in nonGpReferrals)
    {
      referral.Status = Complete.ToString();
      referral.DateCompletedProgramme = DateTimeOffset.Now;
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return $"Processed {failedToContactReferrals.Count} FailedToContact referrals.";
  }

  public async Task<IReferral> UpdateEmail(Guid id, string email)
  {
    if (!RegexUtilities.IsValidEmail(email))
    {
      throw new ArgumentException("Email is invalid", nameof(email));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    referralEntity.Email = email.Trim();
    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      referral.Providers = new List<Provider>();
    }
    else
    {
      referral.Providers = providers.ToList();
    }

    return referral;
  }
  public virtual async Task<IReferral> UpdateReferralToStatusExceptionAsync(
   IReferralExceptionUpdate request)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    if (request.ExceptionType == CreateReferralException.Undefined)
    {
      throw new ReferralCreateException("Invalid " +
        $"{nameof(CreateReferralException)} enum value of " +
        $"{request.ExceptionType}");
    }

    ValidateModelResult validationResult = Validators.ValidateModel(request);

    if (!validationResult.IsValid)
    {
      throw new ReferralCreateException(validationResult.GetErrorMessage());
    }

    Entities.Referral entity = await _context.Referrals
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync(r => r.Ubrn == request.Ubrn);

    if (entity == null)
    {
      throw new ReferralNotFoundException(
        $"Referral not found with UBRN {request.Ubrn}.");
    }

    entity.IsErsClosed = false;
    entity.Status = ReferralStatus.Exception.ToString();
    UpdateModified(entity);

    List<string> validationErrors = new List<string>();

    switch (request.ExceptionType)
    {
      case CreateReferralException.NhsNumberMismatch:
        validationErrors.Add(
          "The NHS number in the eRS work list " +
          $"'{request.NhsNumberWorkList}' does not match the NHS number '" +
          $"{request.NhsNumberAttachment}' in the attached referral " +
          $"letter.");
        break;
      case CreateReferralException.MissingAttachment:
        validationErrors.Add(Constants.WarningMessages.NO_ATTACHMENT);
        break;
      case CreateReferralException.InvalidAttachment:
        validationErrors.Add(Constants.WarningMessages.INVALID_FILE_TYPE);
        break;
    }

    entity.StatusReason = string.Join(' ', validationErrors);

    await _context.SaveChangesAsync();

    Referral referralModel =
      _mapper.Map<Entities.Referral, Referral>(entity);

    return referralModel;

  }
  public async Task<IReferral> UpdateEthnicity(Guid id, Models.Ethnicity ethnicity)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException(
        $"{nameof(id)} cannot be empty.",
        nameof(id));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(id);

    if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(referralEntity.Id, referralEntity.ProviderId);
    }

    if (!ethnicity.MinimumBmi.HasValue)
    {
      throw new EthnicityNotFoundException($"MinimumBmi for Ethnicity {ethnicity.TriageName} " +
        "is not set.");
    }

    referralEntity.Ethnicity = ethnicity.TriageName;
    referralEntity.ServiceUserEthnicity = ethnicity.DisplayName;
    referralEntity.ServiceUserEthnicityGroup = ethnicity.GroupName;

    decimal bmi = referralEntity.CalculatedBmiAtRegistration ?? -1;
    bool isBmiTooLow = bmi < ethnicity.MinimumBmi.Value;

    if (isBmiTooLow)
    {
      referralEntity.TriagedCompletionLevel = null;
      referralEntity.TriagedWeightedLevel = null;
      referralEntity.OfferedCompletionLevel = null;
    }
    else
    {
      UpdateTriage(referralEntity);
    }

    UpdateModified(referralEntity);
    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);
    referral.IsBmiTooLow = isBmiTooLow;
    referral.SelectedEthnicGroupMinimumBmi = ethnicity.MinimumBmi.Value;

    if (isBmiTooLow)
    {
      referral.Providers = [];
    }
    else
    {
      IEnumerable<Provider> providers = await
        UpdateOfferedCompletionLevelAsync(referralEntity);

      if (providers == null || providers.Any() == false)
      {
        referral.Providers = [];
      }
      else
      {
        referral.Providers = providers.ToList();
      }
    }

    return referral;
  }

  public async Task<IReferral> UpdateDateOfBirth(
    Guid id,
    DateTimeOffset dateOfBirth)
  {
    int age = dateOfBirth.GetAge();
    if (age < Constants.MIN_GP_REFERRAL_AGE
      || age > Constants.MAX_GP_REFERRAL_AGE)
    {
      throw new AgeOutOfRangeException($"The {nameof(dateOfBirth)} " +
        $"must result in a service user's age between " +
        $"{Constants.MIN_GP_REFERRAL_AGE} and " +
        $"{Constants.MAX_GP_REFERRAL_AGE}.");
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    referralEntity.DateOfBirth = dateOfBirth;

    // Re-triage only if a provider has not been selected
    if (!referralEntity.ProviderId.HasValue)
    {
      UpdateTriage(referralEntity);
    }

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    Referral referral = _mapper.Map<Referral>(referralEntity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      referral.Providers = new List<Provider>();
    }
    else
    {
      referral.Providers = providers.ToList();
    }

    return referral;
  }

  public async Task<IReferral> UpdateMobile(
    Guid id,
    string mobile)
  {
    if (!PhoneExtensions.IsUkMobile(mobile))
    {
      throw new ArgumentException("Mobile is invalid", nameof(mobile));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    referralEntity.Mobile = mobile;

    UpdateModified(referralEntity);

    await _context.SaveChangesAsync();

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    Referral referral = _mapper.Map<Referral>(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      referral.Providers = new List<Provider>();
    }
    else
    {
      referral.Providers = providers.ToList();
    }

    return referral;
  }

  public async virtual Task<IEnumerable<Models.Ethnicity>> GetEthnicitiesAsync(
    ReferralSource referralSource)
  {
    IEnumerable<Models.Ethnicity> ethnicities = await _context
      .Ethnicities
      .Include(e => e.Overrides)
      .Where(e => e.IsActive)
      .OrderBy(o => o.GroupOrder).ThenBy(y => y.DisplayOrder)
      .Select(e => new Models.Ethnicity
      {
        DisplayName = e.Overrides
          .Where(o => o.IsActive && o.ReferralSource == referralSource)
          .FirstOrDefault().DisplayName ?? e.DisplayName,
        DisplayOrder = e.DisplayOrder,
        GroupName = e.Overrides
          .Where(o => o.IsActive && o.ReferralSource == referralSource)
          .FirstOrDefault().GroupName ?? e.GroupName,
        GroupOrder = e.GroupOrder,
        Id = e.Id,
        IsActive = e.IsActive,
        MinimumBmi = e.MinimumBmi,
        ModifiedAt = e.ModifiedAt,
        ModifiedByUserId = e.ModifiedByUserId,
        Census2001 = e.Census2001,
        NhsDataDictionary2001Code = e.NhsDataDictionary2001Code,
        NhsDataDictionary2001Description = e.NhsDataDictionary2001Description,
        TriageName = e.TriageName
      })
      .ToArrayAsync();

    if (!ethnicities.Any())
    {
      throw new EthnicityNotFoundException();
    }

    return ethnicities;
  }

  /// <inheritdoc/>
  public async Task<string> GetIdFromUbrn(string ubrn)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
    }

    try
    {
      string idMatchingUbrn = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Ubrn == ubrn || r.ProviderUbrn == ubrn)
        .Select(r => r.Id.ToString())
        .SingleOrDefaultAsync();

      return idMatchingUbrn;
    }
    catch (InvalidOperationException ex)
    {
      throw new InvalidOperationException(
        $"Multiple referrals with the UBRN '{ubrn}' exist.", ex);
    }
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<string>> GetIdsFromUbrns(IEnumerable<string> ubrns)
  {
    ArgumentNullException.ThrowIfNull(ubrns);

    if (ubrns.Any() == false)
    {
      throw new ArgumentException("Provided UBRNS must not be empty.");
    }

    List<string> idsMatchingUbrns = [];

    foreach (string ubrn in ubrns)
    {
      idsMatchingUbrns.Add(await GetIdFromUbrn(ubrn));
    }

    return idsMatchingUbrns;
  }

  public async Task<bool> EthnicityToServiceUserEthnicityMatch
    (string ethnicity, string serviceUserEthnicity)
  {
    if (string.IsNullOrWhiteSpace(ethnicity) ||
        string.IsNullOrWhiteSpace(serviceUserEthnicity))
    {
      return false;
    }

    return await _context.Ethnicities
      .Where(t => t.IsActive)
      .Where(t => t.TriageName.Equals(ethnicity))
      .Where(t => t.DisplayName.Equals(serviceUserEthnicity))
      .AnyAsync();

  }

  public async Task<bool> EthnicityToGroupNameMatch
    (string ethnicity, string groupName)
  {
    if (string.IsNullOrWhiteSpace(ethnicity) ||
        string.IsNullOrWhiteSpace(groupName))
    {
      return false;
    }

    return await _context.Ethnicities
      .Where(t => t.IsActive)
      .Where(t => t.TriageName.Equals(ethnicity))
      .Where(t => t.GroupName.Equals(groupName))
      .AnyAsync();

  }

  public async Task<bool> ReferralUpdateValidator(Guid referralId,
    GeneralReferralCreate referralCreate)
  {
    Entities.Referral entity = await _context.Referrals
      .SingleOrDefaultAsync(t => t.Id == referralId);

    if (entity == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    List<string> validationErrors = new List<string>();
    if (referralCreate.Ethnicity != entity.Ethnicity)
    {
      validationErrors.Add("Ethnicity does not match previous value.");
      entity.Ethnicity = referralCreate.Ethnicity;
    }

    if (referralCreate.ServiceUserEthnicityGroup !=
        entity.ServiceUserEthnicityGroup)
    {
      validationErrors.Add(
        "ServiceUserEthnicityGroup does not match previous value.");
      entity.ServiceUserEthnicityGroup =
        referralCreate.ServiceUserEthnicityGroup;
    }

    if (referralCreate.ServiceUserEthnicity != entity.ServiceUserEthnicity)
    {
      validationErrors.Add(
        "ServiceUserEthnicity does not match previous value.");
      entity.ServiceUserEthnicity = referralCreate.ServiceUserEthnicity;
    }

    if (referralCreate.Email != entity.Email)
    {
      validationErrors.Add(
        "Email does not match previous value.");
      entity.Email = referralCreate.Email;
    }

    if (referralCreate.WeightKg != entity.WeightKg)
    {
      validationErrors.Add(
        "WeightKg does not match previous value.");
      entity.WeightKg = referralCreate.WeightKg;
    }

    if (referralCreate.HeightCm != entity.HeightCm)
    {
      validationErrors.Add(
        "HeightCm does not match previous value.");
      entity.HeightCm = referralCreate.HeightCm;
    }

    if (referralCreate.DateOfBirth != entity.DateOfBirth)
    {
      validationErrors.Add(
        "DateOfBirth does not match previous value.");
      entity.DateOfBirth = referralCreate.DateOfBirth;
    }

    if (validationErrors.Any())
    {
      entity.StatusReason = string.Join(' ', validationErrors);
      await _context.SaveChangesAsync();
      return false;
    }

    return true;

  }

  public async Task<IReferral> UpdateGeneralReferral(
    IGeneralReferralUpdate update)
  {
    if (update is null)
    {
      throw new ArgumentNullException(nameof(update));
    }

    Entities.Referral entity = await _context
      .Referrals
      .Where(r => r.Id == update.Id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (entity == null)
    {
      throw new ReferralNotFoundException(update.Id);
    }

    if (entity.NhsNumber != update.NhsNumber)
    {
      throw new NhsNumberUpdateReferralMismatchException(
        entity.NhsNumber, update.NhsNumber);
    }

    ReferralStatus isValidUpdateStatus =
      New | RmcCall | RmcDelayed |
      TextMessage1 | TextMessage2 |
      ChatBotCall1 | ChatBotTransfer | TextMessage3;

    entity.Status.TryParseToEnumName(out ReferralStatus status);

    if (!isValidUpdateStatus.HasFlag(status) || status == 0)
    {
      throw new ReferralInvalidStatusException(
        $"Referral {entity.Id} has a status of {status} and " +
        $"cannot be updated.");
    }

    _mapper.Map(update, entity);

    await ValidateReferral(update, entity);

    await UpdateDeprivation(entity);
    UpdateModified(entity);

    UpdateTriage(entity);

    //Save to DB
    await _context.SaveChangesAsync();

    IReferral referralModel = _mapper.Map<Referral>(entity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(entity);

    if (providers == null || providers.Any() == false)
    {
      throw new NoProviderChoicesFoundException(referralModel.Id);
    }
    else
    {
      referralModel.Providers = providers.ToList();
    }

    return referralModel;
  }

  public async Task<IEnumerable<IStaffRole>> GetStaffRolesAsync()
  {
    IEnumerable<IStaffRole> staffRoles = await _context
      .StaffRoles
      .AsNoTracking()
      .Where(e => e.IsActive)
      .OrderBy(o => o.DisplayOrder)
      .ProjectTo<StaffRole>(_mapper.ConfigurationProvider)
      .ToListAsync();

    if (!staffRoles.Any())
    {
      throw new StaffRolesNotFoundException();
    }

    return staffRoles;
  }

  /// <inheritdoc/>
  public async Task<TriageLevel> GetTriagedCompletionLevelAsync(Guid id)
  {
    Referral referral = await _context
      .Referrals
      .Where(x => x.Id == id)
      .Select(x => new Referral() { TriagedCompletionLevel = x.TriagedCompletionLevel })
      .SingleOrDefaultAsync();

    if (referral == null)
    {
      throw new ReferralNotFoundException(id);
    }
    if (string.IsNullOrWhiteSpace(referral.TriagedCompletionLevel))
    {
      throw new InvalidOperationException(
        $"The referral with an id of {id} does not have a TriagedCompletionLevel.");
    }

    if (!Enum.TryParse(referral.TriagedCompletionLevel, out TriageLevel triagedCompletionLevel))
    {
      throw new TriageNotFoundException(
        $"The referral with an id of {id} has an invalid TriagedCompletionLevel of " +
        $"{referral.TriagedCompletionLevel}.");
    }

    return triagedCompletionLevel;
  }

  /// <inheritdoc />
  public async Task<List<string>> GetProviderNamesOrderAsync(Guid id)
  {
    List<string> providersOrder = null;

    string providerList = await _context
      .Analytics
      .Where(x => x.LinkId == id)
      .Where(x => x.LinkDescription == "Referral")
      .Where(x => x.PropertyLookup == (int)PropertyLookup.ProviderOrder)
      .Select(x => x.Value)
      .SingleOrDefaultAsync();

    if (providerList != null)
    {
      providersOrder = providerList
        .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();
    }

    return providersOrder;
  }

  public virtual async Task<IReferral> CreateSelfReferral(
    ISelfReferralCreate selfReferralCreate)
  {
    if (selfReferralCreate == null)
    {
      throw new ArgumentNullException(nameof(selfReferralCreate));
    }

    await CheckSelfReferralIsUniqueAsync(selfReferralCreate.Email);

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(selfReferralCreate);

    referralEntity.CalculatedBmiAtRegistration = BmiHelper
      .CalculateBmi(selfReferralCreate.WeightKg, selfReferralCreate.HeightCm);
    referralEntity.DateOfReferral = DateTimeOffset.Now;
    referralEntity.ReferringGpPracticeNumber =
      Constants.UNKNOWN_GP_PRACTICE_NUMBER;
    referralEntity.ReferringGpPracticeName =
      Constants.UNKNOWN_GP_PRACTICE_NAME;
    referralEntity.IsActive = true;
    referralEntity.Status = New.ToString();
    referralEntity.StatusReason = null;
    referralEntity.ReferralSource = ReferralSource.SelfReferral.ToString();
    referralEntity.CreatedDate = DateTimeOffset.Now;

    await ValidateReferral(selfReferralCreate, referralEntity);

    await UpdateDeprivation(referralEntity);
    UpdateModified(referralEntity);

    UpdateTriage(referralEntity);

    //Save to DB
    _context.Referrals.Add(referralEntity);
    await _context.SaveChangesAsync();

    try
    {
      await UpdateSelfReferralUbrnAsync(referralEntity.Id);
    }
    catch (Exception ex)
    {
      _log.Error(ex.Message);

      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason = "Error adding Ubrn to Referral.";
      referralEntity.IsActive = false;
      UpdateModified(referralEntity);
      await _context.SaveChangesAsync();

      throw new ReferralUpdateException("An error occurred creating this referral and it was " +
        "not saved. Please try again later.");
    }

    IReferral referralModel = _mapper.Map<Referral>(referralEntity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      throw new NoProviderChoicesFoundException(referralModel.Id);
    }
    else
    {
      referralModel.Providers = providers.ToList();
    }

    return referralModel;
  }

  public virtual async Task<IReferral> CreateGeneralReferral(
    IGeneralReferralCreate referralCreate)
  {
    if (referralCreate == null)
    {
      throw new ArgumentNullException(nameof(referralCreate));
    }

    await CheckReferralCanBeCreatedWithNhsNumberAsync(
      referralCreate.NhsNumber);

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(referralCreate);

    referralEntity.CalculatedBmiAtRegistration = BmiHelper
      .CalculateBmi(referralCreate.WeightKg, referralCreate.HeightCm);
    referralEntity.DateOfReferral = DateTimeOffset.Now;
    referralEntity.ReferringGpPracticeNumber = referralCreate
      .ReferringGpPracticeNumber;
    referralEntity.ReferringGpPracticeName = referralCreate
      .ReferringGpPracticeName;

    referralEntity.IsActive = true;
    referralEntity.Status = New.ToString();
    referralEntity.StatusReason = null;
    referralEntity.ReferralSource =
      ReferralSource.GeneralReferral.ToString();
    referralEntity.CreatedDate = DateTimeOffset.Now;

    await ValidateReferral(referralCreate, referralEntity);

    await UpdateDeprivation(referralEntity);
    UpdateModified(referralEntity);

    UpdateTriage(referralEntity);

    //Save to DB
    _context.Referrals.Add(referralEntity);
    await _context.SaveChangesAsync();

    try
    {
      await UpdateGeneralReferralUbrnAsync(referralEntity.Id);
    }
    catch (Exception ex)
    {
      _log.Error(ex.Message);

      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason = "Error adding Ubrn to Referral.";
      referralEntity.IsActive = false;
      UpdateModified(referralEntity);
      await _context.SaveChangesAsync();

      throw new ReferralUpdateException("An error occurred creating this referral and it was " +
        "not saved. Please try again later.");
    }

    IReferral referralModel = _mapper.Map<Referral>(referralEntity);

    IEnumerable<Provider> providers = await
      UpdateOfferedCompletionLevelAsync(referralEntity);

    if (providers == null || providers.Any() == false)
    {
      throw new NoProviderChoicesFoundException(referralModel.Id);
    }
    else
    {
      referralModel.Providers = providers.ToList();
    }

    return referralModel;
  }

  private async Task<IEnumerable<Provider>> UpdateOfferedCompletionLevelAsync(
    Entities.Referral entity)
  {
    if (entity is null)
    {
      throw new ArgumentNullException(nameof(entity));
    }

    IEnumerable<Provider> providers = null;

    if (entity.TriagedCompletionLevel != null)
    {
      if (Enum.TryParse(entity.TriagedCompletionLevel, out TriageLevel triageLevel))
      {
        providers = await _providerService.GetProvidersAsync(triageLevel);

        if ((providers == null || providers.Any() == false) && triageLevel != TriageLevel.Low)
        {
          entity.OfferedCompletionLevel = TriageLevel.Low.ToString("d");

          providers = await _providerService.GetProvidersAsync(TriageLevel.Low);
        }
        else
        {
          entity.OfferedCompletionLevel = entity.TriagedCompletionLevel;
        }

        await _context.SaveChangesAsync();
      }
      else
      {
        throw new UnexpectedEnumValueException(
          "Unable to update offered completion level for referral " +
          $"{entity.Id} because it has an invalid TriageLevel enum of " +
          $"'{entity.TriagedCompletionLevel}'");
      }
    }

    return providers;
  }

  public virtual async Task<IReferral> CreatePharmacyReferral(
    IPharmacyReferralCreate pharmacyReferralCreate)
  {
    if (pharmacyReferralCreate == null)
    {
      throw new ArgumentNullException(nameof(pharmacyReferralCreate));
    }

    await CheckReferralCanBeCreatedWithNhsNumberAsync(
      pharmacyReferralCreate.NhsNumber);

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(pharmacyReferralCreate);

    referralEntity.DateOfReferral = DateTimeOffset.Now;

    if (string.IsNullOrWhiteSpace(referralEntity.ReferringGpPracticeNumber))
    {
      referralEntity.ReferringGpPracticeNumber =
        Constants.UNKNOWN_GP_PRACTICE_NUMBER;
    }

    if (string.IsNullOrWhiteSpace(referralEntity.ReferringGpPracticeName))
    {
      referralEntity.ReferringGpPracticeName =
        Constants.UNKNOWN_GP_PRACTICE_NAME;
    }

    referralEntity.IsActive = true;
    referralEntity.Status = New.ToString();
    referralEntity.StatusReason = null;
    referralEntity.ReferralSource = ReferralSource.Pharmacy.ToString();
    referralEntity.CreatedDate = DateTimeOffset.Now;

    await ValidateReferral(pharmacyReferralCreate, referralEntity);

    await UpdateDeprivation(referralEntity);
    UpdateModified(referralEntity);

    UpdateTriage(referralEntity);

    //Save to DB
    _context.Referrals.Add(referralEntity);
    await _context.SaveChangesAsync();

    try
    {
      await UpdatePharmacyReferralUbrnAsync(referralEntity.Id);
    }
    catch (Exception ex)
    {
      _log.Error(ex.Message);

      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason = "Error adding Ubrn to Referral.";
      referralEntity.IsActive = false;
      UpdateModified(referralEntity);
      await _context.SaveChangesAsync();

      throw new ReferralUpdateException("An error occurred creating this referral and it was " +
        "not saved. Please try again later.");
    }

    IReferral referralModel = _mapper.Map<Referral>(referralEntity);

    return referralModel;
  }

  public async Task<IPharmacistKeyCodeGenerationResponse>
    GetPharmacistKeyCodeAsync(IPharmacistKeyCodeCreate create)
  {
    ValidateModelResult result = ValidateModel(create);
    if (!result.IsValid)
    {
      throw new PharmacyReferralValidationException(result.Results);
    }

    PharmacistKeyCodeGenerationResponse response =
      new PharmacistKeyCodeGenerationResponse
      {
        KeyCode = create.KeyCode,
        ReferringPharmacyEmail = create.ReferringPharmacyEmail,
        Expires = DateTimeOffset.Now.AddMinutes(create.ExpireMinutes),
        ExpireMinutes = create.ExpireMinutes
      };

    Pharmacist pharmacist =
      await _context.Pharmacists.SingleOrDefaultAsync(t =>
        t.ReferringPharmacyEmail == create.ReferringPharmacyEmail);
    if (pharmacist == null)
    {
      pharmacist = new Pharmacist
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        ReferringPharmacyEmail = create.ReferringPharmacyEmail
      };
      await _context.Pharmacists.AddAsync(pharmacist);
    }

    if (!pharmacist.IsActive)
    {
      response.Errors.Add(
        $"Pharmacist email, {create.ReferringPharmacyEmail} exists " +
        $"but account is not active.");
      return response;

    }

    pharmacist.KeyCode = response.KeyCode;
    pharmacist.Expires = response.Expires;
    pharmacist.TryCount = 0;

    response.Id = pharmacist.Id;
    UpdateModified(pharmacist);
    await _context.SaveChangesAsync();

    return response;
  }

  public async Task<IPharmacistKeyCodeValidationResponse>
    ValidatePharmacistKeyCodeAsync(IPharmacistKeyCodeCreate create)
  {
    ValidateModelResult result = ValidateModel(create);
    if (!result.IsValid)
    {
      throw new PharmacyReferralValidationException(result.Results);
    }

    PharmacistKeyCodeValidationResponse response =
      new PharmacistKeyCodeValidationResponse
      {
        KeyCode = create.KeyCode,
        ReferringPharmacyEmail = create.ReferringPharmacyEmail
      };

    Pharmacist pharmacist = await _context.Pharmacists
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(p =>
        p.ReferringPharmacyEmail == create.ReferringPharmacyEmail);

    if (pharmacist == null)
    {
      throw new PharmacyReferralValidationException(
        new List<ValidationResult>() {
          new ValidationResult("Pharmacist not found with email " +
            $"{create.ReferringPharmacyEmail}.", new string[] { "Email" })
        });
    }

    if (pharmacist.Expires < DateTimeOffset.Now)
    {
      throw new PharmacyKeyCodeExpiredException();
    }

    const int MAX_TRYCOUNT = 3;

    if (pharmacist.KeyCode != create.KeyCode ||
      pharmacist.TryCount > MAX_TRYCOUNT)
    {
      pharmacist.TryCount++;
      UpdateModified(pharmacist);
      await _context.SaveChangesAsync();

      if (pharmacist.TryCount > MAX_TRYCOUNT)
      {
        throw new PharmacyKeyCodeTooManyAttemptsException();
      }
      else
      {
        throw new PharmacyKeyCodeIncorrectException();
      }
    }

    pharmacist.TryCount = 0;
    UpdateModified(pharmacist);
    await _context.SaveChangesAsync();

    response.Id = pharmacist.Id;
    response.ValidCode = true;

    return response;
  }

  public async Task<bool> PharmacyEmailListedAsync(
    string referringPharmacyEmail)
  {
    if (string.IsNullOrWhiteSpace(referringPharmacyEmail))
    {
      return false;
    }

    return await _context.Pharmacists.AnyAsync(
      t => t.ReferringPharmacyEmail == referringPharmacyEmail && t.IsActive);
  }

  public async Task<string> GetProviderNameAsync(Guid providerId)
  {
    if (providerId == Guid.Empty)
    {
      return "A provider";
    }

    Entities.Provider entity =
      await _context.Providers.SingleOrDefaultAsync(t => t.Id == providerId);

    return entity.Name ?? "A provider";

  }

  public async Task<IReferral> UpdateReferralWithProviderAsync(
    Guid referralId, Guid providerId,
    ReferralSource source = ReferralSource.GpReferral)
  {
    Entities.Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync(r => r.Id == referralId)
      ?? throw new ReferralNotFoundException(referralId);

    if (referral.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(
        $"Referral Id {referralId} already has a provider selected.");
    }

    string[] validGeneralReferralUpdateStatuses =
    [
      ChatBotCall1.ToString(),
      ChatBotTransfer.ToString(),
      New.ToString(),
      RmcCall.ToString(),
      RmcDelayed.ToString(),
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      TextMessage3.ToString()
    ];

    if (source.ToString().Is(ReferralSource.GeneralReferral))
    {

      if (!validGeneralReferralUpdateStatuses.Contains(referral.Status))
      {
        throw new ReferralInvalidStatusException(
          $"Referral Id {referral.Id} has an invalid status of " +
          $"{referral.Status}.");
      }
    }
    else
    {
      if (referral.Status != New.ToString())
      {
        throw new ReferralInvalidStatusException(
          $"Referral Id {referral.Id} has an invalid status of " +
          $"{referral.Status}, expecting a status of {New}.");
      }
    }

    IEnumerable<Provider> providers =
      await GetProvidersByTriageAsync(referral.TriagedCompletionLevel);

    if (!providers.Any(t => t.Id == providerId))
    {
      throw new ProviderSelectionMismatch(providerId);
    }

    referral.ProviderId = providerId;
    referral.Status = string.IsNullOrWhiteSpace(referral.NhsNumber)
      ? ProviderAwaitingTrace.ToString()
      : ProviderAwaitingStart.ToString();
    referral.StatusReason = string.IsNullOrWhiteSpace(referral.NhsNumber)
      ? "NHS number awaiting trace."
      : referral.StatusReason;
    referral.DateOfProviderSelection = DateTimeOffset.Now;

    UpdateModified(referral);

    await _context.SaveChangesAsync();

    IReferral referralModel = _mapper.Map<Referral>(referral);

    return referralModel;
  }

  protected virtual async Task<IEnumerable<Provider>>
    GetProvidersByTriageAsync(string triageCompletionLevel)
  {
    if (string.IsNullOrWhiteSpace(triageCompletionLevel))
    {
      throw new TriageNotFoundException("Triage completion level is null");
    }

    if (int.TryParse(triageCompletionLevel, out int triageLevel))
    {
      if (Enum.IsDefined(typeof(TriageLevel), triageLevel))
      {
        IEnumerable<Provider> providers = await _providerService
          .GetProvidersAsync(triageLevel);

        return providers.ToList();
      }
    }

    throw new ProviderNotFoundException(
      $"Providers not found for Triage Completion " +
      $"Level {triageCompletionLevel}");
  }

  /// <summary>
  /// Updates the Ubrn and ProviderUbrn properties of the provided referral.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <exception cref="ReferralInvalidCreationException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralUpdateException"></exception>
  public virtual async Task UpdateSelfReferralUbrnAsync(
    Guid referralId)
  {
    // Does the SelfReferral already exist.
    SelfReferral found = await _context
      .SelfReferrals
      .SingleOrDefaultAsync(t => t.ReferralId == referralId);

    if (found != null)
    {
      throw new ReferralInvalidCreationException(
        $"There is already a self referral with the ID {referralId} has " +
        $"already been created");
    }

    Entities.Referral referral = await _context
      .Referrals
      .SingleOrDefaultAsync(t => t.Id == referralId);

    if (referral == null)
    {
      throw new ReferralNotFoundException(
        $"Referral with the ID of {referralId} was not found ");
    }

    SelfReferral selfReferral = new()
    {
      ReferralId = referralId
    };

    _context.SelfReferrals.Add(selfReferral);
    await _context.SaveChangesAsync();
    Models.SelfReferral model = _mapper
      .Map<SelfReferral, Models.SelfReferral>(selfReferral);
    if (!string.IsNullOrWhiteSpace(model.Ubrn))
    {
      referral.Ubrn = model.Ubrn;
      referral.ProviderUbrn = model.Ubrn;

      await _context.SaveChangesAsync();
    }
    else
    {
      throw new ReferralUpdateException(
        $"SelfReferral for Referral ID {referralId} was not saved.");
    }
  }

  /// <summary>
  /// Updates the Ubrn and ProviderUbrn properties of the provided referral.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <exception cref="ReferralInvalidCreationException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralUpdateException"></exception>
  public virtual async Task UpdateGeneralReferralUbrnAsync(Guid referralId)
  {
    //Does the DirectReferral already exists
    GeneralReferral found =
      await _context.GeneralReferrals.SingleOrDefaultAsync(t =>
        t.ReferralId == referralId);

    if (found != null)
    {
      throw new ReferralInvalidCreationException(
        $"There is already a public referral with the ID {referralId} has " +
        $"already been created");
    }

    Entities.Referral referral =
      await _context.Referrals.SingleOrDefaultAsync(t => t.Id == referralId);

    if (referral == null)
    {
      throw new ReferralNotFoundException(
        $"Referral with the ID of {referralId} was not found ");
    }

    Entities.GeneralReferral directReferral = new()
    {
      ReferralId = referralId
    };

    _context.GeneralReferrals.Add(directReferral);
    await _context.SaveChangesAsync();
    Models.GeneralReferral model = _mapper
      .Map<GeneralReferral, Models.GeneralReferral>(directReferral);
    if (!string.IsNullOrWhiteSpace(model.Ubrn))
    {
      referral.Ubrn = model.Ubrn;
      referral.ProviderUbrn = model.Ubrn;

      await _context.SaveChangesAsync();
    }
    else
    {
      throw new ReferralUpdateException(
        $"DirectReferral for Referral ID {referralId} was not saved.");
    }
  }

  /// <summary>
  /// Updates the Ubrn and ProviderUbrn properties of the provided referral.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <exception cref="ReferralInvalidCreationException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralUpdateException"></exception>
  public virtual async Task UpdatePharmacyReferralUbrnAsync(
    Guid referralId)
  {
    PharmacyReferral found =
      await _context.PharmacyReferrals.SingleOrDefaultAsync(t =>
        t.ReferralId == referralId);

    if (found != null)
    {
      throw new ReferralInvalidCreationException(
        $"There is already a pharmacy referral with the ID {referralId} " +
        $"has already been created");
    }

    Entities.Referral referral =
      await _context.Referrals.SingleOrDefaultAsync(t => t.Id == referralId);

    if (referral == null)
    {
      throw new ReferralNotFoundException(
        $"Referral with the ID of {referralId} was not found ");
    }

    PharmacyReferral pharmacyReferral = new PharmacyReferral
    {
      ReferralId = referralId
    };

    _context.PharmacyReferrals.Add(pharmacyReferral);
    await _context.SaveChangesAsync();
    Models.PharmacyReferral model = _mapper
      .Map<PharmacyReferral, Models.PharmacyReferral>(pharmacyReferral);
    if (!string.IsNullOrWhiteSpace(model.Ubrn))
    {
      referral.Ubrn = model.Ubrn;
      referral.ProviderUbrn = model.Ubrn;

      await _context.SaveChangesAsync();
    }
    else
    {
      throw new ReferralUpdateException(
        $"PharmacyReferral for Referral ID {referralId} was not saved.");
    }
  }

  protected virtual async Task<ValidationResult> BmiValidationAsync(
    Entities.IReferral entity)
  {
    Entities.Ethnicity ethnicity =
      await _context.Ethnicities.FirstOrDefaultAsync(t =>
        t.TriageName == entity.Ethnicity);

    if (entity.CalculatedBmiAtRegistration == null)
    {
      return new ValidationResult(
        "Validation of BMI cannot proceed without a valid BMI value.",
        new string[] { nameof(entity.CalculatedBmiAtRegistration) });
    }

    if (ethnicity == null)
    {
      throw new EthnicityNotFoundException(
        $"Ethnicity {entity.Ethnicity} not found.");
    }

    if (ethnicity.MinimumBmi > (entity.CalculatedBmiAtRegistration ?? 0))
    {
      return new ValidationResult(
        $"The calculated BMI of {entity.CalculatedBmiAtRegistration} is " +
        $"too low, the minimum for an ethnicity of {ethnicity.TriageName} " +
        $"is {Math.Round(ethnicity.MinimumBmi, 2)}.",
        new string[] { nameof(entity.CalculatedBmiAtRegistration) });
    }

    return null;
  }

  /// <summary>
  /// Updates a referral matched on UBRN
  /// </summary>
  /// <param name="ubrn">The UBRN to update</param>
  /// <returns>A boolean indicating if the update was successful or not
  /// </returns>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralInvalidStatusException"></exception>
  public virtual async Task<int> UpdateReferralCancelledByEReferralAsync(
    string ubrn)
  {
    List<Entities.Referral> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Ubrn == ubrn)
      .ToListAsync();

    if (!referrals.Any())
    {
      throw new ReferralNotFoundException(
        $"An active referral was not found with a ubrn of {ubrn}.");
    }

    foreach (Entities.Referral referral in referrals)
    {
      if (referral.Status == RejectedToEreferrals.ToString())
      {
        referral.Status = CancelledByEreferrals.ToString();
        UpdateModified(referral);
      }
      else if (referral.Status == CancelledByEreferrals.ToString())
      {
        if (referrals.Count == 1)
        {
          throw new ReferralInvalidStatusException("Unable to cancel the " +
            $"referral because its status is {referral.Status}.");
        }
      }
      else
      {
        throw new ReferralInvalidStatusException("Unable to cancel the " +
          $"referral because its status is {referral.Status}.");
      }
    }

    return await _context.SaveChangesAsync();
  }

  public async Task TriageReferralUpdateAsync(Guid id)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException(nameof(id));
    }

    Entities.Referral entity = await _context
      .Referrals
      .Where(r => r.Id == id)
      .Where(r => r.IsActive)
      .SingleOrDefaultAsync();

    if (entity == null)
    {
      throw new ReferralNotFoundException(id);
    }

    UpdateTriage(entity);
    UpdateModified(entity);
    await _context.SaveChangesAsync();
  }

  public virtual bool UpdateTriage(Entities.Referral referral)
  {
    if (!referral.Sex.TryParseSex(out Sex sex))
    {
      return false;
    }

    if (!referral.Ethnicity.TryParseToEnumName(out Ethnicity ethnicity))
    {
      return false;
    }

    if (!referral.Deprivation
      .TryParseToEnumName(out Enums.Deprivation deprivation))
    {
      return false;
    }

    int age = referral.DateOfBirth.GetAge() ?? 0;

    CourseCompletionResult result =
      _patientTriageService.GetScores(new(age, sex, ethnicity, deprivation));

    referral.TriagedCompletionLevel = result
      .TriagedCompletionLevel.ToString("d");

    referral.TriagedWeightedLevel = result
      .TriagedWeightedLevel.ToString("d");

    referral.OfferedCompletionLevel = referral.TriagedCompletionLevel;

    return true;
  }

  private bool IncludeReferralStatusInSearch(ReferralSearch search)
  {
    bool hasSearchStatuses = search.Statuses != null;

    bool hasSearchParameters = search.Ubrn != null
      || search.FamilyName != null
      || search.TelephoneNumber != null
      || search.Postcode != null
      || search.NhsNumber != null
      || search.GivenName != null
      || search.MobileNumber != null
      || search.EmailAddress != null;

    return hasSearchStatuses && !hasSearchParameters;
  }

  public async Task UpdateDeprivation(Entities.Referral referralEntity)
  {
    if (referralEntity is null)
    {
      throw new ArgumentNullException(nameof(referralEntity));
    }

    try
    {
      string lsoa = await _postcodeService
        .GetLsoaAsync(referralEntity.Postcode);

      if (string.IsNullOrWhiteSpace(lsoa))
      {
        referralEntity.Deprivation = Enums.Deprivation.IMD1.ToString();
      }
      else
      {
        Models.Deprivation deprivation = await _deprivationService
          .GetByLsoa(lsoa);

        referralEntity.Deprivation = deprivation.ImdQuintile().ToString();
      }
    }
    catch (Exception ex)
    {
      if (ex is PostcodeNotFoundException
        || ex is DeprivationNotFoundException)
      {
        // Unable Postcode (i.e. new build) rather than creating exception
        // default the deprivation to IMD1 (the most deprived).
        referralEntity.Deprivation = Enums.Deprivation.IMD1.ToString();
      }
      else
      {
        throw;
      }
    }
  }

  public virtual async Task<CriCrudResponse> CriCrudAsync(
    ReferralClinicalInfo model, bool isDelete = false)
  {
    CriCrudResponse response = new CriCrudResponse(model);

    // Find active referral based on ubrn.
    Entities.Referral referral = await _context.Referrals
      .Include(t => t.Cri)
      .Where(t => t.IsActive)
      .SingleOrDefaultAsync(t => t.Ubrn == model.Ubrn);

    if (referral == null)
    {
      response.SetStatus(StatusType.UnableToFindReferral,
        $"Not found with UBRN of {model.Ubrn}");
      return response;
    }

    ReferralCri entity =
      _mapper.Map<ReferralClinicalInfo, Entities.ReferralCri>(model);

    if (entity == null)
    {
      response.SetStatus(StatusType.Invalid,
        "Mapping of object failed");
      return response;
    }

    // Check if already has CRI.
    if (referral.Cri == null)
    {
      referral.Cri = entity;
    }
    else
    {
      // Check if last update was before current.
      if (referral.Cri.ClinicalInfoLastUpdated >
          model.ClinicalInfoLastUpdated)
      {
        response.SetStatus(StatusType.NoRowsUpdated,
          $"Last Update referral of {model.ClinicalInfoLastUpdated} was " +
          $"before the current update date " +
          $"of {referral.Cri.ClinicalInfoLastUpdated}");
        return response;
      }

      Guid criId = referral.Cri.Id;

      referral.Cri.ClinicalInfoPdfBase64 = null;

      await _context.SaveChangesAsync();

      referral.Cri = entity;
      referral.Cri.UpdateOfCriId = criId;
    }

    UpdateModified(referral);

    await _context.SaveChangesAsync();

    await UpdateReferralAuditCri(model.Ubrn, referral.Cri.Id);

    // Return Cri as response.

    response.ResponseStatus = StatusType.Valid;

    return response;
  }

  public async Task<bool> UpdateReferralAuditCri(string ubrn, Guid criId)
  {
    List<Entities.ReferralAudit> audits =
      await _context.ReferralsAudit.Where(t => t.Ubrn == ubrn)
      .OrderByDescending(t => t.AuditId).Take(1).ToListAsync();

    if (!audits.Any())
    {
      throw new ReferralNotFoundException(
        $"Referral Audit not found for UBRN {ubrn}.");
    }

    Entities.ReferralAudit audit = audits.FirstOrDefault();

    audit.CriId = criId;

    return await _context.SaveChangesAsync() > 0;

  }

  public async Task<DateTimeOffset?> GetLastCriUpdatedAsync(string ubrn)
  {
    Entities.Referral referral =
      await _context.Referrals.Include(t => t.Cri).SingleOrDefaultAsync(t =>
        t.Ubrn == ubrn);

    if (referral == null)
    {
      throw new ArgumentNullException($"Not found with UBRN of {ubrn}");
    }

    if (referral.Cri == null)
    {
      return null;
    }

    return referral.Cri.ClinicalInfoLastUpdated;
  }

  public async Task<string> GetServiceUserLinkIdAsync(IReferral referral)
  {
    string serviceUserLinkId = await _context.TextMessages
     .Where(t => t.IsActive)
     .Where(t => t.ReferralId == referral.Id)
     .Where(t => !string.IsNullOrWhiteSpace(t.ServiceUserLinkId))
     .OrderBy(t => t.Sent)
     .Select(t => t.ServiceUserLinkId)
     .FirstOrDefaultAsync();

    if (string.IsNullOrWhiteSpace(serviceUserLinkId))
    {
      try
      {
        serviceUserLinkId = await _linkIdService.GetUnusedLinkIdAsync(3);
        Entities.TextMessage textMessage = new()
        {
          ServiceUserLinkId = serviceUserLinkId,
          IsActive = true,
          Number = referral.Mobile,
          ReferralId = referral.Id,
          ReferralStatus = referral.Status,
          Outcome = CallbackStatus.GeneratedByRmcCall
          .GetDescriptionAttributeValue(),
          Sent = DateTimeOffset.Now,
          Received = DateTimeOffset.Now,
        };

        UpdateModified(textMessage);
        _context.TextMessages.Add(textMessage);

        await _context.SaveChangesAsync();
      }
      catch
      {
        throw;
      }
    }

    return serviceUserLinkId;
  }

  public async Task<byte[]> GetCriDocumentAsync(string ubrn)
  {
    Entities.Referral referral =
      await _context.Referrals.Include(t => t.Cri).SingleOrDefaultAsync(t =>
        t.Ubrn == ubrn);

    if (referral == null)
    {
      throw new ArgumentNullException($"Not found with UBRN of {ubrn}");
    }

    if (referral.Cri == null)
    {
      return null;
    }

    return referral.Cri.ClinicalInfoPdfBase64.Decompress();
  }

  /// <summary>
  /// Inserts or Updates Analytics row linked to Referral.
  /// </summary>
  /// <param name="id">ReferralId</param>
  /// <param name="value">Comma separated list of Providers.</param>
  /// <returns></returns>
  public async Task UpdateAnalyticsForProviderList(Guid id, string value)
  {
    if (id == default)
    {
      throw new ArgumentException("Must not be an empty GUID.", nameof(id));
    }

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("Must not be null or white space.", nameof(value));
    }

    Entities.Referral referralEntity = await _context
      .Referrals
      .SingleOrDefaultAsync(r => r.Id == id);

    if (referralEntity == null)
    {
      throw new ReferralNotFoundException(id);
    }
    else if (referralEntity.ProviderId != null)
    {
      throw new ReferralProviderSelectedException(referralEntity.Id, referralEntity.ProviderId);
    }

    Analytics analytics = await _context.Analytics.SingleOrDefaultAsync(t => t.LinkId == id);

    if (analytics == null)
    {
      analytics = new Analytics
      {
        LinkId = id,
        LinkDescription = "Referral",
        PropertyLookup = (int)PropertyLookup.ProviderOrder,
      };
      _context.Analytics.Add(analytics);
    }

    analytics.Value = value;
    analytics.IsActive = true;
    UpdateModified(analytics);

    await _context.SaveChangesAsync();
  }

  private void UpdateCri(Referral model, Entities.Referral entity)
  {
    model.CriLastUpdated = entity.Cri?.ClinicalInfoLastUpdated;
  }

  public async Task IsEmailInUseAsync(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentException(
        $"'{nameof(email)}' cannot be null or whitespace.",
        nameof(email));
    }

    await CheckSelfReferralIsUniqueAsync(email);
  }

  public async Task<IReferralPostResponse> GetReferralCreateResponseAsync(
    IReferral referral)
  {
    SelfReferralPostResponse response = new SelfReferralPostResponse
    {
      Id = referral.Id,
      ProviderChoices =
        _mapper.Map<IEnumerable<ProviderForSelection>>(referral.Providers)
    };

    if (referral.Providers != null && referral.Providers.Any())
    {
      await UpdateAnalyticsForProviderList(
        referral.Id,
        string.Join(",", referral.Providers.Select(t => t.Name))
      );
    }

    if (referral.TriagedCompletionLevel != "1")
    {
      foreach (Provider provider in referral.Providers)
      {
        ProviderForSelection pfs =
          response.ProviderChoices.SingleOrDefault(t =>
            t.Id == provider.Id);
        if (referral.TriagedCompletionLevel == "2")
        {
          pfs.Summary = provider.Summary2;
        }
        else if (referral.TriagedCompletionLevel == "3")
        {
          pfs.Summary = provider.Summary3;
        }
      }
    }

    return response;

  }

  public async Task<List<ReferralDischarge>> GetDischarges()
  {
    List<ReferralDischarge> entities = await _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.Status == AwaitingDischarge.ToString())
      .Select(r => new ReferralDischarge
      {
        DateCompletedProgramme = r.DateCompletedProgramme.Value,
        Id = r.Id,
        LastRecordedWeight = r.LastRecordedWeight,
        LastRecordedWeightDate = r.LastRecordedWeightDate,
        ProgrammeOutcome = r.ProgrammeOutcome,
        ProviderName = r.Provider.Name,
        TriageLevel = r.OfferedCompletionLevel,
        Ubrn = r.Ubrn,
        WeightOnReferral = r.FirstRecordedWeight,
        NhsNumber = r.NhsNumber
      })
      .ToListAsync();

    return entities;
  }

  /// <inheritdoc/>
  public virtual async Task<List<GpDocumentProxyReferralDischarge>>
    GetDischargesForGpDocumentProxy()
  {
    IQueryable<Entities.Referral> referrals = _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.DateOfBirth.HasValue)
      .Where(r => r.DateOfReferral.HasValue)
      .Where(r => r.Status == AwaitingDischarge.ToString());

    if (_gpDocumentProxyOptions.PostDischargesLimit > 0)
    {
      referrals = referrals.OrderBy(r => r.DateOfReferral)
        .Take(_gpDocumentProxyOptions.PostDischargesLimit);
    }

    IQueryable<Entities.MskOrganisation> mskOrganisations = _context.MskOrganisations
      .Where(o => o.IsActive)
      .Where(o => o.SendDischargeLetters);

    IQueryable<Entities.Referral> mskReferrals =
      mskOrganisations
      .Join(
        referrals
          .Where(r => r.ReferralSource == ReferralSource.Msk.ToString())
          .Where(r => r.ConsentForReferrerUpdatedWithOutcome.HasValue)
          .Where(r => r.ConsentForReferrerUpdatedWithOutcome.Value),
        o => o.OdsCode,
        r => r.ReferringOrganisationOdsCode,
        (o, r) => r);

    List<GpDocumentProxyReferralDischarge> discharges =
      await GetDischargesFromQuery(referrals);

    List<GpDocumentProxyReferralDischarge> mskDischarges =
      await GetDischargesFromQuery(mskReferrals, DischargeDestination.Msk);

    return discharges.Concat(mskDischarges).ToList();
  }

  private async Task<List<GpDocumentProxyReferralDischarge>> GetDischargesFromQuery(
    IQueryable<Entities.Referral> referrals,
    DischargeDestination dischargeDestination = DischargeDestination.Gp)
  {
    string[] outcomesRequiringMessage = GpDocumentProxyHelper.ProgrammeOutcomesRequiringMessage();

    List<GpDocumentProxyReferralDischarge> discharges = await referrals.Select(referral =>
      new GpDocumentProxyReferralDischarge
      {
        DateCompletedProgramme = referral.DateCompletedProgramme,
        DateOfBirth = referral.DateOfBirth.Value,
        DateOfReferral = referral.DateOfReferral.Value,
        FamilyName = referral.FamilyName,
        GivenName = referral.GivenName,
        Id = referral.Id,
        LastRecordedWeight = referral.LastRecordedWeight,
        LastRecordedWeightDate = referral.LastRecordedWeightDate,
        Message = outcomesRequiringMessage.Contains(referral.ProgrammeOutcome)
          ? referral.StatusReason
          : null,
        NhsNumber = referral.NhsNumber,
        ProviderName = referral.Provider.Name,
        ProgrammeOutcome = referral.ProgrammeOutcome,
        ReferringOrganisationOdsCode = dischargeDestination == DischargeDestination.Gp
          ? referral.ReferringGpPracticeNumber
          : referral.ReferringOrganisationOdsCode,
        ReferralSource = referral.ReferralSource,
        Sex = referral.Sex,
        Ubrn = referral.Ubrn,
        WeightOnReferral = referral.FirstRecordedWeight,
      }).ToListAsync();

    foreach (GpDocumentProxyReferralDischarge discharge in discharges)
    {
      try
      {
        discharge.TemplateId = dischargeDestination == DischargeDestination.Gp
        ? _gpDocumentProxyOptions.Gp.GetTemplateId(discharge.ProgrammeOutcome)
        : _gpDocumentProxyOptions.Msk.GetTemplateId(discharge.ProgrammeOutcome);
      }
      catch (Exception ex)
      {
        _log.Error(ex, "Get Discharge for referral {Id} failed: {message}", discharge.Id, ex.Message);
      }
    }

    return discharges;
  }

  /// <inheritdoc/>
  public virtual async Task<List<Guid>> PostDischarges(
    List<GpDocumentProxyReferralDischarge> discharges)
  {
    List<GpDocumentProxyPostDischarge> postDischarges =
      GetPostDischargesFromDischarges(discharges);

    List<Guid> successfullyProcessedIds = new();
    int errors = 0;
    string latestError = null;

    foreach (GpDocumentProxyPostDischarge postDischarge in postDischarges)
    {
      if (postDischarge.TemplateId == null)
      {
        _log.Error(
          "Post Discharge for referral {ReferralId} has no Template Id " +
            "that matches its programme outcome.",
          postDischarge.ReferralId);
        errors++;
        latestError = $"Post Discharge for referral {postDischarge.ReferralId} has no Template " +
          "Id that matches its programme outcome.";
      }
      else
      {
        try
        {
          HttpContent content = new StringContent(
            JsonConvert.SerializeObject(postDischarge),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

          HttpResponseMessage responseMessage = await _httpClient
            .PostAsync(_gpDocumentProxyOptions.Endpoint + _gpDocumentProxyOptions.PostEndpoint,
              content);

          if (responseMessage.StatusCode == HttpStatusCode.OK)
          {
            string successErrorMessage = await HandlePostDischargeSuccessResponse(
              responseMessage,
              postDischarge.ReferralId);

            if (string.IsNullOrWhiteSpace(successErrorMessage))
            {
              successfullyProcessedIds.Add(postDischarge.ReferralId);
            }
            else
            {
              errors++;
              latestError = successErrorMessage;
            }
          }
          else if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
          {
            errors++;
            latestError = await HandlePostDischargeBadRequestResponse(
              responseMessage,
              postDischarge.ReferralId);
          }
          else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
          {
            throw new InvalidTokenException();
          }
          else
          {
            _log.Error(
              "Post Discharge for referral {ReferralId} returned status " +
                "{StatusCode}.",
              postDischarge.ReferralId,
              responseMessage.StatusCode);
            errors++;
            latestError = $"Post Discharge for referral {postDischarge.ReferralId} returned " +
                $"status {responseMessage.StatusCode}.";
          }
        }
        catch (InvalidTokenException)
        {
          throw;
        }
        catch (Exception ex)
        {
          _log.Error(ex, ex.Message);
          errors++;
          latestError = ex.Message;
        }
      }
    }

    if (errors > 0)
    {
      throw new PostDischargesException(latestError);
    }

    return successfullyProcessedIds;
  }

  private List<GpDocumentProxyPostDischarge> GetPostDischargesFromDischarges(
    List<GpDocumentProxyReferralDischarge> discharges)
  {
    List<GpDocumentProxyPostDischarge> postDischarges = discharges
      .Select(d => new GpDocumentProxyPostDischarge
      {
        DateOfBirth = d.DateOfBirth,
        DateCompletedProgramme = d.DateCompletedProgramme,
        DateOfReferral = d.DateOfReferral,
        FamilyName = d.FamilyName,
        FirstRecordedWeight = d.WeightOnReferral,
        GivenName = d.GivenName,
        LastRecordedWeight = d.LastRecordedWeight,
        LastRecordedWeightDate = d.LastRecordedWeightDate,
        Message = d.Message,
        NhsNumber = d.NhsNumber,
        ProviderName = d.ProviderName,
        ReferralId = d.Id,
        ReferralSource = d.ReferralSourceDescription,
        ReferringOrganisationOdsCode = d.ReferringOrganisationOdsCode,
        Sex = d.Sex,
        TemplateId = d.TemplateId
      })
      .ToList();

    return postDischarges;
  }

  private async Task<string> HandlePostDischargeSuccessResponse(
    HttpResponseMessage responseMessage,
    Guid referralId)
  {
    GpDocumentProxyPostResponse gpDocumentProxyResponse = JsonConvert
      .DeserializeObject<GpDocumentProxyPostResponse>(
        await responseMessage.Content.ReadAsStringAsync());
    string errorMessage = null;

    if (gpDocumentProxyResponse == null)
    {
      _log.Error(
        "Post Discharge for referral {referralId} returned 200 OK with empty request body.",
        referralId);
      errorMessage = $"Post Discharge for referral {referralId} returned 200 OK with empty " +
        "request body.";
    }
    else
    {
      Entities.Referral referral = await _context.Referrals
        .FindAsync(gpDocumentProxyResponse.ReferralId);

      if (referral == null)
      {
        _log.Error(
          "Post Discharge for referral {referralId} has no matching referral.",
          referralId);
        errorMessage = $"Post Discharge for referral {referralId} has no matching referral.";
      }
      else if (referral.Status == AwaitingDischarge.ToString())
      {
        if (gpDocumentProxyResponse.DocumentStatus == 
          DocumentStatus.OrganisationNotSupported.ToString())
        {
          referral.Status = UnableToDischarge.ToString();
          referral.StatusReason = gpDocumentProxyResponse.Message;
          UpdateModified(referral);

          await _context.SaveChangesAsync();

          referral.Status = Complete.ToString();
          UpdateModified(referral);

          await _context.SaveChangesAsync();
        }
        else if (gpDocumentProxyResponse.DocumentStatus ==
          DocumentStatus.Received.ToString())
        {
          referral.Status = SentForDischarge.ToString();
          UpdateModified(referral);

          await _context.SaveChangesAsync();
        }

        return null;
      }
      else if (referral.ReferralSource == ReferralSource.Msk.ToString() 
        && referral.Status == SentForDischarge.ToString()
        && (gpDocumentProxyResponse.DocumentStatus == 
          DocumentStatus.OrganisationNotSupported.ToString()
          || gpDocumentProxyResponse.DocumentStatus == DocumentStatus.Received.ToString()))
      {
        return null;
      }
      else
      {
        _log.Error(
          "Post Discharge for referral {referralId} with status {Status} returned document " +
            "status {DocumentStatus} and message {Message}.",
          referralId,
          referral.Status,
          gpDocumentProxyResponse.DocumentStatus,
          gpDocumentProxyResponse.Message);
        errorMessage = $"Post Discharge for referral {referralId} with status {referral.Status} " +
          $"returned document status {gpDocumentProxyResponse.DocumentStatus} and message " +
          $"{gpDocumentProxyResponse.Message}.";
      }
    }

    return errorMessage;
  }

  private async Task<string> HandlePostDischargeBadRequestResponse(
    HttpResponseMessage responseMessage,
    Guid referralId)
  {
    GpDocumentProxyPostBadRequest badRequest = JsonConvert
      .DeserializeObject<GpDocumentProxyPostBadRequest>(
        await responseMessage.Content.ReadAsStringAsync());
    string errorMessage = null;

    if (badRequest == null)
    {
      _log.Error(
        "Post Discharge for referral {referralId} returned 400 Bad Request with empty request " + 
          "body.",
        referralId);
      errorMessage = $"Post Discharge for referral {referralId} returned 400 Bad Request with " + 
        "empty request body.";
    }
    else
    {
      if (badRequest.Errors?.Count > 0)
      {
        List<string> errors = new();

        foreach (KeyValuePair<string, string[]> error in badRequest.Errors)
        {
          string errorString = string.Join(" ", error.Value);
          errors.Add(errorString);
        }

        string allErrors = string.Join(" ", errors);
        _log.Error(
          "Post Discharge for referral {referralId} returned 400 Bad Request with error {Title}: " +
            "{allErrors}.",
          referralId,
          badRequest.Title,
          allErrors);
        errorMessage = $"Post Discharge for referral {referralId} returned 400 Bad Request with " +
          $"error {badRequest.Title}: {allErrors}.";
      }
      else
      {
        _log.Error(
          "Post Discharge for referral {referralId} returned 400 Bad Request with error {Title}.",
          referralId,
          badRequest.Title);
        errorMessage = $"Post Discharge for referral {referralId} returned 400 Bad Request with " + 
          $"error {badRequest.Title}.";
      }
    }

    return errorMessage;
  }

  /// <inheritdoc/>
  public virtual async Task<GpDocumentProxyUpdateResponse> UpdateDischarges()
  {
    List<Entities.Referral> dischargedReferrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == SentForDischarge.ToString())
      .ToListAsync();

    string latestError = null;
    StringBuilder requestUrl = new();
    GpDocumentProxyUpdateResponse response = new();
    Guid templateId;

    foreach (Entities.Referral referral in dischargedReferrals)
    {
      templateId = Guid.Empty;
      requestUrl.Clear();

      try
      {
        templateId = _gpDocumentProxyOptions.Gp.GetTemplateId(referral.ProgrammeOutcome);

        requestUrl.Append(_gpDocumentProxyOptions.Endpoint);
        requestUrl.Append(_gpDocumentProxyOptions.UpdateEndpoint);
        requestUrl.Append(referral.Id);
        requestUrl.Append("?templateId=");
        requestUrl.Append(templateId);

        HttpResponseMessage responseMessage = await _httpClient.GetAsync(requestUrl.ToString());

        GpDocumentProxyHandleUpdateResponse handleUpdateResponse = 
          await HandleUpdateDischargesResponse(
            responseMessage,
            referral,
            templateId);

        response.Discharges.Add(new GpDocumentProxyUpdateResponseItem
        {
          Ubrn = referral.Ubrn,
          Status = referral.Status,
          UpdateStatus = handleUpdateResponse.DocumentUpdateStatus.ToString()
        });

        if (handleUpdateResponse.DocumentUpdateStatus == DocumentUpdateStatus.Error)
        {
          latestError = handleUpdateResponse.ErrorMessage;
        }
      }
      catch (InvalidTokenException)
      {
        throw;
      }
      catch (Exception ex)
      {
        if (ex.GetType() == typeof(ArgumentException))
        {
          _log.Error("Update Discharge for referral {Id} failed: {message}.",
            referral.Id,
            ex.Message);
          latestError = $"Update Discharge for referral {referral.Id} failed: {ex.Message}.";
        }
        else
        {
          _log.Error(ex, ex.Message);
          latestError = ex.Message;
        }

        response.Discharges.Add(new GpDocumentProxyUpdateResponseItem
        {
          Ubrn = referral.Ubrn,
          Status = referral.Status,
          UpdateStatus = DocumentUpdateStatus.Error.ToString()
        });
      }
    }

    if (response.CountOfError > 0)
    {
      throw new UpdateDischargesException(latestError);
    }

    return response;
  }

  private async Task<GpDocumentProxyHandleUpdateResponse> HandleUpdateDischargesResponse(
    HttpResponseMessage responseMessage,
    Entities.Referral referral,
    Guid templateId)
  {
    GpDocumentProxyHandleUpdateResponse handleUpdateResponse = new();

    if (responseMessage.StatusCode == HttpStatusCode.OK)
    {
      GpDocumentProxyDocumentUpdateResponse update = JsonConvert
       .DeserializeObject<GpDocumentProxyDocumentUpdateResponse>(
         await responseMessage.Content.ReadAsStringAsync());

      if (update == null)
      {
        _log.Error(
          "Update Discharge for referral {Id} with template {templateId} returned 200 OK with " +
            "empty request body.",
          referral.Id,
          templateId);
        handleUpdateResponse.ErrorMessage = $"Update Discharge for referral {referral.Id} with " +
          $"template {templateId} returned 200 OK with empty request body.";
      }
      else
      {
        if (update.DocumentStatus ==
          DocumentStatus.OrganisationNotSupported.ToString())
        {
          referral.Status = UnableToDischarge.ToString();
          referral.StatusReason = ORGANISATION_NOT_SUPPORTED;
          UpdateModified(referral);
          await _context.SaveChangesAsync();

          referral.Status = Complete.ToString();
          referral.StatusReason = null;
          UpdateModified(referral);
          await _context.SaveChangesAsync();
        }
        else if (update.DocumentStatus == DocumentStatus.DischargePending.ToString())
        {
          _log.Information("Discharge for referral {Id} is waiting to be sent.", referral.Id);
        }
        else if (update.DocumentStatus == DocumentStatus.Accepted.ToString())
        {
          referral.Status = Complete.ToString();
          referral.StatusReason = update.Information;

          UpdateModified(referral);
          await _context.SaveChangesAsync();
        }
        else if (update.DocumentStatus == DocumentStatus.Rejected.ToString())
        {
          GpDocumentProxyHandleRejectionReasonResult handleRejectionReason = 
            await HandleRejectionReason(referral, update);

          if (!handleRejectionReason.IsUpdateSuccessful)
          {
            handleUpdateResponse.ErrorMessage = handleRejectionReason.ErrorMessage;
          }
        }
        else if (update.DocumentStatus ==
          DocumentStatus.RejectionResolved.ToString())
        {
          referral.Status = AwaitingDischarge.ToString();
          referral.StatusReason = update.Information;

          UpdateModified(referral);
          await _context.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(update.UpdateStatus)
          && Enum.TryParse(
            update.UpdateStatus,
            out DocumentUpdateStatus updateStatus))
        {
          handleUpdateResponse.DocumentUpdateStatus = updateStatus;
          return handleUpdateResponse;
        }

        _log.Error(
          "Update Discharge for referral {Id} with template {templateId} returned invalid " +
            "UpdateStatus: {UpdateStatus}.",
          referral.Id,
          templateId,
          update.UpdateStatus);
        handleUpdateResponse.DocumentUpdateStatus = DocumentUpdateStatus.Error;
        handleUpdateResponse.ErrorMessage = $"Update Discharge for referral {referral.Id} with " + 
          $"template {templateId} returned invalid UpdateStatus: {update.UpdateStatus}.";
        return handleUpdateResponse;
      }
    }
    else if (responseMessage.StatusCode == HttpStatusCode.NoContent)
    {
      _log.Error(
        "Update Discharge for referral {Id} with template {templateId} returned " + 
          "204 No Content.", 
        referral.Id,
        templateId);
      handleUpdateResponse.ErrorMessage = $"Update Discharge for referral {referral.Id} with " + 
        $"template {templateId} returned 204 No Content.";
    }
    else if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
    {
      _log.Error(
        "Update Discharge for referral {Id} with template {templateId} and UBRN {Ubrn} " + 
          "returned 400 Bad Request.",
        referral.Id,
        templateId,
        referral.Ubrn);
      handleUpdateResponse.ErrorMessage = 
        $"Update Discharge for referral {referral.Id} with template {templateId} and UBRN " + 
        $"{referral.Ubrn} returned 400 Bad Request.";
    }
    else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
    {
      throw new InvalidTokenException();
    }
    else
    {
      _log.Error(
        "Update Discharge for referral {Id} with template {templateId} returned " + 
          "status {StatusCode}.",
        referral.Id,
        templateId,
        responseMessage.StatusCode);
      handleUpdateResponse.ErrorMessage = $"Update Discharge for referral {referral.Id} with " +
        $"template {templateId} returned status {responseMessage.StatusCode}.";
    }

    handleUpdateResponse.DocumentUpdateStatus = DocumentUpdateStatus.Error;
    return handleUpdateResponse;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> HandleRejectionReason(
    Entities.Referral referral,
    GpDocumentProxyDocumentUpdateResponse update)
  {
    GpDocumentProxyHandleRejectionReasonResult handleReject = new();

    if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.TracePatientRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusDischargeAwaitingTrace(referral);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.CompleteRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusComplete(referral, update.Information);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.UnableToDischargeRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusUnableToDischargeComplete(
        referral,
        update.Information);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.AwaitingDischargeRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusAwaitingDischarge(referral, update.Information);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpTracePatientRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusDischargeAwaitingTrace(referral, true);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpCompleteRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusComplete(referral, update.Information, true);
    }
    else if (update.Information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpUnableToDischargeRejectionReasons))
    {
      handleReject = await ResolveAndSetStatusUnableToDischargeComplete(
        referral,
        update.Information,
        true);
    }
    else
    {
      handleReject = await SetDocumentDelay(referral.Id);
    }

    if (!handleReject.IsUpdateSuccessful)
    {
      update.UpdateStatus = DocumentUpdateStatus.Error.ToString();
    }

    return handleReject;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> 
    ResolveAndSetStatusAwaitingDischarge(Entities.Referral referral, string information)
  {
    GpDocumentProxyHandleRejectionReasonResult result = await ResolveDocumentRejection(referral.Id);

    if (result.IsUpdateSuccessful)
    {
      referral.Status = AwaitingDischarge.ToString();
      referral.StatusReason = information;

      UpdateModified(referral);
      await _context.SaveChangesAsync();
    }

    return result;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> ResolveAndSetStatusComplete(
    Entities.Referral referral,
    string information,
    bool resolveNow = false)
  {
    GpDocumentProxyHandleRejectionReasonResult result = await ResolveDocumentRejection(
      referral.Id,
      resolveNow);

    if (result.IsUpdateSuccessful)
    {
      await SetStatusComplete(referral, information);
    }

    return result;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> 
    ResolveAndSetStatusDischargeAwaitingTrace(
      Entities.Referral referral,
      bool resolveNow = false)
  {
    GpDocumentProxyHandleRejectionReasonResult result = await ResolveDocumentRejection(
      referral.Id,
      resolveNow);

    if (result.IsUpdateSuccessful)
    {
      referral.Status = DischargeAwaitingTrace.ToString();

      UpdateModified(referral);
      await _context.SaveChangesAsync();
    }

    return result;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> 
    ResolveAndSetStatusUnableToDischargeComplete(
      Entities.Referral referral,
      string information,
      bool resolveNow = false)
  {
    GpDocumentProxyHandleRejectionReasonResult result = await ResolveDocumentRejection(
      referral.Id,
      resolveNow);

    if (result.IsUpdateSuccessful)
    {
      referral.Status = UnableToDischarge.ToString();
      referral.StatusReason = information;

      UpdateModified(referral);
      await _context.SaveChangesAsync();

      await SetStatusComplete(referral);
    }

    return result;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> ResolveDocumentRejection(
    Guid referralId,
    bool resolveNow = false)
  {
    GpDocumentProxyHandleRejectionReasonResult result = new();

    try
    {
      string resolveUrl = _gpDocumentProxyOptions.Endpoint +
        _gpDocumentProxyOptions.ResolveEndpoint +
        referralId;

      if (resolveNow)
      {
        resolveUrl += "?resolveNow=true";
      }

      HttpResponseMessage responseMessage = await _httpClient
        .GetAsync(resolveUrl);

      if (responseMessage.StatusCode == HttpStatusCode.OK)
      {
        result.IsUpdateSuccessful = true;
      }
      else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
      {
        throw new InvalidTokenException();
      }
      else
      {
        _log.Error(
          "Resolve Document Rejection for referral {Id} returned status {StatusCode}.",
          referralId,
          responseMessage.StatusCode);
        result.ErrorMessage = $"Resolve Document Rejection for referral {referralId} returned " +
          $"status {responseMessage.StatusCode}.";
      }
    }
    catch (InvalidTokenException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _log.Error(ex, ex.Message);
      result.ErrorMessage = ex.Message;
    }

    return result;
  }

  private async Task<GpDocumentProxyHandleRejectionReasonResult> SetDocumentDelay(Guid referralId)
  {
    GpDocumentProxyHandleRejectionReasonResult result = new();

    try
    {
      HttpResponseMessage responseMessage = await _httpClient
        .GetAsync(
          _gpDocumentProxyOptions.Endpoint +
          _gpDocumentProxyOptions.DelayEndpoint +
          referralId);

      if (responseMessage.StatusCode == HttpStatusCode.OK)
      {
        result.IsUpdateSuccessful = true;
      }
      else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
      {
        throw new InvalidTokenException();
      }
      else
      {
        _log.Error(
          "Set Document Delay for referral {Id} returned status {StatusCode}.",
          referralId,
          responseMessage.StatusCode);
        result.ErrorMessage = $"Set Document Delay for referral {referralId} returned status " +
          $"{responseMessage.StatusCode}.";
      }
    }
    catch (InvalidTokenException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _log.Error(ex, ex.Message);
      result.ErrorMessage = ex.Message;
    }

    return result;
  }

  private async Task<bool> SetStatusComplete(
    Entities.Referral referral,
    string information = null)
  {
    referral.Status = Complete.ToString();
    referral.StatusReason = information;

    UpdateModified(referral);
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task<GpDocumentProxySetRejection>
    UpdateDischargedReferralWithRejection(
      Guid referralId,
      string information)
  {
    GpDocumentProxySetRejection response = new();

    if (string.IsNullOrWhiteSpace(information))
    {
      response.StatusCode = (int)HttpStatusCode.BadRequest;
      response.Message = "Missing rejection code.";
      return response;
    }

    Entities.Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Id == referralId)
      .FirstOrDefaultAsync();

    if (referral == null)
    {
      response.StatusCode = (int)HttpStatusCode.NotFound;
      response.Message = $"Referral not found.";
      return response;
    }

    if (referral.Status != SentForDischarge.ToString()
      && referral.Status != Complete.ToString())
    {
      response.StatusCode = (int)HttpStatusCode.Conflict;
      response.Message = $"Referral status prevents update.";
      return response;
    }

    await SaveDischargedReferralWithRejection(
      referral,
      information,
      response);

    return response;
  }

  public async Task SaveDischargedReferralWithRejection(
    Entities.Referral referral,
    string information,
    GpDocumentProxySetRejection response)
  {
    if (information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpTracePatientRejectionReasons))
    {
      referral.Status = DischargeAwaitingTrace.ToString();

      UpdateModified(referral);
      await _context.SaveChangesAsync();

      response.StatusCode = (int)HttpStatusCode.OK;
    }
    else if (information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpCompleteRejectionReasons))
    {
      referral.Status = Complete.ToString();
      referral.StatusReason = information;

      UpdateModified(referral);
      await _context.SaveChangesAsync();

      response.StatusCode = (int)HttpStatusCode.OK;
    }
    else if (information.StartsWithMatchInArray(
      _gpDocumentProxyOptions.GpdpUnableToDischargeRejectionReasons))
    {
      referral.Status = UnableToDischarge.ToString();
      referral.StatusReason = information;

      UpdateModified(referral);
      await _context.SaveChangesAsync();

      referral.Status = Complete.ToString();

      UpdateModified(referral);
      await _context.SaveChangesAsync();

      response.StatusCode = (int)HttpStatusCode.OK;
    }
    else
    {
      response.StatusCode = (int)HttpStatusCode.BadRequest;
      response.Message = $"Unrecognized rejection code.";
    }
  }

  public async Task DischargeReferralAsync(Guid id)
  {
    Entities.Referral referral = await _context.Referrals.FindAsync(id);

    if (referral == null)
    {
      throw new ReferralNotFoundException(id);
    }

    if (referral.Status != AwaitingDischarge.ToString())
    {
      throw new ReferralInvalidStatusException($"Referral {id} has an " +
        $"invalid status of {referral.Status}, " +
        $"expected {AwaitingDischarge}.");
    }

    referral.Status = Complete.ToString();
    UpdateModified(referral);

    await _context.SaveChangesAsync();
  }

  /// <summary>
  /// Creates an MSK referral
  /// </summary>
  /// <param name="mskReferralCreate">The IMskReferralCreate with the 
  /// properties to create the referral</param>
  /// <returns>void</returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="ReferralNotUniqueException"></exception>
  public virtual async Task CreateMskReferralAsync(IMskReferralCreate mskReferralCreate)
  {
    if (mskReferralCreate == null)
    {
      throw new ArgumentNullException(nameof(mskReferralCreate));
    }

    await ValidateReferral(mskReferralCreate);

    await ApplyMskEthnicityOverrides(mskReferralCreate);

    await CheckReferralCanBeCreatedWithNhsNumberAsync(
      mskReferralCreate.NhsNumber);

    Entities.Referral referralEntity =
      _mapper.Map<Entities.Referral>(mskReferralCreate);

    await UpdateBmiAndValidateAsync(referralEntity);

    await UpdateReferringGpPracticeName(referralEntity);

    await UpdateDeprivation(referralEntity);

    UpdateTriage(referralEntity);

    UpdateModified(referralEntity);

    _context.Referrals.Add(referralEntity);
    await _context.SaveChangesAsync();

    try
    {
      await UpdateMskReferralUbrnAsync(referralEntity.Id);
    }
    catch (Exception ex)
    {
      _log.Error(ex.Message);

      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason = "Error adding Ubrn to Referral.";
      referralEntity.IsActive = false;
      UpdateModified(referralEntity);
      await _context.SaveChangesAsync();

      throw new ReferralUpdateException("An error occurred creating this referral and it was " +
        "not saved. Please try again later.");
    }
  }

  public async Task<ReferralStatusReason[]>
    GetRmcRejectedReferralStatusReasonsAsync()
  {
    ReferralStatusReason[] referralStatusReasons = await _context
      .ReferralStatusReasons
      .Where(x => x.IsActive)
      .Where(x => x.Groups.HasFlag(ReferralStatusReasonGroup.RmcRejected))
      .OrderBy(x => x.Description)
      .Select(x => new ReferralStatusReason()
      {
        Description = x.Description,
        Id = x.Id
      })
      .ToArrayAsync();

    return referralStatusReasons;
  }

  private async Task UpdateBmiAndValidateAsync(
    Entities.Referral referralEntity)
  {
    if (referralEntity is null)
    {
      throw new ArgumentNullException(nameof(referralEntity));
    }

    referralEntity.CalculatedBmiAtRegistration = BmiHelper.CalculateBmi(
      referralEntity.WeightKg ?? 0,
      referralEntity.HeightCm ?? 0);

    ValidationResult validation = await BmiValidationAsync(referralEntity);
    if (validation != null)
    {
      throw new MskReferralValidationException(new() { validation });
    }
  }

  private async Task ApplyMskEthnicityOverrides(IMskReferralCreate referralCreate)
  {
    Entities.Ethnicity ethnicityOverride = await _context.EthnicityOverrides
      .Include(o => o.Ethnicity)
      .Where(o => o.IsActive)
      .Where(o => o.ReferralSource == ReferralSource.Msk)
      .Where(o => o.DisplayName == referralCreate.ServiceUserEthnicity)
      .Where(o => o.GroupName == referralCreate.ServiceUserEthnicityGroup)
      .Select(o => o.Ethnicity)
      .SingleOrDefaultAsync();

    if (ethnicityOverride == default)
    {
      return;
    }

    referralCreate.Ethnicity = ethnicityOverride.TriageName;
    referralCreate.ServiceUserEthnicity = ethnicityOverride.GroupName;
    referralCreate.ServiceUserEthnicityGroup = ethnicityOverride.DisplayName;
  }

  private async Task UpdateReferringGpPracticeName(
    Entities.Referral referralEntity)
  {
    string referringGpPracticeName = Constants.UNKNOWN_GP_PRACTICE_NAME;
    try
    {
      OdsOrganisation org = await _odsOrganisationService
        .GetOdsOrganisationAsync(referralEntity.ReferringGpPracticeNumber);
      if (org.WasFound)
      {
        referringGpPracticeName = org.Organisation.Name;
      }
    }
    finally
    {
      referralEntity.ReferringGpPracticeName = referringGpPracticeName;
    }
  }

  private async Task ValidateReferral(IMskReferralCreate referralModel)
  {
    ValidateModelResult result = Validators.ValidateModel(referralModel);

    bool isEthnicityGroupingValid = await _context.Ethnicities
      .Where(e => e.IsActive)
      .Where(e =>
        e.DisplayName == referralModel.ServiceUserEthnicity ||
        e.Overrides.Any(o =>
          o.DisplayName == referralModel.ServiceUserEthnicity &&
          o.ReferralSource == ReferralSource.Msk &&
          o.IsActive))
      .Where(e =>
        e.GroupName == referralModel.ServiceUserEthnicityGroup ||
        e.Overrides.Any(o =>
          o.GroupName == referralModel.ServiceUserEthnicityGroup &&
          o.ReferralSource == ReferralSource.Msk &&
          o.IsActive))
      .AnyAsync();

    if (!isEthnicityGroupingValid)
    {
      result.Results.Add(new ValidationResult("The combination of " +
        $"Ethnicity: {nameof(referralModel.ServiceUserEthnicity)}" +
        $" and Ethnicity Group:" +
        $"{nameof(referralModel.ServiceUserEthnicityGroup)} " +
        "is invalid.",
        new string[]
        {
          $"{nameof(referralModel.ServiceUserEthnicity)}",
          $"{nameof(referralModel.ServiceUserEthnicityGroup)}"
        }));

      result.IsValid = false;
    }

    if (!result.IsValid)
    {
      throw new MskReferralValidationException(result.Results);
    }
  }

  /// <summary>
  /// Updates the Ubrn and ProviderUbrn properties of the provided referral.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <exception cref="ReferralInvalidCreationException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralUpdateException"></exception>
  public virtual async Task UpdateMskReferralUbrnAsync(Guid referralId)
  {
    if (await _context.MskReferrals
      .AnyAsync(t => t.ReferralId == referralId))
    {
      throw new ReferralInvalidCreationException(
        $"There is already an MSK referral with the Id {referralId}.");
    }

    Entities.Referral referral = await _context.Referrals
      .SingleOrDefaultAsync(t => t.Id == referralId);

    if (referral == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    Entities.MskReferral mskReferral = new()
    {
      ReferralId = referralId
    };
    _context.MskReferrals.Add(mskReferral);
    await _context.SaveChangesAsync();
    Models.MskReferral model = _mapper.Map<Models.MskReferral>(mskReferral);
    if (!string.IsNullOrWhiteSpace(model.Ubrn))
    {
      referral.Ubrn = model.Ubrn;
      referral.ProviderUbrn = model.Ubrn;
      await _context.SaveChangesAsync();
    }
    else
    {
      throw new ReferralUpdateException(
        $"MskReferral for Referral Id {referralId} was not saved.");
    }
  }

  /// <summary>
  /// Updates the Ubrn and ProviderUbrn properties of the provided referral.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <exception cref="ReferralInvalidCreationException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralUpdateException"></exception>
  public virtual async Task UpdateGpReferralUbrnAsync(Guid referralId)
  {
    if (await _context.GpReferrals.AnyAsync(t => t.ReferralId == referralId))
    {
      throw new ReferralInvalidCreationException(
        $"There is already an GP referral with the Id {referralId}.");
    }

    Entities.Referral referral = await _context.Referrals
      .SingleOrDefaultAsync(t => t.Id == referralId);

    if (referral == null)
    {
      throw new ReferralNotFoundException(referralId);
    }

    Entities.GpReferral gpReferral = new()
    {
      ReferralId = referralId
    };
    _context.GpReferrals.Add(gpReferral);
    await _context.SaveChangesAsync();

    Models.GpReferral model = _mapper.Map<Models.GpReferral>(gpReferral);
    if (!string.IsNullOrWhiteSpace(model.ProviderUbrn))
    {
      referral.ProviderUbrn = model.ProviderUbrn;
      await _context.SaveChangesAsync();
    }
    else
    {
      throw new ReferralUpdateException(
        $"GpReferral for Referral Id {referralId} was not saved.");
    }
  }

  /// <inheritdoc/>
  public async Task<ICreateAccessKeyResponse> CreateAccessKeyAsync(
    ICreateAccessKey createAccessKey)
  {
    ValidateModelResult result = ValidateModel(createAccessKey);
    if (!result.IsValid)
    {
      return new CreateAccessKeyResponse(
        ResponseBase.ErrorTypes.Validation,
        result.GetErrorMessage());
    }

    int numberOfCreatedAccessKeys = _context.AccessKeys
      .Count(x => x.Email == createAccessKey.Email
        && x.Type == createAccessKey.AccessKeyType
        && x.Expires > DateTimeOffset.Now);

    if (numberOfCreatedAccessKeys >= createAccessKey.MaxActiveAccessKeys)
    {
      return new CreateAccessKeyResponse(
        ResponseBase.ErrorTypes.MaxActiveAccessKeys,
        "You have requested too many tokens, " +
        "please wait for the emails containing the token to arrive.");
    }

    DateTimeOffset expiry = DateTimeOffset.UtcNow
      .AddMinutes(createAccessKey.ExpireMinutes);

    AccessKey accessKey = new()
    {
      Email = createAccessKey.Email,
      Key = Generators.GenerateKeyCode(new Random(), 6, false, false),
      Expires = expiry,
      TryCount = 0,
      Type = createAccessKey.AccessKeyType
    };

    _context.AccessKeys.Add(accessKey);

    await _context.SaveChangesAsync();

    await DeleteExpiredAccessKeysAsync();

    CreateAccessKeyResponse response = new(
      accessKey: accessKey.Key,
      email: accessKey.Email,
      expires: accessKey.Expires);

    return response;
  }

  /// <inheritdoc/>
  public async Task<IValidateAccessKeyResponse> ValidateAccessKeyAsync(
    IValidateAccessKey validateAccessKey)
  {
    ValidateModelResult result = ValidateModel(validateAccessKey);
    if (!result.IsValid)
    {
      return new ValidateAccessKeyResponse(
        result.GetErrorMessage(),
        ResponseBase.ErrorTypes.Validation);
    }

    List<AccessKey> accessKeys = await _context.AccessKeys
      .Where(x => x.Email == validateAccessKey.Email
        && x.Type == validateAccessKey.Type)
      .ToListAsync();

    if (!accessKeys.Any())
    {
      return new ValidateAccessKeyResponse(
        $"Access key for email {validateAccessKey.Email} was not found.",
        ResponseBase.ErrorTypes.NotFound);
    }

    AccessKey accessKey =
      accessKeys.SingleOrDefault(x => x.Key == validateAccessKey.AccessKey);

    if (accessKey == null)
    {
      int tryCount = accessKeys.Max(x => x.TryCount) + 1;

      if (tryCount > Constants.ACCESS_CODE_MAX_TRY_COUNT)
      {
        if (accessKeys.Count == validateAccessKey.MaxActiveAccessKeys)
        {
          AccessKey keyToDelete = accessKeys.OrderBy(x => x.Expires).First();
          _context.AccessKeys.Remove(keyToDelete);

          accessKeys.Remove(keyToDelete);
        }

        foreach (AccessKey key in accessKeys)
        {
          key.TryCount = 0;
          _context.Update(key);
        }

        await _context.SaveChangesAsync();

        return new ValidateAccessKeyResponse(
          $"Access key for email {validateAccessKey.Email} " +
            "does not match the " +
            "expected access key, and there have been too many attempts.",
          ResponseBase.ErrorTypes.TooManyAttempts);
      }

      foreach (AccessKey key in accessKeys)
      {
        key.TryCount = tryCount;
        _context.Update(key);
      }

      await _context.SaveChangesAsync();

      return new ValidateAccessKeyResponse(
        $"Access key for email {validateAccessKey.Email} " +
          "does not match the expected access key.",
        ResponseBase.ErrorTypes.Incorrect);
    }

    if (accessKey.Expires < DateTimeOffset.Now)
    {
      return new ValidateAccessKeyResponse(
        $"Access key for email {validateAccessKey.Email} expired on " +
          $"{accessKey.Expires}.",
        ResponseBase.ErrorTypes.Expired);
    }

    accessKey.TryCount = 0;
    await _context.SaveChangesAsync();

    await DeleteExpiredAccessKeysAsync();

    return new ValidateAccessKeyResponse(
      expires: accessKey.Expires,
      isValidCode: true);
  }

  /// <inheritdoc/>
  public async Task<KeyValuePair<Guid, IReferral>?> GetMessageQueue(
    string linkId)
  {
    if (linkId == null || !linkId.All(char.IsLetterOrDigit))
    {
      string value = string.IsNullOrEmpty(linkId)
        ? "Null"
        : linkId;

      throw new ReferralNotFoundException(
        $"Invalid {nameof(linkId)} of [{value}].");
    }

    var messageQueueAndReferral = await
      (from r in _context.Referrals
       join m in _context.MessagesQueue on r.Id equals m.ReferralId
       where m.ServiceUserLinkId == linkId
       select new { MessageQueue = m, Referral = r })
       .ToListAsync();

    if (messageQueueAndReferral == null || !messageQueueAndReferral.Any())
    {
      return null;
    }

    IReferral referral =
      _mapper.Map<Referral>(messageQueueAndReferral.First().Referral);
    Entities.MessageQueue message =
      messageQueueAndReferral.First().MessageQueue;

    if (message.SendResult == Constants.DO_NOT_CONTACT_EMAIL)
    {
      throw new TextMessageExpiredByEmailException("Text message token " +
        $"{linkId} expired by Email not submitted.");
    }

    if (referral.ProviderId != null)
    {
      throw new TextMessageExpiredByProviderSelectionException("Text " +
        $"message with token {linkId} had its provider selected" +
        $"on {referral.DateOfProviderSelection}.");
    }

    if (message.SendResult == Constants.DATE_OF_BIRTH_EXPIRY)
    {
      throw new TextMessageExpiredByDoBCheckException("Text message with " +
        $"token {linkId} expired by Date of Birth check.");
    }

    ReferralStatus validStatusFlags = TextMessage1 |
      TextMessage2 |
      ChatBotCall1 |
      ChatBotTransfer |
      RmcCall |
      RmcDelayed |
      TextMessage3;

    if (!validStatusFlags.HasFlag(referral.Status.ToEnum<ReferralStatus>()))
    {
      throw new ReferralInvalidStatusException(
        $"Invalid status of {referral.Status}, expecting a " +
        $"status of one of following: {validStatusFlags.ToString()}.");
    }

    return new KeyValuePair<Guid, IReferral>(message.Id, referral);
  }

  public async Task<Dictionary<ApiKeyType, DateTime?>> GetDatesMessageSent(
    Guid referralId)
  {
    Dictionary<ApiKeyType, DateTime?> messageDict =
      await _context.MessagesQueue
        .Where(m => m.ReferralId == referralId)
        .ToDictionaryAsync(m => m.ApiKeyType, m => m.SentDate);

    return messageDict;
  }

  public async Task<DateTimeOffset?> GetDateOfFirstContact(Guid referralId)
  {
    if (referralId == Guid.Empty)
    {
      throw new ArgumentException("Must provide a valid referral Id.", nameof(referralId));
    }

    Entities.TextMessage referralTextMessage1 = await _context.TextMessages
      .Where(t => t.IsActive)
      .Where(t => t.ReferralId == referralId)
      .Where(t => t.ReferralStatus == TextMessage1.ToString())
      .Where(t => t.Sent != default)
      .OrderByDescending(t => t.Sent)
      .FirstOrDefaultAsync();

    if (referralTextMessage1 != null)
    {
      return referralTextMessage1.Sent;
    }

    Entities.Call referralChatBotCall1 = await _context.Calls
      .Where(c => c.IsActive)
      .Where(c => c.ReferralId == referralId)
      .Where(c => c.Sent != default)
      .OrderByDescending(c => c.Sent)
      .FirstOrDefaultAsync();

    if (referralChatBotCall1 != null)
    {
      return referralChatBotCall1.Sent;
    }

    return null;
  }

  private async Task DeleteExpiredAccessKeysAsync()
  {
    AccessKey[] accessKeys = await _context.AccessKeys
      .Where(x => x.Expires < DateTimeOffset.Now.AddDays(-1))
      .ToArrayAsync();

    if (accessKeys.Any())
    {
      _context.AccessKeys.RemoveRange(accessKeys);

      await _context.SaveChangesAsync();
    }
  }

  public async Task CancelGeneralReferralAsync(
    IGeneralReferralCancel cancellation)
  {
    if (cancellation is null)
    {
      throw new ArgumentNullException(nameof(cancellation));
    }

    Entities.Referral referral = await _context.Referrals
      .FindAsync(cancellation.Id);

    if (referral == null)
    {
      throw new ReferralNotFoundException(cancellation.Id);
    }

    if (referral.ReferralSource != ReferralSource.ElectiveCare.ToString())
    {
      throw new ReferralInvalidReferralSourceException(
        referral.Id,
        referral.ReferralSource);
    }

    List<string> validStatuses = new()
    {
      New.ToString(),
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      ChatBotCall1.ToString(),
      RmcCall.ToString(),
      RmcDelayed.ToString(),
      TextMessage3.ToString()
    };

    if (!validStatuses.Contains(referral.Status))
    {
      throw new ReferralInvalidStatusException(referral.Id, referral.Status);
    }

    referral.CalculatedBmiAtRegistration = BmiHelper
      .CalculateBmi(cancellation.WeightKg, cancellation.HeightCm);
    referral.Ethnicity = cancellation.Ethnicity;
    referral.HasActiveEatingDisorder = cancellation.HasActiveEatingDisorder;
    referral.HasHadBariatricSurgery = cancellation.HasHadBariatricSurgery;
    referral.HeightCm = cancellation.HeightCm;
    referral.IsPregnant = cancellation.IsPregnant;
    referral.ServiceUserEthnicity = cancellation.ServiceUserEthnicity;
    referral.ServiceUserEthnicityGroup =
      cancellation.ServiceUserEthnicityGroup;
    referral.Status = Cancelled.ToString();
    referral.WeightKg = cancellation.WeightKg;

    await ValidateCancelGeneralReferral(referral);

    UpdateModified(referral);

    await _context.SaveChangesAsync();
  }

  public async Task<CanCreateReferralResponse>
    CanGeneralReferralBeCreatedWithNhsNumberAsync(string nhsNumber)
  {
    if (string.IsNullOrWhiteSpace(nhsNumber))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(nhsNumber));
    }

    // NHS number matches existing referrals?
    List<Referral> referrals = await _context.Referrals
      .Include(r => r.Provider)
      .Where(r => r.IsActive)
      .Where(r => r.NhsNumber == nhsNumber)
      .ProjectTo<Referral>(_mapper.ConfigurationProvider)
      .OrderByDescending(r => r.CreatedDate)
      .ToListAsync();

    if (referrals.Count == 0)
    {
      return new CanCreateReferralResponse(
        CanCreateReferralResult.CanCreate,
        "No existing referrals match NHS number.");
    }

    // All matching referrals status Complete, Cancelled
    // or CancelledToEreferrals?
    List<Referral> cancelledOrComplete = referrals
      .Where(r => r.Status == CancelledByEreferrals.ToString()
        || r.Status == Cancelled.ToString()
        || r.Status == Complete.ToString())
      .ToList();

    if (referrals.Count == cancelledOrComplete.Count)
    {
      // Any matching referrals have a provider selected?
      if (referrals.Any(x => x.ProviderId != null))
      {
        // Yes, Get last referral by DateOfProviderSelection
        Referral lastReferral = cancelledOrComplete
          .Where(r => r.ProviderId != null)
          .MaxBy(r => r.DateOfProviderSelection);

        // If ProviderId is not null DateOfProviderSelection must not be null.
        if (lastReferral.DateOfProviderSelection.HasValue == false)
        {
          throw new InvalidOperationException("The previous referral " +
            $"(Id {lastReferral.Id}) has a selected provider without " +
            $"a date of provider selection.");
        }

        // Did the last referral start the programme?
        if (lastReferral.DateStartedProgramme.HasValue)
        {
          // Yes, was the referral's programme started more than 252 days ago?
          DateTimeOffset canCreateDate = lastReferral
            .DateStartedProgramme.Value.Date
            .AddDays(Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME);

          if (canCreateDate <= DateTimeOffset.Now.Date)
          {
            // Yes, can create referral
            return new CanCreateReferralResponse(
              CanCreateReferralResult.CanCreate,
              $"The last referral for this NHS number completed the " +
                $"programme more than " +
                $"{Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME} days ago.",
              lastReferral);
          }
          // No, cannot create programme previously started
          return new CanCreateReferralResponse(
            CanCreateReferralResult.ProgrammeStarted,
            "According to our records you have previously registered for " +
              "or completed the NHS Digital Weight Management Programme. " +
              "The guidance states you can re-register on or after " +
              $"{canCreateDate:dd/MM/yyyy}.",
            lastReferral);
        }
        else
        {
          // No, was the referral's provider selected more than 42 days ago?
          DateTimeOffset canCreateDate = lastReferral
            .DateOfProviderSelection.Value.Date
            .AddDays(Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION);

          if (canCreateDate <= DateTimeOffset.Now.Date)
          {
            // Yes, can create referral
            return new CanCreateReferralResponse(
              CanCreateReferralResult.CanCreate,
              $"The last referral for this NHS number selected a provider " +
                $"the more than " +
                $"{Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION} days " +
                $"ago.",
            lastReferral);
          }
          // No, cannot create provider previously selected
          return new CanCreateReferralResponse(
            CanCreateReferralResult.ProviderSelected,
            "According to our records you have previously registered for " +
              "or completed the NHS Digital Weight Management Programme. " +
              "The guidance states you can re-register on or after " +
              $"{canCreateDate:dd/MM/yyyy}.",
            lastReferral);
        }
      }
      // No provider selected, can create referral
      return new CanCreateReferralResponse(
        CanCreateReferralResult.CanCreate,
        "All existing referrals that match the NHS number are cancelled or " +
        "complete without a provider being selected.");
    }

    if (referrals.Count - cancelledOrComplete.Count > 1)
    {
      throw new InvalidOperationException("There is more than one referral " +
        $"that does not have a status of {Complete}, {Cancelled} or " +
        $"{CancelledByEreferrals} with an NHS number of {nhsNumber}.");
    }

    // Get the in progress referral
    Referral referral = referrals
      .Where(r => r.Status != CancelledByEreferrals.ToString()
        && r.Status != Cancelled.ToString()
        && r.Status != Complete.ToString())
      .Single();

    // Is referral source NOT General Referral or Elective Care
    if (referral.ReferralSource != ReferralSource.GeneralReferral.ToString()
      && referral.ReferralSource != ReferralSource.ElectiveCare.ToString())
    {
      string reason;
      if (referral.ReferralSource == ReferralSource.SelfReferral.ToString())
      {
        reason = "According to our records you have already registered for " +
          "this programme through the NHS Digital Weight Management " +
          "Programme Staff Self Referral Site. Please refer to the text " +
          "message sent at the time of referral or alternatively call " +
          "(01772 660 010) and a member of the team will be able to assist " +
          "you further.";
      }
      else
      {
        string referredBy;
        if (referral.ReferralSource == ReferralSource.Msk.ToString())
        {
          referredBy = "Physiotherapist";
        }
        else if (referral.ReferralSource ==
          ReferralSource.Pharmacy.ToString())
        {
          referredBy = "Community Pharmacy";
        }
        else
        {
          referredBy = "General Practice";
        }

        reason = "According to our records you have already been referred " +
          "for the NHS Digital Weight Management Programme by your " +
          $"{referredBy}. Please refer to the text message sent at the " +
          $"time of referral or alternatively call (01772 660 010) and a " +
          $"member of the team will be able to assist you further.";
      }

      return new CanCreateReferralResponse(
        CanCreateReferralResult.IneligibleReferralSource,
        reason,
        referral);
    }

    // Does the referral have a selected provider
    if (referral.ProviderId != null)
    {
      return new CanCreateReferralResponse(
        CanCreateReferralResult.ProviderSelected,
        "According to our records you have recently registered for the " +
          "NHS Digital Weight Management Programme and selected " +
          $"{referral.Provider.Name} as your provider." +
          "They will be in touch with you shortly.",
        referral);
    }

    // All checks passed can create referral
    return new CanCreateReferralResponse(
      CanCreateReferralResult.UpdateExisting,
      "Existing referral can be updated.",
      referral);
  }

  public async Task CloseErsReferral(
    ReferralSource referralSourceFlag = ReferralSource.GpReferral,
    ReferralStatus referralStatusFlag =
      CancelledByEreferrals
      | ReferralStatus.Exception
      | RejectedToEreferrals,
    string ubrn = null)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentOutOfRangeException(
        nameof(ubrn),
        "Cannot be null or white space.");
    }

    Entities.Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Ubrn == ubrn)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(
        $"An active referral was not found with a ubrn of {ubrn}.");

    await CloseErsReferral(
      referral.Id,
      referralSourceFlag,
      referralStatusFlag);
  }

  /// <inheritdoc/>
  public async Task CloseErsReferral(
    Guid id,
    ReferralSource referralSourceFlag = ReferralSource.GpReferral,
    ReferralStatus referralStatusFlag =
      CancelledByEreferrals
      | ReferralStatus.Exception
      | RejectedToEreferrals)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentOutOfRangeException(nameof(id), "Cannot be empty.");
    }

    Entities.Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Id == id)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(
        $"An active referral was not found with an id of {id}.");

    if (!referralSourceFlag
      .HasFlag(referral.ReferralSource.ToEnum<ReferralSource>()))
    {
      List<string> referralSourceNames = referralSourceFlag.ToFlagNames();

      throw new ReferralInvalidReferralSourceException($"Referral Id {id} " +
        $"has a unexpected ReferralSource of {referral.ReferralSource}. " +
        $"A referral can only have its eRS record closed it has one of the " +
        $"following referral sources: " +
        $"{string.Join(", ", referralSourceNames)}.");
    }

    if (referralStatusFlag.HasFlag(referral.Status.ToEnum<ReferralStatus>()))
    {
      List<string> referralStatusNames = referralStatusFlag
        .ToFlagNames(excludeZero: false);

      throw new ReferralInvalidStatusException($"Referral Id {id} " +
        $"has a unexpected Status of {referral.Status}. " +
        $"A referral cannot have its eRS record closed if it has one of " +
        $"the following statuses: " +
        $"{string.Join(", ", referralStatusNames)}.");
    }

    if (referral.IsErsClosed != true)
    {
      referral.IsErsClosed = true;

      UpdateModified(referral);
      await _context.SaveChangesAsync();
    }
  }

  public async Task UpdateDobAttemptsAsync(Guid textMessageId, int attempts)
  {
    Entities.TextMessage textMessage = await _context
      .TextMessages
      .Where(x => x.Id == textMessageId)
      .SingleOrDefaultAsync();

    if (textMessage == null)
    {
      throw new
        TextMessageNotFoundException($"Unable to find textMessage with id {textMessageId}.");
    }

    textMessage.DobAttempts = attempts;

    UpdateModified(textMessage);

    await _context.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public virtual async Task<int> TerminateNotStartedProgrammeReferralsAsync()
  {
    List<Entities.Referral> referralsToTerminate = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.DateOfProviderSelection < DateTimeOffset.UtcNow.Date
        .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection))
      .Where(r => r.Status == ProviderAccepted.ToString() 
        || r.Status == ProviderContactedServiceUser.ToString())
      .ToListAsync();

    int terminatedReferralsCount = 0;

    if (referralsToTerminate.Count > 0)
    {
      foreach (Entities.Referral referral in referralsToTerminate)
      {
        referral.Status = referral.ReferralSource == ReferralSource.GpReferral.ToString()
          ? ProviderTerminated.ToString()
          : ProviderTerminatedTextMessage.ToString();
        referral.StatusReason = $"Service user did not start the programme within " + 
          _referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection + 
          " days of selecting a provider. Referral automatically terminated.";

        UpdateModified(referral);
      }

      terminatedReferralsCount = await _context.SaveChangesAsync();
    }

    return terminatedReferralsCount;
  }
}
