using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation
{
  public class RangeValidationResult : ValidationResult
  {
    public RangeValidationResult(string fieldName, double min, double max)
      : base($"The field {fieldName} must be between {min} and {max}.")
    { }
  }
}
