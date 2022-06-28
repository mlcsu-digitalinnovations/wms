using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class MaxDaysBehindAttribute : ValidationAttribute
  {
    private readonly int _days;

    public MaxDaysBehindAttribute(int days)
    {
      _days = -(Math.Abs(days));
    }


    protected override ValidationResult IsValid(
      object value,
      ValidationContext validationContext)
    {
      if (value == null)
      {
        return ValidationResult.Success;
      }
      string fieldName = validationContext?.DisplayName ?? "{unknown}";

      if (value.GetType() != typeof(DateTimeOffset))
      {
        return new ValidationResult($"The {fieldName} " +
          "field must be of type DateTimeOffset");
      }

      DateTimeOffset valueDateTimeOffset = (DateTimeOffset)value;
      DateTimeOffset minDateTimeOffset =
        DateTimeOffset.Now.AddDays(_days);

      if (minDateTimeOffset > valueDateTimeOffset)
      {
        return new ValidationResult($"The {fieldName} " +
            $"field is more than {_days} day(s) behind of the server " +
            $"time {DateTimeOffset.Now}.");
      }

      return ValidationResult.Success;
    }
  }
}