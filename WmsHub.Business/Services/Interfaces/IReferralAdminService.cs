using System;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Services
{
  public interface IReferralAdminService : IServiceBase
  {
    Task<string> ChangeDateOfBirthAsync(
      string ubrn,
      DateTimeOffset originalDateOfBirth,
      DateTimeOffset updatedDateOfBirth);
    Task<string> ChangeMobileAsync(
      string ubrn,
      string originalMobile,
      string updatedMobile);
    Task<string> DeleteCancelledGpReferralAsync(string ubrn, string reason);
    Task<string> DeleteReferralAsync(Models.Referral referral);
    Task<string> FixNonGpReferralsWithStatusProviderCompletedAsync();
    Task<object> FixProviderAwaitingTraceAsync();
    Task<Referral> ResetReferralAsync(
      Models.Referral referralToDelete, 
      ReferralStatus referralStatus);
  }
}