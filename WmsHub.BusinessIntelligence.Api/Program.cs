using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using WmsHub.Business.Configuration;
using WmsHub.Common.Helpers;

namespace WmsHub.BusinessIntelligence.Api;

public class Program
{
  private const string CONFIG_PREFIX = "WmsHub_BusinessIntelligence_Api_";

  public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

  public static IHostBuilder CreateHostBuilder(string[] args)
  {
    string sqlConnStr = new ConfigurationBuilder()
      .AddEnvironmentVariables()
      .Build()
      .GetConnectionString(Constants.WMSHUB);

    if (sqlConnStr == Constants.UNIT_TESTING)
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration((configHost) =>
        {
          _ = configHost.AddEnvironmentVariables(prefix: CONFIG_PREFIX);
        })
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .UseSerilog((ctx, cfg) => cfg.WriteTo.Debug());
    }
    else
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration((configHost) =>
        {
          _ = configHost.AddEfConfiguration(
            options => options.UseSqlServer(sqlConnStr),
            prefix: CONFIG_PREFIX);
          _ = configHost.AddEnvironmentVariables(prefix: CONFIG_PREFIX);
        })
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
    }
  }
}
