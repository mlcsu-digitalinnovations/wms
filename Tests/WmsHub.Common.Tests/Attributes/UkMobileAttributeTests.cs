using FluentAssertions;
using WmsHub.Common.Attributes;
using Xunit;

namespace WmsHub.Common.Tests.Attributes;

public class UkMobileAttributeTests
{
  [Fact]
  public void IsValidUkNumber()
  {
    // Arrange.
    UkMobileAttribute ukMobileAttribute = new();

    // Act.
    bool result = ukMobileAttribute.IsValid("+447000000001");

    // Assert.
    result.Should().BeTrue();
  }

  [Theory]
  [InlineData("07000000001")]
  [InlineData("+4470000000011")]
  [InlineData("0700000000")]
  [InlineData("+441623111897")]
  public void IsInValidUkNumber(string mobile)
  {
    // Arrange.
    UkMobileAttribute ukMobileAttribute = new();

    // Act.
    bool result = ukMobileAttribute.IsValid(mobile);

    // Assert.
    result.Should().BeFalse();
  }
}
