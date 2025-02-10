using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models;

public class OrganisationPutRequest
{
  [Required]
  public int QuotaTotal { get; set; }
  [Required]
  public int QuotaRemaining { get; set; }
}
