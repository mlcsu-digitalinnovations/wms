using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation;

public class InvalidValidationResult : ValidationResult
{
  public InvalidValidationResult(string fieldName, object fieldValue)
    : base($"The {fieldName} field '{fieldValue}' is invalid.", [fieldName])
  { }

  public InvalidValidationResult(
    string fieldName,
    object fieldValue,
    string additionalText)
    : base($"The {fieldName} field '{fieldValue}' is invalid. {additionalText}", [fieldName])
  { }
}
