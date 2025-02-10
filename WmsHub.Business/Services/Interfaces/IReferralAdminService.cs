using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Services.Interfaces;

public interface IReferralAdminService : IServiceBase
{
  /// <summary>
  /// Changes a referral's date of birth.
  /// </summary>
  /// <param name="ubrn">The UBRN to change to date of birth for.</param>
  /// <param name="originalDateOfBirth">The current date of birth held for the
  /// UBRN's referral.</param>
  /// <param name="updatedDateOfBirth">The date of birth to update the 
  /// referral with.</param>
  /// <returns>A description of what was changed.</returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="AgeOutOfRangeException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<string> ChangeDateOfBirthAsync(
    string ubrn,
    DateTimeOffset originalDateOfBirth,
    DateTimeOffset updatedDateOfBirth);

  /// <summary>
  /// Changes a referral's mobile number.
  /// </summary>
  /// <param name="ubrn">The UBRN to change to mobile number for.</param>
  /// <param name="originalDateOfBirth">The current mobile number held for the
  /// UBRN's referral.</param>
  /// <param name="updatedDateOfBirth">The mobile number to update the 
  /// referral with.</param>
  /// <returns>A description of what was changed.</returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<string> ChangeMobileAsync(
    string ubrn,
    string originalMobile,
    string updatedMobile);

  /// <summary>
  /// Changes a referral's NHS number.
  /// </summary>
  /// <param name="ubrn">The UBRN to change to NHS number for.</param>
  /// <param name="originalDateOfBirth">The current NHS number held for the
  /// UBRN's referral.</param>
  /// <param name="updatedDateOfBirth">The NHS number to update the 
  /// referral with.</param>
  /// <returns>A description of what was changed.</returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="AgeOutOfRangeException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<string> ChangeNhsNumberAsync(
    string ubrn,
    string originalNhsNumber,
    string updatedNhsNumber);

  /// <summary>
  /// Changes a referral's Sex, where Sex matches "Male", "Female", "Not Specified", "Not Known".
  /// </summary>
  /// <param name="id">Referral's Id.</param>
  /// <param name="originalSex">Referral's current Sex value.</param>
  /// <param name="ubrn">Referral's Ubrn</param>
  /// <param name="updatedSex">String value to update referral's Sex to.</param>
  /// <returns>String confirming change.</returns>
  public Task<string> ChangeSexAsync(Guid id, string originalSex, string ubrn, string updatedSex);
  Task<string> DeleteCancelledGpReferralAsync(string ubrn, string reason);
  Task<string> DeleteReferralAsync(Models.Referral referralToDelete);

  /// <summary>
  /// Updates the ProgrammeOutcome to DidNotCommence for all referrals with a 
  /// corresponding ReferralsAudit row where Status is either 
  /// ProviderDeclinedByServiceUser or ProviderRejected.
  /// </summary>
  /// <returns>The number of referarls updated. </returns>
  Task<string> FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcome();
  Task<object> FixGPReferralsWithStatusLetterOrLetterSent();
  Task<string> FixMSKReferralsWithStatusRejectedToEreferrals();
  Task<string> FixNonGpReferralsWithStatusProviderCompletedAsync();

  /// <summary>
  /// Updates the Mobile, Telelphone, isValidMobile and isValidTelephone
  /// properties if necessary.
  /// </summary>
  /// <param name="ubrn">The UBRN of the referral to process.</param>
  /// <returns>A string describing the operations performed on the referal.
  /// </returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<string> FixNumbersAsync(string ubrn);
  Task<object> FixPharmacyReferralsWithInvalidStatus();
  Task<object> FixProviderAwaitingTraceAsync();
  Task<object> FixReferralProviderUbrnAsync();

  /// <summary>
  /// Updates referrals with invalid statuses to Complete.
  /// </summary>
  /// <returns>A list of referrals whose status has been updated.
  /// </returns>
  /// <remarks>
  /// For general, msk, pharmacy and self referrals invalid statuses are:
  /// <br/>- CancelledByEreferrals
  /// <br/>- CancelledDueToNonContact
  /// <br/>- CancelledDuplicate
  /// <br/>- Exception if email address is **DON'T CONTACT BY EMAIL**
  /// <br/>- FailedToContact
  /// <br/>- Letter
  /// <br/>- LetterSent
  /// <br/>- RejectedToEreferrals<br/>
  /// For GP referrals invalid statuses are:
  /// <br/>- CancelledDueToNonContact
  /// <br/>- CancelledDuplicate
  /// <br/>- DischargeOnHold
  /// <br/>- Letter
  /// <br/>- LetterSent
  /// </remarks>
  Task<List<string>> FixReferralsWithInvalidStatuses();

  /// <summary>
  /// Updates the DateStartedProgramme property of referrals where the value is
  /// null to be equal to the earliest linked ProviderSubmission Date.
  /// </summary>
  /// <returns>
  /// A list of referrals whose status has been updated, including their
  /// updated DateSelectedProgramme value.
  /// </returns>
  Task<List<string>> FixReferralsWithMissingDateStartedProgramme();

  /// <summary>
  /// Updates the ProviderUbrn property of referrals where the value is null.
  /// </summary>
  /// <returns>A list of outcomes for referrals without a provider ubrn.
  /// </returns>
  Task<List<string>> FixReferralsWithNullProviderUbrn();
  Task<object> FixSelfReferralsWithInvalidStatus();

  /// <summary>
  /// Resets an referral back to the provided status, resetting associated
  /// propeties
  /// </summary>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="ReferralNotFoundException"></exception>
  Task<Models.Referral> ResetReferralAsync(
    Models.Referral referralToReset,
    ReferralStatus referralStatus);

  /// <summary>
  /// Send discharge letters for MSK referrals with a status of "Complete".
  /// </summary>
  /// <remarks>
  /// This method is for retrospective use on existing MSK referrals. Future
  /// MSK referrals will have referral letters created via a separate process.
  /// </remarks>
  /// <param name="fromDate">The inclusive date from which referrals 
  /// will be included.</param>
  /// <param name="toDate">The inclusive date to which referrals will
  /// be included.</param>
  /// <returns>
  /// A list of the Ids of all sent letters.
  /// </returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="DateRangeNotValidException"></exception>
  Task<List<Guid>> SendDischargeLettersForCompleteMskReferrals(
    DateTimeOffset fromDate,
    DateTimeOffset toDate);
  /// <summary>
  /// Sets IsErsClosed to false on active GP referral matching id and ubrn parameters. Status must 
  /// be equal to RejectedToEreferrals.
  /// </summary>
  /// <param name="id">Id of the referral to update.</param>
  /// <param name="ubrn">Ubrn of the referral to update</param>
  /// <returns>The updated referral.</returns>
  Task<Models.Referral> SetIsErsClosedToFalse(Guid id, string ubrn);
  ///<summary>
  /// Set Ethnicity to null for referrals with specified Ids where ServiceUserEthnicity and
  /// ServiceUserEthnicityGroup do not correspond correctly to Ethnicity.
  ///</summary>
  ///<param name="ids">Array of Ids to be updated</param>
  ///<returns>A list of Ids of referrals which have been updated.</returns>
  Task<List<Guid>> SetMismatchedEthnicityToNull(Guid[] ids);
  /// <summary>
  /// Update ReferringOrganisationOdsCode for MSK referrals to a different 
  /// code present in the MskOrganisations table.
  /// </summary>
  /// <param name="currentOdsCode">Required. The ReferringOrganisationOdsCode 
  /// to be changed. Must be present in MskOrganisations table.
  /// <param name="newOdsCode">Required. The value ReferringOrganisationOds
  /// be updated to. Must be present in the MskOrganistions table.</param>
  /// <returns>
  /// A list of Ids of referrals which have been updated.
  /// </returns>
  Task<List<Guid>> UpdateMskReferringOrganisationOdsCode(
    string currentOdsCode,
    string newOdsCode);
}
