using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ReferralService
{
  public abstract class AGeneralReferralCreate
    : AReferralCreate, IValidatableObject
  {
    [Required, NhsNumber]
    public string NhsNumber { get; set; }
    public bool? HasArthritisOfKnee { get; set; }
    public bool? HasArthritisOfHip { get; set; }
    public bool? IsPregnant { get; set; }
    public bool? HasActiveEatingDisorder { get; set; }
    public bool? HasHadBariatricSurgery { get; set; }

    public string ReferringGpPracticeNumber { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public bool ConsentForGpAndNhsNumberLookup { get; set; }
    public bool ConsentForReferrerUpdatedWithOutcome { get; set; }
    [Required]
    public bool? ConsentForFutureContactForEvaluation { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    public string NhsLoginClaimFamilyName { get; set; }
    
    [Required, MinLength(1), MaxLength(200)]
    public string NhsLoginClaimGivenName { get; set; }
    
    [MaxLength(200),
      RegularExpression(Constants.REGEX_MOBILE_PHONE_UK,
      ErrorMessage = "The field Mobile is not a valid UK mobile number.")]
    public string NhsLoginClaimMobile { get; set; }

    [Required, EmailAddress, MaxLength(200)]
    public virtual string NhsLoginClaimEmail { get; set; }

    [Range(Constants.MIN_HEIGHT_FEET, Constants.MAX_HEIGHT_FEET)]
    public decimal? HeightFeet { get; set; }
    [Range(Constants.MIN_HEIGHT_INCHES, Constants.MAX_HEIGHT_INCHES)]
    public decimal? HeightInches { get; set; }
    public UnitsType HeightUnits { get; set; }
    [Range(Constants.MIN_WEIGHT_STONES, Constants.MAX_WEIGHT_STONES)]
    public decimal? WeightStones { get; set; }
    [Range(Constants.MIN_WEIGHT_POUNDS, Constants.MAX_WEIGHT_POUNDS)]
    public decimal? WeightPounds { get; set; }
    public UnitsType WeightUnits { get; set; }

    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      base.Validate(validationContext);

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
      }

      if (string.IsNullOrWhiteSpace(ReferringGpPracticeNumber))
      {
        yield return new
          RequiredValidationResult(nameof(ReferringGpPracticeNumber));
      }

      if (!Sex.IsValidSexString())
      {
        yield return new InvalidValidationResult(nameof(Sex), Sex);
      }

      if (ConsentForFutureContactForEvaluation == null)
      {
        yield return new InvalidValidationResult(
          nameof(ConsentForFutureContactForEvaluation),
          ConsentForFutureContactForEvaluation);
      }

      if (!ConsentForGpAndNhsNumberLookup)
      {
        yield return new ValidationResult(
          "ConsentForGpAndNhsNumberLookup must be true for the referral to " +
            $"be eligible.",
          new[] { "ConsentForGpAndNhsNumberLookup" });
      }

      if (IsPregnant.HasValue && IsPregnant.Value)
      {
        yield return new ValidationResult(
          $"{nameof(IsPregnant)} must not be true for the referral to be " +
            $"eligible.",
          new[] { nameof(IsPregnant) });
      }

      if (HasActiveEatingDisorder.HasValue && HasActiveEatingDisorder.Value)
      {
        yield return new ValidationResult(
           $"{nameof(HasActiveEatingDisorder)} must not be true for the " +
            $"referral to be eligible.",
          new[] { nameof(HasActiveEatingDisorder) });
      }

      if (HasHadBariatricSurgery.HasValue && HasHadBariatricSurgery.Value)
      {
        yield return new ValidationResult(
          $"{nameof(HasHadBariatricSurgery)} must not be true for the " +
            $"referral to be eligible.",
          new[] { nameof(HasHadBariatricSurgery) });
      }
    }
  }
}
