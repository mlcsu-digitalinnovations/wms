using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests
{
  public class ServiceTestsBase
  {
    protected const string ETHNICITY__IRISH = "Irish";
    protected const string ETHNICITY_GROUP__WHITE = "White";
    protected const string STAFF_ROLE__AMBULANCE_STAFF = "Ambulance staff";
    public const string TEST_USER_ID = "76D69A87-D9A7-EAC6-2E2D-A6017D02E04F";
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
      { new Claim(ClaimTypes.Sid, TEST_USER_ID),
        new Claim(ClaimTypes.Sid, "SelfReferral.Service")
      };

      var claimsIdentity = new ClaimsIdentity(claims);

      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }

    protected static ClaimsPrincipal GetGeneralReferralServiceClaimsPrincipal()
    {
      List<Claim> claims = new List<Claim>()
      { 
        new Claim(ClaimTypes.Name, "GeneralReferral.Service")
      };

      var claimsIdentity = new ClaimsIdentity(claims);

      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
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