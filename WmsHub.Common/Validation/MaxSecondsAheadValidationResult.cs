using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation
{
  public class MaxSecondsAheadValidationResult : ValidationResult
  {
    public MaxSecondsAheadValidationResult(
      string fieldName, int maxSecondsAhead)
      : base($"The {fieldName} field is more than {maxSecondsAhead} " + 
          $"seconds(s) ahead of the server time {DateTimeOffset.Now}.")
    {
    }
  }
}
