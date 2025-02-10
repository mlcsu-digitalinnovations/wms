using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin;

public class MskOrganisationPutRequest
{
  [Required]
  public bool SendDischargeLetters { get; set; }
  [Required]
  [StringLength(200)]
  public string SiteName { get; set; }
}
