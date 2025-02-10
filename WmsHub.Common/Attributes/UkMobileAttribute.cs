using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UkMobileAttribute : ValidationAttribute
{
  private int _length;
  private bool _validateLength;
  public UkMobileAttribute()
  {
    _validateLength = false;
    _length = 0;
  }
  public UkMobileAttribute(
    int length,
    bool validateLength,
    string errorMessage)
  {
    _length = length;
    _validateLength = validateLength;
    ErrorMessage = errorMessage;
  }
  protected override ValidationResult IsValid(
    object value,
    ValidationContext validationContext)
  {
    string[] memberNames;

    if (validationContext == null)
    {
      memberNames = new string[] { "UkMobile" };
    }
    else
    {
      memberNames = new string[] { validationContext.MemberName };
    }

    if (value == null)
    {
      return ValidationResult.Success;
    }

    if (value.ToString().Replace(" ","").Length != _length && _validateLength)
    {
      return new ValidationResult("The field 'Mobile' does not contain enough" +
        " digits to be a valid UK mobile number.");
    }

    Regex regex = new(@"^(\+44)7\d{9}$");

    if (!regex.IsMatch(value.ToString()))
    {
      if (string.IsNullOrWhiteSpace(ErrorMessage))
      {
        return new ValidationResult(
          $"The field {memberNames[0]} must be a valid UK mobile number.",
          memberNames);
      }
      return new ValidationResult(ErrorMessage);
    }

    return ValidationResult.Success;
  }
}
