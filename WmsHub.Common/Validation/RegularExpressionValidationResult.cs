using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation
{
  public class RegularExpressionValidationResult : ValidationResult
  {
    public RegularExpressionValidationResult(
      string fieldName, string pattern)
      : base($"The field {fieldName} must match the regular expression " + 
          $"'{pattern}'.")
    {
    }
  }
}
