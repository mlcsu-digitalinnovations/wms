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

      string[] memberNames;

      if (validationContext == null)
      {
        memberNames = new string[] { "MaxDaysBehind" };
      }
      else
      {
        memberNames = new string[] { validationContext.MemberName };
      }

      if (value == null)
      {
        return ValidationResult.Success;
      }

      if (value.GetType() != typeof(DateTimeOffset))
      {
        return new ValidationResult(
          $"The field {memberNames[0]} must be of type DateTimeOffset.");
      }

      DateTimeOffset valueDateTimeOffset = (DateTimeOffset)value;

      DateTimeOffset minDateTimeOffset = DateTimeOffset.Now.AddDays(_days);

      if (minDateTimeOffset > valueDateTimeOffset)
      {
        if (string.IsNullOrWhiteSpace(ErrorMessage))
        {
          ErrorMessage = $"The field {memberNames[0]} is more than {_days} " +
            $"day(s) behind the server time {DateTimeOffset.Now}.";
        }
        return new ValidationResult(ErrorMessage, memberNames);
      }

      return ValidationResult.Success;
    }
  }
}