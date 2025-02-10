using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using Xunit;

namespace WmsHub.Common.Tests.Attributes;

public class SexAtBirthAttributeTests
{
  public class IsValidTests : SexAtBirthAttributeTests
  {
    [Fact]
    public void InvalidSexAtBirthReturnsFalse()
    {
      // Arrange.
      string sexAtBirth = "Not Valid";
      SexAtBirthAttribute attribute = new();

      // Act.
      bool isValid = attribute.IsValid(sexAtBirth);

      // Assert.
      isValid.Should().BeFalse();
    }

    [Fact]
    public void NullSexAtBirthReturnsTrue()
    {
      // Arrange.
      SexAtBirthAttribute attribute = new();

      // Act.
      bool isValid = attribute.IsValid(null);

      // Assert.
      isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("F")]
    [InlineData("FEMALE")]
    [InlineData("M")]
    [InlineData("MALE")]
    [InlineData("NK")]
    [InlineData("NOT KNOWN")]
    [InlineData("NOT SPECIFIED")]
    [InlineData("NS")]
    public void ValidSexAtBirthReturnsTrue(string sexAtBirth)
    {
      // Arrange.
      SexAtBirthAttribute attribute = new();

      // Act.
      bool isValid = attribute.IsValid(sexAtBirth);

      // Assert.
      isValid.Should().BeTrue();
    }
  }

  public class GetValidationResultTests : SexAtBirthAttributeTests
  {
    [Fact]
    public void InvalidSexAtBirthReturnsValidationResultWithErrorMessage()
    {
      // Arrange.
      string sexAtBirth = "Not Valid";
      SexAtBirthAttribute attribute = new();
      ValidationContext validationContext = new(sexAtBirth);

      // Act.
      ValidationResult validationResult = 
        attribute.GetValidationResult(sexAtBirth, validationContext);

      // Assert.
      validationResult.ErrorMessage.Should().Be("The field String must be one of the following: " +
        "F, FEMALE, M, MALE, NK, NOT KNOWN, NOT SPECIFIED, NS");
    }

    [Fact]
    public void NonStringParameterReturnsValidationResultWithErrorMessage()
    {
      // Arrange.
      bool invalidParameter = false;
      SexAtBirthAttribute attribute = new();
      ValidationContext validationContext = new(invalidParameter);

      // Act.
      ValidationResult validationResult = 
        attribute.GetValidationResult(invalidParameter, validationContext);

      // Assert.
      validationResult.ErrorMessage.Should().Be("The field Boolean must be of type string");
    }
  }
}
