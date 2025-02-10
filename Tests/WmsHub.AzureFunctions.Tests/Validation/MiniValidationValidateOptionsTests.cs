using AutoMapper.Execution;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MiniValidation;
using System.ComponentModel.DataAnnotations;
using WmsHub.AzureFunctions.Validation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WmsHub.AzureFunctions.Tests.Validation;

public class MiniValidationValidateOptionsTests
{
  public class Validate : MiniValidationValidateOptionsTests
  {

    [Fact]
    public void When_NameMismatch_ReturnsSkip()
    {
      // Arrange.
      MiniValidationValidateOptions<TestOptions> validator = new("ExpectedName");
      TestOptions options = new() { Name = "ValidName" };

      // Act.
      ValidateOptionsResult result = validator.Validate("DifferentName", options);

      // Assert.
      result.Should().Be(ValidateOptionsResult.Skip);
    }

    [Fact]
    public void When_OptionsIsNull_ThrowsArgumentNullException()
    {
      // Arrange.
      MiniValidationValidateOptions<TestOptions> validator = new(null);

      // Act.
      Action act = () => validator.Validate(null, null!);

      // Assert.
      act.Should().Throw<ArgumentNullException>().WithMessage("*Value cannot be null.*");
    }

    [Fact]
    public void When_ValidationPasses_ReturnsSuccess()
    {
      // Arrange.
      MiniValidationValidateOptions<TestOptions> validator = new(null);
      TestOptions options = new() { Name = "ValidName" };

      // Act.
      ValidateOptionsResult result = validator.Validate(null, options);

      // Assert.
      result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void When_ValidationFails_ReturnsFail()
    {
      // Arrange.
      MiniValidationValidateOptions<TestOptions> validator = new(null);
      // Invalid because Name is required.
      TestOptions options = new() { Name = string.Empty };

      // Act.
      ValidateOptionsResult result = validator.Validate(null, options);

      // Assert.
      result.Failed.Should().BeTrue();
      result.Failures.Should().ContainSingle()
        .Which.Should().Be("DataAnnotation validation failed for 'TestOptions' member: 'Name' " +
          "with errors: 'The Name field is required.'.");
    }
  }

  private class TestOptions
  {
    [Required]
    public string Name { get; set; } = string.Empty;
  }
}