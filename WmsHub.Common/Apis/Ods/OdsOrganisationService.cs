using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods.Models;

namespace WmsHub.Common.Apis.Ods
{
  public class OdsOrganisationService : IOdsOrganisationService
  {
    private readonly OdsOrganisationOptions _options = null;

    public HttpMessageHandler HttpMessageHandler { get; set; } =
      new HttpClientHandler();

    public OdsOrganisationService(
      IOptions<OdsOrganisationOptions> options)
    {
      _options = options.Value;
    }

    public async Task<OdsOrganisation> GetOdsOrganisationAsync(string odsCode)
    {
      if (string.IsNullOrWhiteSpace(odsCode))
      {
        throw new ArgumentException(
          $"'{nameof(odsCode)}' cannot be null or whitespace.",
          nameof(odsCode));
      }

      string requestUri = $"{_options.Endpoint}{odsCode}";

      using HttpClient client = new(HttpMessageHandler);
      HttpResponseMessage response = await client.GetAsync(requestUri);

      OdsOrganisation odsOrganisation;
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
        odsOrganisation = new OdsOrganisation();
      }
      else
      {
        throw new HttpRequestException(
          $"GET request to {requestUri} failed with response " +
            $"{response.ReasonPhrase}",
          null,
          response.StatusCode);
      }

      return odsOrganisation;
    }
  }
}
