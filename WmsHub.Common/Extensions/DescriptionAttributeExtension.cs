#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using WmsHub.Common.Exceptions;

namespace WmsHub.Common.Extensions;

public static class DescriptionAttributeExtension
{
  public static string? GetDescriptionAttributeValue<TEnum>(this TEnum value)
  {
    if (value != null)
    {
      System.Type type = value.GetType();
      string? name = value?.ToString();

      if (!string.IsNullOrWhiteSpace(name))
      {
        System.Reflection.FieldInfo? fieldInfo =
        type.GetField(name);

        if (fieldInfo != null)
        {
          return fieldInfo.GetCustomAttributes(
            typeof(DescriptionAttribute),
            false) is DescriptionAttribute[] attribs
              && attribs.Length > 0 ? attribs[0].Description : null;
        }
      }
    }

    return null;
  }

  public static bool TryParseEnumFromDescription<TEnum>(this string description, out TEnum? value)
    where TEnum : Enum
  {

    Array enumValues = Enum.GetValues(typeof(TEnum));
    if (enumValues.Length == 0)
    {
      throw new EnumNoMemberException(typeof(TEnum));
    }

    value = default;

    if (description.Trim().Length == 0)
    {
      return false;
    }

    foreach (TEnum enumValue in enumValues)
    {
      if (string.Equals(
        description,
        enumValue.GetDescriptionAttributeValue(),
        StringComparison.Ordinal))
      {
        value = enumValue;
        return true;
      }
    }

    return false;
  }
}