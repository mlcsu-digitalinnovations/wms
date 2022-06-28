using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

using static WmsHub.Business.Enums.ReferralStatus;
using static WmsHub.Business.Models.ReferralSearch;
using Analytics = WmsHub.Business.Entities.Analytics;
using Ethnicity = WmsHub.Business.Enums.Ethnicity;
using GeneralReferral = WmsHub.Business.Entities.GeneralReferral;
using IReferral = WmsHub.Business.Models.IReferral;
using IStaffRole = WmsHub.Business.Models.ReferralService.IStaffRole;
using PharmacyReferral = WmsHub.Business.Entities.PharmacyReferral;
using Provider = WmsHub.Business.Models.Provider;
using Referral = WmsHub.Business.Models.Referral;
using SelfReferral = WmsHub.Business.Entities.SelfReferral;
using StaffRole = WmsHub.Business.Models.ReferralService.StaffRole;


namespace WmsHub.Business.Services
{
  public class ReferralService
    : ServiceBase<Entities.Referral>, IReferralService
  {
    private readonly IMapper _mapper;
    private readonly IDeprivationService _deprivationService;
    private readonly IPostcodeService _postcodeService;
    private readonly IProviderService _providerService;
    private readonly ICsvExportService _csvExportService;
    private readonly IPatientTriageService _patientTriageService;
    private readonly IOdsOrganisationService _odsOrganisationService;

    public ReferralService(
      DatabaseContext context,
      IMapper mapper,
      IProviderService providerService,
      IDeprivationService deprivationService,
      IPostcodeService postcodeService,
      IPatientTriageService patientTriageService,
      IOdsOrganisationService odsOrganisationService)
      : base(context)
    {
      _mapper = mapper;
      _deprivationService = deprivationService;
      _postcodeService = postcodeService;
      _providerService = providerService;
      _patientTriageService = patientTriageService;
      _odsOrganisationService = odsOrganisationService;
    }

    public ReferralService(
      DatabaseContext context,
      IMapper mapper,
      IProviderService providerService,
      ICsvExportService csvExportService,
      IPatientTriageService patientTriageService)
      : base(context)
    {
      _mapper = mapper;
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

      CanCreateGpReferral canCreateResult =
        await CanCreateGpReferral(referralCreate);

      if (canCreateResult.IsUpdatingCancelledReferral)
      {
        // a special case where the referral has been removed by the GP practice
        // from the eRS worklist and then re-added at a later date
        // In this case the status needs to be updated back to
        // RejectedToEReferrals and an update performed
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
        await _context.SaveChangesAsync();

        ReferralUpdate referralUpdate = _mapper
          .Map<ReferralUpdate>(referralCreate);

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
          $"{canCreateResult.StatusReason} {referralEntity.StatusReason}".Trim();
      }
      _context.Referrals.Add(referralEntity);

      await _context.SaveChangesAsync();

      Referral referralModel =
        _mapper.Map<Entities.Referral, Referral>(referralEntity);
      UpdateCri(referralModel, referralEntity);
      return referralModel;
    }


    public virtual async Task<IReferral> CreateException(
      IReferralExceptionCreate referralExceptionCreate)
    {
      if (referralExceptionCreate == null)
        throw new ArgumentNullException(nameof(referralExceptionCreate));

      if (referralExceptionCreate.ExceptionType ==
        CreateReferralException.Undefined)
      {
        throw new ReferralCreateException("Invalid " +
          $"{nameof(CreateReferralException)} enum value of " +
          $"{referralExceptionCreate.ExceptionType}");
      }

      ValidateModelResult validationResult = Validators.ValidateModel(
        referralExceptionCreate);
      if (!validationResult.IsValid)
        throw new ReferralCreateException(validationResult.GetErrorMessage());

      Entities.Referral existingReferral = await _context.Referrals
        .Where(r => r.IsActive)
        .SingleOrDefaultAsync(r => r.Ubrn == referralExceptionCreate.Ubrn);

      if (existingReferral != null)
      {
        // if there is an existing referral but it's status is
        // CancelledByEreferrals allow an update
        if (existingReferral.Status == CancelledByEreferrals.ToString())
        {
          ReferralExceptionUpdate update = new()
          {
            ExceptionType = referralExceptionCreate.ExceptionType,
            NhsNumberAttachment = referralExceptionCreate.NhsNumberAttachment,
            NhsNumberWorkList = referralExceptionCreate.NhsNumberWorkList,
            Ubrn = referralExceptionCreate.Ubrn
          };

          return await UpdateReferralToStatusExceptionAsync(update);
        }
        else
        {
          throw new ReferralNotUniqueException($"A referral with a Ubrn " +
            $"of { referralExceptionCreate.Ubrn} already exists.");
        }
      }

      Entities.Referral referralEntity =
        _mapper.Map<Entities.Referral>(referralExceptionCreate);

      referralEntity.IsActive = true;
      referralEntity.NumberOfContacts = 0;
      referralEntity.Status = ReferralStatus.Exception.ToString();
      UpdateModified(referralEntity);

      List<string> validationErrors = new List<string>();

      switch (referralExceptionCreate.ExceptionType)
      {
        case CreateReferralException.NhsNumberMismatch:
          validationErrors.Add("The NHS number in the eRS work list " +
            $"'{referralExceptionCreate.NhsNumberWorkList}' does not match " +
            $"the NHS number '{referralExceptionCreate.NhsNumberAttachment}' " +
            "in the attached referral letter.");
          break;
        case CreateReferralException.MissingAttachment:
          validationErrors.Add("The eRS referral does not have an attached " +
            "referral letter.");
          break;
        case CreateReferralException.InvalidAttachment:
          validationErrors.Add(
            "The eRS referral has an invalid referral letter file type. " +
            "Accepted types are .doc, .docx, .pdf and .rtf; Non-Pdf files " +
            "should be exportable as pdf.");
          break;
      }

      referralEntity.StatusReason = string.Join(' ', validationErrors);
      referralEntity.ServiceId = referralExceptionCreate.ServiceId;
      referralEntity.SourceSystem = referralExceptionCreate.SourceSystem;
      referralEntity.DocumentVersion = referralExceptionCreate.DocumentVersion;

      _context.Referrals.Add(referralEntity);

      await _context.SaveChangesAsync();

      Referral referralModel =
        _mapper.Map<Entities.Referral, Referral>(referralEntity);

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

    public async Task<IReferral> TestCreateWithRmcStatus(
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

      CanCreateGpReferral canCreateGpReferralResult =
        await CanCreateGpReferral(referralCreate);

      Entities.Referral referralEntity =
        _mapper.Map<Entities.Referral>(referralCreate);

      referralEntity.IsActive = true;
      if (canCreateGpReferralResult.Status == ReferralStatus.Exception)
      {
        referralEntity.Status = ReferralStatus.Exception.ToString();
        referralEntity.StatusReason += canCreateGpReferralResult.StatusReason;
      }
      else
      {
        referralEntity.Status = ReferralStatus.ChatBotCall2.ToString();
        referralEntity.StatusReason = "TestCreateWithChatBotStatus";
      }

      UpdateModified(referralEntity);

      // add text messages 144 and 192 hours in the past
      referralEntity.TextMessages = new List<Entities.TextMessage>
      {
        RandomEntityCreator.CreateRandomTextMessage(
          number: referralCreate.Mobile,
          sent: DateTimeOffset.Now.AddHours(-144),
          modifiedAt: DateTimeOffset.Now.AddHours(-144),
          modifiedByUserId: User.GetUserId(),
          outcome: "delivered",
          received: DateTimeOffset.Now.AddHours(-144).AddMinutes(5)),
        RandomEntityCreator.CreateRandomTextMessage(
          number: referralCreate.Mobile,
          sent: DateTimeOffset.Now.AddHours(-192),
          modifiedAt: DateTimeOffset.Now.AddHours(-192),
          modifiedByUserId: User.GetUserId(),
          outcome: "delivered",
          received: DateTimeOffset.Now.AddHours(-192).AddMinutes(5))
      };

      // add text messages 48 and 96 hours in the past
      referralEntity.Calls = new List<Entities.Call>
      {
        RandomEntityCreator.CreateRandomChatBotCall(
          number: referralCreate.Mobile,
          sent: DateTimeOffset.Now.AddHours(-48),
          modifiedAt: DateTimeOffset.Now.AddHours(-48),
          modifiedByUserId: User.GetUserId(),
          outcome: ChatBotCallOutcome.NoAnswer.ToString(),
          called: DateTimeOffset.Now.AddHours(-48).AddMinutes(5)),
        RandomEntityCreator.CreateRandomChatBotCall(
          number: referralCreate.Mobile,
          sent: DateTimeOffset.Now.AddHours(-96),
          modifiedAt: DateTimeOffset.Now.AddHours(-96),
          modifiedByUserId: User.GetUserId(),
          outcome: ChatBotCallOutcome.NoAnswer.ToString(),
          called: DateTimeOffset.Now.AddHours(-96).AddMinutes(5))
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

      Entities.Referral existingReferral = existingUbrns.SingleOrDefault();

      if (existingReferral == null)
      {
        List<Entities.Referral> existingNhsNos = await GetReferrals(
          r => r.NhsNumber == referralCreate.NhsNumber);

        if (existingNhsNos.Any())
        {
          if (existingNhsNos
            .All(r => r.Status == CancelledByEreferrals.ToString()))
          {
            if (existingNhsNos.Any(r => r.ProviderId != null))
            {
              MatchesCancelledProviderSelected(result, existingNhsNos);
            }
          }
          else
          {
            MatchesCurrentReferrals(result, existingNhsNos);
          }
        }
      }
      else
      {
        result.ExistingReferralId = existingReferral.Id;
        if (existingReferral.Status != CancelledByEreferrals.ToString())
        {
          throw new ReferralNotUniqueException(
            $"A referral with a Ubrn of {referralCreate.Ubrn} already exists.");
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
            ProviderId = r.ProviderId,
            Status = r.Status,
            Ubrn = r.Ubrn
          })
          .ToListAsync();
      }

      static void MatchesCurrentReferrals(
        CanCreateGpReferral result,
        List<Entities.Referral> existingNhsNos)
      {
        result.Status = ReferralStatus.Exception;

        string[] duplicateUbrns = existingNhsNos
          .Where(r => r.Status != CancelledByEreferrals.ToString())
          .Select(r => r.Ubrn)
          .ToArray();

        result.StatusReasons.Add(
          "NHS number matches existing referral" +
          $"{(duplicateUbrns.Length > 1 ? "s" : "")} " +
          $"{string.Join("' ,'", duplicateUbrns)}.");
      }

      static void MatchesCancelledProviderSelected(
        CanCreateGpReferral result,
        List<Entities.Referral> existingNhsNos)
      {
        result.Status = ReferralStatus.Exception;

        string[] duplicateUbrns = existingNhsNos
          .Where(r => r.Status == CancelledByEreferrals.ToString())
          .Where(r => r.ProviderId != null)
          .Select(r => r.Ubrn)
          .ToArray();

        result.StatusReasons.Add(
          "NHS number matches existing referral" +
          $"{(duplicateUbrns.Length > 1 ? "s" : "")} " +
          $"{string.Join("' ,'", duplicateUbrns)} that had " +
          $"previously chosen a provider.");
      }
    }

    private async Task CheckSelfReferralIsUniqueAsync(
      ISelfReferralCreate selfReferralCreate)
    {
      if (selfReferralCreate is null)
      {
        throw new ArgumentNullException(nameof(selfReferralCreate));
      }

      Entities.Referral existingReferral = await _context.Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Email == selfReferralCreate.Email)
        .Select(r => new Entities.Referral
        {
          DateOfReferral = r.DateOfReferral,
          ReferralSource = r.ReferralSource,
          Status = r.Status
        })
        .FirstOrDefaultAsync();

      if (existingReferral != null)
      {
        throw new ReferralNotUniqueException(
          $"The email {selfReferralCreate.Email} is associated with an " +
          $"existing {existingReferral.ReferralSource} with a status of " +
          $"{existingReferral.Status} that was created on " +
          $"{existingReferral.DateOfReferral:yyyy-MM-ddTHH:mm:ss}.");
      }
    }

    private async Task CheckGeneralReferralIsUniqueAsync(string nhsNumber)
    {

      Entities.Referral existingReferral = await _context.Referrals
        .Where(r => r.IsActive)
        .Where(r => r.NhsNumber == nhsNumber)
        .Select(r => new Entities.Referral
        {
          DateOfReferral = r.DateOfReferral,
          ReferralSource = r.ReferralSource,
          Status = r.Status
        })
        .FirstOrDefaultAsync();

      if (existingReferral != null)
      {
        throw new ReferralNotUniqueException(
          $"The NHS number {nhsNumber} is associated with an existing " +
          $"{existingReferral.ReferralSource} with a status of " +
          $"{existingReferral.Status} that was created on " +
          $"{existingReferral.DateOfReferral:yyyy-MM-ddTHH:mm:ss}.");
      }
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
    private async Task CheckReferralCanBeCreatedWithNhsNumberAsync(
      string nhsNumber)
    {
      InUseResponse result = await IsNhsNumberInUseAsync(nhsNumber);

      // Existing referral with same NHS number?
      if (result.InUseResult.HasFlag(InUseResult.NotFound))
      {
        // No - Referral can be created
        return;
      }
      // Yes, Is referral cancelled?
      else if (result.InUseResult.HasFlag(InUseResult.Cancelled))
      {
        // Yes, Was provider selected?
        if (result.InUseResult.HasFlag(InUseResult.ProviderNotSelected))
        {
          // No - Referral can be created
          return;
        }
      }

      // A referral with the same NHS number exists, is not cancelled,
      // or is cancelled but a provider was already selected.
      // Referral cannot be created
      throw new ReferralNotUniqueException("Referral cannot be created " +
        "because there is an existing referral with the same NHS number: " +
        $"'{result.Referral.Id}'.");
    }

    public virtual async Task<List<ActiveReferralAndExceptionUbrn>>
      GetActiveReferralAndExceptionUbrns(string serviceId)
    {
      var query = _context
        .Referrals
        .Where(r => r.IsActive);

      if (serviceId != null)
      {
        query = query.Where(r => r.ServiceId == serviceId);
      }

      List<ActiveReferralAndExceptionUbrn> ubrns = await query
        .Where(r => r.Status != CancelledByEreferrals.ToString())
        .Where(r => r.ReferralSource == ReferralSource.GpReferral.ToString())
        // TODO - Remove the comment when completed referrals have been
        // sent discharge letters.
        // .Where(r => r.Status != Complete.ToString())
        .Select(r => new ActiveReferralAndExceptionUbrn
        {
          CriLastUpdated = r.Cri.ClinicalInfoLastUpdated,
          MostRecentAttachmentId =
            r.MostRecentAttachmentId ?? r.ReferralAttachmentId,
          ReferralAttachmentId = r.Status == RejectedToEreferrals.ToString()
              ? r.ReferralAttachmentId
              : null,
          Status = r.Status == RejectedToEreferrals.ToString()
            ? ActiveReferralAndExceptionUbrnStatus.AwaitingUpdate.ToString()
            : ActiveReferralAndExceptionUbrnStatus.InProgress.ToString(),
          Ubrn = r.Ubrn,
          ServiceId = r.ServiceId
        })
        .ToListAsync();

      return ubrns;
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

    public async Task<IReferral> GetReferralWithTriagedProvidersById(Guid id)
    {
      Entities.Referral referralEntity = await _context
        .Referrals
        .Include(r => r.Calls.Where(c => c.IsActive))
        .Include(r => r.TextMessages.Where(c => c.IsActive))
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
      string base36SentDate)
    {
      if (string.IsNullOrWhiteSpace(base36SentDate))
      {
        throw new ArgumentNullOrWhiteSpaceException(nameof(base36SentDate));
      }

      Entities.TextMessage textMessage = await _context
        .TextMessages
        .SingleOrDefaultAsync(t => t.Base36DateSent == base36SentDate);

      if (textMessage == null)
      {
        throw new ReferralNotFoundException("Cannot find a TextMessage with " +
          $"a base36SentDate of '{base36SentDate}'");
      }

      textMessage.Outcome = Constants.DATE_OF_BIRTH_EXPIRY;
      UpdateModified(textMessage);

      await _context.SaveChangesAsync();
    }

    public async Task<IReferral> GetServiceUserReferralAsync(
      string base36SentDate)
    {
      if (base36SentDate == null ||
        !base36SentDate.All(char.IsLetterOrDigit))
      {
        string value = string.IsNullOrEmpty(base36SentDate)
          ? "Null"
          : base36SentDate;

        throw new ReferralNotFoundException(
          $"Invalid {nameof(base36SentDate)} of [{value}].");
      }

      Entities.TextMessage textMessage = await _context
        .TextMessages
        .Include(t => t.Referral)
        .Where(t => t.IsActive)
        .Where(t => t.Base36DateSent == base36SentDate)
        .SingleOrDefaultAsync();

      if (textMessage == null)
      {
        throw new ReferralNotFoundException("No match with text message id: " +
          (string.IsNullOrEmpty(base36SentDate) ? "Null" : base36SentDate));
      }

      if (textMessage.Outcome == Constants.DO_NOT_CONTACT_EMAIL)
      {
        throw new TextMessageExpiredByEmailException("Text message token " +
          $"{base36SentDate} expired by Email not submitted.");
      }

      if (textMessage.Referral.ProviderId != null)
      {
        throw new TextMessageExpiredByProviderSelectionException("Text " +
          $"message with token {base36SentDate} had its provider selected" +
          $"on {textMessage.Referral.DateOfProviderSelection}.");
      }

      if (textMessage.Outcome == Constants.DATE_OF_BIRTH_EXPIRY)
      {
        throw new TextMessageExpiredByDoBCheckException("Text message with " +
          $"token {base36SentDate} expired by Date of Birth check.");
      }

      string[] validStatuses = new[] {
        TextMessage1.ToString(),
        TextMessage2.ToString(),
        ChatBotCall1.ToString(),
        ChatBotCall2.ToString(),
        ChatBotTransfer.ToString(),
        RmcCall.ToString(),
        RmcDelayed.ToString()
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

      if (referralEntity.Status != ReferralStatus.TextMessage1.ToString() &&
          referralEntity.Status != ReferralStatus.TextMessage2.ToString())
      {
        throw new ReferralInvalidStatusException(
          $"Expected referral status of {ReferralStatus.TextMessage1} " +
          $"or {ReferralStatus.TextMessage2} but status is " +
          $"{referralEntity.Status}.");
      }

      Referral referral = _mapper.Map<Referral>(referralEntity);

      return referral;
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
        query = query.Where(r => r.Ubrn == search.Ubrn.Replace(" ", ""));
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
      // are no search parameters
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
        Ubrn = r.Ubrn
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

      referralEntity.ConsentForFutureContactForEvaluation = false;
      referralEntity.Email = Constants.DO_NOT_CONTACT_EMAIL;
      referralEntity.Status = ReferralStatus.Exception.ToString();
      referralEntity.StatusReason =
        "Service user did not want to provide an email address.";
      referralEntity.TextMessages.ForEach(
        t => t.Outcome = Constants.DO_NOT_CONTACT_EMAIL);
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
        referralEntity.ConsentForFutureContactForEvaluation = consentForContact;
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

    /// <summary>
    /// Updates the UBRN's referral status to RejectedToEreferrals and the 
    /// status reason if it is passed.
    /// </summary>
    /// <param name="referralId">The Id of the referral to update.</param>
    /// <param name="statusReason">The status reason, pass null or white space
    /// to leave the status reason unchanged.</param>
    /// <returns>The updated referral</returns>
    public async Task<IReferral> UpdateStatusToRejectedToEreferralsAsync(
      Guid referralId, string statusReason)
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

      var validStatuses = new string[]
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
        throw new ReferralInvalidStatusException("Unable to update status to " +
          $"{ReferralStatus.RmcCall} because the referral has a status of " +
          $"{referralEntity.Status} and not one of the required statuses: " +
          $"{string.Join(", ", validStatuses)}.");
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
          Address1 = ra.Address1,
          Address2 = ra.Address2,
          Address3 = ra.Address3,
          DateOfBirth = ra.DateOfBirth,
          DateOfReferral = ra.DateOfReferral,
          DateToDelayUntil = ra.DateToDelayUntil,
          DelayReason = ra.DelayReason,
          Email = ra.Email,
          GivenName = ra.GivenName,
          FamilyName = ra.FamilyName,
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

      audits.ForEach(a => a.Username ??= $"Unknown: {a.ModifiedByUserId}");


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

      if (referral.ReferralSource == ReferralSource.GpReferral.ToString())
      {
        referral.Status = FailedToContact.ToString();
      }
      else
      {
        referral.Status = FailedToContactTextMessage.ToString();
      }

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
        .Select(r => r.NhsNumber).ToArray();

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
      Guid referralId, string reason, DateTimeOffset until)
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
        throw new ReferralNotFoundException("An active referral with UBRN of " +
          $"{referralUpdate.Ubrn} was not found.");
      }
      else if (existingReferrals.Count() > 1)
      {
        throw new ReferralNotUniqueException(
          $"There are {existingReferrals.Count} active referrals with the " +
          $"same UBRN of {referralUpdate.Ubrn}.");
      }

      Entities.Referral existingReferral = existingReferrals.Single();

      // referral being updated must have status of RejectedToEreferrals
      if (existingReferral.Status != RejectedToEreferrals.ToString())
      {
        throw new ReferralInvalidStatusException("The referral with a UBRN " +
          $"of {referralUpdate.Ubrn} cannot be updated because it's status " +
          $"is {existingReferral.Status} when it needs to be " +
          $"{RejectedToEreferrals}.");
      }
      // if referral previously selected a provider then it must be made an
      // Exception because a UBRN can only choose a provider once
      else if (existingReferral.ProviderId != null)
      {
        string providerName = await _context.Providers
          .AsNoTracking()
          .Where(p => p.Id == existingReferral.ProviderId)
          .Select(p => p.Name)
          .FirstOrDefaultAsync();

        existingReferral.Status = ReferralStatus.Exception.ToString();
        existingReferral.StatusReason = "The service user has previously " +
          $"started a programme with {providerName} and therefore cannot " +
          $"be referred again.";
      }
      else
      {
        // there cannot be any exisiting referrals with the same 
        // nhs number as the one being updated unless the previous referrals are
        // cancelled and a provider was not selected
        List<Entities.Referral> existingNhsNumReferrals = await _context
          .Referrals
          .Where(r => r.IsActive)
          .Where(r => r.NhsNumber == referralUpdate.NhsNumber)
          .Where(r => r.Ubrn != referralUpdate.Ubrn)
          .Where(r =>
            r.Status != CancelledByEreferrals.ToString() ||
            (r.Status == CancelledByEreferrals.ToString() &&
              r.ProviderId != null))
          .Select(r => new Entities.Referral
          {
            ProviderId = r.ProviderId,
            Ubrn = r.Ubrn
          })
          .ToListAsync();

        if (existingNhsNumReferrals.Any())
        {
          existingReferral.Status = ReferralStatus.Exception.ToString();
          existingReferral.NhsNumber = referralUpdate.NhsNumber;
          existingReferral.StatusReason = "The NHS number is already " +
            "associated with existing UBRN(s) " +
            $"{string.Join(',', existingNhsNumReferrals.Select(r => r.Ubrn))}";

          if (existingNhsNumReferrals.Any(r => r.ProviderId != null))
          {
            existingReferral.StatusReason += " where the service user has " +
              "previously started a programme with a provider and therefore " +
              "cannot be referred again.";
          }
          else
          {
            existingReferral.StatusReason += ".";
          }
        }
        else
        {
          string existingPostcode = existingReferral.Postcode;

          _mapper.Map((ReferralUpdate)referralUpdate, existingReferral);
          existingReferral.StatusReason = null;

          await ValidateReferral(referralUpdate, existingReferral);

          existingReferral.NumberOfContacts = 0;

          if (existingPostcode != referralUpdate.Postcode)
          {
            await UpdateDeprivation(existingReferral);
          }

          existingReferral.ReferralSource = ReferralSource.GpReferral.ToString();
        }
      }

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

            using (MemoryStream documentStream = new MemoryStream(documentBytes))
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
      if (validationResult.IsValid)
      {
        existingReferral.Status = ReferralStatus.New.ToString();
        existingReferral.StatusReason = null;
        existingReferral.MethodOfContact = (int)MethodOfContact.NoContact;
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

      referralEntity.ServiceUserEthnicityGroup = ethnicity.GroupName;
      referralEntity.ServiceUserEthnicity = ethnicity.DisplayName;
      referralEntity.Ethnicity = ethnicity.TriageName;
      UpdateTriage(referralEntity);
      UpdateModified(referralEntity);

      await _context.SaveChangesAsync();

      decimal bmi = referralEntity.CalculatedBmiAtRegistration ?? -1;

      if (bmi < ethnicity.MinimumBmi)
      {
        throw new BmiTooLowException("A referral that has the ethnicity " +
          $"of {ethnicityDisplayName} needs a BMI greater than " +
          $"{ethnicity.MinimumBmi} but this referral has a BMI of {bmi}.");
      }

      Referral referral = _mapper.Map<Referral>(referralEntity);

      return referral;
    }

    /// <summary>
    /// Updates the status of each referral that has a status of ChatBotCall2
    /// or ChatBotTransfer to RmcCall if the last chat bot call was more than
    /// 48 hours ago
    /// </summary>
    /// <returns></returns>
    public virtual async Task<string> PrepareRmcCallsAsync()
    {
      DateTimeOffset after =
        DateTimeOffset.Now.AddHours(-Constants.HOURS_BEFORE_NEXT_STAGE).Date;

      string[] statuses = new string[]
      {
        ChatBotCall2.ToString(),
        ChatBotTransfer.ToString()
      };

      List<Entities.Referral> referrals = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => statuses.Contains(r.Status))
        .Where(r => !r.Calls.Any(c => c.IsActive
          && (c.Sent.Date > after || c.Sent == default)))
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
    /// Updates the status of each referral to UnableToContact where the
    /// referral has a LetterSent date set more than 14 days ago.
    /// </summary>
    /// <returns>string detailing number of referrals updated</returns>
    public async Task<string[]> PrepareUnableToContactAsync()
    {
      DateTimeOffset after =
        DateTimeOffset.Now.AddDays(-Constants.LETTERSENT_GRACE_PERIOD).Date;

      List<Entities.Referral> referrals = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => r.DateLetterSent < after)
        .Where(r => r.Status == ReferralStatus.LetterSent.ToString())
        .ToListAsync();

      foreach (Entities.Referral referral in referrals)
      {
        referral.Status = FailedToContact.ToString();
        //Where an sms message can be sent set status to 
        // FailedToConnect on TextMessageApi.SmsController.Get()
        if (referral.Mobile.IsUkMobile() &&
           referral.ReferralSource == ReferralSource.SelfReferral.ToString())
          referral.Status = FailedToContactTextMessage.ToString();
        UpdateModified(referral);
      }

      await _context.SaveChangesAsync();

      return referrals.Select(t => t.Id.ToString()).ToArray();
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
        throw new ArgumentNullException(nameof(request));

      if (request.ExceptionType ==
          CreateReferralException.Undefined)
      {
        throw new ReferralCreateException("Invalid " +
          $"{nameof(CreateReferralException)} enum value of " +
          $"{request.ExceptionType}");
      }


      ValidateModelResult validationResult = Validators.ValidateModel(
        request);
      if (!validationResult.IsValid)
        throw new ReferralCreateException(validationResult.GetErrorMessage());


      Entities.Referral entity =
        await _context.Referrals.SingleOrDefaultAsync(t =>
          t.Ubrn == request.Ubrn);
      if (entity == null)
      {
        throw new ReferralNotFoundException(
          $"Referral not found with UBRN {request.Ubrn}.");
      }

      entity.Status = ReferralStatus.Exception.ToString();
      UpdateModified(entity);

      List<string> validationErrors = new List<string>();

      switch (request.ExceptionType)
      {
        case CreateReferralException.NhsNumberMismatch:
          validationErrors.Add(
            "The NHS number in the eRS work list " +
            "'{request.NhsNumberWorkList}' does not match the NHS number '" +
            $"{request.NhsNumberAttachment}' in the attached referral letter.");
          break;
        case CreateReferralException.MissingAttachment:
          validationErrors.Add(
            "The eRS referral does not have an attached referral letter.");
          break;
        case CreateReferralException.InvalidAttachment:
          validationErrors.Add(
            "The eRS referral has an invalid referral letter file type. " +
            "Accepted types are .doc, .docx, .pdf and .rtf; Non-Pdf files " +
            "should be exportable as pdf.");
          break;
      }

      entity.StatusReason = string.Join(' ', validationErrors);

      await _context.SaveChangesAsync();

      Referral referralModel =
        _mapper.Map<Entities.Referral, Referral>(entity);

      return referralModel;

    }
    public async Task<IReferral> UpdateEthnicity(Guid id, Ethnicity ethnicity)
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

      decimal? minimumBmi = await _context
        .Ethnicities
        .Where(e => e.TriageName.Equals(ethnicity.ToString()))
        .Where(e => e.IsActive)
        .MinAsync(e => e.MinimumBmi);

      if (!minimumBmi.HasValue)
      {
        throw new EthnicityNotFoundException("An ethnicity with a " +
          $"triage name of {ethnicity} cannot be found or its minimum BMI" +
          $"is not set.");
      }

      referralEntity.Ethnicity = ethnicity.ToString();

      decimal bmi = referralEntity.CalculatedBmiAtRegistration ?? -1;
      bool isBmiTooLow = (bmi < minimumBmi);

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
      referral.SelectedEthnicGroupMinimumBmi = minimumBmi.Value;

      if (isBmiTooLow)
      {
        referral.Providers = new();
      }
      else
      {
        IEnumerable<Provider> providers = await
          UpdateOfferedCompletionLevelAsync(referralEntity);

        if (providers == null || providers.Any() == false)
        {
          referral.Providers = new();
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


    public async Task<IEnumerable<Models.Ethnicity>> GetEthnicitiesAsync(
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
          OldName = e.OldName,
          TriageName = e.TriageName
        })
        .ToArrayAsync();

      if (!ethnicities.Any())
      {
        throw new EthnicityNotFoundException();
      }

      return ethnicities;
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
        ChatBotCall1 | ChatBotCall2 | ChatBotTransfer;

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

    public virtual async Task<IReferral> CreateSelfReferral(
      ISelfReferralCreate selfReferralCreate)
    {
      if (selfReferralCreate == null)
      {
        throw new ArgumentNullException(nameof(selfReferralCreate));
      }

      await CheckSelfReferralIsUniqueAsync(selfReferralCreate);

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

      await UpdateSelfReferralUbrnAsync(referralEntity.Id);

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

      await CheckGeneralReferralIsUniqueAsync(referralCreate.NhsNumber);

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
      referralEntity.ReferralSource = ReferralSource.GeneralReferral.ToString();
      referralEntity.CreatedDate = DateTimeOffset.Now;

      await ValidateReferral(referralCreate, referralEntity);

      await UpdateDeprivation(referralEntity);
      UpdateModified(referralEntity);

      UpdateTriage(referralEntity);

      //Save to DB
      _context.Referrals.Add(referralEntity);
      await _context.SaveChangesAsync();

      await UpdateGeneralReferralUbrnAsync(referralEntity.Id);

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

        if (Enum.TryParse(
          entity.TriagedCompletionLevel,
          out TriageLevel triageLevel))
        {
          providers = await _providerService
           .GetProvidersAsync(triageLevel);

          if ((providers == null || providers.Any() == false)
            && triageLevel != TriageLevel.Low)
          {
            entity.OfferedCompletionLevel = TriageLevel.Low.ToString("d");

            providers = await _providerService
              .GetProvidersAsync(TriageLevel.Low);
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

      await UpdatePharmacyReferralUbrnAsync(referralEntity.Id);

      IReferral referralModel = _mapper.Map<Referral>(referralEntity);

      return referralModel;
    }

    public async Task<IPharmacistKeyCodeGenerationResponse>
      GetPharmacistKeyCodeAsync(IPharmacistKeyCodeCreate create)
    {
      ValidateModelResult result = ValidateModel(create);
      if (!result.IsValid)
        throw new PharmacyReferralValidationException(result.Results);

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
        throw new PharmacyReferralValidationException(result.Results);

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

    public async Task<InUseResponse> IsNhsNumberInUseAsync(
      string nhsNumber)
    {
      if (string.IsNullOrWhiteSpace(nhsNumber))
      {
        throw new ArgumentException(
          $"'{nameof(nhsNumber)}' cannot be null or whitespace.",
          nameof(nhsNumber));
      }

      InUseResponse response = new();

      response.Referral = await _context.Referrals
        .Include(r => r.Provider)
        .Where(r => r.IsActive)
        .Where(r => r.NhsNumber == nhsNumber)
        .OrderByDescending(r => r.CreatedDate)
        .ProjectTo<Referral>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();

      return response;
    }

    public async Task<bool> PharmacyEmailListedAsync(
      string referringPharmacyEmail)
    {
      if (string.IsNullOrWhiteSpace(referringPharmacyEmail))
        return false;

      return await _context.Pharmacists.AnyAsync(
        t => t.ReferringPharmacyEmail == referringPharmacyEmail && t.IsActive);
    }

    public async Task<string> GetProviderNameAsync(Guid providerId)
    {
      if (providerId == Guid.Empty)
        return "A provider";

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
        .SingleOrDefaultAsync(r => r.Id == referralId);

      if (referral == null)
      {
        throw new ReferralNotFoundException(referralId);
      }

      if (referral.ProviderId != null)
      {
        throw new ReferralProviderSelectedException(
          $"Referral Id {referralId} already has a provider selected.");
      }

      ReferralStatus isValidUpdateStatus =
        New | RmcCall | RmcDelayed |
        TextMessage1 | TextMessage2 |
        ChatBotCall1 | ChatBotCall2 | ChatBotTransfer;

      referral.Status.TryParseToEnumName(out ReferralStatus status);

      if (source.ToString().Is(ReferralSource.GeneralReferral))
      {

        if (!isValidUpdateStatus.HasFlag(status) || (int)status == 0)
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

      if (providers.Count(t => t.Id == providerId) == 0)
        throw new ProviderSelectionMismatch(providerId);

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
        throw new TriageNotFoundException("Triage completion level is null");

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

    protected virtual async Task UpdateSelfReferralUbrnAsync(
      Guid referralId)
    {
      //Does the SelfReferral already exists
      SelfReferral found =
        await _context.SelfReferrals.SingleOrDefaultAsync(t =>
          t.ReferralId == referralId);

      if (found != null)
      {
        throw new ReferralInvalidCreationException(
          $"There is already a self referral with the ID {referralId} has " +
          $"already been created");
      }

      Entities.Referral referral =
        await _context.Referrals.SingleOrDefaultAsync(t => t.Id == referralId);

      if (referral == null)
      {
        throw new ReferralNotFoundException(
          $"Referral with the ID of {referralId} was not found ");
      }

      SelfReferral selfReferral = new SelfReferral
      {
        ReferralId = referralId
      };

      _context.SelfReferrals.Add(selfReferral);

      if (await _context.SaveChangesAsync() > 0)
      {
        Models.SelfReferral model =
          _mapper.Map<SelfReferral, Models.SelfReferral>(selfReferral);
        referral.Ubrn = model.Ubrn;
        await _context.SaveChangesAsync();
      }
      else
      {
        throw new ReferralUpdateException(
          $"SelfReferral for Referral ID {referralId} was not saved.");
      }
    }

    protected virtual async Task UpdateGeneralReferralUbrnAsync(
      Guid referralId)
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

      if (await _context.SaveChangesAsync() > 0)
      {
        Models.GeneralReferral model =
          _mapper.Map<Entities.GeneralReferral, Models.GeneralReferral>(
            directReferral);
        referral.Ubrn = model.Ubrn;
        await _context.SaveChangesAsync();
      }
      else
      {
        throw new ReferralUpdateException(
          $"DirectReferral for Referral ID {referralId} was not saved.");
      }
    }

    protected virtual async Task UpdatePharmacyReferralUbrnAsync(
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

      if (await _context.SaveChangesAsync() > 0)
      {
        Models.PharmacyReferral model =
          _mapper.Map<PharmacyReferral,
            Models.PharmacyReferral>(pharmacyReferral);
        referral.Ubrn = model.Ubrn;
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
          $"is {Math.Round(ethnicity.MinimumBmi, 2)}",
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
      if (!referral.Sex.TryParseToEnumName(out Sex sex))
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

    private async Task UpdateDeprivation(Entities.Referral referralEntity)
    {
      if (referralEntity is null)
      {
        throw new ArgumentNullException(nameof(referralEntity));
      }

      try
      {
        string lsoa = await _postcodeService.GetLsoa(referralEntity.Postcode);
        Models.Deprivation deprivation =
          await _deprivationService.GetByLsoa(lsoa);
        referralEntity.Deprivation = deprivation.ImdQuintile().ToString();
      }
      catch (Exception ex)
      {
        if (ex is PostcodeNotFoundException
          || ex is DeprivationNotFoundException)
        {
          // Unable Postcode (i.e. new build) rather than creating exception
          // default the deprivation to IMD1 (the most deprived)
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

      //find active referral based on ubrn
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

      //Check if already has CRI
      if (referral.Cri == null)
      {
        referral.Cri = entity;
      }
      else
      {
        //Check if last update was before current;
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

      //return Cri as response

      response.ResponseStatus = StatusType.Valid;

      return response;
    }

    public async Task<bool> UpdateReferralAuditCri(string ubrn, Guid criId)
    {
      List<Entities.ReferralAudit> audits =
        await _context.ReferralsAudit.Where(t => t.Ubrn == ubrn)
        .OrderByDescending(t => t.AuditId).Take(1).ToListAsync();

      if (!audits.Any())
        throw new ReferralNotFoundException(
          $"Referral Audit not found for UBRN {ubrn}.");

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
    /// Inserts or Updates Analytics row linked to Referral
    /// </summary>
    /// <param name="id">ReferralId</param>
    /// <param name="value">Comman seperated list of Providers</param>
    /// <returns></returns>
    public async Task UpdateAnalyticsForProviderList(Guid id, string value)
    {
      if (id == default)
      {
        throw new ArgumentException(nameof(id));
      }
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new ArgumentException(nameof(value));
      }

      Entities.Referral referralEntity = await _context.Referrals
        .SingleOrDefaultAsync(r => r.Id == id);

      if (referralEntity == null)
      {
        throw new ReferralNotFoundException(id);
      }
      else if (referralEntity.ProviderId != null)
      {
        throw new ReferralProviderSelectedException(
          referralEntity.Id, referralEntity.ProviderId);
      }

      Analytics analytics =
        await _context.Analytics.SingleOrDefaultAsync(t => t.LinkId == id);

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

    public async Task<InUseResponse> IsEmailInUseAsync(
      string email)
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        throw new ArgumentException(
          $"'{nameof(email)}' cannot be null or whitespace.",
          nameof(email));
      }

      InUseResponse response = new();

      response.Referral = await _context.Referrals
        .Include(r => r.Provider)
        .Where(r => r.IsActive)
        .Where(r => r.Email == email)
        .OrderByDescending(r => r.CreatedDate)
        .ProjectTo<Referral>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();

      return response;
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
          $"invalid status of {referral.Status}, expected {AwaitingDischarge}.");
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
    public virtual async Task CreateMskReferralAsync(
      IMskReferralCreate mskReferralCreate)
    {
      if (mskReferralCreate == null)
      {
        throw new ArgumentNullException(nameof(mskReferralCreate));
      }

      await ValidateReferral(mskReferralCreate);

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

      await UpdateMskReferralUbrnAsync(referralEntity.Id);
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
          $"Ethnicity: {nameof(referralModel.ServiceUserEthnicity)} and " +
          $"Ethnicity Group:{nameof(referralModel.ServiceUserEthnicityGroup)} " +
          "is invalid.",
          new string[] 
          { 
            $"{nameof(referralModel.ServiceUserEthnicity)}",
            $"{nameof(referralModel.ServiceUserEthnicityGroup)}"
          }));

        throw new MskReferralValidationException(result.Results);
      }
    }

    protected virtual async Task UpdateMskReferralUbrnAsync(Guid referralId)
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

      if (await _context.SaveChangesAsync() > 0)
      {
        Models.MskReferral model = _mapper
          .Map<Models.MskReferral>(mskReferral);
        referral.Ubrn = model.Ubrn;
        await _context.SaveChangesAsync();
      }
      else
      {
        throw new ReferralUpdateException(
          $"MskReferral for Referral Id {referralId} was not saved.");
      }
    }
  }
}
