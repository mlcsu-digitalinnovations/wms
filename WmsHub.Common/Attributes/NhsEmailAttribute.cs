using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class NhsEmailAttribute : ValidationAttribute
  {
    private bool _isValid;
    protected override ValidationResult IsValid(
      object value, ValidationContext validationContext)
    {
      string[] membersNames;
      if (validationContext == null)
      {
        membersNames = new string[] { "Email" };
      }
      else
      {
        membersNames = new string[] { validationContext.MemberName };
      }

      if (value is null)
      {
        return new ValidationResult(
          $"The {membersNames[0]} cannot be null.");
      }

      List<string> errors = Validate(value.ToString());

      if (_isValid)
      {
        return ValidationResult.Success;
      }

      string errorMessage = string.Join(",", errors);

      return new ValidationResult(errorMessage, membersNames);

    }

    private List<string> Validate(string email)
    {
      List<string> errors = new List<string>();

      if (!RegexUtilities.IsValidEmail(email))
      {
        errors.Add($"The Email is not a valid email.");
      }

      bool isNhsDomain = false;

      foreach (string d in Constants.NHS_DOMAINS)
      {
        if (email.EndsWith(d))
        {
          isNhsDomain = true;
          break;
        }
      }

      if (!isNhsDomain)
      {
        errors.Add($"The Email is not a valid NHS email.");
      }

      _isValid = !errors.Any();
      return errors;

    }
  }
}