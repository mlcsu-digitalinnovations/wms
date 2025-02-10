using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class IsNhsNumberInUseRequestTests: AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      //arrange
      Random rnd = new Random();
      IsNhsNumberInUseRequest model = new IsNhsNumberInUseRequest();
      model.NhsNumber = Generators.GenerateNhsNumber(rnd);
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_NhsNumberIsNull()
    {
      //arrange
      Random rnd = new Random();
      IsNhsNumberInUseRequest model = new IsNhsNumberInUseRequest();
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.First().ErrorMessage.Should().Be(
        "The NhsNumber field is required.");
    }

    [Fact]
    public void Invalid_Not_NhsNumber()
    {
      //arrange
      Random rnd = new Random();
      IsNhsNumberInUseRequest model = new IsNhsNumberInUseRequest();
      model.NhsNumber = "1234123456";
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.First().ErrorMessage.Should().Be(
        "The field NhsNumber is invalid.");
    }

    [Fact]
    public void Invalid_Size_NhsNumber()
    {
      //arrange
      Random rnd = new Random();
      IsNhsNumberInUseRequest model = new ();
      model.NhsNumber = "Fake123456";
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.First().ErrorMessage.Should().Be(
        "The field NhsNumber must be 10 numbers only.");
    }

    [Fact]
    public void CorrectFieldCount()
    {
      //Arrange
      var fields = new string[] { "NhsNumber" };
      //Act
      PropertyInfo[] propinfo = 
        typeof(IsNhsNumberInUseRequest).GetProperties();

      //Assert
      propinfo.Length.Should().Be(fields.Length);
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(fields, info.Name).Should().BeGreaterThan(-1);
      }
    }
  }
}
