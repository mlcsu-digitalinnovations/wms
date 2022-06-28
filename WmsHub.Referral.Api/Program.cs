using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using WmsHub.Business.Configuration;

namespace WmsHub.Referral.Api
{
  [ExcludeFromCodeCoverage]
  public class Program
  {
    private const string CONFIG_PREFIX = "WmsHub_Referral_Api_";

    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {

      string sqlConnStr = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build()
        .GetConnectionString("WmsHub");

      return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          config.AddEfConfiguration(
            options => options.UseSqlServer(sqlConnStr),
            prefix: CONFIG_PREFIX);
          config.AddEnvironmentVariables(prefix: CONFIG_PREFIX);
        })
        .UseSerilog((ctx, cfg) =>
          cfg.ReadFrom.Configuration(ctx.Configuration));
    }
  }
}
