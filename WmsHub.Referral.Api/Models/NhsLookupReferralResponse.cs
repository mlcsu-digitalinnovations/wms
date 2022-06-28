using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;

namespace WmsHub.Referral.Api.Models
{
  public class NhsLookupReferralResponse
  {
    public string Ubrn { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public decimal? CalculatedBmiAtRegistration { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    public DateTimeOffset? DateOfReferral { get; set; }
    public string Email { get; set; }
    public string Ethnicity { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public decimal? HeightCm { get; set; }
    public bool? IsVulnerable { get; set; }
    public string Mobile { get; set; }
    public bool? ConsentForFutureContactForEvaluation { get; set; }
    public DateTimeOffset? DateCompletedProgramme { get; set; }
    public DateTimeOffset? DateOfProviderSelection { get; set; }
    public DateTimeOffset? DateStartedProgramme { get; set; }
    public string ProgrammeOutcome { get; set; }
    public string Status { get; set; }
    public string StatusReason { get; set; }
    public string TriagedCompletionLevel { get; set; }
    public string TriagedWeightedLevel { get; set; }
    public string ServiceUserEthnicity { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
    public string Deprivation { get; set; }
    public MethodOfContact ContactMethod { get; set; }
    public string ReferralSource { get; set; }
    public bool? ConsentForGpAndNhsNumberLookup { get; set; }
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    public int? TraceCount { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string NhsNumber { get; set; }
    public string Postcode { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public long? ReferralAttachmentId { get; set; }
    public long? MostRecentAttachmentId { get; set; }
    public Provider Provider { get; set; }
    public bool? HasArthritisOfKnee { get; set; }
    public bool? HasArthritisOfHip { get; set; }
    public bool? IsPregnant { get; set; }
    public bool? HasActiveEatingDisorder { get; set; }
    public bool? HasHadBariatricSurgery { get; set; }
    public bool? HasGivenBirthInPast3Months { get; set; }
    public bool? HasCaesareanInPast3Months { get; set; }
    public bool? IsBrestFeeding { get; set; }
    public string Sex { get; set; }
    public string Telephone { get; set; }
    public string VulnerableDescription { get; set; }
    public decimal? WeightKg { get; set; }
    public Guid Id { get; set; }
  }
}
