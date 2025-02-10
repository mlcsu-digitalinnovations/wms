using System;
using System.Text.RegularExpressions;

namespace WmsHub.Business.Helpers
{
  public static class VulnerableDescriptionHelper
  {
    public static string TryParseToAnonymous(this string vulnerableDescription)
    {
      if (string.IsNullOrWhiteSpace(vulnerableDescription))
      {
        return null;
      }
      string result = vulnerableDescription;
      var pattern = "\\d{4}-[01]\\d-[0-3]\\d?([T ])[0-2]\\d:[0-5]\\d:[0-5]\\d(?:\\.\\d+)?Z?";
      var isDate = Regex.Match(result, pattern);
      if (isDate.Success)
      {
        result = Regex.Replace(result, pattern, "XXXX-XX-XX xx:xx:xx.xxx");
      }

      pattern = "\\d{4}-[01]\\d-[0-3]\\d?([T ])[0-2]\\d:[0-5]\\d";
      isDate = Regex.Match(result, pattern);
      if (isDate.Success)
      {
        result = Regex.Replace(result, pattern, "XXXX-XX-XX xx:xx");
      }

      pattern = "[1-2][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]";
      isDate = Regex.Match(result, pattern);
      if (isDate.Success)
      {
        result = Regex.Replace(result, pattern, "XXXX-XX-XX");
      }

      pattern = "\\d{4}[-/][01]\\d[-/][0-3]\\d";
      isDate = Regex.Match(result, pattern);
      if (isDate.Success)
      {
        result = Regex.Replace(result, pattern, "XXXX-XX-XX");
      }

      pattern = "[0-9]{1}\\d[-/][0-1][0-9][-/]\\d{4}";
      isDate = Regex.Match(result, pattern);
      if (isDate.Success)
      {
        result = Regex.Replace(result, pattern, "XX/XX/XXXX");
      }


      //UBRN NUmbers
      pattern = "\\d{12}";
      var IsUbrn = Regex.Match(result, pattern);
      if (IsUbrn.Success)
      {
        result = Regex.Replace(result, pattern, "xxxxxxxxxxxx");
      }
      pattern = "[A-Z][A-Z]\\d{10}";
      IsUbrn = Regex.Match(result, pattern);
      if (IsUbrn.Success)
      {
        result = Regex.Replace(result, pattern, "xxxxxxxxxxxx");
      }

      //NhsNumbers

      pattern = "\\d{10}";
      var IsUNhsNumber = Regex.Match(result, pattern);
      if (IsUNhsNumber.Success)
      {
        result = Regex.Replace(result, pattern, "xxxxxxxxxx");
      }
      pattern = "\\d{3}[ -/]\\d{3}[ -/]\\d{4}";
      IsUNhsNumber = Regex.Match(result, pattern);
      if (IsUNhsNumber.Success)
      {
        result = Regex.Replace(result, pattern, "xxx-xxx-xxxx");
      }




      return result;
    }
  }
}