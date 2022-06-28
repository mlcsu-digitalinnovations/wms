using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class GpPracticeOdsCode : ValidationAttribute
  {

    private const int REQUIRED_LENGTH = 6;
    private const string VALID_FIRST_CHARS = "A-H, J-N, P-W or Y";
    private const string VALID_FIRST_THREE_CHARS = "ALD, GUE or JER";
    private static readonly char[] _validFirstChars = new char[]
      { 'A','B','C','D','E','F','G','H',
        'J','K','L','M','N',
        'P','Q','R','S','T','U','V','W',
        'Y'};
    private static readonly string[] _validFirst3Chars = new string[]
      {"ALD","GUE","JER"};


    public GpPracticeOdsCode()
    { }

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
        memberNames = new string[] { "GpPracticeOdsCode" };
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

      string strValue = value.ToString();

      if (strValue.Length != REQUIRED_LENGTH)
      {
        return new ValidationResult($"The {memberNames[0]} " +
          $"field must have a length of {REQUIRED_LENGTH} characters.", 
          memberNames);
      }

      if (!_validFirstChars.Contains(strValue[0]))
      {
        return new ValidationResult($"The {memberNames[0]} " +
          $"field must start with a capital letter of {VALID_FIRST_CHARS}.", 
          memberNames);
      }

      int indexOfFirstNumber = 1;      
      if (!char.IsNumber(strValue[1]))
      {
        if (!_validFirst3Chars.Contains(strValue.Substring(0, 3)))
        {
          return new ValidationResult($"The {memberNames[0]} " +
            "field must start with 3 capital letters of " +
            $"{VALID_FIRST_THREE_CHARS}.", memberNames);
        }
        indexOfFirstNumber = 3;
      }

      for (int i = indexOfFirstNumber; i < REQUIRED_LENGTH; i++)
      {
        if (!char.IsNumber(strValue[i]))
        {
          return new ValidationResult($"The {memberNames[0]} " +
            $"field that starts with {indexOfFirstNumber} letter" +
            $"{(indexOfFirstNumber > 1 ? "s" : "")} must " +
            $"end with {REQUIRED_LENGTH - indexOfFirstNumber} numbers.", 
            memberNames);
        }
      }

      return ValidationResult.Success;
    }
  }
}