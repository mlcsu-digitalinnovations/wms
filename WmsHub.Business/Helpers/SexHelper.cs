using System.Globalization;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Helpers
{
  public static class SexHelper
  {
    public static string TryParseSex(this string sex)
    {
      if (sex != null)
      {
        TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;
        sex = textInfo.ToTitleCase(sex);

        if (sex == "F")
        {
          sex = Sex.Female.ToString();
        }
        else if (sex == "M")
        {
          sex = Sex.Male.ToString();
        }
      }

      return sex.TryParseToEnumName(out Sex t)
        ? t.ToString()
        : null;
    }
  }
}
