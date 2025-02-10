using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation
{
  public class RequiredValidationResult : ValidationResult
  {
    public RequiredValidationResult(string fieldName)
      : base($"The {fieldName} field is required.")
    { }
  }
}
