using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false)]
  public class MustBeTrueAttribute : ValidationAttribute
  {
    public const string DefaultErrorMessage =
      "The {0} field must be true.";

    public MustBeTrueAttribute() : base(DefaultErrorMessage)
    { }

    public override bool IsValid(object value)
    {
      if (value == null || value.GetType() == typeof(bool))
      {
        return (bool?)value ?? false;
      }
      else
      {
        return false;
      }
    }
  }
}