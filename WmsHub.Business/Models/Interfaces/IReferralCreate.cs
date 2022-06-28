using System;

namespace WmsHub.Business.Models
{
  public interface IReferralCreate : IReferralCreateBase
  {
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    decimal? CalculatedBmiAtRegistration { get; set; }
    DateTimeOffset? DateOfBirth { get; set; }
    DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    DateTimeOffset? DateOfReferral { get; set; }
    string Email { get; set; }
    string Ethnicity { get; set; }
    string FamilyName { get; set; }
    string GivenName { get; set; }
    bool? HasALearningDisability { get; set; }
    bool? HasAPhysicalDisability { get; set; }
    bool? HasDiabetesType1 { get; set; }
    bool? HasDiabetesType2 { get; set; }
    bool? HasHypertension { get; set; }
    bool? HasRegisteredSeriousMentalIllness { get; set; }
    decimal? HeightCm { get; set; }
    bool? IsVulnerable { get; set; }
    string Mobile { get; set; }
    string NhsNumber { get; set; }
    string Postcode { get; set; }
    string ReferringGpPracticeName { get; set; }
    string ReferringGpPracticeNumber { get; set; }
    long? ReferralAttachmentId { get; set; }
    long? MostRecentAttachmentId { get; set; }
    string Sex { get; set; }
    string Telephone { get; set; }
    decimal? DocumentVersion { get; set; }
    Common.Enums.SourceSystem? SourceSystem { get; set; }
    string ServiceId { get; set; }
    string VulnerableDescription { get; set; }
    decimal? WeightKg { get; set; }
    string CriDocument { get; set; }
    DateTimeOffset? CriLastUpdated { get; set; }
  }
}