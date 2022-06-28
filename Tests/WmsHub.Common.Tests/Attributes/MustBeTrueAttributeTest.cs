using FluentAssertions;
using WmsHub.Common.Attributes;
using Xunit;

namespace WmsHub.Common.Tests.Attributes
{
  public class MustBeTrueAttributeTest
  {
    [Fact]
    public void True_True()
    {
      // arrange
      MustBeTrueAttribute mustBeTrueAttribute = new();

      // act
      bool result = mustBeTrueAttribute.IsValid(true);

      // assert
      result.Should().BeTrue();
    }

    [Fact]
    public void False_False()
    {
      // arrange
      MustBeTrueAttribute mustBeTrueAttribute = new();

      // act
      bool result = mustBeTrueAttribute.IsValid(false);

      // assert
      result.Should().BeFalse();
    }

    [Fact]
    public void Null_False()
    {
      // arrange
      MustBeTrueAttribute mustBeTrueAttribute = new();

      // act
      bool result = mustBeTrueAttribute.IsValid(null);

      // assert
      result.Should().BeFalse();
    }
  }
}
