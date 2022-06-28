using System;

namespace WmsHub.Common.Extensions
{
  public static class EnumIsMatchExtension
  {
    public static bool Is<TEnum>(this string value1, TEnum value2) 
      where TEnum : Enum
    {
      return value2.ToString() == value1;
    }
  }
}