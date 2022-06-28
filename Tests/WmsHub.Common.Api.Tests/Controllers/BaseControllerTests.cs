using FluentAssertions;
using System;
using WmsHub.Common.Api.Controllers;
using Xunit;

namespace WmsHub.Common.Api.Tests.Controllers
{
  public class BaseControllerTests
  {
    public const string IP_CALLBACK = "127.0.0.1";
    public const string IP_HOME = "192.168.1.1";
    public const string IP_UNAUTHORISED_1 = "8.8.8.8";
    public const string IP_UNAUTHORISED_2 = "4.4.4.4";
    public const string HEADER_X_AZURE_SOCKETIP = "X-Azure-SocketIP";

    public readonly string[] traceIpWhitelist = new string[]
      { IP_HOME, IP_CALLBACK };

    // A mock controller required because the base controller has a protected
    // constructor
    public class MockController : BaseController
    {
      private readonly string _azureSocketIp;

      public MockController(string azureSocketIp) : base(null)
      {
        _azureSocketIp = azureSocketIp;
      }

      public new void CheckAzureSocketIpAddressInWhitelist(
        string[] traceIpWhitelist)
      {
        base.CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist);
      }
      protected override string GetAzureSocketIp()
      {
        return _azureSocketIp;
      }
    }


    public class CheckAzureSocketIpAddressInWhitelist : BaseControllerTests
    {
      [Theory]
      [InlineData(IP_HOME)]
      [InlineData(IP_CALLBACK)]
      public void AzureSocketIpInWhiteList(string ipAddress)
      {
        // arrange
        MockController baseController = new MockController(ipAddress);

        // act
        var exception = Record.Exception(() => baseController
          .CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist));

        exception.Should().BeNull();
      }

      [Theory]
      [InlineData(IP_UNAUTHORISED_1)]
      [InlineData(IP_UNAUTHORISED_2)]
      public void AzureSocketIpNotInWhiteList(string ipAddress)
      {
        // arrange
        MockController baseController = new MockController(ipAddress);
        var expectedExceptionMessage = 
          $"{HEADER_X_AZURE_SOCKETIP}: '{ipAddress}' not in " +
          $"whitelist '{string.Join(", ", traceIpWhitelist)}'.";

        // act
        var exception = Record.Exception(() => baseController
          .CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist));

        exception.Should().BeOfType(typeof(UnauthorizedAccessException));
        exception.Message.Should().Be(expectedExceptionMessage);
      }
    }
  }
}
