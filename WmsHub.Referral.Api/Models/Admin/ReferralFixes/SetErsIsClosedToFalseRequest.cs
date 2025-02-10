using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes;

public class SetErsIsClosedToFalseRequest
{
  [Required, MinLength(12), MaxLength(12)]
  public string Ubrn { get; set; }
}