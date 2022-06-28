using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Extensions;

namespace WmsHub.Common.Attributes
{
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
      string[] membersNames;
      if (validationContext == null)
      {
        membersNames = new string[] { "Age" };
      }
      else
      {
        membersNames = new string[] { validationContext.MemberName };
      }

      if (value == null)
        return ValidationResult.Success;

      if (value.GetType() != typeof(DateTimeOffset))
      {
        return new ValidationResult($"The {membersNames[0]} " +
          "field must be of type DateTimeOffset", membersNames);
      }

      int age = ((DateTimeOffset)value).GetAge();

      if (age > _max || age < _min)
      {
        return new ValidationResult($"The {membersNames[0]} " +
          $"field must equate to an age between {_min} and {_max}.", 
          membersNames);
      }

      return ValidationResult.Success;
    }
  }
}