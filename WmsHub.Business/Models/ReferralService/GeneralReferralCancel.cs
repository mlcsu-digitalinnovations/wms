using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ReferralService;

public class GeneralReferralCancel : IGeneralReferralCancel, IValidatableObject
{
  [Required]
  public string Ethnicity { get; set; }

  public bool? HasActiveEatingDisorder { get; set; }

  public bool? HasHadBariatricSurgery { get; set; }

  [Range(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM)]
  public decimal HeightCm { get; set; }

  public bool? IsPregnant { get; set; }

  [Required]
  [NotEmpty]
  public Guid Id { get; set; }

  [Required]
  public string ServiceUserEthnicity { get; set; }

  [Required]
  public string ServiceUserEthnicityGroup { get; set; }

  [Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
  public decimal WeightKg { get; set; }

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (!Ethnicity.TryParseToEnumName<Enums.Ethnicity>(out _))
    {
      yield return new InvalidValidationResult(nameof(Ethnicity), Ethnicity);
    }
  }
}