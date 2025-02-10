using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Attributes;

/// <summary>
/// When provided NHS Numbers must conform to NHS Guidance
/// https://digital.nhs.uk/services/nhs-number
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class NhsNumberAttribute(bool allowNulls = true) : ValidationAttribute
{
  public bool AllowNulls { get; set; } = allowNulls;

  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    string memberName = validationContext?.MemberName ?? "field name not found";

    if (value is null && AllowNulls)
    {
      return ValidationResult.Success;
    }
    else if (value is null && !AllowNulls)
    {
      return new ValidationResult(ErrorMessage ?? $"The field {memberName} cannot be null.");
    }

    List<string> errors = Validate(value.ToString(), memberName);

    if (errors.Count == 0)
    {
      return ValidationResult.Success;
    }

    string errorMessage = string.Join(",", errors);

    return new ValidationResult(ErrorMessage ?? errorMessage, [memberName]);
  }

  private readonly int[] _multiplier = new[] {10, 9, 8, 7, 6, 5, 4, 3, 2};

  private List<string> Validate(string number, string memberName)
  {
    Regex regex = new(@"^\d+$");
    List<string> errors = new();

    if (!regex.IsMatch(number) || number.Length != 10)
    {
      errors.Add($"The field {memberName} must be 10 numbers only.");
      return errors;
    }

    char[] chars = number.ToCharArray();
    int len = _multiplier.Length;
    List<int> numbers = new();

    for (var i = 0; i < len; i++)
    {
      numbers.Add(int.Parse(chars[i].ToString()) * _multiplier[i]);
    }

    int total = numbers.Sum();

    int remainder = total % 11;

    int checkDigit = int.Parse(chars[9].ToString());

    int testNumber = 11 - remainder;

    if (testNumber.Equals(11)) testNumber = 0;

    if (testNumber.Equals(10))
    {
      errors.Add($"The field {memberName} is invalid.");
      return errors;
    }

    if (testNumber.Equals(checkDigit))
    {
      return errors;
    }
    
    errors.Add($"The field {memberName} is invalid.");
    return errors;
  }
}