using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes
{
  public class ReasonStatusUbrnRequestBase
  {
    [Required]
    public string CurrentStatus { get; set; }
    [Required, MinLength(10)]
    public string Reason { get; set; }
    [Required, MinLength(12), MaxLength(12)]
    public string Ubrn { get; set; }
  }
}