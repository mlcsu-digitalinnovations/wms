using System.Reflection;

namespace WmsHub.Business.Helpers
{
  public static class ReflectionHelper
  {
    public static object GetPropertyValue(object source, string propertyName)
    {
      PropertyInfo property = source.GetType().GetProperty(propertyName);
      return property.GetValue(source, null);
    }
  }
}