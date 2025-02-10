using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using WmsHub.Common.Extensions;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class PharmacyOdsCode : ValidationAttribute
  {
    private const string PATTERN_P = @"P[A-Z,0-9][A-Z,0-9][A-Z,0-9]";
    private const string PATTERN_F = @"F[A-Y][A-Z,0-9][A-Z,0-9][A-Z,0-9]";
    public bool IncludeHQs { get; set; }
    public bool AllowNull { get; set; }

    public PharmacyOdsCode() : this(false, false)
    {
    }

    public PharmacyOdsCode(bool includeHqs)
    {
      IncludeHQs = includeHqs;
    }

    public PharmacyOdsCode(bool includeHqs, bool allowNull): this(includeHqs)
    {
      AllowNull = allowNull;
    }

    protected override ValidationResult IsValid(
      object value,
      ValidationContext validationContext)
    {
      string[] memberNames;
      if (validationContext == null)
      {
        memberNames = new string[] { "PharmacyOdsCode" };
      }
      else
      {
        memberNames = new string[] { validationContext.MemberName };
      }

      if (value == null)
      {
        if (AllowNull)
        {
          return ValidationResult.Success;
        }
        else
        {
          return new ValidationResult(
            $"The {memberNames[0]} field is required.", memberNames);
        }
      }

      if (value.GetType() != typeof(string))
      {
        return new ValidationResult(
          $"The {memberNames[0]} field must be of type string.", memberNames);
      }

      string strValue = value.ToString();

      if (strValue.StartsWith("P") && IncludeHQs)
      {
        if (!strValue.ValidLength(4))
        {
          return new ValidationResult(
            $"The {memberNames[0]} field must have a length of 4 characters.",
            memberNames);
        }

        if (!Regex.Match(strValue, PATTERN_P).Success)
        {
          return new ValidationResult(
            $"The {memberNames[0]} field that starts with letter P must " +
            $"end with 3 numbers.", memberNames);
        }
      }
      else if (strValue.StartsWith("P") && !IncludeHQs)
      {
        return new ValidationResult(
          $"The {memberNames[0]} field cannot include HQ's.", memberNames);
      }
      else if (strValue.StartsWith("F"))
      {
        if (!strValue.ValidLength(5))
        {
          return new ValidationResult(
            $"The {memberNames[0]} field must have a length of 5 characters.",
            memberNames);
        }

        if (!Regex.Match(strValue, PATTERN_F).Success)
        {
          return new ValidationResult(
            $"The {memberNames[0]} field that starts with letter F then the" +
            $" next character must be a letter A - Y followed by 3 characters" +
            $" between A-Z or 0-9.",
            memberNames);
        }
      }
      else
      {
        return new ValidationResult(
          $"The {memberNames[0]} field must start with a capital letter " +
          $"of F or P.", memberNames);
      }


      return ValidationResult.Success;
    }

  }
}