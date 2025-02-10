using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WmsHub.Business;
using WmsHub.Business.Configuration;

namespace WmsHub.AzureFunction.ResetOrganisationQuotas;

public class Program
{
  private const string CONFIG_PREFIX = "WmsHub_ResetOrganisationQuotas_";

  public static void Main()
  {
    string connectionString = Environment
      .GetEnvironmentVariable("SQLCONNSTR_WmsHub");

    IHost host = new HostBuilder()
      .ConfigureAppConfiguration((ctx, config) => config.AddEfConfiguration(
          options => options.UseSqlServer(connectionString),
          prefix: CONFIG_PREFIX))
      .ConfigureFunctionsWorkerDefaults()
      .ConfigureServices(services =>
      {
        services.AddDbContext<DatabaseContext>(
          options => options.UseSqlServer(connectionString));
        services.AddHttpClient();
        services.AddOptions<ResetOrganisationQuotasOptions>()
        .BindConfiguration(ResetOrganisationQuotasOptions.SectionKey)
        .ValidateDataAnnotations()
        .ValidateOnStart();
      })
      .Build();

    host.Run();
  }
}
