using FluentAssertions;
using System;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;
using Xunit;
namespace WmsHub.Common.Api.Tests.Controllers;

public class BaseControllerTests
{
  public const string IP_CALLBACK = "127.0.0.1";
  public const string IP_HOME = "192.168.1.1";
  public const string IP_UNAUTHORISED_1 = "8.8.8.8";
  public const string IP_UNAUTHORISED_2 = "4.4.4.4";
  public const string HEADER_X_AZURE_SOCKETIP = "X-Azure-SocketIP";

  public readonly string[] traceIpWhitelist = new string[]
    { IP_HOME, IP_CALLBACK };

  // A mock controller is required because the base controller has a protected constructor
  public class MockController(string azureSocketIp) : BaseController(null)
  {
    private readonly string _azureSocketIp = azureSocketIp;

    public new void CheckAzureSocketIpAddressInWhitelist(string[] traceIpWhitelist)
    {
      base.CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist);
    }

    public static new DateRange GetDateRange(
      DateTimeOffset? fromDate,
      DateTimeOffset? toDate,
      int offset = 31)
    {
      return BaseController.GetDateRange(fromDate, toDate, offset);
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
      MockController baseController = new(ipAddress);

      // act
      Exception exception = Record.Exception(() => baseController
        .CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist));

      exception.Should().BeNull();
    }

    [Theory]
    [InlineData(IP_UNAUTHORISED_1)]
    [InlineData(IP_UNAUTHORISED_2)]
    public void AzureSocketIpNotInWhiteList(string ipAddress)
    {
      // arrange
      MockController baseController = new(ipAddress);
      string expectedExceptionMessage =
        $"{HEADER_X_AZURE_SOCKETIP}: '{ipAddress}' not in " +
        $"whitelist '{string.Join(", ", traceIpWhitelist)}'.";

      // act
      Exception exception = Record.Exception(() => baseController
        .CheckAzureSocketIpAddressInWhitelist(traceIpWhitelist));

      exception.Should().BeOfType(typeof(UnauthorizedAccessException));
      exception.Message.Should().Be(expectedExceptionMessage);
    }
  }

  public class DateRangeChecks : BaseControllerTests
  {
    public readonly TimeSpan _oneMinutePrecision = new(0, 1, 0);

    [Fact]
    public void FromDateAfterToDate_Throws_DateRangeException()
    {
      // Arrange.
      string expectedMessage = "*from*cannot be later than*to*";
      DateTimeOffset to = DateTimeOffset.UtcNow.AddDays(-31);
      DateTimeOffset from = DateTimeOffset.UtcNow;

      // Act.
      Action act = () => MockController.GetDateRange(from, to);

      // Assert.
      act.Should().Throw<DateRangeNotValidException>()
        .Which.Message.Should().Match(expectedMessage);
    }

    [Fact]
    public void NullDates_Return_ValidDates()
    {
      // Arrange.
      DateTimeOffset expectedFrom = DateTimeOffset.Now.AddDays(-31);
      DateTimeOffset expectedTo = DateTimeOffset.Now;

      // Act.
      DateRange result = MockController.GetDateRange(null, null);

      // Assert.
      result.From.Should().BeCloseTo(expectedFrom, _oneMinutePrecision);
      result.To.Should().BeCloseTo(expectedTo, _oneMinutePrecision);
    }

    [Fact]
    public void ToDateInPast_FromIsNull_Return_ValidDates()
    {
      // Arrange.
      DateTimeOffset to = DateTimeOffset.UtcNow.AddDays(-31);
      DateTimeOffset expectedFrom = to.AddDays(-31);

      // Act.
      DateRange result = MockController.GetDateRange(null, to);

      // Assert.
      result.From.Should().Be(expectedFrom);
      result.To.Should().BeCloseTo(to, _oneMinutePrecision);
    }

    [Fact]
    public void ToDateNull_From60DaysPast_Return_ValidDates()
    {
      // Arrange.
      DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(-60);
      DateTimeOffset expectedTo = from.AddDays(31);

      // Act.
      DateRange result = MockController.GetDateRange(from, null);

      // Assert.
      result.From.Should().Be(from);
      result.To.Should().BeCloseTo(expectedTo, _oneMinutePrecision);
    }

    [Fact]
    public void ToDateNull_FromDate10DaysPast_Return_ValidDates()
    {
      // Arrange.
      DateTimeOffset from = DateTimeOffset.UtcNow.AddDays(-10);
      DateTimeOffset expectedTo = from.AddDays(31);

      // Act.
      DateRange result = MockController.GetDateRange(from, null);

      // Assert.
      result.From.Should().Be(from);
      result.To.Should().BeCloseTo(expectedTo, _oneMinutePrecision);
    }

    [Fact]
    public void ToDateToday_FromDate30DaysPast_Return_ValidDatesIgnoringOffset()
    {
      // Arrange.
      DateTimeOffset to = DateTimeOffset.UtcNow;
      DateTimeOffset from = to.AddDays(-30);

      // Act.
      DateRange result = MockController.GetDateRange(from, to, 10);

      // Assert.
      result.From.Should().Be(from);
      result.To.Should().Be(to);
    }

    [Fact]
    public void ToDateToday_FromDate40DaysPast_Return_ValidDatesIgnoringOffset()
    {
      // Arrange.
      DateTimeOffset to = DateTimeOffset.UtcNow;
      DateTimeOffset from = to.AddDays(-40);

      // Act.
      DateRange result = MockController.GetDateRange(from, to, 31);

      // Assert.
      result.From.Should().Be(from);
      result.To.Should().Be(to);
    }
  }
}
