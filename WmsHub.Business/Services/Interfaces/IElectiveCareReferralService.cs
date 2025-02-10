using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models.ElectiveCareReferral;

namespace WmsHub.Business.Services.Interfaces;

public interface IElectiveCareReferralService : IServiceBase
{
  Task<GetQuotaDetailsResult> GetQuotaDetailsAsync(string odsCode);
  Task<ElectiveCareUserManagementResponse> ManageUsersUsingUploadAsync(
    IEnumerable<ElectiveCareUserData> data);
  Task<ProcessTrustDataResult> ProcessTrustDataAsync(
    IEnumerable<ElectiveCareReferralTrustData> trustData,
    string trustOdsCode,
    Guid trustUserId);
  Task<bool> UserHasAccessToOdsCodeAsync(
    Guid trustUserId, 
    string trustOdsCode);
}
