using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.ChatBotService;

namespace WmsHub.ChatBot.Api.Clients
{
  public class ArcusClientHelper : IDisposable, IArcusClientHelper
  {
    private readonly ArcusOptions _options;
    private readonly HttpClient _client;
    private bool disposedValue;

    public ArcusClientHelper(IOptions<ArcusOptions> options)
    {
      _options = options.Value;
      _client = new HttpClient();
    }

    public HttpRequestMessage GetBlankPutRequest(string path)
    {
      return new HttpRequestMessage(HttpMethod.Put, path)
      {
        Content = new StringContent("", Encoding.UTF8, "application/json")
      };
    }

    public HttpRequestMessage GetPutObjectRequestAsJson(string json,
      string path)
    {
      return new HttpRequestMessage(HttpMethod.Put, path)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
    }

    protected async Task<HttpResponseMessage> PutAsync(string path,
      StringContent content)
    {

      var response = await _client.PutAsync(path, content);
      if (!response.IsSuccessStatusCode)
        throw new ArgumentException($"Put request to '{path}' with content " +
          $"'{content}' failed with {response.StatusCode}: " +
          $"{response.ReasonPhrase}");
      return response;

    }

    protected async Task<HttpResponseMessage> SendAsync(string path,
      HttpRequestMessage request)
    {
      var req = request ?? GetBlankPutRequest(path);
      var response = await _client.SendAsync(req);
      if (!response.IsSuccessStatusCode)
        throw new ArgumentException(response.ReasonPhrase);
      return response;

    }

    public virtual async Task<HttpResponseMessage> BatchPost(IArcusCall request)
    {
      if (!request.Callees.Any()) throw new ArgumentOutOfRangeException(
        nameof(request.Callees), "Request must contain callee numbers");

      //Turn into json
      string json = JsonConvert.SerializeObject(request);
      //_logger.Debug($"Put to Arcus: {json}", json);

      //Add header api-key
      _client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);

      //Post
      HttpResponseMessage response = await PutAsync(_options.Endpoint,
        new StringContent(json, Encoding.UTF8, "application/json"));

      return response;

    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (_client != null) _client.Dispose();
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. 
      // Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
