using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Attributes
{
  /// <summary>
  /// When provided NHS Numbers must conform to NHS Guidance
  /// https://digital.nhs.uk/services/nhs-number
  /// </summary>
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class NhsNumberAttribute : ValidationAttribute
  {
    public bool AllowNulls { get; set; }


    public NhsNumberAttribute(bool allowNulls = true)
    {
      AllowNulls = allowNulls;
    }

    protected override ValidationResult IsValid(
      object value, ValidationContext validationContext)
    {
      string[] membersNames;
      if (validationContext == null)
      {
        membersNames = new string[] { "NhsNumber" };
      }
      else
      {
        membersNames = new string[] { validationContext.MemberName };
      }

      if (value is null && AllowNulls)
      {
        return ValidationResult.Success;
      }
      else if (value is null && !AllowNulls)
      {
        return new ValidationResult(
          $"The {membersNames[0]} cannot be null.");
      }

      List<string> errors = Validate(value.ToString());

      if(errors.Count == 0)
        return ValidationResult.Success;

      string errorMessage = string.Join(",", errors);

      return new ValidationResult(errorMessage, membersNames);
    }

    private int[] multiplier = new[] {10, 9, 8, 7, 6, 5, 4, 3, 2};

    private List<string> Validate(string number)
    {
      Regex regex = new Regex(@"^\d+$");
      List<string> errors = new List<string>();

      if (!regex.IsMatch(number) || number.Length != 10)
      {
        errors.Add("The NhsNumber must be 10 numbers only, " +
                    "remove any spaces or dashes.");
        return errors;
      }

      char[] chars = number.ToCharArray();
      int len = multiplier.Length;
      List<int> numbers = new List<int>();

      for (var i = 0; i < len; i++)
      {
        numbers.Add(int.Parse(chars[i].ToString()) * multiplier[i]);
      }

      int total = numbers.Sum();

      int remainder = total % 11;

      int checkDigit = int.Parse(chars[9].ToString());

      int testNumber = 11 - remainder;

      if (testNumber.Equals(11)) testNumber = 0;

      if (testNumber.Equals(10))
      {
        errors.Add("The NhsNumber is not valid.");
        return errors;
      }

      if (testNumber.Equals(checkDigit))
      {
        return errors;
      }
      
      errors.Add("The NhsNumber is not valid.");
      return errors;

    }
  }
}