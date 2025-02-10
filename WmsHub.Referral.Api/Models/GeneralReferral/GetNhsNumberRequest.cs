using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models.GeneralReferral;
public class GetNhsNumberRequest
{
  [Required]
  [NhsNumber]
  public string NhsNumber { get; set; }
}