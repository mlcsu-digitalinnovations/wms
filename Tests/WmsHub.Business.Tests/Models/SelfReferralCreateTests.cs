using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Equivalency;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class SelfReferralCreateTests : AModelsBaseTests
  {
    private readonly SelfReferralCreate _model;
    private Random _random = new Random();
    public SelfReferralCreateTests()
    {
      _model = RandomModelCreator.CreateRandomSelfReferralCreate(
        heightCm: 150m,
        weightKg: 120m,
        staffRole: "Ambulance staff"
      );
      _model.ConsentForFutureContactForEvaluation = true;
    }

    [Fact]
    public void Invalid_FamilyName_Required()
    {
      // arrange
      string expected = "The FamilyName field is required.";
      _model.FamilyName = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_FamilyName_Too_SHort()
    {
      // arrange
      string expected = "The FamilyName field is required.";
      _model.FamilyName = ""!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_FamilyName_Too_Long()
    {
      // arrange
      string expected =
        "The field FamilyName must be a string or array type " +
        "with a maximum length of '200'.";
      _model.FamilyName = ""!;
      for (var i = 0; i < 250; i++)
        _model.FamilyName += "a";
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_GivenName_Required()
    {
      // arrange
      string expected = "The GivenName field is required.";
      _model.GivenName = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_GivenName_Too_SHort()
    {
      // arrange
      string expected = "The GivenName field is required.";
      _model.GivenName = ""!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_GivenName_Too_Long()
    {
      // arrange
      string expected =
        "The field GivenName must be a string or array type " +
        "with a maximum length of '200'.";
      _model.GivenName = ""!;
      for (var i = 0; i < 250; i++)
        _model.GivenName += "a";
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_Sex_IsNull()
    {
      // arrange
      string expected = "The Sex field is required.";
      _model.Sex = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_Sex_NoMatchEnum()
    {
      // arrange
      string expected = "The Sex field 'none' is invalid.";
      _model.Sex = "none"!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      bool validMessage = false;
      foreach (var r in result.Results)
      {
        if (r.ErrorMessage == expected)
        {
          validMessage = true;
        }
      }

      validMessage.Should().BeTrue($"Expected: {expected}");
    }

    [Fact]
    public void Invalid_Sex_AllLowerCase()
    {
      // arrange
      string expected = "The Sex field 'male' is invalid.";
      _model.Sex = "male"!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      bool validMessage = false;
      foreach (var r in result.Results)
      {
        if (r.ErrorMessage == expected)
        {
          validMessage = true;
        }
      }

      validMessage.Should().BeTrue($"Expected: {expected}");
    }

    [Fact]
    public void Invalid_Ethnicity_IsNull()
    {
      // arrange
      string expected = "The Ethnicity field is required.";
      _model.Ethnicity = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_Ethnicity_NoMatchEnum()
    {
      // arrange
      string expected = "The Ethnicity field 'none' is invalid.";
      _model.Ethnicity = "none"!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_Ethnicity_AllLowerCase()
    {
      // arrange
      string expected = "The Ethnicity field 'white' is invalid.";
      _model.Ethnicity = "white"!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }
  }
}