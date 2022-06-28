using FluentAssertions;
using WmsHub.Common.Attributes;
using Xunit;

namespace WmsHub.Common.Tests.Attributes
{
  public class NotTrueAttributeTest
  {
    [Fact]
    public void True_False()
    {
      // arrange
      NotTrueAttribute notTrueAttribute = new();

      // act
      bool result = notTrueAttribute.IsValid(true);

      // assert
      result.Should().BeFalse();
    }

    [Fact]
    public void False_True()
    {
      // arrange
      NotTrueAttribute notTrueAttribute = new();

      // act
      bool result = notTrueAttribute.IsValid(true);

      // assert
      result.Should().BeFalse();
    }

    [Fact]
    public void Null_True()
    {
      // arrange
      NotTrueAttribute notTrueAttribute = new();

      // act
      bool result = notTrueAttribute.IsValid(null);

      // assert
      result.Should().BeTrue();
    }
  }
}
