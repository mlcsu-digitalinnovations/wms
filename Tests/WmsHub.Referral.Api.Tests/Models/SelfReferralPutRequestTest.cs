using System;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class SelfReferralPutRequestTest
  {
    [Fact]
    public void Valid()
    {
      //arrange
      SelfReferralPutRequest model = new SelfReferralPutRequest();
      model.Id = Guid.NewGuid();
      model.ProviderId = Guid.NewGuid();
      //act
      ValidateModelResult result = Validators.ValidateModel(model);
      //assert
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InValid_GuidsNotSet()
    {
      //arrange
      Guid expected = Guid.Empty;
      SelfReferralPutRequest model = new SelfReferralPutRequest();
      //act
      ValidateModelResult result = Validators.ValidateModel(model);
      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(2);
      result.Results[0].ErrorMessage.Should().Be(
        $"The Id field '{expected}' is invalid.");
      result.Results[1].ErrorMessage.Should().Be(
        $"The ProviderId field '{expected}' is invalid.");
    }

    [Fact]
    public void CorrectFieldCount()
    {
      //Arrange
      var fields = new string[] {"Id", "ProviderId"};
      //Act
      PropertyInfo[] propinfo = typeof(SelfReferralPutRequest).GetProperties();

      //Assert
      propinfo.Length.Should().Be(fields.Length);
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(fields, info.Name).Should().BeGreaterThan(-1);
      }
    }
  }
}