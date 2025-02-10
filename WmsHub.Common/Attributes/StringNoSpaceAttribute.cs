using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class StringNoSpaceAttribute : ValidationAttribute
  {
    public const string DefaultErrorMessage =
      "The {0} field must only contains AlphaNumeric characters and no spaces";

    public StringNoSpaceAttribute() : base(DefaultErrorMessage)
    {
    }

    public override bool IsValid(object value)
    {
      if (value is null)
      {
        return false;
      }

      string pattern = @"^[a-zA-Z0-9]*$";
      Regex regex = new Regex(pattern);
      
      return regex.IsMatch(value.ToString());
    }
  }
}