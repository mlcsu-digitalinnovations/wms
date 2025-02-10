using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Common.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class GpPracticeOdsCodeAttribute(bool allowDefaultCodes = true) : ValidationAttribute
  {
    private const int REQUIRED_LENGTH = 6;
    private const string VALID_FIRST_CHARS = "A-H, J-N, P-W or Y";
    private const string VALID_FIRST_THREE_CHARS = "ALD, GUE or JER";
    private static readonly string[] _defaultCodes = ["V81997", "V81998", "V81999"];
    private static readonly char[] _validFirstChars = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J',
      'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'Y'];
    private static readonly string[] _validFirst3Chars = ["ALD", "GUE", "JER"];

    public bool AllowDefaultCodes { get; set; } = allowDefaultCodes;

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      string memberName = validationContext?.MemberName ?? "field name not found";
      string objectName = validationContext?.ObjectType?.FullName ?? "object name not found";
      string errorMessage;

      if (value == null)
      {
        return ValidationResult.Success;
      }

      Type valueType = value.GetType();
      if (valueType != typeof(string))
      {
        throw new InvalidOperationException($"The {nameof(GpPracticeOdsCodeAttribute)} must be " +
          $"used with a type of string, but the field '{memberName}' on the object " +
          $"'{objectName}' has a type of {valueType}.");
      }

      string strValue = value.ToString();

      if (AllowDefaultCodes == false && _defaultCodes.Contains(strValue))
      {
        errorMessage = ErrorMessage
          ?? $"The {memberName} field must not contain one of the default codes " +
            $"{string.Join(",", _defaultCodes)} when {nameof(AllowDefaultCodes)} is false.";
        return new ValidationResult(errorMessage, [memberName]);
      }

      if (strValue.Length != REQUIRED_LENGTH)
      {
        errorMessage = ErrorMessage
          ?? $"The {memberName} field must have a length of {REQUIRED_LENGTH} characters.";
        return new ValidationResult(errorMessage, [memberName]);
      }

      if (!_validFirstChars.Contains(strValue[0]))
      {
        errorMessage = ErrorMessage
          ?? $"The {memberName} field must start with a capital letter of {VALID_FIRST_CHARS}.";
        return new ValidationResult(errorMessage, [memberName]);
      }

      int indexOfFirstNumber = 1;
      if (!char.IsNumber(strValue[1]))
      {
        if (!_validFirst3Chars.Contains(strValue[..3]))
        {
          errorMessage = ErrorMessage
            ?? $"The {memberName} field must start with 3 capital letters of " +
              $"{VALID_FIRST_THREE_CHARS}.";
          return new ValidationResult(errorMessage, [memberName]);
        }
        indexOfFirstNumber = 3;
      }

      for (int i = indexOfFirstNumber; i < REQUIRED_LENGTH; i++)
      {
        if (!char.IsNumber(strValue[i]))
        {
          errorMessage = ErrorMessage
            ?? $"The {memberName} field that starts with {indexOfFirstNumber} letter" +
              $"{(indexOfFirstNumber > 1 ? "s" : "")} must end with " +
              $"{REQUIRED_LENGTH - indexOfFirstNumber} numbers.";
          return new ValidationResult(errorMessage, [memberName]);
        }
      }

      return ValidationResult.Success;
    }
  }
}