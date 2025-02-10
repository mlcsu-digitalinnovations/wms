using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Attributes;

public class DateInFuture: ValidationAttribute
{
  protected override ValidationResult IsValid(
    object value,
    ValidationContext validationContext)
  {
    if (value is null)
    {
      return ValidationResult.Success;
    }

    string[] memberNames;

    if (validationContext == null)
    {
      memberNames = new string[] { "DateOfBirth" };
    }
    else
    {
      memberNames = new string[] { validationContext.MemberName };
    }

    DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset date);

    if (date > DateTimeOffset.Now)
    {
      if (string.IsNullOrWhiteSpace(ErrorMessage))
      {
        return new ValidationResult(
          $"The field '{memberNames[0]}' cannot be in the future.",
          memberNames);
      }
      return new ValidationResult(ErrorMessage);
    }

    return ValidationResult.Success;
  }
}
