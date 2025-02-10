using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IGeneralReferralCreate
  {
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    bool? ConsentForFutureContactForEvaluation { get; set; }
    DateTimeOffset DateOfBirth { get; set; }
    DateTimeOffset DateOfBmiAtRegistration { get; set; }
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
    decimal HeightCm { get; set; }
    string Mobile { get; set; }
    string Postcode { get; set; }
    string ServiceUserEthnicity { get; set; }
    string ServiceUserEthnicityGroup { get; set; }
    string Sex { get; set; }
    string Telephone { get; set; }
    decimal WeightKg { get; set; }
    bool ConsentForGpAndNhsNumberLookup { get; set; }
    bool ConsentForReferrerUpdatedWithOutcome { get; set; }
    bool? HasActiveEatingDisorder { get; set; }
    bool? HasArthritisOfHip { get; set; }
    bool? HasArthritisOfKnee { get; set; }
    bool? HasHadBariatricSurgery { get; set; }
    bool? IsPregnant { get; set; }
    string NhsLoginClaimEmail { get; set; }
    string NhsLoginClaimFamilyName { get; set; }
    string NhsLoginClaimGivenName { get; set; }
    string NhsLoginClaimMobile { get; set; }
    string NhsNumber { get; set; }
    string ReferringGpPracticeName { get; set; }
    string ReferringGpPracticeNumber { get; set; }

    decimal? HeightFeet { get; set; }
    decimal? HeightInches { get; set; }
    public UnitsType HeightUnits { get; set; }
    decimal? WeightPounds { get; set; }
    decimal? WeightStones { get; set; }
    public UnitsType WeightUnits { get; set; }

    IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
  }
}