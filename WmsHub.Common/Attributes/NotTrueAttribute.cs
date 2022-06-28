using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class NotTrueAttribute : ValidationAttribute
  {
    public const string DefaultErrorMessage =
      "The {0} field must not be true.";

    public NotTrueAttribute() : base(DefaultErrorMessage)
    { }

    public override bool IsValid(object value)
    {
      if (value == null || value.GetType() == typeof(bool))
      {
        return !(bool?)value ?? true;
      }
      else
      {
        return false;
      }
    }
  }
}