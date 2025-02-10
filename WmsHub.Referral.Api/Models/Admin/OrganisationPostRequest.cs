using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin;

public class OrganisationPostRequest
{
  private string _odsCode;

  [Required]
  public string OdsCode { get => _odsCode; set => _odsCode = value.ToUpper(); }
  [Required]
  public int QuotaTotal { get; set; }
  [Required]
  public int QuotaRemaining { get; set; }
}
