using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.GeneralReferral;

public class GeneralReferralCancelRequest
{
  [Required]
  public string Ethnicity { get; set; }

  public bool? HasActiveEatingDisorder { get; set; }

  public bool? HasHadBariatricSurgery { get; set; }

  [Range(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM)]
  public decimal HeightCm { get; set; }

  public bool? IsPregnant { get; set; }

  [Required]
  public string ServiceUserEthnicity { get; set; }

  [Required]
  public string ServiceUserEthnicityGroup { get; set; }

  [Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
  public decimal WeightKg { get; set; }
}