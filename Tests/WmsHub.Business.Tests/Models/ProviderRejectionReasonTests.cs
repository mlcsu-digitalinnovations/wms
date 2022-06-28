using FluentAssertions;
using System;
using System.Linq;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ProviderRejectionReasonTests : AModelsBaseTests
  {
    private readonly Business.Models.ProviderRejectionReason _classToTest;

    public ProviderRejectionReasonTests()
    {
      _classToTest = new Business.Models.ProviderRejectionReason()
      {
        Id = Guid.NewGuid(),
        ModifiedAt = DateTimeOffset.Now,
        IsActive = true,
        Description = "This is a test description",
        Title = "TestDescription",
        ModifiedByUserId = Guid.Empty
      };
    }
    [Fact]
    public void ValidModel()
    {
      //Arrange

      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeTrue();
      result.Results.Count.Should().Be(0);
    }

    [Fact]
    public void InValid_TitleWithSpecialChars()
    {
      //Arrange
      string expected =
        "The Title field must only contains " +
        "AlphaNumeric characters and no spaces";
      _classToTest.Title = "Test%Title";
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }

    [Fact]
    public void InValid_TitleWithSpaces()
    {
      //Arrange
      string expected =
        "The Title field must only contains " +
        "AlphaNumeric characters and no spaces";
      _classToTest.Title = "Test % Title";
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }

    [Fact]
    public void InValid_TitleRequired()
    {
      //Arrange
      string expected = "The Title field is required.";
      _classToTest.Title = string.Empty;
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }

    [Fact]
    public void InValid_DescriptionRequired()
    {
      //Arrange
      string expected = "The Description field is required.";
      _classToTest.Description = string.Empty;
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }

    [Fact]
    public void InValid_Title_GreaterThan100()
    {
      //Arrange
      string expected = "The field Title must be a string or array type with" +
                        " a maximum length of '100'.";
      string title = "";
      Random rnd = new Random();
      while (title.Length < 120)
      {
        title += (char)rnd.Next('a', 'z');
      }
      _classToTest.Title = title;
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }

    [Fact]
    public void InValid_Description_GreaterThan500()
    {
      //Arrange
      string expected =
        "The field Description must be a string or array type with a " +
        "maximum length of '500'.";
      string description = "";
      Random rnd = new Random();
      while (description.Length < 510)
      {
        description += (char)rnd.Next('a', 'z');
      }
      _classToTest.Description = description;
      //Act
      ValidateModelResult result = ValidateModel(_classToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }
  }
}
