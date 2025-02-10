using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models;

public class SpineTraceResult : IValidatableObject
{
  [Required]
  public Guid Id { get; set; }
  [NhsNumber]
  public string NhsNumber { get; set; }
  [GpPracticeOdsCode]
  public string GpPracticeOdsCode { get; set; }
  public string GpPracticeName { get; set; }

  public virtual bool IsTraceSuccessful =>
    !string.IsNullOrWhiteSpace(NhsNumber);

  public bool HasValidId => Id != Guid.Empty;

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (!string.IsNullOrWhiteSpace(NhsNumber) &&
      string.IsNullOrWhiteSpace(GpPracticeOdsCode))
    {
      yield return new ValidationResult($"The {nameof(GpPracticeOdsCode)} " +
        $"field cannot be null or empty if the {nameof(NhsNumber)} field " +
        $"is provided. Provide a {nameof(GpPracticeOdsCode)} of " +
        $"{Constants.UNKNOWN_GP_PRACTICE_NUMBER} if this is intentional.");
    }
    if (!string.IsNullOrWhiteSpace(NhsNumber) &&
      string.IsNullOrWhiteSpace(GpPracticeName))
    {
      yield return new ValidationResult($"The {nameof(GpPracticeName)} " +
        $"field cannot be null or empty if the {nameof(NhsNumber)} field " +
        $"is provided. Provide a {nameof(GpPracticeName)} of " +
        $"{Constants.UNKNOWN_GP_PRACTICE_NAME} if this is intentional.");
    }

    if (Id == Guid.Empty)
    {
      yield return new ValidationResult(
        $"The {nameof(Id)} field cannot be empty.");
    }
  }
}
