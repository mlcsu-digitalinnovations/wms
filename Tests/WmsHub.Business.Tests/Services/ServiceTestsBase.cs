using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using WmsHub.Business.Enums;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public class ServiceTestsBase : ATheoryData
  {
    public const string VALID_NHS_NUMBER = "9999999999";
    public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
    public static readonly Guid INVALID_ID =
      new("11111111-1111-1111-1111-111111111111");
    protected readonly ServiceFixture _serviceFixture;
    protected ILogger _log;
    protected Random _rnd = new();
    protected string[] _expectedTriageLevels = new[] { "1", "2", "3" };

    public ServiceTestsBase(ITestOutputHelper testOutputHelper)
    {
      _log = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.TestOutput(testOutputHelper, LogEventLevel.Verbose)
        .CreateLogger();
    }

    protected Guid TestUserId => Guid.Parse(TEST_USER_ID);

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

    public static IEnumerable<object[]> ReferralStatusStrings()
    {
      var statuses = new List<object[]>();
      foreach (string status in Enum.GetNames<ReferralStatus>())
      {
        statuses.Add(new object[] { status.ToString() });
      }
      return statuses;
    }

    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      return GetClaimsPrincipalWithId(TEST_USER_ID);
    }


    protected static ClaimsPrincipal GetInvalidClaimsPrincipal()
    {
      return GetClaimsPrincipalWithId(Guid.NewGuid().ToString());
    }

    protected static ClaimsPrincipal GetClaimsPrincipalWithId(string id)
    {
      List<Claim> claims = new List<Claim>()
        { new Claim(ClaimTypes.Sid, id) };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }
  }
}
