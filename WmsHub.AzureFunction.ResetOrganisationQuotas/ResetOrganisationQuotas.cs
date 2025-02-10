using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WmsHub.AzureFunction.ResetOrganisationQuotas;

public class ResetOrganisationQuotas
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger _logger;
  private readonly ResetOrganisationQuotasOptions _options;

#if DEBUG
  private const bool RUN_ON_STARTUP = true;
#else
  private const bool RUN_ON_STARTUP = false;
#endif

  public ResetOrganisationQuotas(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    IOptions<ResetOrganisationQuotasOptions> options)
  {
    _httpClientFactory = httpClientFactory;
    _logger = loggerFactory.CreateLogger<ResetOrganisationQuotas>();

    if (options.Value != null)
    {
      _options = options.Value;
    }
  }

  [Function("ResetOrganisationQuotas")]
  public async Task Run(
    [TimerTrigger("0 0 0 1 * *", RunOnStartup = RUN_ON_STARTUP)] Timer timer)
  {
    string apiKey = _options.ReferralApiAdminKey;
    string resetOrganisationQuotasUrl = _options.ResetOrganisationQuotasUrl;

    if (timer.IsPastDue)
    {
      _logger.LogInformation($"Function executed late.");
    }

    HttpResponseMessage response = new();

    if (_options.OverrideDate)
    {
      resetOrganisationQuotasUrl += "?overrideDate=true";
    }

    using (HttpClient client = _httpClientFactory.CreateClient())
    {
      client.DefaultRequestHeaders.Add("x-api-key", apiKey);
      response = await client.PostAsync(resetOrganisationQuotasUrl, null);
    }

    string msg = resetOrganisationQuotasUrl + " {0}. Review logs for details.";

    if (response.IsSuccessStatusCode
      || response.StatusCode == HttpStatusCode.UnprocessableEntity)
    {
      _logger.LogInformation(string.Format(msg, response.StatusCode));
    }
    else
    {
      _logger.LogError(string.Format(
        msg,
        $"returned unexpected result {response.StatusCode}"));
    }
  }
}