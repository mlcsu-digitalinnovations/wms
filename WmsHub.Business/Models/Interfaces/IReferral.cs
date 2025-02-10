using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models;

public interface IReferral : IReferralCreate, IBaseModel
{
  List<Call> Calls { get; set; }
  bool? ConsentForFutureContactForEvaluation { get; set; }
  bool? ConsentForGpAndNhsNumberLookup { get; set; }
  bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  MethodOfContact ContactMethod { get; }
  string CreatedByUserId { get; set; }
  DateTimeOffset? CreatedDate { get; set; }
  DateTimeOffset? DateCompletedProgramme { get; set; }
  DateTimeOffset? DateLetterSent { get; set; }
  DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  DateTimeOffset? DateOfProviderSelection { get; set; }
  DateTimeOffset? DateStartedProgramme { get; set; }
  string DelayReason { get; set; }
  DateTimeOffset? DelayUntil { get; set; }
  string Deprivation { get; set; }
  decimal? FirstRecordedWeight { get; set; }
  DateTimeOffset? FirstRecordedWeightDate { get; set; }
  bool? HasActiveEatingDisorder { get; set; }
  bool? HasArthritisOfHip { get; set; }
  bool? HasArthritisOfKnee { get; set; }
  bool? HasHadBariatricSurgery { get; set; }
  bool HasProviders { get; }
  bool HasValidNumber { get; }
  bool IsExceptionDueToEmailNotProvided { get; }
  bool IsBmiTooLow { get; set; }
  decimal SelectedEthnicGroupMinimumBmi { get; set; }
  bool IsGpReferral { get; }
  bool? IsPregnant { get; set; }
  bool IsProviderSelected { get; }
  decimal? LastRecordedWeight { get; set; }
  DateTimeOffset? LastRecordedWeightDate { get; set; }
  DateTimeOffset? LastTraceDate { get; set; }
  int? MethodOfContact { get; set; }
  string NhsLoginClaimEmail { get; set; }
  string NhsLoginClaimFamilyName { get; set; }
  string NhsLoginClaimGivenName { get; set; }
  string NhsLoginClaimMobile { get; set; }
  int? NumberOfContacts { get; set; }
  int NumberOfDelays { get; set; }
  string OfferedCompletionLevel { get; set; }
  string ProgrammeOutcome { get; set; }
  Provider Provider { get; set; }
  Guid? ProviderId { get; set; }
  List<Provider> Providers { get; set; }
  List<ProviderSubmission> ProviderSubmissions { get; set; }
  string ReferralSource { get; set; }
  string ReferringClinicianEmail { get; set; }
  string ReferringOrganisationEmail { get; set; }
  string ReferringOrganisationOdsCode { get; set; }
  string ServiceUserEthnicity { get; set; }
  string ServiceUserEthnicityGroup { get; set; }
  string StaffRole { get; set; }
  string Status { get; set; }
  string StatusReason { get; set; }
  List<TextMessage> TextMessages { get; set; }
  int? TraceCount { get; set; }
  string TriagedCompletionLevel { get; set; }
  string TriagedWeightedLevel { get; set; }
  string ProviderUbrn { get; set; }

  string GetValidNumber();

  string SpellIdentifier { get; set; }

  public decimal? HeightFeet { get; set; }
  public decimal? HeightInches { get; set; }
  public UnitsType? HeightUnits { get; set; }
  public decimal? WeightStones { get; set; }
  public decimal? WeightPounds { get; set; }
  public UnitsType? WeightUnits { get; set; }
}