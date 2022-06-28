using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ReferralCreateTests : AModelsBaseTests
  {
    private ReferralCreate _model;
    private Random _random = new Random();

    public ReferralCreateTests()
    {
      _model = RandomModelCreator.CreateRandomReferralCreate(
        dateOfReferral: DateTimeOffset.Now,
        dateOfBirth: DateTimeOffset.Now.AddYears(-25),
        sex: Generators.GenerateSex(_random),
        ethnicity: Generators.GenerateEthnicity(_random),
        heightCm: 181,
        weightKg: 110,
        calculatedBmiAtRegistration: 31m
      );
    }

    [Fact]
    public void Valid()
    {
      // arrange

      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("m", "m should be converted to Male")]
    [InlineData("male", "male should be converted to Male")]
    [InlineData("f", "f should be converted to Female")]
    [InlineData("female", "female should be converted to Female")]
    public void Valid_Sex_ConvertableValues(string sex, string because)
    {
      // arrange
      _model.Sex = sex;

      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeTrue(because);
      result.Results.Count.Should().Be(0, because);
    }

    [Fact]
    public void Invalid_NoUbrn()
    {
      // arrange
      string expected = "The Ubrn field is required.";
      _model.Ubrn = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_NoNhsNumber()
    {
      // arrange
      string expected = "The NhsNumber field is required.";
      _model.NhsNumber = null!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }


    [Fact]
    public void Invalid_NoMobileAnd_NoTelephone()
    {
      // arrange
      string expected = "One of the fields: Telephone or Mobile is required.";
      _model.Mobile = null!;
      _model.Telephone = null;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
      _model.IsMobileValid.Should().BeFalse();
      _model.IsTelephoneValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_TelephoneHasMobile_MobileIsNull()
    {
      // arrange
      string expectedMobile = Generators.GenerateMobile(_random);
      _model.Mobile = null!;
      _model.Telephone = expectedMobile;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
      _model.Mobile.Should().Be(expectedMobile);      
      _model.IsMobileValid.Should().BeNull();
      _model.Telephone.Should().BeNullOrWhiteSpace();
      _model.IsTelephoneValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_TelephoneHasMobile_MobileHasTelephone()
    {
      // arrange
      string expectedMobile = Generators.GenerateMobile(_random);
      string expectedPhone = Generators.GenerateTelephone(_random);
      _model.Mobile = expectedPhone!;
      _model.Telephone = expectedMobile;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Count.Should().Be(0);
      _model.Mobile.Should().Be(expectedMobile);
      _model.Telephone.Should().Be(expectedPhone);
      _model.IsMobileValid.Should().BeNull();
      _model.IsTelephoneValid.Should().BeNull();

    }

    [Fact]
    public void Valid_TelephoneHasMobile_MobileIsValid_Set_PhoneNull()
    {
      // arrange
      string expectedMobile = Generators.GenerateMobile(_random);
      _model.Mobile = expectedMobile!;
      _model.Telephone = expectedMobile;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      result.IsValid.Should().BeTrue();
      _model.Mobile.Should().Be(expectedMobile);      
      _model.IsMobileValid.Should().BeNull();
      _model.Telephone.Should().BeNullOrWhiteSpace();
      _model.IsTelephoneValid.Should().BeFalse();
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
      _model.Sex.Should().BeNull();
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().BeGreaterThan(0);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    [Fact]
    public void Invalid_Sex_NoValidValue()
    {
      // valid vaules are now M,male,F,female in any character case
      // arrange
      string expected = "The Sex field is required.";
      _model.Sex = "none"!;
      // act
      ValidateModelResult result = ValidateModel(_model);

      // assert
      _model.Sex.Should().BeNull();
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
      result.Results.First().ErrorMessage.Should().Be(expected);
    }

    [Theory]
    [InlineData("M12345", "M12345", "it is valid")]
    [InlineData("m12345", "M12345", "it should be converted to upper case")]
    [InlineData("M123456789", "V81999", "it is too long")]
    [InlineData("M1234", "V81999", "it is too short")]
    [InlineData("", "V81999", "it is a blank string")]
    [InlineData(null, "V81999", "it is null")]
    [InlineData("System:EMIS V1.0 H83028", "H83028",
      "it should extract H83028")]
    [InlineData("L82036 Practice Address 1: Bodriggy Health Centre", "L82036",
      "it should extract L82036")]
    [InlineData(
      "Version 1.0 | Data Quality Team | July 2021 | 111 222 4444 B86042",
      "B86042", "it should extract B86042")]
    public void ReferringGpPracticeNumber(
      string code, string expectedValue, string because)
    {
      // arrange
      ReferralCreate referralCreate = new();

      // act
      referralCreate.ReferringGpPracticeNumber = code;

      // assert
      referralCreate.ReferringGpPracticeNumber.Should()
        .Be(expectedValue, because: because);
    }


    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, false, false, false)]
    public void Required_Hypertension_Or_Diabetes(
      bool hasDiabetesType1,
      bool hasDiabetesType2,
      bool hasHypertension,
      bool isValid)
    {
      //arrange
      IReferralCreate referralCreate = RandomModelCreator
        .CreateRandomReferralCreate(
          hasDiabetesType1: hasDiabetesType1,
          hasDiabetesType2: hasDiabetesType2,
          hasHypertension: hasHypertension);

      // act
      var result = ValidateModel(referralCreate);

      //assert
      result.IsValid.Should().Be(isValid);
    }


  [Fact]
  public void CorrectNumberFields()
    {
      //arrange
      FieldNames = new string[]
      {
        "Ubrn",
        "NhsNumber",
        "DateOfReferral",
        "ReferringGpPracticeNumber",
        "FamilyName",
        "GivenName",
        "Address1",
        "Address2",
        "Address3",
        "Postcode",
        "Telephone",
        "IsTelephoneValid",
        "Mobile",
        "IsMobileValid",
        "Email",
        "DateOfBirth",
        "Sex",
        "IsVulnerable",
        "VulnerableDescription",
        "Ethnicity",
        "HasAPhysicalDisability",
        "HasALearningDisability",
        "HasRegisteredSeriousMentalIllness",
        "HasHypertension",
        "HasDiabetesType1",
        "HasDiabetesType2",
        "HeightCm",
        "WeightKg",
        "CriDocument",
        "CriLastUpdated",
        "CalculatedBmiAtRegistration",
        "DateOfBmiAtRegistration",
        "ReferringGpPracticeName",
        "ReferralAttachmentId",
        "MostRecentAttachmentId",
        "DocumentVersion",
        "SourceSystem",
        "ServiceId"
      };
      string message = "";
      //act
      PropertyInfo[] propinfo = 
        CorrectNumberOfFields<ReferralCreate>(out message);
      //Assert
      propinfo.Length.Should().Be(FieldNames.Length, message);
    }

    [Fact]
    public void Required_Missing()
    {
      //Arrange
      ReferralCreate model = new ReferralCreate();
      ValidateModelResult result = ValidateModel(model);
      var requiredFields = RequiredFields<ReferralCreate>(model);

      //assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(requiredFields.Count);
      foreach (var message in result.Results)
      {
        if (message.ErrorMessage.Contains("Postcode"))
        {
          message.ErrorMessage.Should()
            .Be("The Postcode field is not a valid postcode.");
        }
        else
        {
          message.ErrorMessage.Should().EndWith("field is required.");
        }
      }
    }

    [Fact]
    public void Range_Outside_Max()
    {
      //Arrange

      var rangeFields = RangeFieldsMax<ReferralCreate>(_model);
      foreach (var field in rangeFields)
      {
        SetPropertyValue<ReferralCreate>(_model, field.Key, field.Value, 1);
      }

      //act
      ValidateModelResult result = ValidateModel(_model);
      //assert
      result.IsValid.Should().BeFalse();
      foreach (var message in result.Results)
      {
        string fieldName = message.MemberNames.ToList()[0];
        if (fieldName == "DateOfBirth")
          message.ErrorMessage.Should().Be(
            $"The {fieldName} field must equate to an age between " +
            $"{Constants.MIN_GP_REFERRAL_AGE} and " +
            $"{Constants.MAX_GP_REFERRAL_AGE}.");
        else
          message.ErrorMessage.Should().StartWith(
            $"The field {fieldName} must be between");
      }

      result.Results.Count.Should().Be(rangeFields.Count);
    }

    [Fact]
    public void Range_Outside_Min()
    {
      //Arrange

      var rangeFields = RangeFieldsMin<ReferralCreate>(_model);
      foreach (var field in rangeFields)
      {
        SetPropertyValue<ReferralCreate>(_model, field.Key, field.Value, 0, 1);
      }

      //act
      ValidateModelResult result = ValidateModel(_model);
      //assert
      result.IsValid.Should().BeFalse();
      foreach (var message in result.Results)
      {
        string fieldName = message.MemberNames.ToList()[0];
        if (fieldName == "DateOfBirth")
          message.ErrorMessage.Should().Be(
            $"The {fieldName} field must equate to an age between " +
            $"{Constants.MIN_GP_REFERRAL_AGE} and " +
            $"{Constants.MAX_GP_REFERRAL_AGE}.");
        else
          message.ErrorMessage.Should().StartWith(
            $"The field {fieldName} must be between");
      }

      result.Results.Count.Should().Be(rangeFields.Count);
    }
  }

}