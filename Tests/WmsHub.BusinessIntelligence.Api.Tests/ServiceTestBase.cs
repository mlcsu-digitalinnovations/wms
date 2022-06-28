using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using WmsHub.BusinessIntelligence.Api.Tests;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.BusinessIntelligence.Api.Test
{
  public class ServiceTestsBase
  {
    public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
    public static readonly Guid INVALID_ID =
      new Guid("11111111-1111-1111-1111-111111111111");
    protected readonly ServiceFixture _serviceFixture;
    protected ILogger _log;

    public ServiceTestsBase(ServiceFixture serviceFixture)
    {
      _serviceFixture = serviceFixture;
    }

    public ServiceTestsBase(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
    {
      _serviceFixture = serviceFixture;
      _log = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.TestOutput(testOutputHelper, LogEventLevel.Verbose)
        .CreateLogger();
    }

    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      List<Claim> claims = new List<Claim>()
      { new Claim(ClaimTypes.Sid, TEST_USER_ID) };

      var claimsIdentity = new ClaimsIdentity(claims);

      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }
  }
}