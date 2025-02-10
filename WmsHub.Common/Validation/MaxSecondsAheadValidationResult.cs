using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Validation;

public class MaxSecondsAheadValidationResult(
  string fieldName,
  int maxSecondsAhead)
    : ValidationResult(
      $"The field {fieldName} is more than {maxSecondsAhead} seconds(s) ahead of the server " +
        $"time {DateTimeOffset.Now}.")
{ }
