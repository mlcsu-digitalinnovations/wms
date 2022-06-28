using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using Microsoft.AspNetCore.Mvc;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.Interfaces;
using Ethnicity = WmsHub.Business.Models.Ethnicity;
using WmsHub.Business.Models.ReferralService.MskReferral;

namespace WmsHub.Business.Services
{
  public interface IReferralService : IServiceBase
  {
    bool UpdateTriage(Entities.Referral referral);
    Task<IReferral> CreateReferral(IReferralCreate referralCreate);
    Task<IReferral> GetReferralWithTriagedProvidersById(Guid id);
    Task<IReferral> GetReferralByNhsNumber(string nhsNumber);

    Task<IReferral> GetServiceUserReferralAsync(string base36DateOfReferral);
    Task<IReferral> GetReferralByTextMessageId(Guid id);
    Task<IReferral> UpdateConsentForFutureContactForEvaluation(
        Guid id,bool emailNotSupplied, bool consented, string emailAddress);
    Task<IReferral> EmailAddressNotProvidedAsync(Guid id);
    Task<IReferral> SetBmiTooLowAsync(Guid referralId);
    Task<IReferral> UpdateEthnicity(Guid id, Enums.Ethnicity ethnicity );
    Task<IReferral> UpdateDateOfBirth(Guid id, DateTimeOffset dateOfBirth);
    Task<IReferral> UpdateMobile(Guid id, string mobile);
    Task<IReferralSearchResponse> Search(ReferralSearch referralSearch);
    Task<IReferral> ConfirmProviderAsync(Referral referral);
		Task<IReferral> ConfirmProviderAsync(Guid referralId,	
      Guid? providerId, 
      bool isRmcCall = false,
      bool consentForContact = false,
      string email = null);
    Task<IReferral> DelayReferralUntilAsync(Guid referralId, string reason,
      DateTimeOffset until);
    Task ExpireTextMessageDueToDobCheckAsync(string base36SentDate);
    Task<IReferral> UpdateStatusFromRmcCallToFailedToContactAsync(
      Guid referralId,
      string reason);
		Task<IReferral> UpdateStatusToRejectedToEreferralsAsync(
      Guid referralId, string statusReason);
		Task<IReferral> UpdateServiceUserEthnicityGroupAsync(Guid id, string ethnicityGroup);
		Task<IReferral> UpdateServiceUserEthnicityAsync(
			Guid id, string ethnicityDisplayName);
    Task<IReferral> CreateSelfReferral(ISelfReferralCreate referralCreate);
    Task<IReferral> CreateGeneralReferral(IGeneralReferralCreate referralCreate);
    string[] GetNhsNumbers(int? required);
    Task<string> PrepareRmcCallsAsync();
    Task<string> PrepareDelayedCallsAsync();
    Task<IEnumerable<IStaffRole>> GetStaffRolesAsync();
    Task<IEnumerable<Ethnicity>> GetEthnicitiesAsync(
      Enums.ReferralSource referralSource);
    Task<byte[]> SendReferralLettersAsync(List<Guid> referrals,
      DateTimeOffset dateLettersExported);
    Task<FileContentResult> CreateDischargeLettersAsync(List<Guid> referrals);

    Task<IReferral> TestCreateWithChatBotStatus(
      IReferralCreate referralCreate);

    Task<IReferral> TestCreateWithRmcStatus(
      IReferralCreate referralCreate);

    Task<List<ActiveReferralAndExceptionUbrn>>
      GetActiveReferralAndExceptionUbrns(string serviceId);

    Task<int> UpdateReferralCancelledByEReferralAsync(string ubrn);
    Task DischargeReferralAsync(Guid id);
    Task<InUseResponse> IsEmailInUseAsync(string email);
    Task<string[]> PrepareUnableToContactAsync();

    Task<CriCrudResponse> CriCrudAsync(ReferralClinicalInfo criDocumentBytes, 
      bool isDelete);

    Task<DateTimeOffset?> GetLastCriUpdatedAsync(string ubrn);
    Task<byte[]> GetCriDocumentAsync(string ubrn);
    Task UpdateAnalyticsForProviderList(Guid referralId, string @join);
    Task TriageReferralUpdateAsync(Guid id);
    Task<IReferral> UpdateEmail(Guid id, string email);
    Task<IReferral> UpdateStatusToRmcCallAsync(Guid id);

    Task<List<ReferralAudit>> GetReferralAuditForServiceUserAsync(
      Guid modelUbrn);
    Task<IReferral> UpdateReferralToStatusExceptionAsync(
      Business.Models.IReferralExceptionUpdate request);

    Task<IReferral> UpdateReferralWithProviderAsync(
      Guid referralId, Guid providerId, 
      ReferralSource source = ReferralSource.GpReferral);

    Task<IReferralPostResponse> GetReferralCreateResponseAsync(
      IReferral referral);

    Task<IReferral> CreatePharmacyReferral(
      IPharmacyReferralCreate referralCreate);
    Task<IPharmacistKeyCodeGenerationResponse> GetPharmacistKeyCodeAsync(
      IPharmacistKeyCodeCreate create);

    Task<IPharmacistKeyCodeValidationResponse> ValidatePharmacistKeyCodeAsync(
      IPharmacistKeyCodeCreate create);

    Task<InUseResponse> 
      IsNhsNumberInUseAsync(string nhsNumber);

    Task<bool> PharmacyEmailListedAsync(string referringPharmacyEmail);
    Task<string> GetProviderNameAsync(Guid providerId);

    Task<bool> EthnicityToServiceUserEthnicityMatch
      (string ethnicity, string serviceUserEthnicity);

    Task<bool> EthnicityToGroupNameMatch(string ethnicity, string groupName);
    Task<List<ReferralDischarge>> GetDischarges();
    Task<IReferral> UpdateGeneralReferral(IGeneralReferralUpdate update);
    Task CreateMskReferralAsync(IMskReferralCreate referralCreate);
  }
}