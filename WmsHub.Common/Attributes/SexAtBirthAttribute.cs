using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace WmsHub.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SexAtBirthAttribute : ValidationAttribute
{
  private static readonly string[] s_sexAtBirthStringValues =
  [
    "F",
    "FEMALE",
    "M",
    "MALE",
    "NK",
    "NOT KNOWN",
    "NOT SPECIFIED",
    "NS"
  ];

  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    string fieldName = validationContext?.DisplayName ?? "SexAtBirth";

    if (value == null)
    {
      return ValidationResult.Success;
    }

    if (value.GetType() != typeof(string))
    {
      return new ValidationResult(
        $"The field {fieldName} must be of type string");
    }

    TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;
    string sexAtBirth = textInfo.ToUpper(value.ToString());

    if (s_sexAtBirthStringValues.Contains(sexAtBirth))
    {
      return ValidationResult.Success;
    }

    if (string.IsNullOrWhiteSpace(ErrorMessage))
    {
      ErrorMessage = $"The field {fieldName} must be one of the following: " +
        $"{string.Join(", ", s_sexAtBirthStringValues)}";
    }

    return new ValidationResult(ErrorMessage);
  }
}