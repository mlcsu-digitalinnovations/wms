using FluentAssertions;
using System;
using WmsHub.Common.Attributes;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests.Attributes;

public class GpPracticeOdsCodeAttributeTests : AModelsBaseTests
{
  private const string ERRORMESSAGE_OVERRIDDEN = "Test error message";

  public class GpPracticeOdsCodeAttributeValidation
  {
    [GpPracticeOdsCode(allowDefaultCodes: false, ErrorMessage = ERRORMESSAGE_OVERRIDDEN)]
    public string GpPracticeOdsCode { get; set; }
  }

  [Theory]
  [InlineData("A123456", "too long")]
  [InlineData("V81997", "default code not allowed")]
  [InlineData("Z12345", "valid length but invalid first character")]
  [InlineData("ALDA23", "valid length and first three characters but invalid fourth character")]
  public void OverrideErrorMessage_OverriddenErrorMessage(string odsCode, string because)
  {
    // Arrange.
    GpPracticeOdsCodeAttributeValidation gpPracticeOdsCodeAttributeValidation = new()
    {
      GpPracticeOdsCode = odsCode
    };

    // Act.
    ValidateModelResult result = ValidateModel(gpPracticeOdsCodeAttributeValidation);

    // Assert.
    result.IsValid.Should().BeFalse(because);
    result.Results.Should().ContainSingle()
      .Which.ErrorMessage.Should().Be(ERRORMESSAGE_OVERRIDDEN);
  }

  [Theory]
  [InlineData("A123456", "too long")]
  [InlineData("A1234", "too short")]
  [InlineData("AA1234", "valid length but invalid first two characters")]
  [InlineData("AAA234", "valid length but invalid first three characters")]
  [InlineData("AAAA34", "valid length but invalid first four characters")]
  [InlineData("AAAAA4", "valid length but invalid first five characters")]
  [InlineData("AAAAAA", "valid length but invalid first six characters")]
  [InlineData("ALDA23", "valid length and first three characters but invalid fourth character")]
  [InlineData("ALD1A3", "valid length and first three characters but invalid fifth character")]
  [InlineData("ALD12A", "valid length and first three characters but invalid sixth character")]
  [InlineData("I12345", "valid length but invalid first character")]
  [InlineData("O12345", "valid length but invalid first character")]
  [InlineData("V81997", "default code not allowed")]
  [InlineData("V81998", "default code not allowed")]
  [InlineData("V81999", "default code not allowed")]
  [InlineData("X12345", "valid length but invalid first character")]
  [InlineData("Z12345", "valid length but invalid first character")]
  public void InvalidCodeAndDoNotAllowDefaultCodes_False(string odsCode, string because)
  {
    // Arrange.
    GpPracticeOdsCodeAttribute gpPracticeOdsCodeAttribute = new(allowDefaultCodes: false);

    // Act.
    bool result = gpPracticeOdsCodeAttribute.IsValid(odsCode);

    // Assert.
    result.Should().BeFalse(because);
  }

  [Fact]
  public void NotUsedWithStringType_Exception()
  {
    // Arrange.
    string expectedMessage = $"*{nameof(GpPracticeOdsCodeAttribute)}*string*";
    GpPracticeOdsCodeAttribute gpPracticeOdsCodeAttribute = new();
    int odsCode = 123;

    // Act.
    Action act = () => _ = gpPracticeOdsCodeAttribute.IsValid(odsCode);

    // Assert.
    act.Should().Throw<InvalidOperationException>()
      .Which.Message.Should().Match(expectedMessage);
  }

  [Theory]
  [InlineData(null, "null is valid")]
  [InlineData("ALD123", "valid length and first three characters")]
  [InlineData("A12345", "valid length and first character")]
  [InlineData("B12345", "valid length and first character")]
  [InlineData("C12345", "valid length and first character")]
  [InlineData("D12345", "valid length and first character")]
  [InlineData("E12345", "valid length and first character")]
  [InlineData("F12345", "valid length and first character")]
  [InlineData("G12345", "valid length and first character")]
  [InlineData("GUE123", "valid length and first three characters")]
  [InlineData("H12345", "valid length and first character")]
  [InlineData("J12345", "valid length and first character")]
  [InlineData("JER123", "valid length and first three characters")]
  [InlineData("K12345", "valid length and first character")]
  [InlineData("L12345", "valid length and first character")]
  [InlineData("M12345", "valid length and first character")]
  [InlineData("N12345", "valid length and first character")]
  [InlineData("P12345", "valid length and first character")]
  [InlineData("Q12345", "valid length and first character")]
  [InlineData("R12345", "valid length and first character")]
  [InlineData("S12345", "valid length and first character")]
  [InlineData("T12345", "valid length and first character")]
  [InlineData("U12345", "valid length and first character")]
  [InlineData("V12345", "valid length and first character")]
  [InlineData("V81997", "allow default code by default")]
  [InlineData("V81998", "allow default code by default")]
  [InlineData("V81999", "allow default code by default")]
  [InlineData("W12345", "valid length and first character")]
  [InlineData("Y12345", "valid length and first character")]
  public void ValidCodeAndAllowDefaultCodes_True(string odsCode, string because)
  {
    // Arrange.
    GpPracticeOdsCodeAttribute gpPracticeOdsCodeAttribute = new();

    // Act.
    bool result = gpPracticeOdsCodeAttribute.IsValid(odsCode);

    // Assert.
    result.Should().BeTrue(because);
  }
}