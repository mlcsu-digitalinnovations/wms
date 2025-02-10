using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace WmsHub.Business.Helpers;

public static class HttpClientHelper
{
  public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
      .WaitAndRetryAsync(6, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }

  public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicyNoNotFound()
  {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .WaitAndRetryAsync(6, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }

  public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
  {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
  }
}
