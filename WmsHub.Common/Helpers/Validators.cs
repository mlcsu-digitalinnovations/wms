using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Common.Validation;

namespace WmsHub.Common.Helpers
{
  public static class Validators
  {
    public static ValidateModelResult ValidateModel(object model)
    {
      ValidationContext context = new ValidationContext(instance: model);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        model, context, result.Results, validateAllProperties: true);
      IValidatableObject validatable = (IValidatableObject)model;
      IEnumerable<ValidationResult> errors = validatable.Validate(context);

      foreach (ValidationResult error in errors)
      {
        ValidationResult found = result.Results
          .FirstOrDefault(t => t.ErrorMessage == error.ErrorMessage);
        if (found == null)
          result.Results.Add(error);
      }

      return result;
    }
  }
}