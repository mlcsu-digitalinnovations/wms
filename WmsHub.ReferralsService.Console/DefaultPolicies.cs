using Polly.Extensions.Http;
using Polly;
using System;
using System.Net.Http;
using System.Net;

namespace WmsHub.ReferralsService.Console;

internal static class DefaultPolicies
{
  /// <summary>
  /// Default Http retry policy with exponential backoff.
  /// </summary>
  internal static IAsyncPolicy<HttpResponseMessage> Retry => HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
