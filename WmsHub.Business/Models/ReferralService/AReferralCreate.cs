using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ReferralService
{
  public abstract class AReferralCreate : IAReferralCreate, IValidatableObject
  {
    private string _postcode;

    [Required]
    [MaxLength(200)]
    public string FamilyName { get; set; }

    [Required]
    [MaxLength(200)]
    public string GivenName { get; set; }

    [Required]
    [MaxLength(200)]
    public string Address1 { get; set; }

    [MaxLength(200)]
    public string Address2 { get; set; }

    [MaxLength(200)]
    public string Address3 { get; set; }

    
    [Required]
    [RegularExpression(StringExtensions.POSTCODE_REGEX,
       ErrorMessage = "The Postcode field is invalid.")]
    public string Postcode
    {
      get => _postcode;
      set => _postcode = value.ConvertToPostcode();
    }

    [MaxLength(200)]
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH,
      ErrorMessage = "The Telephone field is invalid.")]
    public string Telephone { get; set; }

    [Required]
    [MaxLength(200)]
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH,
      ErrorMessage = "The Mobile field is invalid.")]
    public string Mobile { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public virtual string Email { get; set; }

    [Required]
    [MaxSecondsAhead]
    [AgeRange(Constants.MIN_SELF_REFERRAL_AGE, Constants.MAX_SELF_REFERRAL_AGE)]
    public DateTimeOffset DateOfBirth { get; set; }

    [Required]
    [MaxLength(200)]
    public string Sex { get; set; }

    [Required]
    [MaxLength(200)]
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

    public bool? HasHypertension { get; set; }

    public bool? HasDiabetesType1 { get; set; }

    public bool? HasDiabetesType2 { get; set; }

    [Range(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM)]
    public decimal HeightCm { get; set; }

    [Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
    public decimal WeightKg { get; set; }

    [Required]
    [MaxDaysBehind(Constants.MAX_DAYS_BMI_DATE_AT_REGISTRATION)]
    [MaxSecondsAhead]
    public DateTimeOffset DateOfBmiAtRegistration { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (!Ethnicity.TryParseToEnumName<Enums.Ethnicity>(out _))
      {
        yield return new InvalidValidationResult(nameof(Ethnicity), Ethnicity);
      }
      if (!Sex.TryParseToEnumName<Enums.Sex>(out _))
      {
        yield return new InvalidValidationResult(nameof(Sex), Sex);
      }
    }
  }
}