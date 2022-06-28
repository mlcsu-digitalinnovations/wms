using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WmsHub.ChatBot.Api
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          config.AddEnvironmentVariables(prefix: "WmsHub_ChatBot_Api_");
        })
        .UseSerilog((ctx, cfg) =>
          cfg.ReadFrom.Configuration(ctx.Configuration));
  }
}
