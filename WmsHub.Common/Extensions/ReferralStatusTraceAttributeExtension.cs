#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using WmsHub.Common.Attributes;

namespace WmsHub.Common.Extensions
{
  public static class ReferralStatusTraceAttributeExtension
  {
    private static ReferralStatusTraceAttribute[]? GetAttributes(
      this FieldInfo fieldInfo)
    {
      return  fieldInfo.GetCustomAttributes(
        typeof(ReferralStatusTraceAttribute), false) 
        as ReferralStatusTraceAttribute[];
    }

    private static int GetAttributeIntValue(
      this ReferralStatusTraceAttribute[] value)
    {
      if (value.Length < 1)
      {
        return -1;
      }
      return value[0].NumberOfDays;
    }

    private static bool GetAttributeBoolValue(
      this IReadOnlyList<ReferralStatusTraceAttribute> value)
    {
      if (value.Count < 1)
      {
        return false;
      }

      return value[0].CanTrace;
    }

    private static ReferralStatusTraceAttribute[]? 
      GetAllAttributes<TEnum>(this TEnum value) where TEnum : Enum
    {
      Type type = value.GetType();
      FieldInfo? fieldInfo = type.GetField(value.ToString());
      if (fieldInfo == null)
      {
        return null;
      }
      else
      {
        return GetAttributes(fieldInfo);
      }
    }

    public static bool IsTraceDateValid<TEnum>(
      this DateTimeOffset? lastTraceDate,
      string value) where TEnum: Enum
    {
      if (lastTraceDate == null)
      {
        return false;
      }

      if (Enum.TryParse(typeof(TEnum), value, true, out object? status))
      {
        DateTimeOffset? testDate = 
          GetTraceDate(((TEnum) status!), lastTraceDate.Value);
        return DateTimeOffset.Now > testDate;
      }

      return false;
    }

    public static DateTimeOffset? GetTraceDate<TEnum>(
      this TEnum value, DateTimeOffset lastTraceDate) where TEnum : Enum
    {
      ReferralStatusTraceAttribute[]? attribs = value.GetAllAttributes();
      int numOfDays = attribs!.GetAttributeIntValue();
      return lastTraceDate.AddDays(numOfDays);
    }

    public static int? GetTraceDays<TEnum>(this TEnum value) where TEnum : Enum
    {
      ReferralStatusTraceAttribute[]? attribs = value.GetAllAttributes();
      return attribs!.GetAttributeIntValue();
    }

    public static bool GetCanTrace<TEnum>(this TEnum value) where TEnum : Enum
    {
      ReferralStatusTraceAttribute[]? attribs = value.GetAllAttributes();
      return attribs!.GetAttributeBoolValue();
    }

    public static bool CanTrace<TEnum>(this string value) where TEnum : Enum
    {
      if (Enum.TryParse(typeof(TEnum), value, true, out object? status))
      {
        return GetCanTrace(((TEnum) status!));
      }

      return false;
    }
  }
}