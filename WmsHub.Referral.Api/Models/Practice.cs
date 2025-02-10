using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;

namespace WmsHub.Referral.Api.Models
{
  public class Practice : IValidatableObject
  {
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; }
    [MaxLength(200)]
    public string Name { get; set; }
    [Required, GpPracticeOdsCode]
    [MaxLength(450)]
    public string OdsCode { get; set; }
    [Required]
    public string SystemName { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (!SystemName.TryParseToEnumName<PracticeSystemName>(out _))
      {
        yield return new ValidationResult(
          $"The {nameof(SystemName)} field '{SystemName}' must be one of " +
          "the following values [" +
          $"{string.Join(',', Enum.GetNames<PracticeSystemName>())}].",
          new string[] { nameof(SystemName) });
      }
    }
  }
}
