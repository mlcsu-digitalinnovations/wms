using FluentAssertions;
using FluentAssertions.Execution;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using Xunit;

namespace WmsHub.Business.Tests.Models.ReferralService;

public class CanCreateReferralResponseTests
{
  public class Constructors : CanCreateReferralResponseTests
  {
    [Fact]
    public void TwoParameters()
    {
      // Arrange.
      CanCreateReferralResult expectedCanCreateResult =
        CanCreateReferralResult.CanCreate;
      string expectedReason = "Test Reason";

      // Act.
      CanCreateReferralResponse canCreateReferralResponse = new(
        expectedCanCreateResult,
        expectedReason);

      // Assert.
      using (new AssertionScope())
      {
        canCreateReferralResponse.CanCreateResult.Should()
          .Be(expectedCanCreateResult);
        canCreateReferralResponse.Reason.Should().Be(expectedReason);
        canCreateReferralResponse.Referral.Should().BeNull();
      }
    }

    [Fact]
    public void ThreeParameters()
    {
      // Arrange.
      CanCreateReferralResult expectedCanCreateResult =
        CanCreateReferralResult.CanCreate;
      string expectedReason = "Test Reason";
      IReferral expectedReferral = new Referral();

      // Act.
      CanCreateReferralResponse canCreateReferralResponse = new(
        expectedCanCreateResult,
        expectedReason,
        expectedReferral);

      // Assert.
      using (new AssertionScope())
      {
        canCreateReferralResponse.CanCreateResult.Should()
          .Be(expectedCanCreateResult);
        canCreateReferralResponse.Reason.Should().Be(expectedReason);
        canCreateReferralResponse.Referral.Should().Be(expectedReferral);
      }
    }
  }
}
