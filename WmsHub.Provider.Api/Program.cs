using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using WmsHub.Business.Configuration;

namespace WmsHub.Provider.Api;

public class Program
{
  private const string CONFIG_PREFIX = "WmsHub_Provider_Api_";

  public static void Main(string[] args) => 
    CreateHostBuilder(args).Build().Run();

  public static IHostBuilder CreateHostBuilder(string[] args)
  {
    string sqlConnStr = new ConfigurationBuilder()
      .AddEnvironmentVariables()
      .Build()
      .GetConnectionString("WmsHub");

    return Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((configHost) =>
    {
      _ = configHost.AddEfConfiguration(
        options => options.UseSqlServer(sqlConnStr),
        prefix: CONFIG_PREFIX);
      _ = configHost.AddEnvironmentVariables(prefix: CONFIG_PREFIX);
    })
    .ConfigureWebHostDefaults(webBuilder => 
      _ = webBuilder.UseStartup<Startup>())
    .UseSerilog((ctx, cfg) =>
      cfg.ReadFrom.Configuration(ctx.Configuration));
  }
}
