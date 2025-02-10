using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class MaxSecondsAheadAttribute : ValidationAttribute
  {
    private readonly int _seconds;

    public MaxSecondsAheadAttribute(int seconds = 300)
    {
      _seconds = seconds;
    }

    protected override ValidationResult IsValid(
      object value,
      ValidationContext validationContext)
    {
      string fieldName = validationContext?.DisplayName ?? string.Empty;

      if (value == null)
      {
        return ValidationResult.Success;
      }

      if (value.GetType() != typeof(DateTimeOffset))
      {
        return new ValidationResult(
          $"The field {fieldName} must be of type DateTimeOffset");
      }

      DateTimeOffset valueDateTimeOffset = (DateTimeOffset)value;
      DateTimeOffset maxDateTimeOffset = 
        DateTimeOffset.Now.AddMinutes(_seconds);

      if (valueDateTimeOffset > maxDateTimeOffset)
      {
        if (string.IsNullOrWhiteSpace(ErrorMessage))
        {
          ErrorMessage = $"The field {fieldName} is more " +
            $"than {_seconds} seconds(s) ahead of the server time " +
            $"{DateTimeOffset.Now}.";
        }
        return new ValidationResult(ErrorMessage);
      }

      return ValidationResult.Success;
    }
  }
}