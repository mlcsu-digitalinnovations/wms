using FluentAssertions;
using System;
using WmsHub.Referral.Api.Models.GeneralReferral;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class GetNhsNumberOkResponseTests
  {
    public class IsDateOfBmiAtRegistrationValid : GetNhsNumberOkResponseTests
    {

      [Theory]
      [InlineData(0, true)]
      [InlineData(1, true)]
      [InlineData(729, true)]
      [InlineData(730, false)]
      [InlineData(1000, false)]
      public void DateOfBmiAtRegistration(
        int days,
        bool expectedResult)
      {
        // arrange
        GetNhsNumberOkResponse response = new()
        {
          DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-days)
        };

        // act
        bool result = response.IsDateOfBmiAtRegistrationValid;

        // assert
        result.Should().Be(expectedResult);
      }
    }
  }
}
