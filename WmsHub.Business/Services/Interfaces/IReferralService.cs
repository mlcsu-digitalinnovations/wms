using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Common.Exceptions;
using Ethnicity = WmsHub.Business.Models.Ethnicity;

namespace WmsHub.Business.Services;
public interface IReferralService : IServiceBase
{
  bool UpdateTriage(Entities.Referral referral);
  Task<IReferral> CreateReferral(IReferralCreate referralCreate);

  /// <summary>
  /// Confirms that an Elective Care referral with a text message that has a matching 
  /// ServiceUserLinkId exists.
  /// </summary>
  /// <param name="linkId">The link id of the referral text message.</param>
  /// <returns>True if the link ID matches a text message associated with an Elective Care
  /// referral with a Status of ChatBotCall1, ChatBotTransfer, New, RmcCall, RmcDelayed, 
  /// TextMessage1, TextMessage2 or TextMessage3, otherwise false.</returns>
  Task<bool> ElectiveCareReferralHasTextMessageWithLinkId(string linkId);

  Task<IReferral> ExceptionOverride(
    Guid id, 
    ReferralStatus status,
    string statusReason);
  Task UpdateDeprivation(Entities.Referral referral);
  Task<IReferral> GetReferralWithTriagedProvidersById(Guid id);
  Task<IReferral> GetReferralByNhsNumber(string nhsNumber);
  Task<List<Guid>> GetReferralIdsByNhsNumber(string nhsNumber);
  Task<IReferral> GetServiceUserReferralAsync(string serviceUserLinkId);
  Task<IReferral> GetReferralByTextMessageId(Guid id);
  Task<IEnumerable<ReferralSourceInfo>> GetReferralSourceInfo();

  /// <summary>
  /// Gets the triaged completion level of the referral.
  /// </summary>
  /// <param name="id">The Id of the referral.</param>
  /// <returns>The triaged completion level of the referral.</returns>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  /// <exception cref="TriageNotFoundException"></exception>
  Task<TriageLevel> GetTriagedCompletionLevelAsync(Guid id);

  /// <summary>
  /// Gets the order of providers for the referral if one exists.
  /// </summary>
  /// <param name="id">The id of the referral.</param>
  /// <returns>A list of the provider names in the order they were shown or null if not analytics 
  /// record exists.</returns>
  Task<List<string>> GetProviderNamesOrderAsync(Guid id);

  Task<IReferral> UpdateConsentForFutureContactForEvaluation(
      Guid id, bool emailNotSupplied, bool consented, string emailAddress);
  Task<IReferral> EmailAddressNotProvidedAsync(Guid id);
  Task<IReferral> SetBmiTooLowAsync(Guid referralId);
  Task<IReferral> UpdateEthnicity(Guid id, Ethnicity ethnicity);
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
  Task<IReferral> UpdateServiceUserEthnicityGroupAsync(
    Guid id, 
    string ethnicityGroup);
  Task<IReferral> UpdateServiceUserEthnicityAsync(
    Guid id, string ethnicityDisplayName);
  Task<IReferral> CreateSelfReferral(ISelfReferralCreate referralCreate);
  Task<IReferral> CreateGeneralReferral(
    IGeneralReferralCreate referralCreate);
  string[] GetNhsNumbers(int? required);
  Task<string> PrepareRmcCallsAsync();
  Task<string> PrepareDelayedCallsAsync();
  Task<IEnumerable<IStaffRole>> GetStaffRolesAsync();
  Task<IEnumerable<Ethnicity>> GetEthnicitiesAsync(
    Enums.ReferralSource referralSource);

  /// <summary>
  /// Returns the <see cref="Referral"/>:<see cref="IBaseModel.Id"/> of the specified
  /// <see cref="Referral.Ubrn"/> or <see cref="Referral.ProviderUbrn"/>.
  /// Returns <see langword="null"/> if no referral is found.
  /// </summary>
  /// <param name="ubrn">
  /// <see cref="Referral.Ubrn"/> or <see cref="Referral.ProviderUbrn"/> to select.
  /// </param>
  /// <inheritdoc/>
  /// <exception cref="ArgumentNullOrWhiteSpaceException">
  /// Throws if <paramref name="ubrn"/> is <see langword="null"/> or whitespace.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Throws if there are multiple referrals with the same <paramref name="ubrn"/>.
  /// </exception>
  /// <remarks>
  /// Only active referrals are searched.
  /// </remarks>
  public Task<string> GetIdFromUbrn(string ubrn);

  /// <summary>
  /// Returns the <see cref="Referral"/>:<see cref="IBaseModel.Id"/>s of the specified
  /// <see cref="Referral.Ubrn"/>s or <see cref="Referral.ProviderUbrn"/>s in the order provided.
  /// Referrals that are not found are <see langword="null"/>.
  /// </summary>
  /// <param name="ubrns">
  /// The <see cref="Referral.Ubrn"/> or <see cref="Referral.ProviderUbrn"/> to select.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// Throws if <paramref name="ubrns"/> is <see langword="null"/>
  /// </exception>
  /// <exception cref="ArgumentNullOrWhiteSpaceException">
  /// Throws if any values within <paramref name="ubrns"/> are <see langword="null"/> or
  /// whitespace.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Throws if there are multiple referrals with the same <paramref name="ubrns"/>.
  /// Throws if the provided <paramref name="ubrns"/> is empty.
  /// </exception>
  /// <remarks>
  /// Only active referrals are searched.
  /// </remarks>
  public Task<IEnumerable<string>> GetIdsFromUbrns(IEnumerable<string> ubrns);

  Task<byte[]> SendReferralLettersAsync(List<Guid> referrals,
    DateTimeOffset dateLettersExported);
  Task<FileContentResult> CreateDischargeLettersAsync(List<Guid> referrals);

  Task<IReferral> TestCreateWithChatBotStatus(
    IReferralCreate referralCreate);

  /// <summary>
  /// Get a list of GP referral entities whose:
  /// IsErsClosed != true and Status != CancelledByEreferrals.
  /// </summary>
  /// <param name="serviceId">The Id of the service to filter the results
  /// to. Passing null will include all services.</param>
  /// <returns>A list of referral entities.</returns>
  Task<List<ActiveReferralAndExceptionUbrn>>
    GetOpenErsGpReferralsThatAreNotCancelledByEreferals(string serviceId);

  Task<int> UpdateReferralCancelledByEReferralAsync(string ubrn);
  Task DischargeReferralAsync(Guid id);
  Task IsEmailInUseAsync(string email);
  Task<string> PrepareFailedToContactAsync();

  Task<CriCrudResponse> CriCrudAsync(
    ReferralClinicalInfo criDocumentBytes,
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

  Task<bool> PharmacyEmailListedAsync(string referringPharmacyEmail);
  Task<string> GetProviderNameAsync(Guid providerId);

  Task<bool> EthnicityToServiceUserEthnicityMatch
    (string ethnicity, string serviceUserEthnicity);

  Task<bool> EthnicityToGroupNameMatch(string ethnicity, string groupName);
  Task<List<ReferralDischarge>> GetDischarges();

  /// <summary>
  /// Get discharges to be posted to GpDocumentProxy
  /// </summary>
  /// <remarks>
  /// Discharges are referrals that are active, have a date of birth,
  /// and a status of AwaitingDischarge.
  /// </remarks>
  /// <returns>A list of discharges to be posted.
  /// </returns>
  Task<List<GpDocumentProxyReferralDischarge>> 
    GetDischargesForGpDocumentProxy();

  /// <summary>
  /// Post the discharges to GpDocumentProxy
  /// </summary>
  /// <remarks>
  /// If a discharge's organisation is not supported by docman, 
  /// first save the referral's status as UnableToDischarge,
  /// then save the referral's status as Complete,
  /// save the referral's status reason as the response message from
  /// GpDocumentProxy.
  /// </remarks>
  /// <returns>The Ids of the successfully discharged referrals.
  /// </returns>
  /// <param name="discharges">The discharges to be posted.</param>
  Task<List<Guid>> PostDischarges(
    List<GpDocumentProxyReferralDischarge> discharges);

  /// <summary>
  /// Update the discharged referrals via GpDocumentProxy using their latest 
  /// status from Docman.
  /// </summary>
  /// <remarks>
  /// Referral status is updated if the document status is Rejected or 
  /// RejectionResolved.
  /// </remarks>
  /// <returns>A summary of the latest statuses of the discharged referrals.
  /// </returns>
  Task<GpDocumentProxyUpdateResponse> UpdateDischarges();
  Task<GpDocumentProxySetRejection> UpdateDischargedReferralWithRejection(
    Guid referralId,
    string information);
  Task<IReferral> UpdateGeneralReferral(IGeneralReferralUpdate update);
  Task CreateMskReferralAsync(IMskReferralCreate referralCreate);
  Task<ReferralStatusReason[]> GetRmcRejectedReferralStatusReasonsAsync();

  Task CheckReferralCanBeCreatedWithNhsNumberAsync(string nhsNumber);
  Task CheckSelfReferralIsUniqueAsync(string email);
  Task UpdateGeneralReferralUbrnAsync(Guid referralId);
  Task UpdateGpReferralUbrnAsync(Guid referralId);
  Task UpdateMskReferralUbrnAsync(Guid referralId);
  Task UpdatePharmacyReferralUbrnAsync(Guid referralId);
  Task UpdateSelfReferralUbrnAsync(Guid referralId);

  /// <summary>
  /// Creates access key.
  /// </summary>
  /// <param name="create">An ICreateAccessKey object containing the 
  /// access key properties.</param>
  /// <returns>An ICreateAccessKeyResponse containing properties of the 
  /// access key or errors if there was a failure.</returns>
  Task<ICreateAccessKeyResponse> CreateAccessKeyAsync(
    ICreateAccessKey createAccessKey);

  /// <summary>
  /// Validates that the provided access key and emails match, that the 
  /// access key has not expired or has had too many attempts to be provided
  /// correctly.
  /// </summary>
  /// <param name="validate">An IValidateAccessKey object containing
  /// the validation properties.</param>
  /// <returns>An IValidateAccessKeyResponse containing the validation
  /// status of the provided access key.</returns>
  Task<IValidateAccessKeyResponse> ValidateAccessKeyAsync(
    IValidateAccessKey validateAccessKey);
  Task<string> GetServiceUserLinkIdAsync(IReferral referral);

  Task CancelGeneralReferralAsync(IGeneralReferralCancel cancellation);
  
  Task<CanCreateReferralResponse> 
    CanGeneralReferralBeCreatedWithNhsNumberAsync(string nhsNumber);

  /// <summary>
  /// Gets date of most recent TextMessage1, or if not present date of most recent Call, or if not
  /// present returns null.
  /// </summary>
  /// <param name="referralId">Id of referral.</param>
  /// <returns>DateTimeOffset of most recent contact, or null if none made.</returns>
  Task<DateTimeOffset?> GetDateOfFirstContact(Guid referralId);

  Task<Dictionary<ApiKeyType, DateTime?>> GetDatesMessageSent(
    Guid referralId);

  /// <summary>
  /// Returns a referral when given a valid linkId.
  /// </summary>
  /// <param name="linkId"></param>
  /// <returns>Key:MessageQueue.Id, Value: Models.Referral</returns>
  Task<KeyValuePair<Guid, IReferral>?> GetMessageQueue(string linkId);

  /// <summary>
  /// Closes the specified open eRS GP Referral if the referral does not 
  /// have a status of CancelledByEreferrals or RejectedToEreferrals.<br />
  /// If the Guid id is empty, an exception, ArgumentOutOfRangeException,
  /// is thrown.<br />
  /// If the referral is not found, an exception, ReferralNotFoundException,
  /// is thrown.<br />
  /// If the referral is not an eRS GP Referral, an exception,
  /// ReferralInvalidReferralSourceException, is thrown.<br />
  /// If the referral is CancelledByEreferrals or RejectedToEreferrals,
  /// an exception, ReferralInvalidStatusException, is throw
  /// </summary>
  /// <param name="id">Guid ReferralId</param>
  /// <param name="source">Default is GpReferral.  However the flag can
  /// be extended if required.</param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralInvalidReferralSourceException"></exception>
  /// <exception cref="ReferralInvalidStatusException"></exception>
  Task CloseErsReferral(
    Guid id, 
    ReferralSource referralSourceFlag = ReferralSource.GpReferral,
    ReferralStatus referralStatusFlag = 
      ReferralStatus.CancelledByEreferrals
      | ReferralStatus.RejectedToEreferrals);

  /// <summary>
  /// Closes the specified open eRS GP Referral if the referral does not 
  /// have a status of CancelledByEreferrals or RejectedToEreferrals.<br />
  /// If the referral is not found, an exception, ReferralNotFoundException,
  /// is thrown.<br />
  /// If the referral is not an eRS GP Referral, an exception,
  /// ReferralInvalidReferralSourceException, is thrown.<br />
  /// If the referral is CancelledByEreferrals or RejectedToEreferrals,
  /// an exception, ReferralInvalidStatusException, is throw
  /// </summary>
  /// <param name="id">Guid ReferralId</param>
  /// <param name="source">Default is GpReferral.  However the flag can
  /// be extended if required.</param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralInvalidReferralSourceException"></exception>
  /// <exception cref="ReferralInvalidStatusException"></exception>
  Task CloseErsReferral(
    ReferralSource referralSourceFlag = ReferralSource.GpReferral,
    ReferralStatus referralStatusFlag =
      ReferralStatus.CancelledByEreferrals
      | ReferralStatus.RejectedToEreferrals,
    string ubrn = null);

  /// <summary>
  /// Updates a rejected referral that has had a provider selected so 
  /// that it is ready for discharge.
  /// </summary>
  /// <param name="id">The id of the rejected referral.</param>
  /// <param name="reason">The rejection reason.</param>
  /// <returns>The updated referral.</returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ReferralInvalidStatusException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralProviderSelectedException"></exception>
  Task<IReferral> RejectAfterProviderSelectionAsync(Guid id, string reason);

  /// <summary>
  /// Updates a rejected referral that has not had a provider selected so 
  /// that it is ready for discharge.
  /// </summary>
  /// <param name="id">The id of the rejected referral.</param>
  /// <param name="reason">The rejection reason.</param>
  /// <returns>The updated referral.</returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ReferralInvalidStatusException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  /// <exception cref="ReferralProviderSelectedException"></exception>
  Task<IReferral> RejectBeforeProviderSelectionAsync(Guid id, string reason);

  /// <summary>
  /// Returns a referral entity if the given id matches an existing active
  /// referral.
  /// </summary>
  /// <param name="id"></param>
  /// <returns>The referral entity with the matching id.</returns>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<Entities.Referral> GetReferralEntity(Guid id);

  /// <summary>
  /// Terminates referrals that have not selected a provider after the provider selection duration
  /// has expired.
  /// </summary>
  /// <remarks>
  /// If the referral source of an expired referral is GpReferral, the status is set to 
  /// ProviderTerminated, otherwise the status is set to ProviderTerminatedTextMessage.
  /// </remarks>
  /// <returns>The number of terminated referrals.
  /// </returns>
  Task<int> TerminateNotStartedProgrammeReferralsAsync();

  /// <summary>
  /// Updates the UBRN's referral status to RejectedToEreferrals and the 
  /// status reason if it is passed.
  /// </summary>
  /// <param name="referralId">The Id of the referral to update.</param>
  /// <param name="statusReason">The status reason, pass null or white space
  /// to leave the status reason unchanged.</param>
  /// <returns>The updated referral.</returns>
  Task<IReferral> UpdateStatusToRejectedToEreferralsAsync(
    Guid referralId,
    string statusReason);

  Task UpdateDobAttemptsAsync(Guid textMessageId, int attempts);
}
