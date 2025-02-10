using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Console.Logging
{
  public class ApiKeyHttpClient : IHttpClient
  {
    private readonly HttpClient _httpClient;

    public ApiKeyHttpClient() => _httpClient = new HttpClient();

    public void Configure(IConfiguration configuration)
    {
      var apiKey = configuration["Data:HubRegistrationAPIKey"];
      if (!string.IsNullOrWhiteSpace(apiKey))
      {
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
      }
    }

    public void Dispose() => _httpClient?.Dispose();

    public Task<HttpResponseMessage> PostAsync(
      string requestUri,
      HttpContent content)
    {
      return _httpClient.PostAsync(requestUri, content);
    }

    public async Task<HttpResponseMessage> PostAsync(
      string requestUri,
      Stream contentStream)
    {

      using StreamContent content = new(contentStream);
      content.Headers.Add("Content-Type", "application/json");

      HttpResponseMessage msg = await _httpClient.PostAsync(requestUri, content);

      return msg;
    }

    public async Task<HttpResponseMessage> PostAsync(
      string requestUri,
      Stream contentStream,
      CancellationToken cancellationToken)
    {
      using StreamContent content = new(contentStream);
      content.Headers.Add("Content-Type", "application/json");

      return await _httpClient.PostAsync(requestUri, content, cancellationToken);
    }
  }
}
