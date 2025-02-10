using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Helpers
{
  /// <summary>
  /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/
  /// how-to-verify-that-strings-are-in-valid-email-format
  /// </summary>
  public class RegexUtilities
  {
    public static bool IsValidGpPracticeOdsCode(
      string odsCode,
      RegexOptions options = RegexOptions.IgnoreCase)
    {
      if (string.IsNullOrWhiteSpace(odsCode))
      {
        return false;
      }

      try
      {
        return Regex.IsMatch(
          odsCode,
          Constants.REGEX_GP_PRACTICE_NUMBER_ODS_CODE,
          options,
          TimeSpan.FromMilliseconds(250));
      }
      catch (RegexMatchTimeoutException)
      {
        return false;
      }
    }

    public static bool IsValidEmail(string email)
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        return false;
      }

      //InvalidDomain
      if (Constants.INVALID_EMAIL_DOMAINS.Any(
        domain => email.ToLower().Contains(domain)))
      {
        return false;
      }

      try
      {
        // Normalize the domain
        email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
          RegexOptions.None, TimeSpan.FromMilliseconds(200));

        // Examines the domain part of the email and normalizes it.
        string DomainMapper(Match match)
        {
          // Use IdnMapping class to convert Unicode domain names.
          var idn = new IdnMapping();

          // Pull out and process domain name
          // (throws ArgumentException on invalid)
          string domainName = idn.GetAscii(match.Groups[2].Value);

          return match.Groups[1].Value + domainName;
        }
      }
      catch (RegexMatchTimeoutException)
      {
        return false;
      }
      catch (ArgumentException)
      {
        return false;
      }

      try
      {
        return Regex.IsMatch(
          email,
          Constants.REGEX_EMAIL_ADDRESS,
          RegexOptions.IgnoreCase,
          TimeSpan.FromMilliseconds(250));
      }
      catch (RegexMatchTimeoutException)
      {
        return false;
      }
    }

    public static bool IsValidLinkId(string linkId)
    {
      if (string.IsNullOrWhiteSpace(linkId))
      {
        return false;
      }

      try
      {
        return Regex.IsMatch(
          linkId,
          $"^[{Constants.LINKIDCHARS}]+$",
          RegexOptions.None,
          TimeSpan.FromMilliseconds(250));
      }
      catch (RegexMatchTimeoutException)
      {
        return false;
      }
    }

    public static bool IsWildcardMatch(string wildcardPattern, string subject)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(wildcardPattern) ||
        string.IsNullOrWhiteSpace(subject))
        {
          return false;
        }

        string regexPattern = string.Concat("^", Regex.Escape(wildcardPattern)
          .Replace("\\*", ".*"), "$");

        int wildcardCount = wildcardPattern.Count(x => x.Equals('*'));
        if (wildcardCount <= 0)
        {
          return subject.Equals(wildcardPattern,
            StringComparison.CurrentCultureIgnoreCase);
        }
        else if (wildcardCount == 1)
        {
          string newWildcardPattern = wildcardPattern.Replace("*", "");

          if (wildcardPattern.StartsWith("*"))
          {
            return subject.EndsWith(newWildcardPattern,
              StringComparison.InvariantCultureIgnoreCase);
          }
          else if (wildcardPattern.EndsWith("*"))
          {
            return subject.StartsWith(newWildcardPattern,
              StringComparison.InvariantCultureIgnoreCase);
          }
          else
          {
            return Regex.IsMatch(subject, regexPattern, RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250));
          }
        }
        else
        {
          return Regex.IsMatch(subject, regexPattern, RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250));
        }
      }
      catch
      {
        return false;
      }
    }
  }
}
