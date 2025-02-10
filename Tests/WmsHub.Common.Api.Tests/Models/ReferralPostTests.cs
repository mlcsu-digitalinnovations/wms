using FluentAssertions;
using WmsHub.Common.Api.Models;
using Xunit;

namespace WmsHub.Common.Api.Tests.Models
{
  public class ReferralPostTests
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
      ReferralPost referralPost = new();

      // ACT
      referralPost.Email = email;

      // ASSERT
      referralPost.Email.Should().Be(email?.Trim().ToLower());
    }

  }
}
