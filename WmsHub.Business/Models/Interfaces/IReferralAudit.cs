using System;

namespace WmsHub.Business.Models
{
  public interface IReferralAudit
  {
    int AuditId { get; set; }
    string AuditAction { get; set; }
    int AuditDuration { get; set; }
    string AuditErrorMessage { get; set; }
    int AuditResult { get; set; }
    bool AuditSuccess { get; set; }
    string NhsNumber { get; set; }
    DateTimeOffset? DateOfReferral { get; set; }
    string ReferringGpPracticeNumber { get; set; }
    string Ubrn { get; set; }
    string FamilyName { get; set; }
    string GivenName { get; set; }
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    string Postcode { get; set; }
    string Telephone { get; set; }
    string Mobile { get; set; }
    string Email { get; set; }
    DateTimeOffset? DateOfBirth { get; set; }
    string Sex { get; set; }
    bool? IsVulnerable { get; set; }
    string VulnerableDescription { get; set; }
    bool? ConsentForFutureContactForEvaluation { get; set; }
    string Ethnicity { get; set; }
    bool? HasAPhysicalDisability { get; set; }
    bool? HasALearningDisability { get; set; }
    bool? HasRegisteredSeriousMentalIllness { get; set; }
    bool? HasHypertension { get; set; }
    bool? HasDiabetesType1 { get; set; }
    bool? HasDiabetesType2 { get; set; }
    decimal? HeightCm { get; set; }
    decimal? WeightKg { get; set; }
    decimal? CalculatedBmiAtRegistration { get; set; }
    DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    string TriagedCompletionLevel { get; set; }
    DateTimeOffset? DateOfProviderSelection { get; set; }
    DateTimeOffset? DateStartedProgramme { get; set; }
    DateTimeOffset? DateCompletedProgramme { get; set; }
    DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
    string ProgrammeOutcome { get; set; }
    Guid? ProviderId { get; set; }
    string ReferringGpPracticeName { get; set; }
    string Status { get; set; }
    string StatusReason { get; set; }
    string TriagedWeightedLevel { get; set; }
    DateTimeOffset? DateToDelayUntil { get; set; }
    bool? IsTelephoneValid { get; set; }
    bool? IsMobileValid { get; set; }
    long? ReferralAttachmentId { get; set; }
    string Deprivation { get; set; }
    string ServiceUserEthnicity { get; set; }
    string ServiceUserEthnicityGroup { get; set; }
    DateTimeOffset? DateLetterSent { get; set; }
    int? MethodOfContact { get; set; }
    int? NumberOfContacts { get; set; }
    string StaffRole { get; set; }
    string ReferralSource { get; set; }
    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}