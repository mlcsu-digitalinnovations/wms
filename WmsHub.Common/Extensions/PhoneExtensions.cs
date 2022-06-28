using System;
using System.Text.RegularExpressions;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Extensions
{
  /// <summary>
  /// Contains string extension methods dealing with telephone numbers
  /// </summary>
  public static class PhoneExtensions
  {
    /// <summary>
    /// Converts the string representation of a UK landline number to a valid 
    /// E.164 formatted UK landline number.</summary>
    /// <param name="value">A string containing the landline number to convert.
    /// </param>
    /// <param name="allowNullOrWhiteSpace">True if <paramref name="value"/>
    /// can be null or white space, otherwise false.</param>
    /// <returns>A E.164 formatted UK landline number.</returns>
    /// <exception cref="ArgumentNullException">Thrown if 
    /// <paramref name="value"/> is null or white space and 
    /// <paramref name="allowNullOrWhiteSpace"/> is false.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="value"/>
    /// after convertion is not a valid E.164 UK landline number.</exception>
    public static string ConvertToUkLandlineNumber(
      this string value,
      bool allowNullOrWhiteSpace)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        if (allowNullOrWhiteSpace)
        {
          return value;
        }
        else
        {
          throw new ArgumentNullException(nameof(value));
        }
      }

      if (value.IsUkLandline())
      {
        return value;
      }

      string convertedNumber = Convert(value);

      if (!convertedNumber.IsUkLandline())
      {
        throw new FormatException($"'{value}' cannot be converted to a " +
          "UK landline number.");
      }

      return convertedNumber;
    }

    /// <summary>
    /// Converts the string representation of a UK mobile number to a valid 
    /// E.164 formatted UK mobile number.</summary>
    /// <param name="value">A string containing the mobile number to convert.
    /// </param>
    /// <param name="allowNullOrWhiteSpace">True if <paramref name="value"/>
    /// can be null or white space, otherwise false.</param>
    /// <returns>A E.164 formatted UK mobile number.</returns>
    /// <exception cref="ArgumentNullException">Thrown if 
    /// <paramref name="value"/> is null or white space and 
    /// <paramref name="allowNullOrWhiteSpace"/> is false.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="value"/>
    /// after convertion is not a valid E.164 UK mobile number.</exception>
    public static string ConvertToUkMobileNumber(
      this string value, 
      bool allowNullOrWhiteSpace)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        if (allowNullOrWhiteSpace)
        {
          return value;
        }
        else
        {
          throw new ArgumentNullException(nameof(value));
        }
      }

      if (value.IsUkMobile())
      {
        return value;
      }

      string convertedNumber = Convert(value);

      if (!convertedNumber.IsUkMobile())
      {
        throw new FormatException($"'{value}' cannot be converted to a " +
          "UK mobile number.");
      }


      return convertedNumber;
    }

    /// <summary>
    /// Converts the string representation of a UK phone number to a valid E.164
    /// formatted UK phone number.</summary>
    /// <param name="value">A string containing the number to convert.</param>
    /// <param name="allowNullOrWhiteSpace">True if <paramref name="value"/>
    /// can be null or white space, otherwise false.</param>
    /// <returns>A E.164 formatted UK phone number.</returns>
    /// <exception cref="ArgumentNullException">Thrown if 
    /// <paramref name="value"/> is null or white space and 
    /// <paramref name="allowNullOrWhiteSpace"/> is false.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="value"/>
    /// after convertion is not a valid E.164 UK phone number.</exception>
    public static string ConvertToUkPhoneNumber(
      this string value,
      bool allowNullOrWhiteSpace)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        if (allowNullOrWhiteSpace)
        {
          return value;
        }
        else
        {
          throw new ArgumentNullException(nameof(value));
        }
      }

      string convertedNumber = Convert(value);

      if (convertedNumber.IsUkLandlineOrMobile())
      {
        return convertedNumber;
      }
      else
      {
        throw new FormatException($"'{value}' cannot be converted to a " +
          "UK phone number.");
      }      
    }

    /// <summary> Determines whether the string instance has the format of a 
    /// UK landline phone number.</summary>
    /// <returns><see cref="bool">true</see> if <paramref name="value" /> has 
    /// the format of a UK landline phone number, otherwise 
    /// <see cref="bool">false</see>.</returns>
    public static bool IsUkLandline(this string value)
    {
      return Regex.Match(
        value ?? string.Empty,
        Constants.REGEX_LANDLINE_PHONE_UK,
        RegexOptions.IgnoreCase).Success;
    }

    /// <summary> Determines whether the string instance has the format of a 
    /// UK mobile phone number.</summary>
    /// <returns><see cref="bool">true</see> if <paramref name="value" /> has 
    /// the format of a UK mobile phone number, otherwise 
    /// <see cref="bool">false</see>.</returns>
    public static bool IsUkMobile(this string value)
    {
      return Regex.Match(
        value ?? string.Empty,
        Constants.REGEX_MOBILE_PHONE_UK,
        RegexOptions.IgnoreCase).Success;
    }

    /// <summary>
    /// Returns a new string stripped of all non-digts and beginning with +44.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <exception cref="ArgumentNullException" />
    private static string Convert(string value)
    {      
      if (value is null)
      {
        throw new ArgumentNullException(nameof(value));
      }

      string convertedNumber = Regex.Replace(value, @"[^\d]", string.Empty);

      if (convertedNumber.StartsWith("0"))
      {
        convertedNumber = $"+44{convertedNumber[1..]}";
      }
      else if (convertedNumber.StartsWith("44"))
      {
        convertedNumber = $"+{convertedNumber}";
      }

      return convertedNumber;
    }

    /// <summary>
    /// Determines whether the string instance has the format of a UK landline 
    /// or mobile phone number.</summary>
    /// <returns><see cref="bool">true</see> if 
    /// <paramref name="value" /> has the 
    /// format of a UK landline or mobile phone number, 
    /// otherwise <see cref="bool">false</see>.</returns>
    private static bool IsUkLandlineOrMobile(this string value)
    {
      return IsUkLandline(value) || IsUkMobile(value);
    }
  }
}