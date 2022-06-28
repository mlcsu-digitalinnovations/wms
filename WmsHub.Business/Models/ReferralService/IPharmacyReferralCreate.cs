using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IPharmacyReferralCreate
  {
    string ReferringPharmacyOdsCode { get; set; }
    string ReferringPharmacyEmail { get; set; }
    string ReferringGpPracticeNumber { get; set; }
    string ReferringGpPracticeName { get; set; }
    string NhsNumber { get; set; }
    string FamilyName { get; set; }
    string GivenName { get; set; }
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    string Postcode { get; set; }
    string Telephone { get; set; }
    string Mobile { get; set; }
    string Email { get; set; }
    DateTimeOffset DateOfBirth { get; set; }
    string Sex { get; set; }
    string Ethnicity { get; set; }
    string ServiceUserEthnicity { get; set; }
    string ServiceUserEthnicityGroup { get; set; }
    bool? HasAPhysicalDisability { get; set; }
    bool? HasALearningDisability { get; set; }
    bool? HasRegisteredSeriousMentalIllness { get; set; }
    bool? HasHypertension { get; set; }
    bool? HasDiabetesType1 { get; set; }
    bool? HasDiabetesType2 { get; set; }
    decimal HeightCm { get; set; }
    decimal WeightKg { get; set; }
    DateTimeOffset DateOfBmiAtRegistration { get; set; }
    decimal CalculatedBmiAtRegistration { get; set; }
    bool? ConsentForGpAndNhsNumberLookup { get; set; }
    bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    bool? IsVulnerable { get; set; }
    string VulnerableDescription { get; set; }
    bool? NhsNumberIsInUse { get; set; }
    bool ReferringPharmacyEmailIsValid { get; set; }
    bool ReferringPharmacyEmailIsWhiteListed { get; set; }
    bool EthnicityAndServiceUserEthnicityValid { get; set; }
    bool EthnicityAndGroupNameValid { get; set; }

    IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext);
  }
}