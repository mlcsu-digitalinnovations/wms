using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WmsHub.Common.Apis.Ods.Fhir;

public class OdsFhirService : IOdsFhirService
{
  private readonly HttpClient _httpClient;
  private readonly OdsFhirServiceOptions _options;

  public OdsFhirService(
    HttpClient httpClient,
    IOptions<OdsFhirServiceOptions> options)
  {
    _httpClient = httpClient;
    _options = options.Value;
  }

  public async Task<bool> OrganisationCodeExistsAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentException(
        $"'{nameof(odsCode)}' cannot be null or whitespace.", nameof(odsCode));
    }

    string requestUri = $"{_options.OrganisationPath}{odsCode}";

    HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

    if (response.StatusCode == HttpStatusCode.OK)
    {
      return true;
    }
    else if (response.StatusCode == HttpStatusCode.NotFound)
    {
      return false;
    }

    throw new Exception(
      $"Failed to obtain result from {requestUri}. " +
      $"Response {response.StatusCode}: {response.ReasonPhrase}.");
  }
}
