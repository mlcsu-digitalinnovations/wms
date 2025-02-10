using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models
{
  public class Practice : BaseModel, IPractice, IValidatableObject
  {
    [EmailAddress]
    public string Email { get; set; }
    public string Name { get; set; }
    [Required, GpPracticeOdsCode]
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
