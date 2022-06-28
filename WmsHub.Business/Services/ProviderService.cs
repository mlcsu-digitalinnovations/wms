using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.ProviderRejection;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Business.Enums.ReferralStatus;
using ApiKeyStore = WmsHub.Business.Entities.ApiKeyStore;
using Provider = WmsHub.Business.Models.Provider;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Services
{
  public class ProviderService
    : ServiceBase<Entities.Referral>, IProviderService
  {
    private readonly IMapper _mapper;
    private readonly ProviderOptions _options;

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

      Entities.Provider provider = await _context
       .Providers
       .Where(r => r.IsActive)
       .SingleOrDefaultAsync(t => t.Id == providerId);

      if (provider == null) throw new ProviderNotFoundException(providerId);

      provider.ProviderSubmissions.AddRange(request.Submissions);

      if (request.ReferralStatus != null)
      {
        Referral referral = await _context.Referrals
          .SingleOrDefaultAsync(t => t.Id == referralId);

        if (referral == null) throw new ReferralNotFoundException(referralId);

        ValidateStatusBeforeUpdate(referral.Status, request.ReferralStatus);

        if (request.ProgrammeOutcome != null)
          referral.ProgrammeOutcome = request.ProgrammeOutcome;

        if (request.DateCompletedProgramme != null)
          referral.DateCompletedProgramme = request.DateCompletedProgramme;

        if (request.DateStartedProgramme != null)
          referral.DateStartedProgramme = request.DateStartedProgramme;

        if (request.DateOfProviderContactedServiceUser != null)
          referral.DateOfProviderContactedServiceUser =
            request.DateOfProviderContactedServiceUser;

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

        // All non-GP referrals should have their statuses updated to Complete
        // immediately after ProviderCompleted so they are not added to the 
        // discharge list
        if (request.ReferralStatus == ProviderCompleted &&
          referral.ReferralSource != ReferralSource.GpReferral.ToString())
        {
          await _context.SaveChangesAsync();

          referral.Status = Complete.ToString();
          UpdateModified(referral);
        }
      }

      return await _context.SaveChangesAsync() > 0;
    }

    private void ValidateStatusBeforeUpdate(string referralStatus,
      ReferralStatus? requestStatus)
    {

      string cannotBeSet = $"cannot have it's status set to {requestStatus} " +
        $"because it's current status is {referralStatus}";

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

    protected internal virtual async Task<Entities.Referral>
      GetProviderReferralByUbrn(Guid providerId, string ubrn)
    {
      Entities.Referral referral = await _context.Referrals
        .Where(r => r.IsActive)
        .Where(r => r.ProviderId == providerId)
        .FirstOrDefaultAsync(r => r.Ubrn == ubrn);

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
    public virtual async Task<int> GetNumOfProvidersAvailableAtTriageLevelAsync(
      TriageLevel triageLevel)
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

    public virtual async Task<Guid> ValidateProviderKeyAsync(
      string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentNullException("Provider ApiKey must be provided");

      Entities.Provider provider =
        await _context.Providers
         .FirstOrDefaultAsync(t =>
            t.ApiKey == key && t.ApiKeyExpires.Value > DateTime.UtcNow);
      return provider?.Id ?? Guid.Empty;
    }

    public virtual async Task<IEnumerable<ServiceUser>> GetServiceUsers()
    {
      IQueryable<Referral> query = _context.Referrals
        .AsNoTracking()
        .Where(r => r.IsActive)
        .Where(r => r.ProviderId == User.GetUserId())
        .Where(r => r.Status == ProviderAwaitingStart.ToString());

      IEnumerable<ServiceUser> serviceUsers =
        await query.ProjectTo<ServiceUser>(_mapper.ConfigurationProvider)
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
        .SingleOrDefaultAsync(t => t.Id == id);

      if (provider == null) throw new ProviderNotFoundException(id);

      ProviderResponse response = _mapper.Map<ProviderResponse>(provider);

      return response;
    }

    public virtual async Task<string> GetProviderNameAsync(Guid id)
    {
      string providerName = await _context.Providers
        .Where(p => p.Id == id)
        .Select(p => p.Name)
        .SingleOrDefaultAsync();

      if (providerName == null) throw new ProviderNotFoundException(id);

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
          NoOfGeneralReferrals = p.Referrals
            .Where(r => r.ReferralSource == ReferralSource
              .GeneralReferral.ToString())
            .Count(r => statuses.Contains(r.Status)),
          NoOfGpReferrals = p.Referrals
            .Where(r => r.ReferralSource == ReferralSource
              .GpReferral.ToString())
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

    public virtual async Task<IEnumerable<Provider>> GetProvidersAsync(
      TriageLevel triageLevel)
    {
      IQueryable<Entities.Provider> query = _context.Providers
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
        .ProjectTo<Provider>(_mapper.ConfigurationProvider)
        .ToListAsync();

      providers = providers.Randomize();

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
      ProviderSubmissionsAsync(
        IEnumerable<IServiceUserSubmissionRequest> requests)
    {
      if (requests == null)
        throw new ArgumentNullException(nameof(requests));

      if (!requests.Any())
        throw new ArgumentOutOfRangeException(nameof(requests));

      List<ServiceUserSubmissionResponse> responses = new(requests.Count());

      string providerName = await GetProviderNameAsync(User.GetUserId());

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        ServiceUserSubmissionResponse response = new();

        ValidateModelResult result = ValidateModel(request);

        if (result.IsValid)
        {
          Referral referral = await GetProviderReferralByUbrn(
            User.GetUserId(), 
            request.Ubrn);

          if (referral == null)
          {
            response.SetStatus(
              StatusType.Invalid,
              $"UBRN {request.Ubrn} for provider " +
              $"{providerName} not found.");
          }
          else
          {
            ProviderSubmissionRequest providerSubmissionRequest = new (
              request, 
              User.GetUserId(), 
              referral.Id);

            if (referral.DateOfProviderSelection == null)
            {
              response.SetStatus(StatusType.Invalid,
                $"UBRN {request.Ubrn} for provider {providerName} " +
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
                  $"UBRN {request.Ubrn} for provider {providerName} " +
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
                  $"UBRN {request.Ubrn} for provider {providerName} " +
                  $"can no longer receive updates because the last date " +
                  $"for submissions was {maxSubmissionDate:yyyy-MM-dd}.");
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
                    $"UBRN {request.Ubrn} for provider {providerName} " +
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
                    $"UBRN {request.Ubrn} for provider {providerName} " +
                    "cannot receive updates because it's status is " +
                    $"{referral.Status}.");

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
                $"UBRN {request.Ubrn} for provider {providerName} " +
                $"{e.Message}.");
            }
          }
        }
        else
        {
          response.SetStatus(
            StatusType.Invalid,
            $"UBRN {request.Ubrn} for provider {providerName} " +
            $"{result.GetErrorMessage()}.");
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
      Entities.Provider provider =
        await GetProviderByIdAsync(request.Id);

      if (request.Level1 != null && provider.Level1 != request.Level1)
        provider.Level1 = request.Level1.Value;

      if (request.Level1 != null && provider.Level2 != request.Level2)
        provider.Level2 = request.Level2.Value;

      if (request.Level1 != null && provider.Level3 != request.Level3)
        provider.Level3 = request.Level3.Value;
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

      Ganss.XSS.HtmlSanitizer sanitizer = new Ganss.XSS.HtmlSanitizer();

      if (provider.Summary != sanitizer.Sanitize(request.Summary))
        provider.Summary = sanitizer.Sanitize(request.Summary);

      if (provider.Summary2 != sanitizer.Sanitize(request.Summary2))
        provider.Summary2 = sanitizer.Sanitize(request.Summary2);

      if (provider.Summary3 != sanitizer.Sanitize(request.Summary3))
        provider.Summary3 = sanitizer.Sanitize(request.Summary3);

      if (!string.IsNullOrEmpty(request.Website)
        && provider.Website != request.Website)
        provider.Website = request.Website;

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
    public async Task<ApiKeyStoreResponse> AddApiKeyStore(ApiKeyStoreRequest request)
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


    public async Task<bool> CreateTestReferralsAsync(int numToCreate = 20,
      bool withHistory = false)
    {
      if (withHistory)
      {
        return await CreateTestGPReferralsAndHistoryAsync(numToCreate);
      }
      Guid providerId = User.GetUserId();

      if (await _context.Referrals
        .AnyAsync(r => r.ReferringGpPracticeName == providerId.ToString()))
      {
        return false;
      }

      Entities.Provider provider = await GetProviderByIdAsync(providerId);
      if (provider == null)
        throw new ProviderNotFoundException(providerId);

      List<string> providerLevels = new List<string>();
      if (provider.Level1) providerLevels.Add("1");
      if (provider.Level2) providerLevels.Add("2");
      if (provider.Level3) providerLevels.Add("3");

      if(!providerLevels.Any())
        providerLevels.Add("1");
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
        return false;
      }

      return true;

    }

    private async Task<bool> CreateTestGPReferralsAndHistoryAsync(
      int numToCreate)
    {
      Guid providerId = User.GetUserId();

      if (await _context.Referrals
        .AnyAsync(r => r.ReferringGpPracticeName == providerId.ToString()))
      {
        return false;
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
        return false;
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
        .Where(t=>t.Status == ProviderRejected.ToString()))
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

      return true;

    }

    private static string GetUbrnPrefix(Guid providerId, 
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

      return ubrn;
    }


    // delete the 20 test referrals beginning with UBRNs beginning with first
    // 3 chars from provider Id if they are present
    public async Task<bool> DeleteTestReferralsAsync()
    {
      Guid providerId = User.GetUserId();

      List<Entities.Referral> existingReferrals = await _context.Referrals
        .Where(r => r.ReferringGpPracticeName == providerId.ToString())
        .ToListAsync();

      if (existingReferrals.Any())
      {
        _context.Referrals.RemoveRange(existingReferrals);
        return true;
      }
      else
      {
        return false;
      }
    }

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

    public virtual async Task<ProviderRejectionReason[]>
      GetRejectionReasonsAsync()
    {
      Entities.ProviderRejectionReason[] entities =
        await _context.ProviderRejectionReasons
          .Where(t => t.IsActive).ToArrayAsync();

      if (entities == null || !entities.Any())
        throw new ArgumentNullException(
          "No active Provider Rejection reasons found");

      ProviderRejectionReason[] models =
        _mapper.Map<ProviderRejectionReason[]>(entities);

      return models;

    }

    public virtual async Task<ProviderRejectionReasonResponse> 
      SetNewRejectionReasonsAsync(ProviderRejectionReasonSubmission reason)
    {
      Entities.ProviderRejectionReason found =
        await _context.ProviderRejectionReasons.FirstOrDefaultAsync(t =>
          t.Title.ToLower() == reason.Title.ToLower());

      if (found != null)
        throw new ProviderRejectionReasonAlreadyExistsException(
          $"Provider Rejection with title '{reason.Title}' already exists.");

      Entities.ProviderRejectionReason entity =
        _mapper.Map<Entities.ProviderRejectionReason>(reason);

      UpdateModified(entity);
      entity.IsActive = true;

      _context.ProviderRejectionReasons.Add(entity);
      StatusType responseStatus =
        await _context.SaveChangesAsync() > 0
          ? StatusType.Valid
          : StatusType.ProviderRejectionResponseInsertFailed;


      ProviderRejectionReasonResponse response =
        _mapper.Map<ProviderRejectionReasonResponse>(entity);

      response.ResponseStatus = responseStatus;

      return response;
    }

    public virtual async Task<ProviderRejectionReasonResponse> 
      UpdateRejectionReasonsAsync(ProviderRejectionReasonUpdate reason)
    {
      Entities.ProviderRejectionReason entity = null!;

      if (reason.Id != null && reason.Id != Guid.Empty)
      {
        entity = await _context.ProviderRejectionReasons
          .FirstOrDefaultAsync(t => t.Id == reason.Id);

        if (entity == null)
          throw new ProviderRejectionReasonDoesNotExistException(
            $"Provider rejection reason with title '{reason.Title}' does " +
            "not exist.");

      } 
      else if (!string.IsNullOrWhiteSpace(reason.Title))
      {
        entity = await _context.ProviderRejectionReasons.FirstOrDefaultAsync(
          t => t.Title.ToLower() == reason.Title.ToLower());

        if (entity == null)
          throw new ProviderRejectionReasonDoesNotExistException(
            $"Provider rejection reason with title '{reason.Title}' does " +
            $"not exist.");

        if (!entity.Title.Equals(reason.Title,
          StringComparison.CurrentCultureIgnoreCase))
          throw new ProviderRejectionReasonMismatchException(
            $"Provider rejection reason with title '{reason.Title}' does not " +
            $"match the existing title of '{entity.Title}'. The 'Title' " +
            "field is read only and cannot be updated.");
      }

      bool isUpdated = false;

      if (reason.IsActive != null && entity.IsActive != reason.IsActive)
      {
        entity.IsActive = reason.IsActive.Value;
        isUpdated = true;
      }

      if (!string.IsNullOrWhiteSpace(reason.Description)
          && !entity.Description.Equals(reason.Description))
      {
        entity.Description = reason.Description;
        isUpdated = true;
      }

      if (isUpdated)
      {
        UpdateModified(entity);
        StatusType responseStatus =
          await _context.SaveChangesAsync() > 0
            ? StatusType.Valid
            : StatusType.ProviderRejectionResponseUpdateFailed;

        ProviderRejectionReasonResponse response =
          _mapper.Map<ProviderRejectionReasonResponse>(entity);

        response.ResponseStatus = responseStatus;

        return response;
      }

      ProviderRejectionReasonResponse model =
        _mapper.Map<ProviderRejectionReasonResponse>(entity);

      model.SetStatus(StatusType.NoRowsUpdated, "Nothing was updated");

      return model;
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
        .Where(r => r.Ubrn == ubrn)
        .Where(r => r.ProviderId == User.GetUserId())
        .Select(r => new Models.Referral()
        {
          DateOfProviderSelection = r.DateOfProviderSelection,
          DateStartedProgramme = r.DateStartedProgramme,
          DateCompletedProgramme = r.DateCompletedProgramme,
          DateOfProviderContactedServiceUser =
            r.DateOfProviderContactedServiceUser,
          Status = r.Status,
          Ubrn = r.Ubrn,
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
  }
}