using FluentAssertions;
using WmsHub.Referral.Api.Models;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class SelfReferralEmailInUseTests
  {
    [Theory]
    [InlineData("mock.test@nhs.net")]
    [InlineData("Mock.test@nhs.net")]
    [InlineData("Mock.Test@nhs.net")]
    [InlineData("Mock.Test@NHS.net")]
    [InlineData("MOCK.TEST@NHS.NET")]
    [InlineData("mock.test@nhs.net ")]
    [InlineData(" Mock.test@nhs.net")]
    [InlineData("")]
    [InlineData(null)]
    public void Email_Trimmed_And_Converted_To_Lower(string email)
    {
      // ARRANGE
      SelfReferralEmailInUse selfReferralEmailInUse = new();

      // ACT
      selfReferralEmailInUse.Email = email;

      // ASSERT
      selfReferralEmailInUse.Email.Should().Be(email?.Trim().ToLower());
    }
  }
}
