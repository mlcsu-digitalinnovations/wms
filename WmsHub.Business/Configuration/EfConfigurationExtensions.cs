using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace WmsHub.Business.Configuration
{
  public static class EfConfigurationExtensions
  {
    public static IConfigurationBuilder AddEfConfiguration(
      this IConfigurationBuilder builder,
      Action<DbContextOptionsBuilder> optionsAction)
    {
      return builder.Add(new EfConfigurationSource(optionsAction));
    }

    public static IConfigurationBuilder AddEfConfiguration(
      this IConfigurationBuilder builder,
      Action<DbContextOptionsBuilder> optionsAction,
      string prefix)
    {
      return builder.Add(new EfConfigurationSource(optionsAction, prefix));
    }
  }
}
