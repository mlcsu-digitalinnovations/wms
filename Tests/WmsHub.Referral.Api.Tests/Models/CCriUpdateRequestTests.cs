using System;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Models;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class CCriUpdateRequestTests : AModelsBaseTests
  {
    private readonly string[] _fields = new string[]
    {
      "CriDocument",
      "ClinicalInfoLastUpdated"
    };

    [Fact]
    public void CorrectNumberFields()
    {
      //Arrange

      //Act
      PropertyInfo[] propinfo =
        typeof(CriUpdateRequest).GetProperties();

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
      CriUpdateRequest model = new();
      string rndString = "123456789";
      DateTimeOffset rndDate = DateTimeOffset.Now;
      foreach (var field in _fields)
      {
        PropertyInfo pInfo =
          typeof(CriUpdateRequest).GetProperty(field);

        if (pInfo.PropertyType.Name == "String")
          pInfo.SetValue(model, rndString);

        if(pInfo.PropertyType.FullName.Contains("DateTimeOffset"))
          pInfo.SetValue(model, rndDate);
      }

      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_MissingValues()
    {
      //Arrange
      CriUpdateRequest model = new();
      //act
      ValidateModelResult result = ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(_fields.Length);
      foreach (var message in result.Results)
      {
        message.ErrorMessage.Should().EndWith("field is required.");
      }
    }
  }
}