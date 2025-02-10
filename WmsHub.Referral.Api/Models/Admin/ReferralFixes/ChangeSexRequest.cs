using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes;

public class ChangeSexRequest
{
  public string OriginalSex { get; set; }
  [Required]
  public required string Ubrn { get; set; }
  [Required]
  public required string UpdatedSex { get; set; }
}
