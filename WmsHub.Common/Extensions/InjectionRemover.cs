using System;
using System.Reflection;

namespace WmsHub.Common.Extensions
{
  public static class InjectionHelper
  {
    public static void InjectionRemover<T>(this T value) where T : class
    {
      if (value == null)
      {
        throw new ArgumentNullException($"{typeof(T).FullName} was null");
      }
      Type type = typeof(T);
      PropertyInfo[] properties = type.GetProperties();
      foreach (var propInfo in properties)
      {
        if (propInfo.PropertyType.FullName!.Contains("System.String"))
        {
          object newValue = propInfo.GetValue(value, null);
          if (newValue != null)
          {
            propInfo.SetValue(value, newValue.ToString().SanitizeInput());
          }
        }
      }

    }
  }
}
