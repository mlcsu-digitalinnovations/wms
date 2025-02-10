using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes
{
  public class DeleteCancelledGpReferralRequest
  {
    [Required, MinLength(10)]
    public string Reason { get; set; }
  }
}
