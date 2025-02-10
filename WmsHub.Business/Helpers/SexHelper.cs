using System.Globalization;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Helpers;

public static class SexHelper
{
  public static bool IsValidSexString(this string sexString)
  {
    if (sexString == null)
    {
      return false;
    }

    return sexString.TryParseEnumFromDescription<Sex>(out _);
  }

  public static bool TryParseSex(this string sexString, out Sex sexEnum)
  {
    if (sexString == null)
    {
      sexEnum = default;
      return false;
    }
    
    TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;

    sexString = textInfo.ToLower(sexString);
    sexString = textInfo.ToTitleCase(sexString);

    sexString = sexString switch
    {
      "F" => Sex.Female.GetDescriptionAttributeValue(),
      "M" => Sex.Male.GetDescriptionAttributeValue(),
      "Nk" => Sex.NotKnown.GetDescriptionAttributeValue(),
      "Notknown" => Sex.NotKnown.GetDescriptionAttributeValue(),
      "Notspecified" => Sex.NotSpecified.GetDescriptionAttributeValue(),
      "Ns" => Sex.NotSpecified.GetDescriptionAttributeValue(),
      _ => sexString
    };

    return sexString.TryParseEnumFromDescription(out sexEnum);
  }
}
