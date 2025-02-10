using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;

namespace WmsHub.Business.Entities;

public interface IReferral : IBaseEntity, IPhoneBase
{
  string Address1 { get; set; }
  string Address2 { get; set; }
  string Address3 { get; set; }
  decimal? CalculatedBmiAtRegistration { get; set; }
  bool? ConsentForFutureContactForEvaluation { get; set; }
  bool? ConsentForGpAndNhsNumberLookup { get; set; }
  bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  string CreatedByUserId { get; set; }
  DateTimeOffset? CreatedDate { get; set; }
  DateTimeOffset? DateCompletedProgramme { get; set; }
  DateTimeOffset? DateLetterSent { get; set; }
  DateTimeOffset? DateOfBirth { get; set; }
  DateTimeOffset? DateOfBmiAtRegistration { get; set; }
  DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  DateTimeOffset? DateOfProviderSelection { get; set; }
  DateTimeOffset? DateOfReferral { get; set; }
  DateTimeOffset? DateStartedProgramme { get; set; }
  DateTimeOffset? DateToDelayUntil { get; set; }
  string DelayReason { get; set; }
  string Deprivation { get; set; }
  decimal? DocumentVersion { get; set; }
  string Email { get; set; }
  string Ethnicity { get; set; }
  string FamilyName { get; set; }
  decimal? FirstRecordedWeight { get; set; }
  DateTimeOffset? FirstRecordedWeightDate { get; set; }
  string GivenName { get; set; }
  bool? HasActiveEatingDisorder { get; set; }
  bool? HasALearningDisability { get; set; }
  bool? HasAPhysicalDisability { get; set; }
  bool? HasArthritisOfHip { get; set; }
  bool? HasArthritisOfKnee { get; set; }
  bool? HasDiabetesType1 { get; set; }
  bool? HasDiabetesType2 { get; set; }
  bool? HasHadBariatricSurgery { get; set; }
  bool? HasHypertension { get; set; }
  bool? HasRegisteredSeriousMentalIllness { get; set; }
  decimal? HeightCm { get; set; }
  bool? IsErsClosed { get; set; }
  bool? IsPregnant { get; set; }
  bool? IsVulnerable { get; set; }
  decimal? LastRecordedWeight { get; set; }
  DateTimeOffset? LastRecordedWeightDate { get; set; }
  DateTimeOffset? LastTraceDate { get; set; }
  int? MethodOfContact { get; set; }
  DateTimeOffset? MostRecentAttachmentDate { get; set; }
  string NhsLoginClaimEmail { get; set; }
  string NhsLoginClaimFamilyName { get; set; }
  string NhsLoginClaimGivenName { get; set; }
  string NhsLoginClaimMobile { get; set; }
  string NhsNumber { get; set; }
  int NumberOfContacts { get; set; }
  string OfferedCompletionLevel { get; set; }
  string Postcode { get; set; }
  string ProgrammeOutcome { get; set; }
  Guid? ProviderId { get; set; }
  string ReferralAttachmentId { get; set; }
  DateTimeOffset? ReferralLetterDate { get; set; }
  string ReferralSource { get; set; }
  string ReferringClinicianEmail { get; set; }
  string ReferringGpPracticeName { get; set; }
  string ReferringGpPracticeNumber { get; set; }
  string ReferringOrganisationEmail { get; set; }
  string ReferringOrganisationOdsCode { get; set; }
  string ServiceId { get; set; }
  string ServiceUserEthnicity { get; set; }
  string ServiceUserEthnicityGroup { get; set; }
  string Sex { get; set; }
  Common.Enums.SourceSystem? SourceSystem { get; set; }
  string StaffRole { get; set; }
  string Status { get; set; }
  string StatusReason { get; set; }
  int? TraceCount { get; set; }
  string TriagedCompletionLevel { get; set; }
  string TriagedWeightedLevel { get; set; }
  string Ubrn { get; set; }
  string VulnerableDescription { get; set; }
  decimal? WeightKg { get; set; }
  string ProviderUbrn { get; set; }
  public decimal? HeightFeet { get; set; }
  public decimal? HeightInches { get; set; }
  public UnitsType? HeightUnits { get; set; }
  public decimal? WeightStones { get; set; }
  public decimal? WeightPounds { get; set; }
  public UnitsType? WeightUnits { get; set; }
}
