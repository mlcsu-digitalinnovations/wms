using System;

namespace WmsHub.Business.Models
{
  public class ReferralAudit : BaseModel, IReferralAudit
  {
    public string Username { get; set; }
    public int AuditId { get; set; }
    public string AuditAction { get; set; }
    public int AuditDuration { get; set; }
    public string AuditErrorMessage { get; set; }
    public int AuditResult { get; set; }
    public bool AuditSuccess { get; set; }
    public string NhsNumber { get; set; }
    public DateTimeOffset? DateOfReferral { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public string Ubrn { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Sex { get; set; }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    public bool? ConsentForFutureContactForEvaluation { get; set; }
    public string Ethnicity { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? CalculatedBmiAtRegistration { get; set; }
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    public string TriagedCompletionLevel { get; set; }
    public DateTimeOffset? DateOfProviderSelection { get; set; }
    public DateTimeOffset? DateStartedProgramme { get; set; }
    public DateTimeOffset? DateCompletedProgramme { get; set; }
    public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
    public string ProgrammeOutcome { get; set; }

    // Additional Properties
    public Guid? ProviderId { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public string Status { get; set; }
    public string StatusReason { get; set; }
    public string TriagedWeightedLevel { get; set; }
    public DateTimeOffset? DateToDelayUntil { get; set; }
    public bool? IsTelephoneValid { get; set; }
    public bool? IsMobileValid { get; set; }
    public long? ReferralAttachmentId { get; set; }
    public string Deprivation { get; set; }
    public string ServiceUserEthnicity { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
    public DateTimeOffset? DateLetterSent { get; set; }
    public int? MethodOfContact { get; set; }
    public int? NumberOfContacts { get; set; }
    public string StaffRole { get; set; }
    public string ReferralSource { get; set; }
    public string ProviderName { get; set; }
    public string DelayReason { get; set; }
  }
}