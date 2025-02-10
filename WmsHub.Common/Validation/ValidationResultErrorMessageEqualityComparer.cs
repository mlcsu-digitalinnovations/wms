using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Common.Validation;
public class ValidationResultErrorMessageEqualityComparer : IEqualityComparer<ValidationResult>
{
  public bool Equals(ValidationResult x, ValidationResult y)
  {
    return x.ErrorMessage == y.ErrorMessage;
  }

  public int GetHashCode([DisallowNull] ValidationResult obj)
  {
    return obj.ErrorMessage.GetHashCode();
  }
}
