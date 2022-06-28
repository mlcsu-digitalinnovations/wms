using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Common.Extensions
{
  public static class ModelStateValidationExtension
  {
    public static ValidationModel IsValid<T>(this T model)
    {
      var result = new ValidationModel();
      var context = new System.ComponentModel.DataAnnotations
         .ValidationContext(model);

      List<ValidationResult> validationResults = new List<ValidationResult>();

      result.IsValid = Validator.TryValidateObject(model, context,
        validationResults, true);
      if (!result.IsValid)
        result.Errors = validationResults.Select(x => x.ErrorMessage).ToList();

      return result;
    }

  }

}