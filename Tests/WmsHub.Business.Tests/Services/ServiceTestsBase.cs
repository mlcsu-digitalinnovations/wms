using Serilog;
using System;
using WmsHub.Business.Enums;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public class ServiceTestsBase : ATheoryData
{
  public static readonly Guid INVALID_ID = new("11111111-1111-1111-1111-111111111111");
  public const string VALID_NHS_NUMBER = "9999999993";

  protected string[] _expectedTriageLevels = ["1", "2", "3"];
  protected ILogger _log = new LoggerConfiguration().CreateLogger();
  protected Random _rnd = new();
  protected readonly ServiceFixture _serviceFixture;

  public ServiceTestsBase()
  { }

  public ServiceTestsBase(ServiceFixture serviceFixture)
  {
    _serviceFixture = serviceFixture;
  }

  public ServiceTestsBase(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
  {
    _serviceFixture = serviceFixture;
    _ = testOutputHelper;
  }

  public static TheoryData<string> ReferralStatusStrings()
  {
    TheoryData<string> statuses = [];
    foreach (string status in Enum.GetNames<ReferralStatus>())
    {
      statuses.Add(status.ToString());
    }
    return statuses;
  }

  protected static Guid TestUserId => Guid.Parse(TEST_USER_ID);
}
