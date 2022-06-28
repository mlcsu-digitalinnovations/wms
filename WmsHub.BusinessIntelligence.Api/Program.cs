using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;
using Serilog;

namespace WmsHub.BusinessIntelligence.Api
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
      configHost.AddEnvironmentVariables(prefix: 
        "WmsHub_BusinessIntelligence_Api_");
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
      webBuilder.UseStartup<Startup>();
    })
    .UseSerilog((ctx, cfg) =>
      cfg.ReadFrom.Configuration(ctx.Configuration));
  }
}
