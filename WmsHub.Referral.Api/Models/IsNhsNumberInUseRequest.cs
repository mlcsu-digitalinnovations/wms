using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models
{
  public class IsNhsNumberInUseRequest
  {
    [Required]
    [NhsNumber]
    public string NhsNumber { get; set; }
  }
}