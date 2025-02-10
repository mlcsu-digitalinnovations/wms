using System;
using System.Reflection;
using WmsHub.Common.Attributes;

namespace WmsHub.Common.Extensions;

public static class ReferralStatusTraceAttributeExtension
{
  public static bool CanTraceReferralStatus<TEnum>(this TEnum value) where TEnum : Enum =>
    value.HasReferralStatusTraceAttribute();

  public static bool CanTraceReferralStatusString<TEnum>(this string value) where TEnum : Enum
  {
    if (Enum.TryParse(typeof(TEnum), value, true, out object status))
    {
      return CanTraceReferralStatus((TEnum)status);
    }

    return false;
  }

  private static bool HasReferralStatusTraceAttribute<TEnum>(this TEnum value) where TEnum : Enum
  {
    Type type = value.GetType();
    FieldInfo fieldInfo = type.GetField(value.ToString());
    if (fieldInfo == null)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
    else
    {
      return fieldInfo.GetCustomAttributes(typeof(ReferralStatusTraceAttribute), false).Length != 0;
    }
  }
}