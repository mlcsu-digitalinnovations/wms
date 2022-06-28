using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Common.Api.Models
{
  public class ReferralNhsNumberMismatchPost 
    : ReferralPostBase, IValidatableObject
  {
    [Required]
    [NhsNumber]
    public string NhsNumberWorkList { get; set; }
    [Required]
    [NhsNumber]
    public string NhsNumberAttachment { get; set; }
    public Enums.SourceSystem? SourceSystem { get; set; }
    public decimal? DocumentVersion { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (NhsNumberAttachment == NhsNumberWorkList)
      {
        yield return new ValidationResult(
          $"The {nameof(NhsNumberAttachment)} field must NOT match the " +
          $"{nameof(NhsNumberWorkList)} field.");
      }
    }
  }
}
