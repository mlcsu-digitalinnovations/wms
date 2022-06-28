using FluentAssertions;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Common.Tests
{
  public class RegexUtilitiesTests
  {

    public class IsValidEmail
    {
      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("a.b.@yahoo.com")]
      public void InvalidEmail_False(string invalidEmail)
      {
        // act
        bool isValid = RegexUtilities.IsValidEmail(invalidEmail);

        // assert
        isValid.Should().BeFalse();
      }

      // NOTE: Only enable this test when updating IsValidEmail because
      // it will add 16993 tests for anonymised emails that were added to the 
      // system prior to 31/01/2022
      //[Theory]
      //[ClassData(typeof(ValidEmailTheoryData))]
      //public void ValidEmail_True(string validEmail)
      //{
      //  // act
      //  bool isValid = RegexUtilities.IsValidEmail(validEmail);

      //  // assert
      //  isValid.Should().BeTrue();
      //}
    }

    public class IsValidGpPracticeOdsCode
    {
      [Theory]
      [InlineData("M12345", true, "it is valid")]
      [InlineData("m12345", true, "it should be converted to upper case")]
      [InlineData("M123456789", false, "it is too long")]
      [InlineData("M1234", false, "it is too short")]
      [InlineData("System EMIS F11111", false, "it contains extra text")]
      [InlineData("", false, "it is a blank string")]
      [InlineData(null, false, "it is null")]
      public void DefaultOptions(
        string code, bool expectedResult, string because)
      {
        // act
        bool result = RegexUtilities.IsValidGpPracticeOdsCode(code);

        // assert
        result.Should().Be(expectedResult, because: because);
      }
    }

    public class IsWildCardTests
    {
      [Theory]
      [InlineData("test1.rtf", "p*", false)]
      [InlineData("some summary.rtf", "*Summary.RTF", true)]
      [InlineData("another summary.rtf", "*summary.rtf", true)]
      [InlineData("some summary.rtf", "some*", true)]
      [InlineData("some summary.rtf", "SOME*", true)]
      [InlineData("some summary.rtf", "SOME*.doc", false)]
      [InlineData("some summary.rtf", "SOME*y.rtf", true)]
      [InlineData("some summary.rtf", "SOME*.rtf", true)]
      [InlineData("this is a big hello", "*big*", true)]
      [InlineData("this is a big hello", "*small*", false)]
      [InlineData("ambulance", "a*b*c*", true)]
      [InlineData("ambulance", "*a*b*c*", true)]
      [InlineData("ambulance", "*c*b*a*", false)]
      [InlineData("ambulance", "*a*b*c", false)]
      [InlineData("ambulance", "ambulance", true)]
      [InlineData("potato", "tomato", false)]
      public void ReturnsExpected(string subject, string wildcard, 
        bool expectedResult)
      {
        bool result = RegexUtilities.IsWildcardMatch(wildcard, subject);

        result.Should().Be(expectedResult);
      }

    }

  }
}
