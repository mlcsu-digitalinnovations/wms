using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WmsHub.Common.Attributes;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Extensions;

public static class StringExtensions
{
  public const string POSTCODE_REGEX = "^(([A-Z]{1,2}[0-9][A-Z0-9]?|ASCN|" +
    "STHL|TDCU|BBND|[BFS]IQQ|PCRN|TKCA) ?[0-9][A-Z]{2}|BFPO ?[0-9]{1,4}|(KY" +
    "[0-9]|MSR|VG|AI)[ -]?[0-9]{4}|[A-Z]{2} ?[0-9]{2}|GE ?CX|GIR ?0A{2}" +
    "|SAN ?TA1)$";
  private const string POSTCODE_NO_FIXED_ABODE = "ZZ99 3CZ";

  public static bool EqualsIgnoreCase(this string thisString, string value)
  {
    return !string.IsNullOrEmpty(thisString)
      && thisString.Equals(value, StringComparison.OrdinalIgnoreCase);
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

  /// <summary> Determines whether the string instance has the format of a 
  /// NHS number.</summary>
  /// <returns><see cref="bool">true</see> if <paramref name="value" /> has 
  /// the format of a NHS number, otherwise 
  /// <see cref="bool">false</see>.</returns>
  public static bool IsNhsNumber(this string value)
  {
    NhsNumberAttribute nhsNumberAttribute = new(allowNulls: false);
    return nhsNumberAttribute.IsValid(value);
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
    this string postcode, bool allowNulls = false)
  {
    if (string.IsNullOrWhiteSpace(postcode))
    {
      if (allowNulls && postcode == null)
      {
        return postcode;
      }
      
      throw new ArgumentNullOrWhiteSpaceException(nameof(postcode));
    }

    return Regex.Replace(postcode, "\\s+", "").ToUpperInvariant();
  }

  /// <summary>
  /// Extracts the domain from an email address.
  /// </summary>
  /// <param name="email">
  /// An email address to extract the domain from.
  /// </param>
  /// <returns>
  /// If the provided email contains an @ character then the string after the
  /// @ character will be returned. If the email does not contain an @
  /// character or it is at the end of the email null will be returned.
  /// </returns>
  public static string ExtractEmailDomain(this string email)
  {
    if (string.IsNullOrWhiteSpace(email)) 
    {
      return null;
    }

    int index = email.LastIndexOf('@');
    if (index == -1 || index == email.Length - 1)
    {
      return null;
    }

    return email[(index + 1)..];
  }

  /// <summary>
  /// To Enum or int '0' if no enum found
  /// </summary>
  /// <typeparam name="TEnum"></typeparam>
  /// <param name="value"></param>
  /// <returns></returns>
  public static TEnum ToEnum<TEnum>(this string value) where TEnum: Enum
  {
    if (TryParseToEnumName(value, out TEnum parsedEnum))
    {
      return parsedEnum;
    }
    else
    {
      return default;
    }
  }

  /// <summary>
  /// Returns a List<string> of the names contained within an Enum flag.</string>
  /// </summary>
  /// <typeparam name="TEnum">Enum with Flag</typeparam>
  /// <param name="value">
  /// Value of the flag i.e. New | RmcCall | TextMessage1
  /// </param>
  /// <param name="excludeZero">If a flag, such as ReferralStatus, has an 
  /// assigned value of zero (0), ReferralStatus.Exception, if you use the 
  /// value of the flag, then zero (0) will always be returned.<br />
  /// For example.<br />
  /// [Flag]<br />
  /// public enum TestType<br />
  /// {<br />
  ///  Exception = 0,<br />
  ///  TypeA = 1,<br />
  ///  TypeB = 2,<br />
  ///  TypeC = 4<br />
  /// }<br />
  /// <br />
  /// TestType _tt1 = TestType.TypeA | TestType.TypeB;<br />
  /// <br />
  /// if (_tt1.HasFlag(TestType.Exception))<br />
  /// {<br />
  ///   throw Exception(...);<br />
  /// }<br />
  /// Would incorrectly throw an exception.<br />
  /// To return a list of string and exclude zero from flag<br />
  /// .Where(f => (!excludeZero || Convert.ToInt32(f) != 0)) when converting 
  /// list.
  /// </param>
  /// <returns>List<string></returns>
  public static List<string> ToFlagNames<TEnum>(
    this TEnum value,
    bool excludeZero = true)
      where TEnum : Enum
  {
    List<string> flagNames = Enum.GetValues(typeof(TEnum))
      .Cast<TEnum>()
      .Where(f => value.HasFlag(f))
      .Where(f => (!excludeZero || Convert.ToInt32(f) != 0))
      .Select(f => f.ToString())
      .ToList();

    return flagNames;
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
    string pattern = @"(^(0x0D))|(^(0x07))|(^[@+=-])|([$%<>*^])+";

    bool isNotNumber = 
      !Regex.Match(value, Constants.REGEX_MOBILE_PHONE_UK).Success && 
      !Regex.Match(value, Constants.REGEX_PHONE_PLUS_NUMLENGTH).Success;

    string replacementChar = "";
    string result = value;
    while (Regex.Match(result, pattern, RegexOptions.IgnoreCase).Success  && isNotNumber)
    {
      result = Regex.Replace(result, pattern, replacementChar, RegexOptions.IgnoreCase);
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
    if (value == null)
    {
      return null;
    }
    else
    {
      return value.EndsWith("/") ? value : $"{value}/";
    }
  }

  /// <summary>
  /// For determining whether there exists a string that starts with a 
  /// specified substring
  /// </summary>
  /// <param name="value"></param>
  /// <param name="strings"></param>
  /// <param name="resultIfNull"></param>
  /// <returns>Returns resultIfNull if value or strings are null, 
  /// otherwise returns true if any of the strings begin with the specified 
  /// value, otherwise returns false</returns>
  public static bool StartsWithMatchInArray(
    this string value,
    string[] strings,
    bool resultIfNull = false)
  {
    bool result = false;

    if (value == null || strings == null)
    {
      result = resultIfNull;
    }
    else
    {
      if (strings.Any(r => value.StartsWith(r)))
      {
        result = true;
      }
    }

    return result;
  }

  /// <summary>
  /// If a vlaue supplied is null, then teh default return is true, else the 
  /// value is checked to see if it contained within the list of values.
  /// </summary>
  /// <param name="value">Null or string</param>
  /// <param name="expected">List of expected strings</param>
  /// <returns>Boolean</returns>
  public static bool IsValueInExpectedListOrNull(
    this string value, 
    string[] expected = null)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return true;
    }

    if (expected != null && expected.Any())
    { 
      return !expected.Contains(value);
    }

    return false;
  }

  /// <summary>
  /// Validates any UBRN to ensure it is in the correct format.
  /// </summary>
  /// <param name="ubrn">12 characters</param>
  /// <exception cref="UbrnException"></exception>
  public static void ValidateUbrn(this string ubrn)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new UbrnException($"UBRN {ubrn} is not allowed to be empty.");
    }

    if(ubrn.Length > 12 || ubrn.Length < 12)
    {
      throw new UbrnException($"UBRN {ubrn} is not the correct length " +
        $"of 12 characters.");
    }

    string pattern = ubrn[..2].ToUpper() switch
    {
      "SR" => @"^SR[0-9]*$",
      "PR" => @"^PR[0-9]*$",
      "GR" => @"^GR[0-9]*$",
      "MS" => @"^MSK[0-9]*$",
      "GP" => @"^GP[0-9]*$",
      "EC" => @"^EC[0-9]*$",
      _ => @"^[0-9]*$",
    };

    Regex regex = new(pattern);
    if (!regex.Match(ubrn).Success)
    {
      throw new UbrnException($"{ubrn} is not valid UBRN.");
    }
  }

  public static string RemoveSpaces(this string value, string replaceWith = "")
  {
    Regex r = new(@"\s");
    return r.Replace(value, replaceWith);
  }
}
