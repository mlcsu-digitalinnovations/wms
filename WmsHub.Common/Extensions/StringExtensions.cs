using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Extensions
{
  public static class StringExtensions
  {
    public const string POSTCODE_REGEX = "^(([A-Z]{1,2}[0-9][A-Z0-9]?|ASCN|" +
      "STHL|TDCU|BBND|[BFS]IQQ|PCRN|TKCA) ?[0-9][A-Z]{2}|BFPO ?[0-9]{1,4}|(KY" +
      "[0-9]|MSR|VG|AI)[ -]?[0-9]{4}|[A-Z]{2} ?[0-9]{2}|GE ?CX|GIR ?0A{2}" +
      "|SAN ?TA1)$";
    private const string POSTCODE_NO_FIXED_ABODE = "ZZ99 3CZ";

    public static bool EqualsIgnoreCase(this string thisString, string value)
    {
      return thisString.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPostcode(this string postcode)
    {

      if (string.IsNullOrWhiteSpace(postcode))
      {
        return false;
      }

      try
      {
        return Regex.IsMatch(postcode, POSTCODE_REGEX);
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Converts a string to a postcode, removing all white space except the 
    /// expected space between inward and outward, converts to upper and
    /// removes any excess text
    /// </summary>
    /// <param name="postcode"></param>
    /// <returns>A postcode in upper case or null if string is 
    /// not a postcode</returns>
    public static string ConvertToPostcode(this string postcode)
    {
      if (postcode == null)
      {
        return null;
      }

      // replace all multiple white space with one space
      postcode = Regex.Replace(postcode, @"\s+", " ").ToUpper().Trim();
      if (string.IsNullOrEmpty(postcode))
      {
        return null;
      }

      string[] splits = postcode.Split(" ", StringSplitOptions.TrimEntries);
      string reformedPostcode = "";

      // just look at the first two splits for a postcode in format XXX XXX
      int length = splits[0].Length;
      if (length >= 7)
      {
        reformedPostcode = splits[0].Substring(0, 7).Insert(4, " ");
      }
      else if (splits.Length > 1)
      {
        length += splits[1].Length + 1; // +1 for the space
        if (length >= 8)
        {
          reformedPostcode = $"{splits[0]} {splits[1]}".Substring(0, 8);
        }
        else
        {
          reformedPostcode = $"{splits[0]} {splits[1]}";
        }
      }
      else
      {
        reformedPostcode = splits[0];
        if (reformedPostcode.StartsWith("ZZ99"))
        {
          reformedPostcode = POSTCODE_NO_FIXED_ABODE;
        }
        else
        {
          if (reformedPostcode.Length > 3)
          {
            reformedPostcode = reformedPostcode
              .Insert(reformedPostcode.Length - 3, " ");
          }
        }
      }

      if (reformedPostcode.IsPostcode())
      {
        return reformedPostcode;
      }
      else
      {
        return null;
      }
    }

    public static string ConvertToNoSpaceUpperPostcode(
      this string postcode, bool nullOrEmtpyAllowed = false)
    {
      if (string.IsNullOrEmpty(postcode) && nullOrEmtpyAllowed)
      {
        return postcode;
      }

      return postcode.Replace(" ", "").ToUpper();
    }

    public static TEnum ParseToEnumName<TEnum>(this string value) 
      where TEnum : Enum
    {
      if (TryParseToEnumName(value, out TEnum parsedEnum))
      {
        return parsedEnum;
      }
      else
      {
        throw new ArgumentException("value is not one of the named constants " +
          "defined for the enumeration.");
      }
    }

    public static bool TryParseToEnumName<TEnum>(
      this string value, out TEnum parsedEnum) where TEnum : Enum
    {
      parsedEnum = default;
      bool tryResult = false;

      if (value != null)
      {
        // don't allow integer representations of the enum
        if (!int.TryParse(value, out _))
        {
          if (Enum.TryParse(
            typeof(TEnum), 
            value, 
            true,
            out object parsedValue))
          {
            parsedEnum = (TEnum)parsedValue;
            tryResult = true;
          }

          if (tryResult && value != parsedEnum.ToString())
          {
            tryResult = false;
          }
        }
      }
      return tryResult;
    }

    public static List<string> TryParseIpWhitelist(
      this string[] whitelist)
    {
      List<string> validIPv4List =
        whitelist.Where(t => !t.Contains("-")).ToList();
      IEnumerable<string> iPv4Range = whitelist.Where(t => t.Contains("-"));
      foreach (string range in iPv4Range)
      {
        string[] rangeArray = range.Split('-');
        int fourth = int.Parse(rangeArray[0].Split('.')[3]);
        int fifth = int.Parse(rangeArray[1]);
        for (int i = fourth; i <= fifth; i++)
        {
          validIPv4List.Add($"{rangeArray[0].Split('.')[0]}." +
            $"{rangeArray[0].Split('.')[1]}." +
            $"{rangeArray[0].Split('.')[2]}.{i}");
        }
      }

      return validIPv4List;
    }

    public static string WithSpaces(this string value)
    {
      Regex r = new Regex(@"(?!^)(?=[A-Z])");
      return r.Replace(value, " ");
    }

    public static bool ValidLength(this string value, int size)
    {
      return value.Length == size;
    }

    public static string SanitizeInput(this string value)
    {
      string pattern = @"^[@+=-]+";

      bool isNotNumber = 
        !Regex.Match(value, Constants.REGEX_MOBILE_PHONE_UK).Success && 
        !Regex.Match(value, Constants.REGEX_PHONE_PLUS_NUMLENGTH).Success;

      string replacementChar = "";
      string result = value;
      while (Regex.Match(result, pattern).Success  && isNotNumber)
      {
        result = Regex.Replace(result, pattern, replacementChar);
      }

      return result;
    }

    public static string Mask(this string value, char maskWith, int showLast)
    {
      if (showLast < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(showLast));
      }

      if (string.IsNullOrEmpty(value))
      {
        return value;
      }
      int valueLength = value.Length;
      if (showLast > valueLength)
      {
        showLast = valueLength;
      }

      string result = string.Concat(
        new string(maskWith, valueLength - showLast),
        value.Substring(valueLength - showLast));

      return result;
    }

    public static string EnsureEndsWithForwardSlash(this string value)
    {
      return value.EndsWith("/") ? value : $"{value}/";
    }
  }
}