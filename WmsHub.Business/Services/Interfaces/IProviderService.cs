using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralStatusReason;

namespace WmsHub.Business.Services.Interfaces;
public interface IProviderService : IServiceBase
{
  Task<ApiKeyStoreResponse> AddApiKeyStore(
    ApiKeyStoreRequest request);
  Task<bool> AddProviderSubmissionsAsync(
    ProviderSubmissionRequest request,
    Guid referralId);
  Task<Referral[]> CreateTestCompleteReferralsAsync(
    int num = 20,
    int providerSelectedAddDays = -50,
    bool notStarted = true,
    int startedProgrammeAddDays = -50,
    string referralStatus = "CancelledByEreferrals",
    bool allRandom = false);
  Task<Referral[]> CreateTestReferralsAsync(
    int numToCreate = 20,
    bool withHistory = false,
    bool setBadContactNumbers = false,
    bool setAsRmcCall = false,
    bool skipExistingCheck = false);
  Task<int> CreateTestUserActionAsync();
  Task<bool> DeleteTestReferralsAsync();
  Task<int> DeleteTestUserActionAsync();
  Task<ProviderAdminResponse> GetAllActiveProvidersAsync();
  Task<int> GetNumOfProvidersAvailableAtTriageLevelAsync(
    TriageLevel triageLevel);
  Task<ProviderResponse> GetProviderAsync(Guid id);
  Task<string> GetProviderNameAsync(Guid id);
  Task<IEnumerable<Provider>> GetProvidersAsync(int triageLevelValue);
  Task<IEnumerable<Provider>> GetProvidersAsync(TriageLevel triageLevel);
  Task<IEnumerable<ProviderInfo>> GetProvidersInfo();
  Task<IReferral> GetReferralStatusAndSubmissions(string ubrn);
  Task<ReferralStatusReason[]> GetReferralStatusReasonsAsync();
  Task<ReferralStatusReason[]> GetReferralStatusReasonsByGroupAsync(
    ReferralStatusReasonGroup group);
  Task<IEnumerable<ServiceUser>> GetServiceUsers();
  Task<IEnumerable<ServiceUserSubmissionResponse>> ProviderSubmissionsAsync(
    IEnumerable<IServiceUserSubmissionRequest> requests);
  Task<ReferralStatusReasonResponse> SetNewRejectionReasonAsync(
    ReferralStatusReasonRequest newReason);
  Task<bool> UpdateProviderAuthAsync(ProviderAuthUpdateRequest request);
  Task<NewProviderApiKeyResponse> UpdateProviderKeyAsync(
    ProviderResponse providerResponse, int validDays = 365);
  Task<ProviderResponse> UpdateProviderLevelsAsync
    (ProviderLevelStatusChangeRequest request);
  Task<ProviderResponse> UpdateProvidersAsync(
    ProviderRequest request);
  Task<Guid> ValidateProviderKeyAsync(string apiKey);
}