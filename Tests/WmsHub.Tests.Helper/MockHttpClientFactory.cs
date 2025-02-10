// TODO: Migrate to Mustard
using RichardSzalay.MockHttp;
using System;
using System.Net.Http;

namespace WmsHub.Tests.Helper;

public class MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler)
  : IHttpClientFactory
{
  private readonly MockHttpMessageHandler _mockHttpMessageHandler = mockHttpMessageHandler
    ?? throw new ArgumentNullException(nameof(mockHttpMessageHandler));

  public HttpClient CreateClient(string name)
  {
    return _mockHttpMessageHandler.ToHttpClient();
  }
}
