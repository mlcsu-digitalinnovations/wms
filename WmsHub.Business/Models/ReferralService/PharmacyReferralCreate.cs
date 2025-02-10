using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Helpers;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ReferralService
{
  public class PharmacyReferralCreate : IValidatableObject,
    IPharmacyReferralCreate
  {
    private decimal _calculatedBmiAtRegistration;

    [PharmacyOdsCode()]
    public string ReferringPharmacyOdsCode { get; set; }

    [Required, EmailWithDomain("nhs.net")]
    public string ReferringPharmacyEmail { get; set; }
    public bool ReferringPharmacyEmailIsValid { get; set; }
    public bool ReferringPharmacyEmailIsWhiteListed { get; set; }
    public bool EthnicityAndServiceUserEthnicityValid { get; set; }
    public bool EthnicityAndGroupNameValid { get; set; }

    [Required, GpPracticeOdsCode]
    [MaxLength(450)]
    public string ReferringGpPracticeNumber { get; set; }
    [Required]
    public string ReferringGpPracticeName { get; set; }
    [Required]
    [NhsNumber]
    public string NhsNumber { get; set; }
    [Required, MinLength(1), MaxLength(200)]
    public string FamilyName { get; set; }
    [Required, MinLength(1), MaxLength(200)]
    public string GivenName { get; set; }
    [Required, MinLength(1), MaxLength(200)]
    public string Address1 { get; set; }
    [MaxLength(200)]
    public string Address2 { get; set; }
    [MaxLength(200)]
    public string Address3 { get; set; }
    private string _postcode;
    [Required(ErrorMessage = "The Postcode field is not a valid postcode.")]
    public string Postcode
    {
      get => _postcode;
      set => _postcode = value.ConvertToPostcode();
    }
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH,
      ErrorMessage = "The field Telephone is not a valid telephone number.")]
    [MaxLength(200)]
    public string Telephone { get; set; }
    [Required]
    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK,
      ErrorMessage = "The field Mobile is not a valid mobile number.")]
    [MaxLength(200)]
    public string Mobile { get; set; }
    [Required]
    [MaxLength(200)]
    public string Email { get; set; }
    [Required, MaxSecondsAhead]
    [AgeRange(Constants.MIN_PHARMACY_REFERRAL_AGE,
      Constants.MAX_PHARMACY_REFERRAL_AGE)]
    public DateTimeOffset DateOfBirth { get; set; }
    [Required]
    public string Sex { get; set; }
    [Required, MinLength(1), MaxLength(200)]
    public string Ethnicity { get; set; }
    [Required]
    [MaxLength(200)]
    public string ServiceUserEthnicity { get; set; }
    [Required]
    [MaxLength(200)]
    public string ServiceUserEthnicityGroup { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    [Required]
    public bool? HasHypertension { get; set; }
    [Required]
    public bool? HasDiabetesType1 { get; set; }
    [Required]
    public bool? HasDiabetesType2 { get; set; }
    [Required, Range(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM)]
    public decimal HeightCm { get; set; }
    [Required, Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
    public decimal WeightKg { get; set; }
    [Required, 
      MaxDaysBehind(Constants.MAX_DAYS_BMI_DATE_AT_REGISTRATION), 
      MaxSecondsAhead]
    public DateTimeOffset DateOfBmiAtRegistration { get; set; }
    [Required, Range(27.5, 90)]
    public decimal CalculatedBmiAtRegistration
    {
      get => _calculatedBmiAtRegistration;
      set => _calculatedBmiAtRegistration = Math.Round(value, 1);
    }
    [Required]
    public bool? ConsentForGpAndNhsNumberLookup { get; set; }
    [Required]
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    public bool? IsVulnerable { get; set; }
    [MaxLength(200)]
    public string VulnerableDescription { get; set; }

    public bool? NhsNumberIsInUse { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      decimal calculatedBmi = BmiHelper.CalculateBmi(WeightKg, HeightCm);
      if (calculatedBmi != CalculatedBmiAtRegistration)
      {
        yield return new InvalidValidationResult(
          nameof(CalculatedBmiAtRegistration),
          CalculatedBmiAtRegistration,
          $"Expected a BMI of {calculatedBmi}.");
      }

      if (string.IsNullOrWhiteSpace(Ethnicity))
      {
        yield return new RequiredValidationResult(nameof(Ethnicity));
      }
      else
      {
        if (!Ethnicity.TryParseToEnumName<Enums.Ethnicity>(
          out var resultEthnicity))
        {
          yield return
            new InvalidValidationResult(nameof(Ethnicity), Ethnicity);
        }

        if (!EthnicityAndServiceUserEthnicityValid && 
            !EthnicityAndGroupNameValid)
        {
          yield return
            new InvalidValidationResult(nameof(Ethnicity),
              "The ethnicty does not match the serviceUserEthnicity and" +
              " the serviceUserEthnicityGroup properties.");
        }
        else if(EthnicityAndServiceUserEthnicityValid &&
                 !EthnicityAndGroupNameValid)
        {
          yield return
            new InvalidValidationResult(nameof(Ethnicity),
              "The ethnicty does not match the serviceUserEthnicityGroup" +
              " properties.");
        }
        else if (!EthnicityAndServiceUserEthnicityValid &&
                 EthnicityAndGroupNameValid)
        {
          yield return
            new InvalidValidationResult(nameof(Ethnicity),
              "The ethnicty does not match the serviceUserEthnicity " +
              "properties.");
        }
      }

      if (!Sex.IsValidSexString())
      {
        yield return new InvalidValidationResult(nameof(Sex), Sex);
      }

      if (ConsentForGpAndNhsNumberLookup == null ||
          !ConsentForGpAndNhsNumberLookup.Value)
        yield return new InvalidValidationResult(
          nameof(ConsentForGpAndNhsNumberLookup),
          "Consent is required to continue with this referral.");

      if ((HasDiabetesType1 ?? false) == false &&
          (HasDiabetesType2 ?? false) == false &&
          (HasHypertension ?? false) == false)
      {
        yield return new InvalidValidationResult(
          "HasDiabetesType1, HasDiabetesType2, HasHypertension",
          "A diagnosis of Diabetes Type 1 or Diabetes Type 2 or " +
          "Hypertension is required.");
      }

      if (IsVulnerable.HasValue && IsVulnerable.Value)
      {
        if (string.IsNullOrWhiteSpace(VulnerableDescription))
          yield return new InvalidValidationResult(
            nameof(VulnerableDescription),
            VulnerableDescription);
      }

      if (string.IsNullOrWhiteSpace(Telephone) &&
          string.IsNullOrWhiteSpace(Mobile))
      {
        yield return new InvalidValidationResult("Telephone, Mobile",
         $"One of the fields: {nameof(Telephone)} or {nameof(Mobile)} " +
         $"is required.");
      }
      else
      {
        if ((string.IsNullOrWhiteSpace(Mobile) ||
             !Mobile.IsUkMobile()) &&
            Telephone.IsUkMobile())
        {
          string tempNumber = Mobile;
          Mobile = Telephone;
          Telephone = tempNumber;
        }
        else if (Mobile.IsUkMobile() &&
                 Telephone.IsUkMobile())
        {
          Telephone = string.Empty;
        }
      }

      if (NhsNumberIsInUse != null &&
         !string.IsNullOrWhiteSpace(NhsNumber) &&
         NhsNumberIsInUse.Value)
        yield return new ValidationResult(
          "The supplied NHS Number is already registered in the " +
          "Weight Management System.", new[] { "NhsNumber" });

      if (!ReferringPharmacyEmailIsWhiteListed &&
         !string.IsNullOrWhiteSpace(ReferringPharmacyEmail))
        yield return new ValidationResult(
          $"Email {ReferringPharmacyEmail} is not in white list.",
          new[] { "ReferringPharmacyEmail" });

      if (!ReferringPharmacyEmailIsValid &&
          !string.IsNullOrWhiteSpace(ReferringPharmacyEmail))
        yield return new ValidationResult(
          $"Email {ReferringPharmacyEmail} is not in pharmacy list",
          new[] { "ReferringPharmacyEmailIsValid" });
    }
  }
}