using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Provider.Api
{
  [ExcludeFromCodeCoverage]
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          config.AddEnvironmentVariables(prefix: "WmsHub_Provider_Api_");
        })
        .UseSerilog((ctx, cfg) =>
          cfg.ReadFrom.Configuration(ctx.Configuration));
    }
  }
}
