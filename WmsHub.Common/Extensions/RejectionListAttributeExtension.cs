#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using WmsHub.Common.Attributes;

namespace WmsHub.Common.Extensions;

public static class RejectionListAttributeExtension
{
  public static List<dynamic> RejectionStatusItems<TEnum>()
  {
    List<dynamic> resultSet = new();
    Type type = typeof(TEnum);
    FieldInfo[] fields = type.GetFields();

    foreach (FieldInfo field in fields)
    {
      bool isRejectionList = field.GetCustomAttributes(
        typeof(RejectionListAttribute), 
        false) is RejectionListAttribute[] attribs
        && attribs.Length > 0 
        && attribs[0].IsRejectionList;

      if (isRejectionList)
      {
        dynamic item = new ExpandoObject();
        item.IsRejectionList = true;
        item.Name = field.Name;
        resultSet.Add(item);
      }

    }
    return resultSet;
  }
}