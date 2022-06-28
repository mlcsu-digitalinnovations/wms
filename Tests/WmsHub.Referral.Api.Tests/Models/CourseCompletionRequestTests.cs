using System;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class CourseCompletionRequestTests : AModelsBaseTests
  {
    private readonly string[] _fields = new string[]
    {
      "MaximumPossibleScoreCompletion",
      "MinimumPossibleScoreCompletion",
      "MinimumPossibleScoreWeight",
      "MaximumPossibleScoreWeight",
      "LowCategoryLowScoreCompletion",
      "MediumCategoryLowScoreCompletion",
      "HighCategoryLowScoreCompletion",
      "LowCategoryHighScoreCompletion",
      "MediumCategoryHighScoreCompletion",
      "HighCategoryHighScoreCompletion",
      "LowCategoryLowScoreWeight",
      "MediumCategoryLowScoreWeight",
      "HighCategoryLowScoreWeight",
      "LowCategoryHighScoreWeight",
      "MediumCategoryHighScoreWeight",
      "HighCategoryHighScoreWeight"
    };

    [Fact]
    public void CorrectNumberFields()
    {
      //Arrange

      //Act
      PropertyInfo[] propinfo =
        typeof(CourseCompletionRequest).GetProperties();

      //Assert
      propinfo.Length.Should().Be(_fields.Length);
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(_fields, info.Name).Should()
          .BeGreaterThan(-1, info.Name);
      }
    }

    [Fact]
    public void Valid()
    {
      //Arrange
      CourseCompletionRequest model = new();
      int midRangeValue = 25;
      foreach (var field in _fields)
      {
        PropertyInfo pInfo =
          typeof(CourseCompletionRequest).GetProperty(field);
        pInfo.SetValue(model, midRangeValue);
      }

      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InValid_MissingValues()
    {
      //Arrange
      CourseCompletionRequest model = new();
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(_fields.Length);
      foreach (var message in result.Results)
      {
        message.ErrorMessage.Should().Contain("must be between 1 and 50.");
      }
    }

    [Fact]
    public void InValid_OutOfRange()
    {
      //Arrange
      CourseCompletionRequest model = new();
      int midRangeValue = 51;
      foreach (var field in _fields)
      {
        PropertyInfo pInfo =
          typeof(CourseCompletionRequest).GetProperty(field);
        pInfo.SetValue(model, midRangeValue);
      }

      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(_fields.Length);
      foreach (var message in result.Results)
      {
        message.ErrorMessage.Should().Contain("must be between 1 and 50.");
      }
    }
  }
}