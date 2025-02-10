using FluentAssertions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class PharmacyModelTest : AModelsBaseTests
  {
    public class PharmacyModelA : APharmacy
    {
      [Required, PharmacyOdsCode(true)]
      [MaxLength(450)]
      public override string OdsCode { get; set; }
    }

    public class PharmacyModelB : APharmacy
    {

    }

    public PharmacyModelTest()
    {
      //arrange
      FieldNames = new string[]
      {
        "Email",
        "OdsCode",
        "TemplateVersion"
      };
    }

    [Fact]
    public void CorrectNumberFields()
    {
      string message = "";
      //act
      PropertyInfo[] propinfo =
        CorrectNumberOfFields<PharmacyModelA>(out message);
      //Assert
      propinfo.Length.Should().Be(FieldNames.Length, message);
    }

    [Fact]
    public void Invalid_MissingValues()
    {
      //Arrange
      PharmacyPut model = new();
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

    [Fact]
    public void ValidModel()
    {
      //Arrange
      PharmacyModelA model = CreateRandomPharmacyA(odsCode: "P123");

      //act
      ValidateModelResult result = ValidateModel(model);

      //Assert
      result.IsValid.Should().BeTrue();

    }

    [Fact]
    public void InValidModelOdsCodeIsHq()
    {
      //Arrange
      PharmacyModelB model = CreateRandomPharmacyB(odsCode: "P123");
      string expected = "The OdsCode field cannot include HQ's.";
      //act
      ValidateModelResult result = ValidateModel(model);

      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.First().ErrorMessage.Should().Be(expected);
    }


    public static PharmacyModelA CreateRandomPharmacyA(
      string email = null,
      string name = null,
      string odsCode = null,
      string version = null
    )
    {
      Random random = new Random();
      return new PharmacyModelA()
      {
        Email = email ?? Generators.GenerateNhsEmail(),
        OdsCode = odsCode ?? Generators.GeneratePharmacyOdsCode(random),
        TemplateVersion = version ?? "1.0"
      };
    }

    public static PharmacyModelB CreateRandomPharmacyB(
      string email = null,
      string name = null,
      string odsCode = null,
      string version = null
    )
    {
      Random random = new Random();
      return new PharmacyModelB()
      {
        Email = email ?? Generators.GenerateNhsEmail(),
        OdsCode = odsCode ?? Generators.GeneratePharmacyOdsCode(random),
        TemplateVersion = version ?? "1.0"
      };
    }
  }
}
