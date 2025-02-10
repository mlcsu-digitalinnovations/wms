using FluentAssertions;
using System;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.BusinessIntelligence
{
  public class BiRmcUserInformationTests : AModelsBaseTests
  {
    private const string STATUS_REASON = "TestStatusReason";
    private const string DELAY_REASON = "TestDelayReason";
    private const string URI = 
      $"https://t.com?StatusReason={STATUS_REASON}" +
      $"&DelayReason={DELAY_REASON}&Ubrn=Test123456";

    [Theory]
    [InlineData("AddToRmcCallList", STATUS_REASON, null)]
    [InlineData("ConfirmDelay", null, DELAY_REASON)]
    [InlineData("ConfirmEmail", null, null)]
    [InlineData("ConfirmEthnicity", null, null)]
    [InlineData("ConfirmProvider", null, null)]
    [InlineData("RejectToEreferrals", STATUS_REASON, null)]
    [InlineData("UnableToContact", STATUS_REASON, null)]
    [InlineData("UpdateDateOfBirth", null, null)]
    [InlineData("UpdateMobileNumber", null, null)]
    public void ActionReturnsExpectedStatusReasonAndDelayReason(
      string action,
      string expectedStatusReason,
      string expectedDelayReason)
    {
      // Arrange.
      BiRmcUserInformation model = new(
        action,
        "Test",
        URI,
        DateTimeOffset.UtcNow,
        Guid.NewGuid()
        );

      // Assert.
      model.StatusReason.Should().Be(expectedStatusReason);
      model.DelayReason.Should().Be(expectedDelayReason);
      model.Ubrn.Should().Be("Test123456");
    }
  }
}
