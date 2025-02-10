using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Extensions;

namespace WmsHub.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AgeRange : ValidationAttribute
{
  private readonly int _min;
  private readonly int _max;

  public AgeRange(int min = 18, int max = 110)
  {
    _min = min;
    _max = max;
  }

  protected override ValidationResult IsValid(
    object value,
    ValidationContext validationContext)
  {
    if (value == null)
    {
      return ValidationResult.Success;
    }

    string[] membersNames;
    if (validationContext == null)
    {
      membersNames = new string[] { "Age" };
    }
    else
    {
      membersNames = new string[] { validationContext.MemberName };
    }

    int age;
    if (value.GetType() == typeof(DateTimeOffset))
    {
      age = ((DateTimeOffset)value).GetAge();
    }
    else if (value.GetType() == typeof(DateTime))
    {
      age = ((DateTime)value).GetAge();
    }
    else
    {
      return new ValidationResult(
        $"The field {membersNames[0]} must be of type DateTime or " +
          $"DateTimeOffset.",
        membersNames);
    }

    if (age > _max || age < _min)
    {
      if (string.IsNullOrWhiteSpace(ErrorMessage))
      {
        return new ValidationResult(
          $"The field {membersNames[0]} must equate to an age between " +
            $"{_min} and {_max}.",
          membersNames);
      }
      return new ValidationResult(ErrorMessage);
    }

    return ValidationResult.Success;
  }
}
