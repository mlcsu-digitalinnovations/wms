// TODO: Migrate to Mustard
using RichardSzalay.MockHttp;
using System.Net.Http;

namespace WmsHub.Tests.Helper;

public static class MockHttpMessageHandlerExtensions
{
  public static IHttpClientFactory ToHttpClientFactory(
    this MockHttpMessageHandler mockHttpMessageHandler)
  {
    return new MockHttpClientFactory(mockHttpMessageHandler);
  }
}
