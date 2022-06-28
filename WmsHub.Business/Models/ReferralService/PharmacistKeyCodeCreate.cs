using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService
{
  public class PharmacistKeyCodeCreate : IPharmacistKeyCodeCreate
  {
    [Required]
    public string ReferringPharmacyEmail { get; set; }
    [Required]
    public string KeyCode { get; set; }
    [Required, Range(1, 1440)]
    public int ExpireMinutes { get; set; }
  }
}