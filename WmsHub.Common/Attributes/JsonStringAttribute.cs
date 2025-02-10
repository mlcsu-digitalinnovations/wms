using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WmsHub.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class JsonStringAttribute : ValidationAttribute
{
  protected override ValidationResult IsValid(
      object value,
      ValidationContext validationContext)
  {

    if (value == null)
    {
      return ValidationResult.Success;
    }

    string[] memberNames;

    if (validationContext == null)
    {
      memberNames = new string[] { "JsonString" };
    }
    else
    {
      memberNames = new string[] { validationContext.MemberName };
    }

    if (value.GetType() != typeof(string))
    {
      return new ValidationResult($"The {memberNames[0]} " +
        "field must be of type string.", memberNames);
    }
    try
    {
      JsonSerializer.Deserialize<dynamic>(value.ToString());
    } 
    catch
    {
      return new ValidationResult($"The {memberNames[0]} " +
        $"must be valid json.",
        memberNames);
    }

    return ValidationResult.Success;
  }
}
