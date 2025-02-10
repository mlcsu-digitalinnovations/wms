using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.GeneralReferral
{
  public abstract class AReferralPostPutRequest :
    AReferralPostRequest, IValidatableObject
  {
    // implied consent via NHS login
    public bool ConsentForGpAndNhsNumberLookup = true;

    [Required, NhsNumber]
    public string NhsNumber { get; set; }
    public bool? HasArthritisOfKnee { get; set; }
    public bool? HasArthritisOfHip { get; set; }
    public bool? IsPregnant { get; set; }
    public bool? HasActiveEatingDisorder { get; set; }
    public bool? HasHadBariatricSurgery { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    [Required]
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    public string NhsLoginClaimFamilyName { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    public string NhsLoginClaimGivenName { get; set; }

    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK,
      ErrorMessage = "The field Mobile is not a valid UK mobile number.")]
    [MaxLength(200)]
    public string NhsLoginClaimMobile { get; set; }

    [Required, EmailAddress]
    [MaxLength(200)]
    public virtual string NhsLoginClaimEmail { get; set; }

    public decimal? HeightFeet { get; set; }
    public decimal? HeightInches { get; set; }
    public UnitsType HeightUnits { get; set; }
      = UnitsType.Metric;

    public decimal? WeightStones { get; set; }
    public decimal? WeightPounds { get; set; }
    public UnitsType WeightUnits { get; set; }
      = UnitsType.Metric;

    [Required]
    public bool? ConsentForFutureContactForEvaluation { get; set; }

    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      base.Validate(validationContext);

      if (IsPregnant.HasValue && IsPregnant.Value)
      {
        yield return new ValidationResult(
          "IsPregnant must be null or false for the referral to be eligible.",
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

      if (HeightUnits == UnitsType.Imperial)
      {
        if (HeightFeet == null || HeightInches == null)
        {
          yield return new ValidationResult(
            $"{nameof(HeightUnits)} is {UnitsType.Imperial} " +
              $"so {nameof(HeightFeet)} and {nameof(HeightInches)} must " +
              $"not be null.",
            new[] 
            { 
              nameof(HeightUnits), 
              nameof(HeightFeet), 
              nameof(HeightInches) 
            });
        }
      }

      if (WeightUnits == UnitsType.Imperial)
      {
        if (WeightStones == null || WeightPounds == null)
        {
          yield return new ValidationResult(
            $"{nameof(WeightUnits)} is {UnitsType.Imperial} " +
              $"so {nameof(WeightStones)} and {nameof(WeightPounds)} must " +
              $"not be null.",
            new[]
            {
              nameof(WeightUnits),
              nameof(WeightStones),
              nameof(WeightPounds)
            });
        }
      }
    }
  }
}