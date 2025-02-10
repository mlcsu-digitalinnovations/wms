using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mlcsu.Diu.Mustard.Apis.MeshMailbox;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Helpers;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;
using WmsHub.AzureFunctions.Validation;

[ExcludeFromCodeCoverage(Justification = "Will not run if this is incorrectly configured.")]
internal class Program
{
  private static void Main()
  {
    IHost host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
      if (hostContext.HostingEnvironment.IsDevelopment())
      {
        config.AddUserSecrets<Program>();
      }
    })
    .ConfigureServices((context, services) =>
    {
      // Common
      services.AddApplicationInsightsTelemetryWorkerService();
      services.AddProcessStatusService();
      services.ConfigureFunctionsApplicationInsights();

      // Send Text Messages
      SendTextMessagesOptions sendTextMessagesOptions = context
        .Configuration
        .GetSection(SendTextMessagesOptions.SectionKey)
        .Get<SendTextMessagesOptions>()
        ?? throw new InvalidOperationException(
          $"Unable to find section {SendTextMessagesOptions.SectionKey} in configuration.");

      services.AddOptions<SendTextMessagesOptions>()
        .Bind(context.Configuration.GetSection(SendTextMessagesOptions.SectionKey))
        .ValidateMiniValidation()
        .ValidateOnStart();
      
      services.AddHttpClient<ISendTextMessagesService, SendTextMessagesService>(httpClient =>
      {
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", sendTextMessagesOptions.ApiKey);
      })
      .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
      .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

      // SQL Maintenance
      services.AddOptions<SqlMaintenanceOptions>()
        .Bind(context.Configuration.GetSection(SqlMaintenanceOptions.SectionKey))
        .ValidateMiniValidation()
        .ValidateOnStart();

      services.AddScoped<IDatabaseContext, DatabaseContext>();
      services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
      services.AddScoped<ISqlMaintenanceService, SqlMaintenanceService>();

      // UDAL Extract
      UdalExtractOptions udalExtractOptions = context
        .Configuration
        .GetSection(UdalExtractOptions.SectionKey)
        .Get<UdalExtractOptions>()
        ?? throw new InvalidOperationException(
          $"Unable to find section {UdalExtractOptions.SectionKey} in configuration.");

      services.AddMeshMailbox(new MeshConfiguration()
      {
        Certificate = KeyVaultHelper.GetCertificateFromKeyVault(
          certificateName: udalExtractOptions.MeshMailboxApi.CertificateName,
          keyVaultUrl: udalExtractOptions.MeshMailboxApi.KeyVaultUrl,
          new SecretClientFactory())
      });

      services.AddOptions<UdalExtractOptions>()
        .Bind(context.Configuration.GetSection(UdalExtractOptions.SectionKey))
        .ValidateMiniValidation()
        .ValidateOnStart();

      services.AddScoped<IFileSystem, FileSystem>();
      services.AddScoped<IProcessFactory, ProcessFactory>();
      services.AddHttpClient<IUdalExtractService, UdalExtractService>(httpClient =>
      {
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", udalExtractOptions.BiApi.ApiKey);
      })
      .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
      .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
    })
    .Build();

    host.Run();
  }
}