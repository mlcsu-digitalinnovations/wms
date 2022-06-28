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
  public class EmailWithDomainAttribute : ValidationAttribute
  {
    private bool _isValid;

    public string[] Domains { get; set; }

    public EmailWithDomainAttribute(string domain)
    {
      Domains = new[] {domain};
    }

    protected override ValidationResult IsValid(
      object value, ValidationContext validationContext)
    {
      if (value is null)
      {
        return new ValidationResult(
          $"The {validationContext.DisplayName} cannot be null.");
      }

      List<string> errors = Validate(value.ToString());

      if (_isValid)
      {
        return ValidationResult.Success;
      }

      string errorMessage = string.Join(",", errors);

      return new ValidationResult(errorMessage, new[] { validationContext.MemberName });

    }

    private List<string> Validate(string email)
    {
      List<string> errors = new List<string>();

      if (!RegexUtilities.IsValidEmail(email))
      {
        errors.Add($"The Email is not a valid email.");
      }

      bool isNhsDomain = false;

      foreach (string d in Domains)
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