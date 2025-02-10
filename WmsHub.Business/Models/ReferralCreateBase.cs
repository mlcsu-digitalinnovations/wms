using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models;

public abstract class ReferralCreateBase : IReferralCreateBase
{
  [Required]
  [StringLength(12, MinimumLength = 12)]
  [RegularExpression(Constants.REGEX_NUMERIC_STRING, 
    ErrorMessage = "The field Ubrn is in an invalid format.")]
  public string Ubrn { get; set; }
}
