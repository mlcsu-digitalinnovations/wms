using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Common.Extensions;
using WmsHub.ReferralsService.Console.Interfaces;
using WmsHub.ReferralsService.Console.Services;
using static WmsHub.ReferralsService.Console.Enums;

namespace WmsHub.ReferralsService.Console
{
  class Program
  {
    static string[] Arguments { get; set; }
    
    static async Task<int> Main(string[] args)
    {
      Arguments= args;
      var host = CreateHostBuilder();      

      await host.RunConsoleAsync();
      return Environment.ExitCode;
    }
    
    private static IHostBuilder CreateHostBuilder()
    {
      return Host.CreateDefaultBuilder()
        .UseSerilog((hostContext, loggerConfiguration) =>
        {
          loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);

        })
        .ConfigureAppConfiguration((hostContext, builder) =>
        {
          builder.AddEnvironmentVariables("WmsHub.ReferralService_");

          string logPath = Path.Combine(
            hostContext.HostingEnvironment.ContentRootPath,
            "logs");

          Directory.CreateDirectory(logPath);

          StreamWriter file = File.CreateText(
            Path.Combine(logPath, "seriloginternal.txt"));

          Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

          if (hostContext.HostingEnvironment.IsDevelopment())
          { 
            builder.AddUserSecrets<Program>();
          }
        })
        .ConfigureServices(services =>
        {
          services.AddHostedService<Worker>();
          services.AddTransient<IConsoleAppService, ConsoleAppService>();
        });      
    }

    public class Worker : IHostedService
    {
      private readonly IConsoleAppService _myService;
      private readonly ILogger<Worker> _logger;

      private readonly IHostApplicationLifetime _hostLifetime;
      public IConfiguration config;

      public Worker(
        IConsoleAppService service,
        IConfiguration configuration,
        IHostApplicationLifetime hostLifetime,
        ILogger<Worker> logger)
      {
        _myService = service ??
          throw new ArgumentNullException(nameof(service));
        _logger = logger ??
          throw new ArgumentNullException(nameof(logger));
        _hostLifetime = hostLifetime ??
          throw new ArgumentNullException(nameof(hostLifetime));
        config = configuration;
      }

      public async Task StartAsync(CancellationToken cancellationToken)
      {

        if (IsAlreadyRunning())
        {
          Log.Logger
          .Warning("An instance of this application is already running.");
          Environment.ExitCode = (int)ExitCode.CriticalFailure;
        }
        else
        {
          _hostLifetime.ApplicationStarted.Register(OnStarted);
        }
      }

      private async void OnStarted()
      {
        try
        {
          _myService.ConfigureService(config);
          Environment.ExitCode = await _myService.PerformProcess(Arguments);
          _hostLifetime.StopApplication();
        }
        catch (OperationCanceledException)
        {
          _logger?.LogInformation("The job has been killed with CTRL+C");
          Environment.ExitCode = (int)ExitCode.Success;
        }
        catch (Exception ex)
        {
          _logger?.LogError(ex, "An error occurred");
          Environment.ExitCode = (int)ExitCode.CriticalFailure;
        }
      }

      public Task StopAsync(CancellationToken cancellationToken)
      {
        _logger?.LogInformation("Shutting down the service with code " +
          $"{Environment.ExitCode}");
        return Task.CompletedTask;
      }
    }

    public static bool IsAlreadyRunning()
    {
      Process[] thisnameprocesslist;
      string modulename, processname;
      Process p = Process.GetCurrentProcess();
      modulename = p.MainModule.ModuleName.ToString();
      processname = System.IO.Path.GetFileNameWithoutExtension(modulename);
      thisnameprocesslist = Process.GetProcessesByName(processname);
      if (thisnameprocesslist.Length > 1)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}
