using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.ProviderRejection;
using WmsHub.Business.Models.ProviderService;

namespace WmsHub.Business.Services
{
  public interface IProviderService : IServiceBase
  {
    Task<Guid> ValidateProviderKeyAsync(string key);
    Task<IEnumerable<ServiceUser>> GetServiceUsers();
    Task<ProviderAdminResponse> GetAllActiveProvidersAsync();
    Task<ProviderResponse> GetProviderAsync(Guid id);
    Task<IEnumerable<Provider>> GetProvidersAsync(int triageLevelValue);
    Task<IEnumerable<Provider>> GetProvidersAsync(TriageLevel triageLevel);
    Task<IReferral> GetReferralStatusAndSubmissions(string ubrn);
    Task<IEnumerable<ServiceUserSubmissionResponse>> ProviderSubmissionsAsync(
        IEnumerable<IServiceUserSubmissionRequest> requests);
    Task<NewProviderApiKeyResponse> UpdateProviderKeyAsync(
      ProviderResponse providerResponse, int validDays = 365);
    Task<ProviderResponse> UpdateProviderLevelsAsync(
      ProviderLevelStatusChangeRequest request);
    Task<ProviderResponse> UpdateProvidersAsync(ProviderRequest request);

    Task<bool> AddProviderSubmissionsAsync(ProviderSubmissionRequest request,
      Guid referralId);

    Task<bool> UpdateProviderAuthAsync(ProviderAuthUpdateRequest request);

    Task<ProviderRejectionReason[]> GetRejectionReasonsAsync();

    Task<ProviderRejectionReasonResponse> SetNewRejectionReasonsAsync(
      ProviderRejectionReasonSubmission reason);

    Task<ProviderRejectionReasonResponse> UpdateRejectionReasonsAsync(
      ProviderRejectionReasonUpdate reason);

    Task<IEnumerable<ProviderInfo>> GetProvidersInfo();

    Task<ApiKeyStoreResponse> AddApiKeyStore(ApiKeyStoreRequest request);

    Task<string> GetProviderNameAsync(Guid id);

    Task<int> GetNumOfProvidersAvailableAtTriageLevelAsync(
      TriageLevel triageLevel);

    #region Test Methods
    Task<bool> CreateTestReferralsAsync(int numToCreate = 20, 
      bool withHistory = false);
    Task<bool> DeleteTestReferralsAsync();
    #endregion
  }
}