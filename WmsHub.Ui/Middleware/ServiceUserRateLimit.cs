using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace WmsHub.Ui.Middleware
{
  public class ServiceUserRateLimit : IpRateLimitMiddleware
  {
    public ServiceUserRateLimit(
      RequestDelegate next,
      IProcessingStrategy processingStrategy,
      IOptions<IpRateLimitOptions> options,
      IIpPolicyStore policyStore,
      IRateLimitConfiguration config,
      ILogger<IpRateLimitMiddleware> logger)
        : base(
          next,
          processingStrategy,
          options,
          policyStore,
          config,
          logger)
    { }

    public override Task ReturnQuotaExceededResponse(
      HttpContext httpContext,
      RateLimitRule rule,
      string retryAfter)
    {
      httpContext.Response.StatusCode = 429;
      httpContext.Response.Redirect(Uri
        .EscapeDataString("/ServiceUser/Error?message=Too Many Requests"));

      return Task.CompletedTask;
    }
  }
}