using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Net;
using WmsHub.Business;
using WmsHub.Business.Configuration;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires;

public class Program
{
  private const string CONFIG_PREFIX = "WmsHub_CreateAndSendQuestionnaires_";

  public static void Main()
  {
    string connectionString = new ConfigurationBuilder()
      .AddEnvironmentVariables()
      .Build()
      .GetConnectionString("WmsHub");

    IHost host = Host.CreateDefaultBuilder()
      .ConfigureAppConfiguration((context, config) =>
      {
        config.AddEfConfiguration(
          options => options.UseSqlServer(connectionString),
          prefix: CONFIG_PREFIX);
      })
      .ConfigureFunctionsWorkerDefaults()
      .ConfigureServices(services =>
      {
        services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
        services.AddHttpClient<CreateAndSendQuestionnaires>()
          .SetHandlerLifetime(TimeSpan.FromMinutes(5))
          .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)))
          .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
        services.AddProcessStatusService();
        services.AddOptions<CreateAndSendQuestionnairesOptions>()
          .BindConfiguration(nameof(CreateAndSendQuestionnairesOptions))
          .ValidateDataAnnotations()
          .ValidateOnStart();
      })
      .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
      .Build();

    host.Run();
  }
}

