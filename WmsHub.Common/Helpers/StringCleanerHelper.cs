using System;
using System.Text.RegularExpressions;

namespace WmsHub.Common.Helpers
{
  public static class StringCleanerHelper
  {
    public static string EmailCleaner(this string value, string[] toRemove)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return value;
      }

      string[] splits = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      string potentialValidEmail = string.Empty;
      foreach (string split in splits)
      {
        potentialValidEmail += split;
        if (RegexUtilities.IsValidEmail(potentialValidEmail))
        {
          break;
        }
      }

      string result = "";
      string pattern = "[^a-zA-Z0-9.@+-_']";
      Regex regex = new Regex(pattern);
      foreach (var test in toRemove)
      {
        string remove = " " + test.ToLower();
        result = regex.Replace(potentialValidEmail, " ").TrimEnd();
        if (result.ToLower().EndsWith(remove))
        {
          result = result.ToLower().Replace(remove, "");
          break;
        }
      }

      result = result.TrimEnd(new char[] { '-' });
      result = result.TrimEnd(new char[] { '.' });
      result = result.TrimEnd(new char[] { ' ' });

      return result;
    }
  }
}
