using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Exceptions;

namespace WmsHub.Common.Apis.Ods;

public class OdsOrganisationService : IOdsOrganisationService
{
  private HttpClient _httpClient;
  private readonly ILogger _logger;
  private readonly OdsOrganisationOptions _options = null;

  public HttpMessageHandler HttpMessageHandler { get; set; }
    = new HttpClientHandler();

  public OdsOrganisationService(
    ILogger logger,
    IOptions<OdsOrganisationOptions> options)
  {
    if (logger is null)
    {
      throw new ArgumentNullException(nameof(logger));
    }

    if (options is null)
    {
      throw new ArgumentNullException(nameof(options));
    }

    _logger = logger.ForContext<OdsOrganisationService>();
    _options = options.Value;
  }

  public OdsOrganisationService(
    HttpClient httpClient,
    ILogger logger,
    IOptions<OdsOrganisationOptions> options)
  {
    if (logger is null)
    {
      throw new ArgumentNullException(nameof(logger));
    }

    if (options is null)
    {
      throw new ArgumentNullException(nameof(options));
    }

    _httpClient = httpClient
      ?? throw new ArgumentNullException(nameof(httpClient));
    _logger = logger.ForContext<OdsOrganisationService>();
    _options = options.Value;
  }

  public async Task<OdsOrganisation> GetOdsOrganisationAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(
        $"'{nameof(odsCode)}' cannot be null or whitespace.",
        nameof(odsCode));
    }

    OdsOrganisation odsOrganisation = new();

    if (_options.IsUnknownOdsCode(odsCode))
    {
      _logger.Debug("ODS API not called because {odsCode} is unknown.",
        odsCode);

      return odsOrganisation;
    }

    string requestUri = $"{_options.Endpoint}{odsCode}";    

    try
    {
      _httpClient ??= new(HttpMessageHandler);

      HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

      if (response.StatusCode == HttpStatusCode.OK)
      {
        odsOrganisation = await JsonSerializer
         .DeserializeAsync<OdsOrganisation>(
           await response.Content.ReadAsStreamAsync(),
           options: new JsonSerializerOptions
           { PropertyNameCaseInsensitive = true });
      }
      else if (response.StatusCode == HttpStatusCode.NotFound)
      {
        _logger.Information("ODS API returned Not Found for URL '{url}'.",
          requestUri);
      }
      else
      {
        _logger.Warning("ODS API returned {phrase} for URL '{url}'",
          response.ReasonPhrase,
          requestUri);
      }
    }
    catch (Exception ex)
    {
      _logger.Error(ex, "ODS API failed with URL '{url}'.", requestUri);
    }

    return odsOrganisation;
  }
}
