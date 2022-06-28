using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Api.Models
{
  public abstract class ReferralPostBase
  {
    [Required]
    [StringLength(12, MinimumLength = 12)]
    [RegularExpression(Constants.REGEX_NUMERIC_STRING, 
      ErrorMessage = "The field Ubrn is in an invalid fomat")]
    public string Ubrn { get; set; }
    [Required]
    [RegularExpression(Constants.REGEX_NUMERIC_STRING,
      ErrorMessage = "The field ServiceId is in an invalid fomat")]
    public string ServiceId { get; set; }
  }
}
