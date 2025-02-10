using System;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class PharmacyPostModelTests : AModelsBaseTests
  {

    public PharmacyPostModelTests()
    {
      //arrange
      FieldNames = new string[]
      {
        "Email",
        "OdsCode",
        "TemplateVersion",
      };
    }
    [Fact]
    public void CorrectNumberFields()
    {
      
      string message = "";
      //act
      PropertyInfo[] propinfo =
        CorrectNumberOfFields<PharmacyPost>(out message);
      //Assert
      propinfo.Length.Should().Be(FieldNames.Length, message);
    }

    [Fact]
    public void Invalid_MissingValues()
    {
      //Arrange
      PharmacyPost model = new();
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(FieldNames.Length);
      foreach (var message in result.Results)
      {
        message.ErrorMessage.Should().EndWith("field is required.");
      }
    }
  }
}