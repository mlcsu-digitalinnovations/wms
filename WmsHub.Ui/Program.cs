using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
namespace WmsHub.Ui
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>

			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddEnvironmentVariables(prefix: "WmsHub_Ui_");
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
				})
				.UseSerilog((ctx, cfg) => 
					cfg.ReadFrom.Configuration(ctx.Configuration))
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
