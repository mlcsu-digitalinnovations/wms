using FluentAssertions;
using System.Linq;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Api.Tests.Models
{  
  public abstract class ReferralPostBaseTests : AModelsBaseTests
  {
    public const string VALID_UBRN = "123456789012";
    public const string VALID_SERVICEID = "1234567";

    protected abstract ReferralPostBase CreateBaseModel(
      string ubrn = VALID_UBRN, string serviceId = VALID_SERVICEID);

    [Fact]
    public void Valid()
    {
      //arrange
      ReferralPostBase modelToTest = CreateBaseModel();
      // act
      ValidateModelResult result = ValidateModel(modelToTest);
      // assert
      result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(
      null, 
      "The Ubrn field is required.", 
      "Ubrn is null")]
    [InlineData(
      "",
      "The Ubrn field is required.",
      "Ubrn is empty string")]
    [InlineData(
      "12345678901", 
      "The field Ubrn must be a string with a minimum length of 12 and a " +
        "maximum length of 12.",
      "Ubrn is too short")]
    [InlineData(
      "1234567890123",
      "The field Ubrn must be a string with a minimum length of 12 and a " +
        "maximum length of 12.",
      "Ubrn is too long")]
    [InlineData(
      "1notnumeric2",
      "The field Ubrn is in an invalid fomat", 
      "Ubrn is not numeric")]
    public void Invalid_Ubrn(string ubrn, string expected, string because)
    {
      // arrange
      ReferralPostBase modelToTest = CreateBaseModel(ubrn: ubrn);
      // act
      ValidateModelResult result = ValidateModel(modelToTest);
      // assert
      result.IsValid.Should().BeFalse(because);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }
  }
}

