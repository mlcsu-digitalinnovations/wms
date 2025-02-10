using FluentAssertions;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Helpers;

public class SexHelperTests
{
  public class IsValidSexStringTests : SexHelperTests
  {

    [Fact]
    public void InvalidSexStringReturnsFalse()
    {
      // Arrange.
      string invalidString = "Invalid";

      // Act.
      bool isValid = invalidString.IsValidSexString();

      // Assert.
      isValid.Should().BeFalse();
    }

    [Fact]
    public void NullReturnsFalse()
    {
      // Arrange.
      string nullString = null;

      // Act.
      bool isValid = nullString.IsValidSexString();

      // Assert.
      isValid.Should().BeFalse();
    }
    
    [Fact]
    public void ValidSexStringReturnsTrue()
    {
      // Arrange.
      string validString = "Not Specified";

      // Act.
      bool isValid = validString.IsValidSexString();

      // Assert.
      isValid.Should().BeTrue();
    }
  }

  public class TryParseSexTests : SexHelperTests
  {
    [Fact]
    public void InvalidStringReturnsFalseAndOutputsDefault()
    {
      // Arrange.
      string invalidString = "Invalid";

      // Act.
      bool parsedSuccessfully = invalidString.TryParseSex(out Sex sex);

      // Arrange.
      parsedSuccessfully.Should().BeFalse();
      sex.Should().Be(default);
    }

    [Fact]
    public void NullStringReturnsFalseAndOutputsDefault()
    {
      // Arrange.
      string nullString = null;

      // Act.
      bool parsedSuccessfully = nullString.TryParseSex(out Sex sex);

      // Arrange.
      parsedSuccessfully.Should().BeFalse();
      sex.Should().Be(default);
    }

    [Theory]
    [MemberData(nameof(SexStringTheoryData))]
    public void ValidStringReturnsTrueAndOutputsEnum(string sexString, Sex expectedSex)
    {
      // Arrange.

      // Act.
      bool parsedSuccessfully = sexString.TryParseSex(out Sex sex);

      // Arrange.
      parsedSuccessfully.Should().BeTrue();
      sex.Should().Be(expectedSex);
    }
  }

  public static TheoryData<string, Sex> SexStringTheoryData =>
    new()
    {
      { "F", Sex.Female },
      { "Female", Sex.Female },
      { "M", Sex.Male },
      { "Male", Sex.Male },
      { "NK", Sex.NotKnown },
      { "Not Known", Sex.NotKnown},
      { "NotKnown", Sex.NotKnown },
      { "Not Specified", Sex.NotSpecified },
      { "NotSpecified", Sex.NotSpecified },
      { "NS", Sex.NotSpecified }
    };
}
