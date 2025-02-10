using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Common.Extensions;
public static class ModelStateValidationExtension
{
  public static ValidationModel IsValid<T>(this T model)
  {
    ValidationModel validationModel = new();

    List<ValidationResult> validationResults = new();

    validationModel.IsValid = Validator.TryValidateObject(
      model,
      new ValidationContext(model),
      validationResults,
      validateAllProperties: true);

    if (!validationModel.IsValid)
    {
      validationModel.Errors = validationResults
        .Select(x => x.ErrorMessage).ToList();
    }

    return validationModel;
  }
}