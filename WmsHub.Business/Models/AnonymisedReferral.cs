using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models;

public class AnonymisedReferral : BaseModel
{
  public string Status { get; set; }
  public string StatusReason { get; set; }
  public string ReferringGpPracticeNumber { get; set; }
  public string Ubrn { get; set; }
  public string Deprivation { get; set; }
  public int? Age { get; set; }
  public string MethodOfContact { get; set; }
  public int NumberOfContacts { get; set; }
  public string Sex { get; set; }
  public DateTimeOffset? DateOfReferral { get; set; }
  public bool? ConsentForFutureContactForEvaluation { get; set; }
  public string Ethnicity { get; set; }
  public bool? HasHypertension { get; set; }
  public bool? HasDiabetesType1 { get; set; }
  public bool? HasDiabetesType2 { get; set; }
  public decimal? HeightCm { get; set; }
  public decimal? GpRecordedWeight { get; set; }
  public decimal? CalculatedBmiAtRegistration { get; set; }
  public bool? IsVulnerable { get; set; }
  public bool? HasRegisteredSeriousMentalIllness { get; set; }
  public int? TriagedCompletionLevel { get; set; }
  public string ProviderName { get; set; }
  public DateTimeOffset? DateCompletedProgramme { get; set; }
  public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
  public DateTimeOffset? DateOfProviderSelection { get; set; }
  public DateTimeOffset? DateStartedProgramme { get; set; }
  public DateTimeOffset? DateToDelayUntil { get; set; }
  public string ProgrammeOutcome { get; set; }
  public List<ProviderSubmission> ProviderSubmissions { get; set; }
  public string VulnerableDescription { get; set; }
  public bool? HasAPhysicalDisability { get; set; }
  public bool? HasALearningDisability { get; set; }
  public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  public string ReferralSource { get; set; }
  public string StaffRole { get; set; }
  public string ReferringOrganisationOdsCode { get; set; }
  public bool? ConsentForGpAndNhsNumberLookup { get; set; }
  public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  public DateTimeOffset? LastTraceDate { get; set; }
  public int? TraceCount { get; set; }
  public bool? HasArthritisOfKnee { get; set; }
  public bool? HasArthritisOfHip { get; set; }
  public bool? IsPregnant { get; set; }
  public bool? HasActiveEatingDisorder { get; set; }
  public bool? HasHadBariatricSurgery { get; set; }
  public string OfferedCompletionLevel { get; set; }
  public decimal? DocumentVersion { get; set; }
  public string ServiceId { get; set; }
  public string SourceSystem { get; set; }

  public string ProviderUbrn { get; set; }
  public string ServiceUserEthnicityGroup { get; set; }
  public string ServiceUserEthnicity { get; set; }
  public DateTimeOffset? ReferralLetterDate { get; set; }
  public string OpcsCodes { get; set; }
  public DateTimeOffset? DatePlacedOnWaitingList { get; set; }
}