using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Business.Enums.ReferralStatus;
using ApiKeyStore = WmsHub.Business.Entities.ApiKeyStore;
using IReferral = WmsHub.Business.Models.IReferral;
using Provider = WmsHub.Business.Models.Provider;
using ReferralStatusReason =
  WmsHub.Business.Models.ReferralStatusReason.ReferralStatusReason;
using ProviderSubmission = WmsHub.Business.Models.ProviderSubmission;
using Referral = WmsHub.Business.Entities.Referral;
using System.Linq.Expressions;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Business.Services;

public class ProviderService
  : ServiceBase<Entities.Referral>, IProviderService
{
  private readonly IMapper _mapper;
  private readonly ProviderOptions _options;
  private static List<Entities.ReferralStatusReason> _referralStatusReasons;

  public ProviderService(
    DatabaseContext context,
    IMapper mapper,
    IOptions<ProviderOptions> options)
    : base(context)
  {
    _mapper = mapper;
    _options = options.Value;
  }

  public virtual async Task<bool> AddProviderSubmissionsAsync(
    ProviderSubmissionRequest request,
    Guid referralId)
  {
    Guid providerId = User.GetUserId();

    bool providerExists = await _context
     .Providers
     .Where(r => r.IsActive)
     .AnyAsync(p => p.Id == providerId);

    if (!providerExists)
    {
      throw new ProviderNotFoundException(providerId);
    }

    IEnumerable<Entities.ProviderSubmission> distinctSubmissions = request.Submissions
      .GroupBy(x => new { x.Coaching, x.Date, x.Measure, x.Weight })
      .Select(x => new Entities.ProviderSubmission()
      {
        Coaching = x.First().Coaching,
        Date = x.First().Date,
        Id = x.First().Id,
        IsActive = x.First().IsActive,
        Measure = x.First().Measure,
        ModifiedAt = x.First().ModifiedAt,
        ModifiedByUserId = x.First().ModifiedByUserId,
        Weight = x.First().Weight,
        ReferralId = x.First().ReferralId,
        ProviderId = providerId
      });

    _context.ProviderSubmissions.AddRange(distinctSubmissions);

    if (request.ReferralStatus != null)
    {
      Referral referral = await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referralId)
        ?? throw new ReferralNotFoundException(referralId);

      ValidateStatusBeforeUpdate(referral.Status, request.ReferralStatus);

      if (request.ProgrammeOutcome != null)
      {
        referral.ProgrammeOutcome = request.ProgrammeOutcome;
      }

      if (request.DateCompletedProgramme != null)
      {
        referral.DateCompletedProgramme = request.DateCompletedProgramme;
      }

      if (request.DateStartedProgramme != null)
      {
        referral.DateStartedProgramme = request.DateStartedProgramme;
      }

      if (request.DateOfProviderContactedServiceUser != null)
      {
        referral.DateOfProviderContactedServiceUser =
          request.DateOfProviderContactedServiceUser;
      }

      if (request.ReferralStatus == ProviderTerminated &&
        referral.ReferralSource != ReferralSource.GpReferral.ToString())
      {
        referral.Status = ProviderTerminatedTextMessage.ToString();
      }
      else if (request.ReferralStatus == ProviderRejected &&
        referral.ReferralSource != ReferralSource.GpReferral.ToString())
      {
        referral.Status = ProviderRejectedTextMessage.ToString();
      }
      else if (request.ReferralStatus == ProviderDeclinedByServiceUser &&
        referral.ReferralSource != ReferralSource.GpReferral.ToString())
      {
        referral.Status = ProviderDeclinedTextMessage.ToString();
      }
      else
      {
        referral.Status = request.ReferralStatus.Value.ToString();
      }

      referral.StatusReason = request.Reason;
      UpdateModified(referral);
    }

    return await _context.SaveChangesAsync() > 0;
  }

  private void ValidateStatusBeforeUpdate(string referralStatus,
    ReferralStatus? requestStatus)
  {

    string cannotBeSet = $"cannot have its status set to {requestStatus} " +
      $"because its current status is {referralStatus}.";

    // To update referral STATUS to ProviderAccepted
    // referral STATUS must be ProviderAwaitingStart
    if (requestStatus == ProviderAccepted
      && referralStatus != ProviderAwaitingStart.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderContactedServiceUser
    // referral STATUS must be ProviderAwaitingStart or ProviderAccepted
    if (requestStatus == ProviderContactedServiceUser
      && referralStatus != ProviderAccepted.ToString()
      && referralStatus != ProviderAwaitingStart.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderDeclinedByServiceUser
    // referral STATUS must be: ProviderAwaitingStart 
    // OR ProviderAccepted 
    // OR ProviderContactedServiceUser
    if (requestStatus == ProviderDeclinedByServiceUser
      && referralStatus != ProviderAwaitingStart.ToString()
      && referralStatus != ProviderAccepted.ToString()
      && referralStatus != ProviderContactedServiceUser.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderRejected
    // referral STATUS must be: ProviderAwaitingStart 
    // OR ProviderAccepted 
    // OR ProviderContactedServiceUser
    if (requestStatus == ProviderRejected
      && referralStatus != ProviderAwaitingStart.ToString()
      && referralStatus != ProviderAccepted.ToString()
      && referralStatus != ProviderContactedServiceUser.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderStarted
    // referral STATUS must be: ProviderAwaitingStart 
    // OR ProviderAccepted 
    // OR ProviderContactedServiceUser
    if (requestStatus == ProviderStarted
      && referralStatus != ProviderAwaitingStart.ToString()
      && referralStatus != ProviderAccepted.ToString()
      && referralStatus != ProviderContactedServiceUser.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderTerminated
    // referral STATUS must be ProviderStarted
    if (requestStatus == ProviderTerminated &&
      referralStatus != ProviderStarted.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }

    // To update referral STATUS to ProviderCompleted
    // referral STATUS must be ProviderStarted
    if (requestStatus == ProviderCompleted &&
      referralStatus != ProviderStarted.ToString())
    {
      throw new StatusChangeException(cannotBeSet);
    }
  }

  protected virtual async Task<Referral> GetProviderReferralByProviderUbrn(
    Guid providerId,
    string providerUbrn)
  {
    Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId == providerId)
      .Where(r => r.ProviderUbrn == providerUbrn)
      .Select(r => new Referral()
      {
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateStartedProgramme = r.DateStartedProgramme,
        Id = r.Id,
        Status = r.Status
      })
      .SingleOrDefaultAsync();

    return referral;
  }

  protected internal virtual async Task<Entities.Provider>
    GetProviderByIdAsync(Guid userId)
  {
    return await _context
     .Providers
     .Where(r => r.IsActive)
     .SingleOrDefaultAsync(t => t.Id == userId);
  }

  /// <summary>
  /// Returns a count of the number of active providers for a given triage 
  /// level
  /// </summary>
  /// <param name="triageLevel">The triage level to search for</param>
  /// <returns>A count of the providers that are active for the given triage 
  /// level</returns>
  public virtual async Task<int>
    GetNumOfProvidersAvailableAtTriageLevelAsync(TriageLevel triageLevel)
  {
    var query = _context.Providers.Where(p => p.IsActive);

    if (triageLevel == TriageLevel.High)
    {
      query = query.Where(p => p.Level3);
    }
    else if (triageLevel == TriageLevel.Medium)
    {
      query = query.Where(p => p.Level2);
    }
    else
    {
      query = query.Where(p => p.Level1);
    }

    return await query.CountAsync();
  }

  public virtual async Task<Guid> ValidateProviderKeyAsync(string apiKey)
  {
    if (string.IsNullOrWhiteSpace(apiKey))
    {
      throw new ArgumentNullException(nameof(apiKey));
    }

    Entities.Provider provider = await _context.Providers
      .FirstOrDefaultAsync(t => t.ApiKey == apiKey
        && t.ApiKeyExpires.Value > DateTime.UtcNow);

    return provider?.Id ?? Guid.Empty;
  }

  public virtual async Task<IEnumerable<ServiceUser>> GetServiceUsers()
  {
    IEnumerable<ServiceUser> serviceUsers = await _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId == User.GetUserId())
      .Where(r => r.ProviderUbrn != null)
      .Where(r => r.Status == ProviderAwaitingStart.ToString())
      .ProjectTo<ServiceUser>(_mapper.ConfigurationProvider)
      .ToListAsync();

    return serviceUsers.OrderBy(r => r.Ubrn).ToList();
  }

  public virtual async Task<ProviderAdminResponse>
    GetAllActiveProvidersAsync()
  {
    Entities.Provider[] providers =
      await _context.Providers.Where(t => t.IsActive).ToArrayAsync();

    IEnumerable<ProviderRequest> providerRequests =
      _mapper.Map<Entities.Provider[],
        IEnumerable<ProviderRequest>>(providers);

    ProviderAdminResponse response = new ProviderAdminResponse
    {
      Providers = providerRequests,
      ResponseStatus =
        providerRequests.Any()
          ? StatusType.Valid
          : StatusType.NoRowsReturned
    };

    return response;
  }

  public virtual async Task<ProviderResponse> GetProviderAsync(Guid id)
  {
    Entities.Provider provider = await _context.Providers
      .SingleOrDefaultAsync(t => t.Id == id) 
      ?? throw new ProviderNotFoundException(id);

    ProviderResponse response = _mapper.Map<ProviderResponse>(provider);

    return response;
  }

  public virtual async Task<string> GetProviderNameAsync(Guid id)
  {
    string providerName = await _context.Providers
      .Where(p => p.Id == id)
      .Select(p => p.Name)
      .SingleOrDefaultAsync();

    if (providerName == null)
      throw new ProviderNotFoundException(id);

    return providerName;
  }

  public virtual async Task<IEnumerable<ProviderInfo>> GetProvidersInfo()
  {
    string[] statuses = new string[]
    {
      ProviderAccepted.ToString(),
      ProviderAwaitingStart.ToString(),
      ProviderContactedServiceUser.ToString(),
      ProviderStarted.ToString()
    };

    IEnumerable<ProviderInfo> providerInfos = await _context.Providers
      .Select(p => new ProviderInfo
      {
        IsActive = p.IsActive,
        IsLevel1Active = p.Level1,
        IsLevel2Active = p.Level2,
        IsLevel3Active = p.Level3,
        Name = p.Name,
        NoOfElectiveCareReferrals = p.Referrals
        .Where(r => r.ReferralSource == ReferralSource
            .ElectiveCare.ToString())
          .Count(r => statuses.Contains(r.Status)),
        NoOfGeneralReferrals = p.Referrals
          .Where(r => r.ReferralSource == ReferralSource
            .GeneralReferral.ToString())
          .Count(r => statuses.Contains(r.Status)),
        NoOfGpReferrals = p.Referrals
          .Where(r => r.ReferralSource == ReferralSource
            .GpReferral.ToString())
          .Count(r => statuses.Contains(r.Status)),
        NoOfMskReferrals = p.Referrals
        .Where(r => r.ReferralSource == ReferralSource
            .Msk.ToString())
          .Count(r => statuses.Contains(r.Status)),
        NoOfPharmacyReferrals = p.Referrals
          .Where(r => r.ReferralSource == ReferralSource
            .Pharmacy.ToString())
          .Count(r => statuses.Contains(r.Status)),
        NoOfSelfReferrals = p.Referrals
          .Where(r => r.ReferralSource == ReferralSource
            .SelfReferral.ToString())
          .Count(r => statuses.Contains(r.Status))
      })
      .OrderBy(p => p.Name)
      .ToListAsync();

    return providerInfos;
  }

  public virtual async Task<IEnumerable<Provider>> GetProvidersAsync(TriageLevel triageLevel)
  {
    IQueryable<Entities.Provider> query = _context
      .Providers
      .Include(x => x.Details)
      .Where(p => p.IsActive);

    if (triageLevel == TriageLevel.High)
    {
      query = query.Where(p => p.Level3);
    }
    else if (triageLevel == TriageLevel.Medium)
    {
      query = query.Where(p => p.Level2);
    }
    else
    {
      query = query.Where(p => p.Level1);
    }

    IEnumerable<Provider> providers = await query
      .Select(x => new Provider()
      {
        Details = x.Details
          .Where(x => x.TriageLevel == (int)triageLevel)
          .Select(y => new Models.ProviderDetail()
          {
            Id = y.Id,
            IsActive = y.IsActive,
            ModifiedAt = y.ModifiedAt,
            ModifiedByUserId = y.ModifiedByUserId,
            ProviderId = y.ProviderId,
            Section = y.Section,
            TriageLevel = y.TriageLevel,
            Value = y.Value
          })
          .ToList(),
        Id = x.Id,        
        IsActive = x.IsActive,
        Level1 = x.Level1,
        Level2 = x.Level2,
        Level3 = x.Level3,
        Logo = x.Logo,
        ModifiedAt = x.ModifiedAt,
        ModifiedByUserId = x.ModifiedByUserId,
        Name = x.Name,
        ProviderAuth = null,
        Summary = triageLevel == TriageLevel.Low ? x.Summary : null,
        Summary2 = triageLevel == TriageLevel.Medium ? x.Summary2 : null,
        Summary3 = triageLevel == TriageLevel.High ? x.Summary3 : null,
        Website = x.Website,
      })
      .ToListAsync();

    return providers;
  }

  public virtual async Task<IEnumerable<Provider>> GetProvidersAsync(
    int triageLevelValue)
  {
    if (Enum.TryParse(
      triageLevelValue.ToString(),
      out TriageLevel triageLevel))
    {
      if (Enum.IsDefined(triageLevel))
      {
        return await GetProvidersAsync(triageLevel);
      }
    }

    throw new UnexpectedEnumValueException(
      typeof(TriageLevel),
      triageLevelValue);
  }

  public virtual async Task<IEnumerable<ServiceUserSubmissionResponse>>
    ProviderSubmissionsAsync(IEnumerable<IServiceUserSubmissionRequest> requests)
  {
    ArgumentNullException.ThrowIfNull(requests);

    if (!requests.Any())
    {
      throw new ArgumentOutOfRangeException(nameof(requests));
    }

    
    if (requests.Count() > 1)
    {
      IEnumerable<IServiceUserSubmissionRequest> updateRequests =
        GetCollatedUpdateRequestsByUbrn(requests);

      requests = requests.Where(r => r.Type != UpdateType.Update.ToString()).Union(updateRequests);
    }
    
    List<ServiceUserSubmissionResponse> responses = new(requests.Count());

    foreach (ServiceUserSubmissionRequest request in requests)
    {
      ServiceUserSubmissionResponse response = new();

      ValidateModelResult result = ValidateModel(request);

      if (result.IsValid)
      {
        Referral referral = await GetProviderReferralByProviderUbrn(
          User.GetUserId(),
          request.Ubrn);

        if (referral == null)
        {
          response.SetStatus(
            StatusType.Invalid,
            $"UBRN {request.Ubrn} " +
            $"not found for provider.");
        }
        else
        {
          ProviderSubmissionRequest providerSubmissionRequest = new(
            request,
            User.GetUserId(),
            referral.Id);

          if (referral.DateOfProviderSelection == null)
          {
            response.SetStatus(StatusType.Invalid,
              $"UBRN {request.Ubrn} " +
              $"Date Of Provider Selection in referral is null");
            responses.Add(response);
            continue;
          }

          if (referral.DateOfProviderSelection.HasValue)
          {
            // only check that its the same date as providers are not 
            // including time element in submissions
            bool dateError =
              referral.DateOfProviderSelection.Value.Date > request.Date;

            if (!dateError &&
                request.Updates != null
                && request.Updates.Any())
            {
              dateError = request.Updates.Any(
                t => t.Date < referral.DateOfProviderSelection.Value.Date);
            }

            if (dateError)
            {
              response.SetStatus(StatusType.Invalid,
                $"UBRN {request.Ubrn} " +
                $"cannot accept some of the update dates because the date " +
                $"of provider selection for this referral " +
                $"{referral.DateOfProviderSelection} is after some of the " +
                $"dates provided.");
              responses.Add(response);
              continue;
            }
          }

          if (referral.DateStartedProgramme.HasValue)
          {
            DateTimeOffset dateStartedProgramme =
              referral.DateStartedProgramme.Value;

            DateTimeOffset maxSubmissionDate =
              dateStartedProgramme.Date.AddDays(_options.MaxCompletionDays);

            if (DateTimeOffset.Now.Date > maxSubmissionDate)
            {
              response.SetStatus(StatusType.Invalid,
                $"UBRN {request.Ubrn} " +
                $"can no longer receive updates because the last date " +
                $"for submissions was {maxSubmissionDate:yyyy-MM-dd}.");
              responses.Add(response);
              continue;
            }

            bool updatesBeforeDateStartedProgramme = request.Updates
              .Where(x => x.Date.HasValue)
              .Any(x => x.Date.Value.Date < dateStartedProgramme.Date);

            if (DateTimeOffset.Now.Date < dateStartedProgramme.Date ||
              updatesBeforeDateStartedProgramme)
            {
              response.SetStatus(
                StatusType.Invalid,
                $"UBRN {request.Ubrn} cannot accept some of the update dates because the programme " +
                $"start date for this referral {referral.DateStartedProgramme} is after some of " +
                "the dates provided.");
              responses.Add(response);
              continue;
            }

            // a request of type completed cannot be accepted until the 
            // expected end of the programme
            if (request.ReasonStatus == ProviderCompleted)
            {
              DateTimeOffset programmeEndDate =
                dateStartedProgramme.Date.AddDays(_options.CompletionDays);

              if (request.Date.Value < programmeEndDate)
              {
                response.SetStatus(StatusType.Invalid,
                  $"UBRN {request.Ubrn} " +
                  $"cannot be completed until the end of the programme " +
                  $"which is {programmeEndDate:yyyy-MM-dd}. If the service " +
                  $"user has disengaged please send a terminated request.");
                responses.Add(response);
                continue;
              }
            }
          }

          if (request.Type == UpdateType.Update.ToString())
          {
            if (!_options.IgnoreStatusRequirementForUpdate)
            {
              if (referral.Status != ProviderStarted.ToString())
              {
                response.SetStatus(StatusType.Invalid,
                  $"UBRN {request.Ubrn} " +
                  $"cannot receive updates because its status is {referral.Status}.");

                responses.Add(response);
                continue;
              }
            }
          }

          try
          {
            await AddProviderSubmissionsAsync(
              providerSubmissionRequest, referral.Id);
          }
          catch (StatusChangeException e)
          {
            response.SetStatus(
              StatusType.Invalid,
              $"UBRN {request.Ubrn} " +
              $"{e.Message}");
          }
        }
      }
      else
      {
        response.SetStatus(
          StatusType.Invalid,
          $"UBRN {request.Ubrn} " +
          $"{result.GetErrorMessage()}");
      }

      responses.Add(response);
    }

    return responses;
  }


  public virtual async Task<NewProviderApiKeyResponse>
    UpdateProviderKeyAsync(ProviderResponse providerResponse,
      int validDays = 365)
  {

    string newApiKey = Generators.GenerateApiKey(providerResponse.Name);

    Entities.Provider provider = await _context.Providers
      .SingleOrDefaultAsync(t => t.Id == providerResponse.Id);


    UpdateModified(provider);
    provider.ApiKey = newApiKey;
    provider.ApiKeyExpires = DateTimeOffset.Now.AddDays(validDays);

    NewProviderApiKeyResponse response =
      _mapper.Map<NewProviderApiKeyResponse>(provider);

    response.ApiKey = provider.ApiKey;
    response.ApiKeyExpiry = provider.ApiKeyExpires.Value;
    response.ResponseStatus =
      await _context.SaveChangesAsync() > 0
        ? StatusType.Valid
        : StatusType.NoRowsUpdated;

    return response;
  }

  public virtual async Task<ProviderResponse> UpdateProviderLevelsAsync(
    ProviderLevelStatusChangeRequest request)
  {
    Entities.Provider provider = await GetProviderByIdAsync(request.Id);

    if (provider == null)
    {
      return new ProviderResponse()
      {
        Errors = new()
        {
          $"Unable to find a provider with an id of {request.Id}."
        },
        ResponseStatus = StatusType.ProviderNotFound
      };
    }

    if (request.Level1 != null && provider.Level1 != request.Level1)
    {
      provider.Level1 = request.Level1.Value;
    }

    if (request.Level1 != null && provider.Level2 != request.Level2)
    {
      provider.Level2 = request.Level2.Value;
    }

    if (request.Level1 != null && provider.Level3 != request.Level3)
    {
      provider.Level3 = request.Level3.Value;
    }

    UpdateModified(provider);

    ProviderResponse response = _mapper.Map<ProviderResponse>(provider);

    response.ResponseStatus =
      await _context.SaveChangesAsync() > 0
        ? StatusType.Valid
        : StatusType.ProviderUpdateFailed;

    return response;
  }

  public virtual async Task<ProviderResponse> UpdateProvidersAsync(
    ProviderRequest request)
  {
    Entities.Provider provider = await _context
      .Providers
      .SingleOrDefaultAsync(t => t.Id == request.Id);

    if (provider == null)
      throw new ProviderNotFoundException(
        $"Provider not found using ID of {request.Id}");

    if (!string.IsNullOrEmpty(request.Name) && provider.Name != request.Name)
      provider.Name = request.Name;

    Ganss.Xss.HtmlSanitizer sanitizer = new Ganss.Xss.HtmlSanitizer();

    if (provider.Summary != sanitizer.Sanitize(request.Summary))
      provider.Summary = sanitizer.Sanitize(request.Summary);

    if (provider.Summary2 != sanitizer.Sanitize(request.Summary2))
      provider.Summary2 = sanitizer.Sanitize(request.Summary2);

    if (provider.Summary3 != sanitizer.Sanitize(request.Summary3))
      provider.Summary3 = sanitizer.Sanitize(request.Summary3);

    if (!string.IsNullOrEmpty(request.Website)
      && provider.Website != request.Website)
    {
      provider.Website = request.Website;
    }

    if (!string.IsNullOrEmpty(request.Logo) && provider.Logo != request.Logo)
      provider.Logo = request.Logo;

    if (request.Level1 != null && provider.Level1 != request.Level1)
      provider.Level1 = request.Level1.Value;

    if (request.Level2 != null && provider.Level2 != request.Level2)
      provider.Level2 = request.Level2.Value;

    if (request.Level3 != null && provider.Level3 != request.Level3)
      provider.Level3 = request.Level3.Value;

    UpdateModified(provider);

    StatusType responseStatus =
      await _context.SaveChangesAsync() > 0
        ? StatusType.Valid
        : StatusType.ProviderUpdateFailed;

    ProviderResponse response = _mapper.Map<ProviderResponse>(provider);
    response.ResponseStatus = responseStatus;

    return response;
  }


  // create 20 test referrals with practice names == provider id
  //
  public async Task<ApiKeyStoreResponse> AddApiKeyStore(
    ApiKeyStoreRequest request)
  {
    try
    {
      var response = new ApiKeyStoreResponse();
      //No checks at this stage, just create a new object if the key is not
      //already being used
      var found =
        await _context.ApiKeyStore.SingleOrDefaultAsync(t =>
          t.Key == request.Key);
      if (found != null)
      {
        response.SetStatus(ValidationType.KeyApiKeyMismatch,
          $"key {request.Key}, is already in use");
        return response;
      }

      ApiKeyStore entity = _mapper.Map<Entities.ApiKeyStore>(request);
      entity.Id = Guid.NewGuid();
      _context.ApiKeyStore.Add(entity);

      var isValid = await _context.SaveChangesAsync() > 0;

      response = _mapper.Map<ApiKeyStoreResponse>(entity);

      response.SetStatus(
        isValid ? ValidationType.Valid : ValidationType.NotSet,
        isValid ? "" : "Unknown problem");

      return response;
    }
    catch (Exception)
    {
      throw;
    }
  }


  public async Task<Models.Referral[]> CreateTestReferralsAsync(
    int numToCreate = 20,
    bool withHistory = false,
    bool setBadContactNumbers = false,
    bool setAsRmcCall = false,
    bool skipExistingCheck = false)
  {
    if (withHistory)
    {
      return await CreateTestGPReferralsAndHistoryAsync(numToCreate,
        setBadContactNumbers, setAsRmcCall);
    }

    Guid providerId = User.GetUserId();
    bool exsitingTestReferrals = await _context.Referrals
      .AnyAsync(r => r.ReferringGpPracticeName == providerId.ToString());
    if (!skipExistingCheck && exsitingTestReferrals)
    {
      return Array.Empty<Models.Referral>();
    }

    Entities.Provider provider = await GetProviderByIdAsync(providerId);
    if (provider == null)
      throw new ProviderNotFoundException(providerId);

    List<string> providerLevels = new List<string>();
    if (provider.Level1)
      providerLevels.Add("1");
    if (provider.Level2)
      providerLevels.Add("2");
    if (provider.Level3)
      providerLevels.Add("3");

    if (!providerLevels.Any())
    {
      providerLevels.Add("1");
    }
    Random random = new Random();

    List<Entities.Referral> referrals =
      new List<Entities.Referral>(numToCreate);

    for (int i = 0; i < numToCreate; i++)
    {
      ReferralSource referralSource = i % 3 == 0
        ? i % 2 == 0
          ? ReferralSource.SelfReferral
          : ReferralSource.Pharmacy
        : ReferralSource.GpReferral;

      Entities.Referral referral = Helpers.RandomEntityCreator
        .CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now,
          modifiedByUserId: providerId,
          providerId: providerId,
          referringGpPracticeName: providerId.ToString(),
          status: ProviderAwaitingStart,
          triagedCompletionLevel:
            providerLevels[random.Next(providerLevels.Count)],
          ubrn: $"{GetUbrnPrefix(providerId, referralSource)}{(i + 1):d8}",
          referralSource: referralSource
        );

      // self-referrals have fixed properties
      if (referralSource == ReferralSource.SelfReferral)
      {
        referral.ReferringGpPracticeNumber =
          Constants.UNKNOWN_GP_PRACTICE_NUMBER;
        referral.IsVulnerable = null;
        referral.HasRegisteredSeriousMentalIllness = null;
      }

      // create some with null emails
      if (i % 8 == 0)
      {
        referral.Email = null;
      }

      if (setBadContactNumbers && i % 3 > 0)
      {
        referral.Mobile = Generators.GenerateTelephone(new Random());
        referral.Telephone = Generators.GenerateMobile(new Random());
      }
      else if (setBadContactNumbers)
      {
        referral.Mobile = null;
        referral.Telephone = Generators.GenerateMobile(new Random());
      }

      if (setAsRmcCall)
      {
        referral.Status = ReferralStatus.RmcCall.ToString();
      }

      referrals.Add(referral);
    }

    try
    {
      _context.Referrals.AddRange(referrals);
      await _context.SaveChangesAsync();

    }
    catch (Exception)
    {
      //Usual reason for failure here is column encryption
      return null;
    }

    return _mapper.Map<Entities.Referral[], Models.Referral[]>(
      referrals.ToArray());
  }

  private async Task<Models.Referral[]> CreateTestGPReferralsAndHistoryAsync(
    int numToCreate,
    bool setBadContactNumbers = false,
    bool setAsRmcCall = false)
  {
    Guid providerId = User.GetUserId();

    if (await _context.Referrals
      .AnyAsync(r => r.ReferringGpPracticeName == providerId.ToString()))
    {
      return Array.Empty<Models.Referral>();
    }

    Entities.Provider provider = await GetProviderByIdAsync(providerId);
    if (provider == null)
      throw new ProviderNotFoundException(providerId);

    List<string> providerLevels = new();
    if (provider.Level1)
      providerLevels.Add("1");
    if (provider.Level2)
      providerLevels.Add("2");
    if (provider.Level3)
      providerLevels.Add("3");

    if (!providerLevels.Any())
      providerLevels.Add("1");
    Random random = new();

    List<Entities.Referral> referrals = new(numToCreate);

    for (int i = 0; i < numToCreate; i++)
    {
      Entities.Referral referral = Helpers.RandomEntityCreator
        .CreateRandomReferral(
          modifiedAt: DateTimeOffset.Now.AddDays(-40),
          modifiedByUserId: providerId,
          providerId: providerId,
          referringGpPracticeName: providerId.ToString(),
          status: New,
          triagedCompletionLevel:
            providerLevels[random.Next(providerLevels.Count)],
          ubrn: $"{GetUbrnPrefix(providerId, ReferralSource.GpReferral)}" +
                $"{(i + 1):d8}",
          referralSource: ReferralSource.GpReferral
        );

      if (setBadContactNumbers && i % 3 > 0)
      {
        referral.Mobile = Generators.GenerateTelephone(new Random());
        referral.Telephone = Generators.GenerateMobile(new Random());
      }
      else if (setBadContactNumbers)
      {
        referral.Mobile = null;
        referral.Telephone = Generators.GenerateMobile(new Random());
      }

      if (setAsRmcCall)
      {
        referral.Status = ReferralStatus.RmcCall.ToString();
      }

      referrals.Add(referral);
    }

    try
    {
      _context.Referrals.AddRange(referrals);
      await _context.SaveChangesAsync();
    }
    catch (Exception)
    {
      //Usual reason for failure here is column encryption
      return Array.Empty<Models.Referral>();
    }

    //Add History to each
    //ProviderSelection
    foreach (Referral referral in referrals)
    {
      Referral entity = await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referral.Id);

      if (entity == null)
      {
        continue;
      }

      entity.DateOfProviderSelection = DateTimeOffset.Now.AddDays(-30);
      entity.ProviderId = providerId;
      entity.ModifiedAt = DateTimeOffset.Now.AddDays(-30);
      entity.ModifiedByUserId = providerId;
      entity.Status = ProviderAwaitingStart.ToString();

      await _context.SaveChangesAsync();
    }

    //Add Provider Started or Provider Rejected
    for (int i = 0; i < referrals.Count; i++)
    {
      Referral entity = await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referrals[i].Id);

      if (entity == null)
      {
        continue;
      }

      string status = i % 3 == 0
        ? i % 2 == 0
          ? ReferralStatus.Exception.ToString()
          : ProviderStarted.ToString()
        : ProviderRejected.ToString();

      entity.DateOfProviderSelection = DateTimeOffset.Now.AddDays(-20);
      entity.ProviderId = providerId;
      entity.ModifiedAt = DateTimeOffset.Now.AddDays(-20);
      entity.ModifiedByUserId = providerId;
      entity.Status = status;

      await _context.SaveChangesAsync();
    }

    //Reject to EReferrals
    foreach (Referral referral in referrals
      .Where(t => t.Status == ProviderRejected.ToString()))
    {
      Referral entity = await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referral.Id);

      if (entity == null)
      {
        continue;
      }

      entity.DateOfProviderSelection = DateTimeOffset.Now.AddDays(-19);
      entity.ProviderId = providerId;
      entity.ModifiedAt = DateTimeOffset.Now.AddDays(-19);
      entity.ModifiedByUserId = providerId;
      entity.Status = RejectedToEreferrals.ToString();
      entity.StatusReason = "Test Setup Rejected";

      await _context.SaveChangesAsync();
    }

    // Reject exceptions
    for (int i = 0; i < referrals.Count; i++)
    {
      if (referrals[i].Status == ReferralStatus.Exception.ToString())
      {
        Referral entity = await _context.Referrals
          .SingleOrDefaultAsync(t => t.Id == referrals[i].Id);

        if (entity == null)
        {
          continue;
        }

        entity.DateOfProviderSelection = DateTimeOffset.Now.AddDays(-19);
        entity.ProviderId = providerId;
        entity.ModifiedAt = DateTimeOffset.Now.AddDays(-19);
        entity.ModifiedByUserId = providerId;
        entity.Status = i % 3 == 0
          ? RejectedToEreferrals.ToString()
          : ProviderAwaitingStart.ToString();
        entity.StatusReason = "Test Setup Rejected";

        await _context.SaveChangesAsync();

      }
    }

    await _context.SaveChangesAsync();

    // add back in as new
    for (int i = 0; i < referrals.Count; i++)
    {
      if (referrals[i].Status == RejectedToEreferrals.ToString() &&
          i % 3 == 0)
      {
        Entities.Referral entity = Helpers.RandomEntityCreator
          .CreateRandomReferral(
            ubrn: referrals[i].Ubrn,
            nhsNumber: referrals[i].NhsNumber,
            familyName: referrals[i].FamilyName,
            givenName: referrals[i].GivenName,
            dateOfBirth: referrals[i].DateOfBirth ??
                         DateTimeOffset.Now.AddYears(-50),
            address1: referrals[i].Address1,
            postcode: referrals[i].Postcode,
            modifiedAt: DateTimeOffset.Now.AddDays(-10),
            modifiedByUserId: providerId,
            referringGpPracticeName: providerId.ToString(),
            status: New,
            triagedCompletionLevel:
            providerLevels[random.Next(providerLevels.Count)],
            referralSource: ReferralSource.GpReferral
          );


        referrals.Add(entity);

      }
    }

    await _context.SaveChangesAsync();

    return _mapper.Map<Entities.Referral[], Models.Referral[]>(
      referrals.ToArray());

  }

  private static IEnumerable<IServiceUserSubmissionRequest> GetCollatedUpdateRequestsByUbrn(
    IEnumerable<IServiceUserSubmissionRequest> requests)
  {
    IEnumerable<IGrouping<string, IServiceUserSubmissionRequest>> updateSubmissions = requests
      .Where(r => r.Type == UpdateType.Update.ToString())
      .GroupBy(r => r.Ubrn);

    List<IServiceUserSubmissionRequest> collatedUpdateSubmissions = new();

    foreach (IGrouping<string, IServiceUserSubmissionRequest> submissionBatch in updateSubmissions)
    {
      List<ServiceUserUpdatesRequest> updates = new();
      foreach (IServiceUserSubmissionRequest submission in submissionBatch)
      {
        updates.AddRange(submission.Updates);
      }

      ServiceUserSubmissionRequestV2 submissionRequest = new()
      {
        Type = UpdateType.Update.ToString(),
        Ubrn = submissionBatch.Key,
        Updates = updates
      };

      collatedUpdateSubmissions.Add(submissionRequest);
    }

    return collatedUpdateSubmissions;
  }

  private static string GetUbrnPrefix(
    Guid providerId,
    ReferralSource? referralSource)
  {
    string ubrn = (providerId.ToString().ToUpper()) switch
    {
      // Xyla Health & Wellbeing
      "2D11868B-6200-4C14-9F49-2A17D735A573" => "1110",
      // Ovivia
      "83D4F0C0-E010-4BEE-BE81-7E86AA9F48F6" => "2220",
      // Slimming World
      "1BE04438-6D16-4924-BAA0-8F2A9DA415E6" => "3330",
      // Liva
      "A7AFB83E-99C4-46A0-86B5-9CD688DD82CF" => "4440",
      // Morelife UK
      "CE235712-395B-449D-9118-AF4736EE1844" => "5550",
      // OurPath
      "05DA4135-9AD3-48B6-900C-FE31F4697835" => "6660",

      // TEMP PROVIDERS

      // OviviaIT
      "21E521D3-3EC4-4C4D-962A-61D003EB21B7" => "2221",
      // OviviaPTA
      "9AE1768C-BF1A-4FB0-965F-2B22D27C7BE0" => "2222",

      // Slimming World 2
      "98D0A6C6-3A0F-4342-BEC4-9B6A2372836B" => "3331",
      // Slimming World 3
      "B2C6F72E-C256-42D1-98A1-76AAC522D819" => "3332",

      // Skyrim Runners
      "AF432ECF-6BF2-461D-AD5B-80701103699B" => "7771",
      // Royston Vasey
      "1EA6450D-7506-4A9C-944C-91F328CB2083" => "7772",
      // Mordroc Castle Fitness
      "EBE236FA-D293-41EC-9B34-9F1BFAE992CE" => "7773",
      // StoVoKor
      "E778091E-E5B7-47B0-90D2-E28D6236A5F5" => "7774",
      // Blue Sun
      "B1530C62-9AB2-4043-9C79-F82A2A32897B" => "7775",

      _ => "0000", // Should not be reached
    };

    if (referralSource == ReferralSource.SelfReferral)
    {
      ubrn = "SR" + ubrn.Substring(2, 2);
    }
    else if (referralSource == ReferralSource.Pharmacy)
    {
      ubrn = "PR" + ubrn.Substring(2, 2);
    }
    else if (referralSource == ReferralSource.GeneralReferral)
    {
      ubrn = "GR" + ubrn.Substring(2, 2);
    }
    else if (referralSource == ReferralSource.GpReferral)
    {
      ubrn = "GP" + ubrn.Substring(2, 2);
    }

    return ubrn;
  }


  // delete the 20 test referrals beginning with UBRNs beginning with first
  // 3 chars from provider Id if they are present
  public async Task<bool> DeleteTestReferralsAsync()
  {
    Guid providerId = User.GetUserId();

    List<Referral> existingReferrals = await _context
      .Referrals
      .Where(r => r.ReferringGpPracticeName == providerId.ToString())
      .ToListAsync();

    if (existingReferrals.Any())
    {
      List<Entities.ReferralAudit> existingReferralAudits = await _context
        .ReferralsAudit
        .Where(x => existingReferrals.Select(x => x.Id).Contains(x.Id))
        .ToListAsync();

      _context.ReferralsAudit.RemoveRange(existingReferralAudits);
      await _context.SaveChangesAsync();

      _context.Referrals.RemoveRange(existingReferrals);
      await _context.SaveChangesAsync();
      return true;
    }
    else
    {
      return false;
    }
  }

  public async Task<int> DeleteTestUserActionAsync()
  {
    List<Entities.UserStore> usersToDelete = await _context
      .UsersStore
      .Where(t => t.Domain.Contains(",||**TEST**||"))
      .ToListAsync();

    _context.UsersStore.RemoveRange(usersToDelete);

    List<Entities.UserActionLog> userLogsToDelete = _context
      .UserActionLogs
      .AsEnumerable()
      .Where(t => t.IpAddress == "::1,||**TEST**||")
      .ToList();

    _context.RemoveRange(userLogsToDelete);

    await _context.SaveChangesAsync();

    return userLogsToDelete.Count;
  }

  public async Task<int> CreateTestUserActionAsync()
  {
    await AddUserStoreAsync();
    return await AddUserActionLogsAsync();
  }

  private async Task AddUserStoreAsync()
  {
    var stores = new Entities.UserStore[]
    {
      new Entities.UserStore()
      {
        Id = Guid.NewGuid(),
        ApiKey = Guid.NewGuid().ToString(),
        IsActive = true,
        OwnerName = "Test User 1",
        Domain = "Rmc.Ui,||**TEST**||"
      },
      new Entities.UserStore()
      {
        Id = Guid.NewGuid(),
        ApiKey = Guid.NewGuid().ToString(),
        IsActive = true,
        OwnerName = "Test User 2",
        Domain = "Rmc.Ui,||**TEST**||"
      },
      new Entities.UserStore()
      {
        Id = Guid.NewGuid(),
        ApiKey = Guid.NewGuid().ToString(),
        IsActive = true,
        OwnerName = "Test User 3",
        Domain = "Rmc.Ui,||**TEST**||"
      },
      new Entities.UserStore()
      {
        Id = Guid.NewGuid(),
        ApiKey = Guid.NewGuid().ToString(),
        IsActive = true,
        OwnerName = "Test User 4",
        Domain = "Rmc.Ui,||**TEST**||"
      },
      new Entities.UserStore()
      {
        Id = Guid.NewGuid(),
        ApiKey = Guid.NewGuid().ToString(),
        IsActive = true,
        OwnerName = "Test User 5",
        Domain = "Rmc.Ui,||**TEST**||"
      }
    };
    _context.UsersStore.AddRange(stores);
    await _context.SaveChangesAsync();
  }

  private async Task<int> AddUserActionLogsAsync()
  {
    int currentDays = 1;
    int maxDays = 50;
    int maxLoopsPerDay = 210;
    int currentLoop = 0;
    while (currentDays < maxDays)
    {
      while (currentLoop < maxLoopsPerDay)
      {
        foreach (var action in _actions)
        {
          DateTimeOffset requestedAt =
            DateTimeOffset.UtcNow.AddDays(-currentDays);
          var userId = GetRandonUserId(new Random());
          var userActionLog = new UserActionLog
          {
            Action = action.Key,
            Method = action.Value,
            Controller = "Rmc",
            IpAddress = "::1,||**TEST**||",
            RequestAt = requestedAt,
            UserId = userId
          };
          if (action.Value == "POST")
          {
            userActionLog.Request =
              $"{_route}/{action.Key}" +
              $"|StatusReason={GetRandomStatusReason(new Random())}" +
              $"&DelayReason={GetRandomDelayReason(new Random())}" +
              $"&{ParameterList}";
          }
          else
          {
            userActionLog.Request =
              $"{_route}/{action.Key}?NotRequiredAsPartOfTest";
          }

          _context.UserActionLogs.Add(userActionLog);
          await _context.SaveChangesAsync();

          currentLoop++;
        }
      }

      currentDays++;
      currentLoop = 0;
    }

    var logs = await _context.UserActionLogs.ToListAsync();
    var temp = logs.Where(t => t.IpAddress == "::1,||**TEST**||").ToList();
    var json = JsonConvert.SerializeObject(temp);
    return logs.Count(t => t.IpAddress == "::1,||**TEST**||");
  }

  private Guid GetRandonUserId(Random random)
  {
    var r = random.Next(0, 4);
    var store = _context.UsersStore.ToArray()[r];
    return store.Id;
  }

  private static string GetRandomDelayReason(Random random)
  {
    string[] delayReasons = new[]
    {
      "", "A Test Delay Reason", "Other Reason", "Not Blank", "Test Delay"
    };
    return delayReasons[random.Next(0, delayReasons.Length)];
  }

  private static string GetRandomStatusReason(Random random)
  {
    string[] statusReasons = new[]
    {
      "Service user has not responded within 28 days of provider " +
      "initiating first contact",
      "Service user was unaware of the referral and does not wish " +
      "to engage with the programme",
      "Service user has requested to withdraw from the programme",
      "Service user's BMI has fallen below 30",
      "Service user contact details unavailable/invalid",
      "Service user does not have any digital capability or capacity",
      "Service user would like to join a face-to-face programme instead",
      "Language barrier",
      "Service user does not have a diagnosis of diabetes type 1, type 2," +
      " or hypertension",
      "Service user's registration information indicates they meet one of" +
      " the exclusion criteria",
      "Service user has a learning disability",
      "Service user's physical health capacity",
      "Service user's BMI is below the threshold for their ethnicity",
      "Service user failed to respond after several contact attempts by RMC",
      "Service user has selected the wrong provider",
      ""
    };

    return statusReasons[random.Next(0, statusReasons.Length)];
  }

  private string _route =
    "https://app-fake-server-uks-pre-1.azurewebsites.net/Rmc";

  private string[] _parameter = new string[]
  {
    "DelayUntil=7%2F1%2F2022+12%3A00%3A00+AM+%2B00%3A00",
    "MinGpReferralAge=18",
    "MaxGpReferralAge=110",
    "IsVulnerable=True",
    "IsException=False",
    "DateOfReferral=3%2F20%2F2022+12%3A00%3A00+AM+%2B00%3A00",
    "DateOfBirth=1%2F1%2F1980+12%3A00%3A00+AM+%2B00%3A00",
    "Id=366496fd-5af8-ec11-b47a-0003ffd65c7a",
    "Ubrn=966600070082",
    "NhsNumber=2785141202",
    "DisplayDateOfReferral=20%2F03%2F2022",
    "GivenName=Test2", "FamilyName=BMI",
    "DelayFrom=2022-06-30T10%3A57%3A37.892%2B00%3A00",
    "DelayUntil=2022-07-01T00%3A00%3A00.000%2B00%3A00",
    "DisplayDateOfBirth=01%2F01%2F1980",
    "DateOfBirthDay=1", "DateOfBirthMonth=1",
    "DateOfBirthYear=1980",
    "Status=RmcDelayed",
    "VulnerableDescription=Vulnerable",
    "Mobile=%2B447823551523",
    "Telephone=%2B447823551523",
    "Address1=Address1", "Address2=Address2", "Address3=",
    "Postcode=ST4+4LX", "ReferringGpPracticeName=TestGpPractice",
    "EmailReadOnly=test2.bmi%40testcsu.com",
    "confirm-add-to-call-list=Confirm",
    "__RequestVerificationToken=FakeToken"
  };

  private string ParameterList => string.Join("&", _parameter);

  private readonly Dictionary<string, string> _actions = new()
  {
    { "AddToRmcCallList", "POST" },
    { "ConfirmDelay", "POST" },
    { "ConfirmEmail", "POST" },
    { "ConfirmEthnicity", "POST" },
    { "ConfirmProvider", "POST" },
    { "ExceptionList", "GET" },
    { "Index", "GET" },
    { "PreviouslyDelayedList", "GET" },
    { "ProviderInfo", "GET" },
    { "ReferralList", "GET" },
    { "ReferralView", "GET" },
    { "RejectionList", "GET" },
    { "RejectToEreferrals", "POST" },
    { "SelectProvider", "POST" },
    { "UnableToContact", "POST" },
    { "UpdateDateOfBirth", "POST" },
    { "UpdateMobileNumber", "POST" },
    { "VulnerableList", "GET" }
  };

  public virtual async Task<bool> UpdateProviderAuthAsync(
    ProviderAuthUpdateRequest request)
  {
    ValidateModelResult result = ValidateModel(request);

    if (!result.IsValid)
      throw new ArgumentException(
        $"Validation had the following error(s):{result.GetErrorMessage()}");

    Entities.Provider entity =
      await _context.Providers.Include(t => t.ProviderAuth)
       .SingleOrDefaultAsync(t => t.Id == request.ProviderId && t.IsActive);
    if (entity == null)
      throw new ArgumentNullException(
        $"Provider not found with ID {request.ProviderId}");

    entity.ProviderAuth ??= new Entities.ProviderAuth();
    entity.ProviderAuth.IsActive = true;

    Entities.ProviderAuth auth = entity.ProviderAuth;
    if (request.KeyViaEmail)
    {
      auth.KeyViaEmail = true;
      auth.EmailContact = request.EmailContact;
    }
    else
    {
      auth.KeyViaEmail = false;
      auth.EmailContact = string.Empty;
    }

    if (request.KeyViaSms)
    {
      auth.KeyViaSms = true;
      auth.MobileNumber = request.MobileNumber;
    }
    else
    {
      auth.KeyViaSms = false;
      auth.MobileNumber = string.Empty;
    }

    auth.IpWhitelist = request.IpWhitelist;

    if (!string.IsNullOrWhiteSpace(request.IpRange))
      if (string.IsNullOrWhiteSpace(auth.IpWhitelist))
        auth.IpWhitelist = request.IpRange;
      else
        auth.IpWhitelist += $",{request.IpRange}";

    UpdateModified(auth);

    try
    {

      return await _context.SaveChangesAsync() > 0;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public virtual async Task<ReferralStatusReason[]> GetReferralStatusReasonsAsync()
      => await GetReferralStatusReasonsAsync(null);

  public virtual async Task<ReferralStatusReason[]> GetReferralStatusReasonsByGroupAsync(
    ReferralStatusReasonGroup group)
    => await GetReferralStatusReasonsAsync(x => x.Groups.HasFlag(group));

  private async Task<ReferralStatusReason[]> GetReferralStatusReasonsAsync(
      Expression<Func<Entities.ReferralStatusReason, bool>> filter)
  {
    _referralStatusReasons ??= await _context
      .ReferralStatusReasons
      .Where(x => x.IsActive)
      .ToListAsync();

    IQueryable<Entities.ReferralStatusReason> query = _referralStatusReasons.AsQueryable();

    query = filter == null
      ? query.Where(x =>
        x.Groups.HasFlag(ReferralStatusReasonGroup.ProviderDeclined)
        || x.Groups.HasFlag(ReferralStatusReasonGroup.ProviderRejected)
        || x.Groups.HasFlag(ReferralStatusReasonGroup.ProviderTerminated))
      : query.Where(filter);

    ReferralStatusReason[] referralStatusReasons = query
      .ProjectTo<ReferralStatusReason>(_mapper.ConfigurationProvider)
      .ToArray();

    return referralStatusReasons;
  }

  public virtual async Task<ReferralStatusReasonResponse>
    SetNewRejectionReasonAsync(ReferralStatusReasonRequest newReason)
  {
    bool doesNewReasonExist = await _context
      .ReferralStatusReasons
      .AnyAsync(x => x.Description
        .Equals(newReason.Description, StringComparison.Ordinal));

    if (doesNewReasonExist)
    {
      throw new ProviderRejectionReasonAlreadyExistsException(
        $"A provider rejection reason with a description of " +
        $"'{newReason.Description}' already exists.");
    }

    Entities.ReferralStatusReason entity =
      _mapper.Map<Entities.ReferralStatusReason>(newReason);

    UpdateModified(entity);
    entity.IsActive = true;

    _context.ReferralStatusReasons.Add(entity);

    StatusType responseStatus = await _context.SaveChangesAsync() > 0
      ? StatusType.Valid
      : StatusType.ProviderRejectionResponseInsertFailed;

    ReferralStatusReasonResponse response =
      _mapper.Map<ReferralStatusReasonResponse>(entity);

    response.ResponseStatus = responseStatus;

    return response;
  }

  public async Task<IReferral> GetReferralStatusAndSubmissions(string ubrn)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentException(
        $"'{nameof(ubrn)}' cannot be null or whitespace.", nameof(ubrn));
    }

    Models.Referral referral = await _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.ProviderUbrn == ubrn)
      .Where(r => r.ProviderId == User.GetUserId())
      .Select(r => new Models.Referral()
      {
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateStartedProgramme = r.DateStartedProgramme,
        DateCompletedProgramme = r.DateCompletedProgramme,
        DateOfProviderContactedServiceUser =
          r.DateOfProviderContactedServiceUser,
        Status = r.Status,
        Ubrn = r.ProviderUbrn,
        ProviderSubmissions = r.ProviderSubmissions
          .Select(ps => new ProviderSubmission()
          {
            Coaching = ps.Coaching,
            Date = ps.Date,
            Measure = ps.Measure,
            SubmissionDate = ps.ModifiedAt,
            Weight = ps.Weight
          })
          .OrderBy(ps => ps.SubmissionDate)
          .ToList()
      })
      .FirstOrDefaultAsync();

    return referral;
  }

  public async Task<Models.Referral[]> CreateTestCompleteReferralsAsync(
    int num = 20,
    int providerSelectedAddDays = -50,
    bool notStarted = true,
    int startedProgrammeAddDays = -50,
    string referralStatus = "CancelledByEreferrals",
    bool allRandom = false)
  {
    Entities.Provider provider =
      await _context.Providers.FirstAsync(t => t.IsActive);

    List<Entities.Referral> referrals = new();
    if (provider == null)
      throw new ProviderNotFoundException();

    Random random = new Random();
    int count = allRandom ? num : 200;
    for (int i = 0; i < count; i++)
    {
      string ubrn1 = Generators.GenerateUbrn(random);
      bool ubrnsUnique = false;
      ReferralSource source = ReferralSource.SelfReferral;
      do
      {
        if (allRandom)
        {
          if (i % 6 == 0)
          {
            source = ReferralSource.Msk;
            ubrn1 = Generators.GenerateUbrnMsk(random);
          }
          else if (i % 5 == 0)
          {
            source = ReferralSource.Pharmacy;
            ubrn1 = Generators.GenerateUbrn(random);
          }
          else if (i % 4 == 0)
          {
            source = ReferralSource.GpReferral;
            ubrn1 = Generators.GenerateUbrnGp(random);
          }
          else if (i % 3 == 0)
          {
            source = ReferralSource.GeneralReferral;
            ubrn1 = Generators.GenerateUbrnGeneral(random);
          }
          else
          {
            source = ReferralSource.SelfReferral;
            ubrn1 = Generators.GenerateUbrnSelf(random);
          }
        }
        ubrnsUnique = _context.Referrals
          .Count(t => t.Ubrn == ubrn1) == 0;

      } while (!ubrnsUnique);

      ReferralStatus status = allRandom ?
        i % 2 == 0 ? Complete : CancelledByEreferrals :
        referralStatus == "Complete" ? Complete : CancelledByEreferrals;

      DateTime providerSelectedDate = allRandom ?
        DateTime.UtcNow.AddDays(-(i + 30)) :
        DateTime.UtcNow.AddDays(providerSelectedAddDays);

      DateTime dateStarted = allRandom ?
        DateTime.UtcNow.AddDays(-(i + 28)) :
        DateTime.UtcNow.AddDays(startedProgrammeAddDays);

      Entities.Referral referral =
        Helpers.RandomEntityCreator.CreateRandomReferral(
          id: Guid.NewGuid(),
          ubrn: ubrn1,
          dateOfProviderSelection: providerSelectedDate,
          dateStartedProgramme:
            i % 6 == 0 ? providerSelectedDate.AddDays(2) : null,
          modifiedByUserId: Guid.NewGuid(),
          providerId: provider.Id,
          referringGpPracticeName: Generators.GenerateName(random, 10),
          status: status,
          referralSource: source,
          delayReason: "ReEntry"
        );

      bool nhsIsUnique = false;
      do
      {
        referral.NhsNumber = Generators.GenerateNhsNumber(random);
        nhsIsUnique = _context.Referrals
          .Count(t => t.NhsNumber == referral.NhsNumber) == 0;
      } while (!nhsIsUnique);

      Referral referralNew = referral.ShallowCopy();
      referralNew.Id = Guid.NewGuid();
      referralNew.Ubrn = ubrn1;
      referralNew.NhsNumber = null;
      referralNew.Status = ProviderAwaitingTrace.ToString();
      referralNew.DateOfReferral = DateTime.UtcNow.AddDays(-5);
      referralNew.DateOfProviderSelection = DateTime.UtcNow.AddDays(-4);
      referralNew.DateStartedProgramme = null;
      referralNew.StatusReason =
        $"CreateTestReEntryReferrals New NHS Number {referral.NhsNumber}";

      Referral found =
        await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referral.Id);

      Referral found2 =
       await _context.Referrals
       .SingleOrDefaultAsync(t => t.Id == referralNew.Id);

      if (found == null && found2 == null)
      {
        _context.Referrals.Add(referral);
        if (i % 2 == 0)
        {
          _context.Referrals.Add(referralNew);
        }
        await _context.SaveChangesAsync();
        referrals.Add(referralNew);
      }
    }

    return _mapper.Map<Entities.Referral[], Models.Referral[]>(
      referrals.ToArray());
  }

}
