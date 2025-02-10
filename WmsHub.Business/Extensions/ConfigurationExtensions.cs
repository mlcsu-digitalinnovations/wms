using Microsoft.Extensions.Configuration;
using System;

namespace WmsHub.Business.Extensions;

public static class ConfigurationExtensions
{
  public static T GetConfigValue<T>(
    this IConfiguration configuration,
    string key,
    T defaultValue = default)
  {
    T value = configuration.GetValue(key, defaultValue);
    if (value == null
      || value is string && string.IsNullOrWhiteSpace(value.ToString()))
    {
      throw new Exception($"Configuration missing for {key}");
    }
    return value;
  }
}
