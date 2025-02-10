using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Common.Extensions;
using WmsHub.Common.Validation;

namespace WmsHub.Common.Helpers;

public static class Validators
{
  public static ValidateModelResult ValidateModel(object model)
  {
    if (model is not IValidatableObject)
    {
      throw new ValidationException($"{nameof(model)} cannot be validated.");
    }

    ValidationContext context = new(instance: model);

    ValidateModelResult result = new();
    result.IsValid = Validator.TryValidateObject(
      model, context, result.Results, validateAllProperties: true);
    IValidatableObject validatable = (IValidatableObject)model;
    IEnumerable<ValidationResult> errors = validatable.Validate(context);

    result.Results = result.Results
      .Union(errors, new ValidationResultErrorMessageEqualityComparer())
      .ToList();

    return result;
  }

  public static IEnumerable<InvalidValidationResult>
    ValidateStringIsNotNullOrWhiteSpace(
      string propertyName,
      string value,
      string[] expected)
  {
    if (value.IsValueInExpectedListOrNull(expected))
    {
      yield return new InvalidValidationResult(propertyName, value);
    }
  }
}