using FluentAssertions;
using FluentAssertions.Execution;
using System;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests
{
  public class PhoneExtensionsTests: ATheoryData
  {
    public class ConvertToUkLandlineNumber : PhoneExtensionsTests 
    {
      [Theory]
      [MemberData(nameof(CanBeConvertedPhoneNumber))]
      public void CanBeConverted_NoNullOrWhiteSpace(
        string number, 
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkLandlineNumber(false);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [InlineData("", "")]
      [InlineData(null, null)]
      public void CanBeConverted_NullOrWhiteSpace(
        string number, 
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkLandlineNumber(true);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [MemberData(nameof(CannotBeConvertedPhoneNumber))]
      public void CannotBeConverted_NoNullOrWhiteSpace(string number)
      {
        // Arrange.
        string expected = 
          $"'{number}' cannot be converted to a UK landline number.";
        // Act.
        Func<string> act = () => number.ConvertToUkLandlineNumber(false);

        // Assert.
        act.Should().Throw<FormatException>().WithMessage(expected);
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public void NullOrWhiteSpace_NotAllowed(string number)
      {
        // Arrange.
        string expected = "Value cannot be null. (Parameter 'value')";

        // Act.
        Func<string> act = () => number.ConvertToUkLandlineNumber(false);

        // Assert.
        act.Should().Throw<ArgumentNullException>().WithMessage(expected);
      }
    }

    public class ConvertToUkMobileNumber : PhoneExtensionsTests
    {
      [Theory]
      [InlineData("07111111111", "+447111111111")]
      [InlineData("+447111111111", "+447111111111")]
      [InlineData("07111 111111 ", "+447111111111")]
      [InlineData("(07111) 111111 ", "+447111111111")]
      [InlineData("07111-111111 ", "+447111111111")]
      public void CanBeConverted_NoNullOrWhiteSpace(
        string number,
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkMobileNumber(false);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [InlineData("", "")]
      [InlineData(null, null)]
      public void CanBeConverted_NullOrWhiteSpace(
        string number,
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkMobileNumber(true);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [InlineData("0")]
      [InlineData("07")]
      [InlineData("077")]
      [InlineData("0777")]
      [InlineData("07777")]
      [InlineData("077777")]
      [InlineData("0777777")]
      [InlineData("07777777")]
      [InlineData("077777777")]
      [InlineData("0777777777")]
      [InlineData("01777777777")]
      [InlineData("02777777777")]
      [InlineData("03777777777")]
      [InlineData("04777777777")]
      [InlineData("05777777777")]
      [InlineData("06777777777")]
      [InlineData("08777777777")]
      [InlineData("09777777777")]
      [InlineData("00777777777")]
      [InlineData("077777777777")]
      [InlineData("0777777777777")]
      [InlineData("not a number")]
      [InlineData("+not a number")]
      public void CannotBeConverted_NoNullOrWhiteSpace(string number)
      {
        // Arrange.
        string expected = 
          $"'{number}' cannot be converted to a UK mobile number.";
        // Act.
        Func<string> act = () => number.ConvertToUkMobileNumber(false);

        // Assert.
        act.Should().Throw<FormatException>().WithMessage(expected);
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public void NullOrWhiteSpace_NotAllowed(string number)
      {
        // Arrange.
        string expected = "Value cannot be null. (Parameter 'value')";

        // Act.
        Func<string> act = () => number.ConvertToUkMobileNumber(false);

        // Assert.
        act.Should().Throw<ArgumentNullException>().WithMessage(expected);
      }
    }

    public class ConvertToUkPhoneNumber : PhoneExtensionsTests
    {
      [Theory]
      [InlineData("07111111111", "+447111111111")]
      [InlineData("+447111111111", "+447111111111")]
      [InlineData("07111 111111 ", "+447111111111")]
      [InlineData("(07111) 111111 ", "+447111111111")]
      [InlineData("07111-111111 ", "+447111111111")]
      [InlineData("0111111111", "+44111111111")]
      [InlineData("0211111111", "+44211111111")]
      [InlineData("0311111111", "+44311111111")]
      [InlineData("0411111111", "+44411111111")]
      [InlineData("0511111111", "+44511111111")]
      [InlineData("0611111111", "+44611111111")]
      [InlineData("0811111111", "+44811111111")]
      [InlineData("0911111111", "+44911111111")]
      [InlineData("+44111111111", "+44111111111")]
      [InlineData("+44211111111", "+44211111111")]
      [InlineData("+44311111111", "+44311111111")]
      [InlineData("+44411111111", "+44411111111")]
      [InlineData("+44511111111", "+44511111111")]
      [InlineData("+44611111111", "+44611111111")]
      [InlineData("+44811111111", "+44811111111")]
      [InlineData("+44911111111", "+44911111111")]
      [InlineData("09111 11111 ", "+44911111111")]
      [InlineData("(09111) 11111 ", "+44911111111")]
      [InlineData("09111-11111 ", "+44911111111")]
      [InlineData("01111111111", "+441111111111")]
      [InlineData("02111111111", "+442111111111")]
      [InlineData("03111111111", "+443111111111")]
      [InlineData("04111111111", "+444111111111")]
      [InlineData("05111111111", "+445111111111")]
      [InlineData("06111111111", "+446111111111")]
      [InlineData("08111111111", "+448111111111")]
      [InlineData("09111111111", "+449111111111")]
      [InlineData("+441111111111", "+441111111111")]
      [InlineData("+442111111111", "+442111111111")]
      [InlineData("+443111111111", "+443111111111")]
      [InlineData("+444111111111", "+444111111111")]
      [InlineData("+445111111111", "+445111111111")]
      [InlineData("+446111111111", "+446111111111")]
      [InlineData("+448111111111", "+448111111111")]
      [InlineData("+449111111111", "+449111111111")]
      [InlineData("09111 111111 ", "+449111111111")]
      [InlineData("(09111) 111111 ", "+449111111111")]
      [InlineData("09111-111111 ", "+449111111111")]
      public void CanBeConverted_NoNullOrWhiteSpace(
        string number,
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkPhoneNumber(false);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [InlineData("", "")]
      [InlineData(null, null)]
      public void CanBeConverted_NullOrWhiteSpace(
        string number,
        string expected)
      {
        // Act.
        string convertedNumber = number.ConvertToUkPhoneNumber(true);

        // Assert.
        convertedNumber.Should().Be(expected);
      }

      [Theory]
      [InlineData("0")]
      [InlineData("01")]
      [InlineData("011")]
      [InlineData("0111")]
      [InlineData("01111")]
      [InlineData("011111")]
      [InlineData("0111111")]
      [InlineData("01111111")]
      [InlineData("011111111")]
      [InlineData("00111111111")]
      [InlineData("011111111111")]
      [InlineData("0111111111111")]
      [InlineData("07")]
      [InlineData("077")]
      [InlineData("0777")]
      [InlineData("07777")]
      [InlineData("077777")]
      [InlineData("0777777")]
      [InlineData("07777777")]
      [InlineData("077777777")]
      [InlineData("0777777777")]
      [InlineData("077777777777")]
      [InlineData("0777777777777")]
      [InlineData("not a number")]
      [InlineData("+not a number")]
      public void CannotBeConverted_NoNullOrWhiteSpace(string number)
      {
        // Arrange.
        string expected = 
          $"'{number}' cannot be converted to a UK mobile number.";

        // Act.
        Func<string> act = () => number.ConvertToUkMobileNumber(false);

        // Assert.
        act.Should().Throw<FormatException>().WithMessage(expected);
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public void NullOrWhiteSpace_NotAllowed(string number)
      {
        // Arrange.
        string expected = "Value cannot be null. (Parameter 'value')";

        // Act.
        Func<string> act = () => number.ConvertToUkMobileNumber(false);

        // Assert.
        act.Should().Throw<ArgumentNullException>().WithMessage(expected);
      }
    }

    public class IsLandline : PhoneExtensionsTests
    {
      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("0")]
      [InlineData("01")]
      [InlineData("021")]
      [InlineData("0311")]
      [InlineData("04111")]
      [InlineData("051111")]
      [InlineData("0611111")]
      [InlineData("07111111")]
      [InlineData("081111111")]
      [InlineData("0911111111")]
      [InlineData("011111111111")]
      [InlineData("+44711111111")]
      [InlineData("+4471111111111")]
      [InlineData("+447111111111")]
      [InlineData("+Not a Number")]
      [InlineData("Not a Number")]
      public void False(string number)
      {
        number.IsUkLandline().Should().BeFalse();
      }

      [Theory]
      [InlineData("+441111111111")]
      [InlineData("+442111111111")]
      [InlineData("+443111111111")]
      [InlineData("+444111111111")]
      [InlineData("+445111111111")]
      [InlineData("+446111111111")]
      [InlineData("+448111111111")]
      [InlineData("+449111111111")]
      public void True(string number)
      {
        number.IsUkLandline().Should().BeTrue();
      }
    }

    public class IsMobile : PhoneExtensionsTests
    {
      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("0")]
      [InlineData("07")]
      [InlineData("071")]
      [InlineData("0711")]
      [InlineData("07111")]
      [InlineData("071111")]
      [InlineData("0711111")]
      [InlineData("07111111")]
      [InlineData("071111111")]
      [InlineData("0711111111")]
      [InlineData("071111111111")]
      [InlineData("+44711111111")]
      [InlineData("+4471111111111")]
      [InlineData("+440111111111")]
      [InlineData("+441111111111")]
      [InlineData("+442111111111")]
      [InlineData("+443111111111")]
      [InlineData("+444111111111")]
      [InlineData("+445111111111")]
      [InlineData("+446111111111")]
      [InlineData("+448111111111")]
      [InlineData("+449111111111")]
      [InlineData("+Not a Number")]
      [InlineData("Not a Number")]
      public void False(string number)
      {
        number.IsUkMobile().Should().BeFalse();
      }

      [Theory]
      [InlineData("+447011111111")]
      [InlineData("+447111111111")]
      [InlineData("+447211111111")]
      [InlineData("+447311111111")]
      [InlineData("+447411111111")]
      [InlineData("+447511111111")]
      [InlineData("+447611111111")]
      [InlineData("+447711111111")]
      [InlineData("+447811111111")]
      [InlineData("+447911111111")]
      public void True(string number)
      {
        number.IsUkMobile().Should().BeTrue();
      }

      [Theory]
      [MemberData(nameof(UkNumbers))]
      public void ValidateUkLandline(string number, bool isValid)
      {
        // Arrange.

        // Act.
        bool result = number.IsUkLandline();

        // Assert.
        result.Should().Be(isValid);
      }

    }
  }
}
