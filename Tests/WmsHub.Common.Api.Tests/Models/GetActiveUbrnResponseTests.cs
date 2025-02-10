using FluentAssertions;
using FluentAssertions.Execution;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Enums;
using Xunit;

namespace WmsHub.Common.Api.Tests.Models;

public class GetActiveUbrnResponseTests
{
  [Theory]
  [InlineData(
    "AWAITINGUPDATE", ErsReferralStatus.AwaitingUpdate, true, false, false)]
  [InlineData(
    "Awaitingupdate", ErsReferralStatus.AwaitingUpdate, true, false, false)]
  [InlineData("INPROGRESS", ErsReferralStatus.InProgress, false, true, false)]
  [InlineData("inProgress", ErsReferralStatus.InProgress, false, true, false)]
  [InlineData("ONHOLD", ErsReferralStatus.OnHold, false, false, true)]
  [InlineData("onhold", ErsReferralStatus.OnHold, false, false, true)]
  [InlineData("invalid", ErsReferralStatus.Undefined, false, false, false)]
  [InlineData("", ErsReferralStatus.Undefined, false, false, false)]
  [InlineData(null, ErsReferralStatus.Undefined, false, false, false)]
  public void ErsReferralStatusSetWhenStatusSet(
    string status,
    ErsReferralStatus ersReferralStatus,
    bool isAwaitingUpdate,
    bool isInProgress,
    bool IsOnHold)
  {
    // Arrange.
    GetActiveUbrnResponse response = new();

    // Act.
    response.Status = status;

    // Assert.
    using (new AssertionScope())
    {
      response.ErsReferralStatus.Should().Be(ersReferralStatus);
      response.IsAwaitingUpdate.Should().Be(isAwaitingUpdate);
      response.IsInProgress.Should().Be(isInProgress);
      response.IsOnHold.Should().Be(IsOnHold);
    }
  }
}
