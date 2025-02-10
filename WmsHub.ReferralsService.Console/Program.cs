using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Mlcsu.Diu.Mustard.Email;
using Polly;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.ReferralsService.Console.Interfaces;
using WmsHub.ReferralsService.Console.Services;
using WmsHub.ReferralsService.Models.Configuration;
using static WmsHub.ReferralsService.Console.Enums;

namespace WmsHub.ReferralsService.Console
{
  class Program
  {
    static string[] Arguments { get; set; }

    static async Task<int> Main(string[] args)
    {
      Arguments = args;
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
        .ConfigureServices((hostContext, services) =>
        {
          services.AddHostedService<Worker>();

          services.AddHttpClient(Config.HttpClientWithClientCertificate, httpClient =>
          {
            httpClient.BaseAddress
              = new Uri(hostContext.Configuration.GetValue<string>("Data:BaseUrl"));
          })
          .ConfigurePrimaryHttpMessageHandler(() =>
          {
            HttpClientHandler httpClientHandler = new()
            {
              ClientCertificateOptions = ClientCertificateOption.Manual,
              // This is required because the server certificate from eReferrals is
              // not in a valid chain -- needs further investigation
              ServerCertificateCustomValidationCallback = (a, b, c, d) => true,
              SslProtocols = SslProtocols.Tls12,
            };

            X509Certificate2 cert = Certificates.LoadCertificateFromFile(
              hostContext.Configuration.GetValue<string>("Data:ClientCertificateFilePath"),
              hostContext.Configuration.GetValue<string>("Data:ClientCertificatePassword"));

            httpClientHandler.ClientCertificates.Add(cert);

            return httpClientHandler;
          })
          .AddPolicyHandler(DefaultPolicies.Retry);

          services.AddHttpClient(Config.HubRegistrationExceptionHttpClient, httpClient =>
          {
            httpClient.BaseAddress = new Uri(
              hostContext.Configuration.GetValue<string>("Data:HubRegistrationExceptionAPIPath"));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add(
              "X-API-KEY",
              hostContext.Configuration.GetValue<string>("Data:HubRegistrationAPIKey"));
          })
          .AddPolicyHandler(DefaultPolicies.Retry);

          services.AddHttpClient(Config.HubRegistrationHttpClient, httpClient =>
          {
            httpClient.BaseAddress
              = new Uri(hostContext.Configuration.GetValue<string>("Data:HubRegistrationAPIPath"));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add(
              "X-API-KEY",
              hostContext.Configuration.GetValue<string>("Data:HubRegistrationAPIKey"));
          })
          .AddPolicyHandler(DefaultPolicies.Retry);

          services.AddProcessStatusService();
          services.AddSingleton<ISendEmailService, SendEmailService>();
          services.AddTransient<IConsoleAppService, ConsoleAppService>();
        });
    }

    public class Worker : IHostedService
    {
      public IConfiguration _config;
      private readonly IConsoleAppService _consoleAppService;
      private readonly IHostApplicationLifetime _hostLifetime;
      private readonly IHttpClientFactory _httpClientFactory;
      private readonly ILogger<Worker> _logger;
      private readonly IProcessStatusService _processStatusService;
      private readonly ISendEmailService _sendEmailService;

      public Worker(
        IConfiguration configuration,
        IConsoleAppService consoleAppService,
        IHostApplicationLifetime hostLifetime,
        IHttpClientFactory httpClientFactory,
        ILogger<Worker> logger,
        IProcessStatusService processStatusService,
        ISendEmailService sendEmailService)
      {
        _config = configuration;
        _consoleAppService = consoleAppService;
        _hostLifetime = hostLifetime;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _processStatusService = processStatusService;
        _sendEmailService = sendEmailService;
      }

      public async Task StartAsync(CancellationToken cancellationToken)
      {

        if (IsAlreadyRunning())
        {
          Log.Logger.Warning(
            "An instance of this application is already running.");

          Environment.ExitCode = (int)ExitCode.CriticalFailure;
        }
        else
        {
          _hostLifetime.ApplicationStarted.Register(OnStarted);
        }

        await Task.CompletedTask;
      }

      private async void OnStarted()
      {
        try
        {
          _consoleAppService.ConfigureService(
            _config,
            _httpClientFactory,
            _processStatusService,
            _sendEmailService);

          Environment.ExitCode = await _consoleAppService.PerformProcess(Arguments);
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
