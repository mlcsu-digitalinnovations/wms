using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class MustBeComplexAttribute: ValidationAttribute
  {
    private bool _isValid;
    protected override ValidationResult IsValid(
      object value, ValidationContext validationContext)
    {
      string[] memberNames;
      if (validationContext == null)
      {
        memberNames = new string[] { "" };
      }
      else
      {
        memberNames = new string[] { validationContext.MemberName };
      }

      if (value is null)
      {
        return new ValidationResult(
          $"The {validationContext.DisplayName} cannot be null.", memberNames);
      }

      List<string> errors = Validate(value.ToString());

      if (_isValid)
      {
        return ValidationResult.Success;
      }

      string errorMessage = string.Join(",", errors);

      return new ValidationResult(errorMessage, memberNames);

    }

    private List<string> Validate(string code)
    {
      List<string> errors = new List<string>();
      string _upperTest = "[A-Z]";
      string _lowerText = "[a-z]";
      string _numberTest = "[0-9]";
      string _specialTest = "[!#$%@<>?^~]";

      MatchCollection numMatches = Regex.Matches(code, _numberTest);
      MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
      MatchCollection upperMatches = Regex.Matches(code, _upperTest);
      MatchCollection specialMatches = Regex.Matches(code, _specialTest);

      if (numMatches.Count < 1)
        errors.Add("Complex code must contain at least 1 number.");

      if (lowerMatches.Count < 1)
        errors.Add("Complex code must contain at least 1 lower case letter.");

      if (upperMatches.Count < 1)
        errors.Add("Complex code must contain at least 1 upper case letter.");

      if (specialMatches.Count < 1)
        errors.Add("Complex code must contain at least 1 special character.");

      _isValid = !errors.Any();
      return errors;

    }
  }
}