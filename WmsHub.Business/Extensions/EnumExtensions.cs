using System;
using System.Reflection;

namespace WmsHub.Business.Extensions;

public static class EnumExtensions
{
  /// <summary>
  /// Gets the type of the attribute of.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="enumVal">The enum value.</param>
  /// <returns></returns>
  public static T GetAttributeOfType<T>(this Enum enumVal)
    where T : Attribute
  {
    Type type = enumVal.GetType();
    MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
    object[] attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
    return attributes.Length > 0 ? (T)attributes[0] : null;
  }
}