using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin;

public class MskOrganisationPostRequest
{
  private string _odsCode;

  [Required]
  [StringLength(5, MinimumLength = 5,
    ErrorMessage = "ODS code must be 5 characters.")]
  public string OdsCode
  {
    get => _odsCode;
    set => _odsCode = value.ToUpperInvariant();
  }
  [Required]
  public bool SendDischargeLetters { get; set; }
  [Required]
  [StringLength(maximumLength: 200)]
  public string SiteName { get; set; }
}
