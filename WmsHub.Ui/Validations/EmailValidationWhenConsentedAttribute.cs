#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Common.Helpers;
using WmsHub.Ui.Models;

namespace WmsHub.Ui.Validations
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class EmailValidationWhenConsentedAttribute: ValidationAttribute
  {
    public string GetErrorMessage() =>
      $"Enter your email address";

    protected override ValidationResult? IsValid(object? value, 
      ValidationContext validationContext)
    {
      ContactModel model = (ContactModel) validationContext.ObjectInstance;
      if(model.DontContactByEmail) return ValidationResult.Success;

      if(string.IsNullOrEmpty(model.Email))
        return new ValidationResult(
          "Enter your email address to continue with your referral");

      if(!RegexUtilities.IsValidEmail(model.Email))
        return new ValidationResult(
          "Enter an email address in the correct format, like " + 
          "name@example.com");

      return ValidationResult.Success;
    }
  }
}
