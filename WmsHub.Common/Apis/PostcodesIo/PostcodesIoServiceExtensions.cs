using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace WmsHub.Common.Apis.Ods.PostcodesIo;

/// <summary>
/// Extension methods for the Postcodes IO service
/// </summary>
public static class PostcodesIoServiceExtensions
{
  /// <summary>
  /// Adds Postcodes IO service
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> for adding 
  /// services.</param>
  /// <param name="configureOptions">A delegate to configure the 
  /// <see cref="PostcodesIoServiceOptions"/>.</param>
  /// <returns></returns>
  public static IServiceCollection AddPostcodesIoService(
    this IServiceCollection services,
    IConfiguration configuration,
    IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy = null,
    IAsyncPolicy<HttpResponseMessage> retryPolicy = null)
  {
    if (services is null)
    {
      throw new ArgumentNullException(nameof(services));
    }

    if (configuration is null)
    {
      throw new ArgumentNullException(nameof(configuration));
    }

    services.AddOptions<PostcodesIoServiceOptions>()
      .Bind(configuration.GetSection(PostcodesIoServiceOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddScoped<IPostcodesIoService, PostcodesIoService>();

    services.AddHttpClient<IPostcodesIoService, PostcodesIoService>(client =>
    {
      client.BaseAddress = new Uri(
        configuration.GetSection(PostcodesIoServiceOptions.SectionKey)
          .GetValue(
            nameof(PostcodesIoServiceOptions.BaseUrl),
            PostcodesIoServiceOptions.BASE_URL));
    })
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler(circuitBreakerPolicy ?? DefaultCircuitBreakerPolicy())
      .AddPolicyHandler(retryPolicy ?? DefaultRetryPolicy());

    return services;
  }

  private static IAsyncPolicy<HttpResponseMessage> DefaultCircuitBreakerPolicy()
  {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
  }

  private static IAsyncPolicy<HttpResponseMessage> DefaultRetryPolicy()
  {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
      .WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }
}
