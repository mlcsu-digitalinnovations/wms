using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models;

public class ReferralCreateTests : AModelsBaseTests
{
  private readonly ReferralCreate _model;
  private readonly Random _random = new();

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
  public void DateOfBmiAtRegistrationAfterDateOfReferralIsNotValid()
  {
    // Arrange.
    _model.DateOfBmiAtRegistration = _model.DateOfReferral.Value.AddDays(1);

    string expectedMessageTemplate = $"*{nameof(_model.DateOfBmiAtRegistration)}*after*" +
      $"date of referral*";

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().Be(1);
    result.Results[0].ErrorMessage.Should().Match(expectedMessageTemplate);
  }

  [Fact]
  public void DateOfBmiAtRegistrationMoreThanTwoYearsBeforeDateOfReferralIsNotValid()
  {
    // Arrange.
    _model.DateOfBmiAtRegistration = _model.DateOfReferral.Value.AddYears(-3);

    string expectedMessageTemplate = $"*{nameof(_model.DateOfBmiAtRegistration)}*two years before*" +
      $"date of referral*";

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().Be(1);
    result.Results[0].ErrorMessage.Should().Match(expectedMessageTemplate);
  }

  [Fact]
  public void NullCriLastUpdatedWhereCriDocumentIsNotNullIsNotValid()
  {
    // Arrange.
    _model.CriDocument = "document";
    _model.CriLastUpdated = null;

    string expectedMessageTemplate =
      $"*{nameof(_model.CriLastUpdated)}*{nameof(_model.CriDocument)}*";

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().Be(1);
    result.Results[0].ErrorMessage.Should().Match(expectedMessageTemplate);
  }

  [Fact]
  public void NullDateOfBmiAtRegistrationIsNotValid()
  {
    // Arrange.
    _model.DateOfBmiAtRegistration = null;

    string expectedMessageTemplate = $"*{_model.DateOfBmiAtRegistration}*";

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().Be(1);
    result.Results[0].ErrorMessage.Should().Match(expectedMessageTemplate);
  }

  [Fact]
  public void Valid()
  {
    // Arrange.

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeTrue();
    result.Results.Should().BeEmpty();
  }

  [Theory]
  [InlineData("nk", "nk should be converted to Not Known")]
  [InlineData("not known", "not known should be converted to Not Known")]
  [InlineData("m", "m should be converted to Male")]
  [InlineData("male", "male should be converted to Male")]
  [InlineData("f", "f should be converted to Female")]
  [InlineData("female", "female should be converted to Female")]
  [InlineData("ns", "ns should be converted to Not Specified")]
  [InlineData("not specified", "not specified should be converted to Not Specified")]
  public void Valid_Sex_ConvertableValues(string sex, string because)
  {
    // Arrange.
    _model.Sex = sex;

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeTrue(because);
    result.Results.Count.Should().Be(0, because);
  }

  [Fact]
  public void Invalid_NoUbrn()
  {
    // Arrange.
    string expected = "The Ubrn field is required.";
    _model.Ubrn = null!;

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().BeGreaterThan(0);
    result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
  }

  [Fact]
  public void Invalid_NoNhsNumber()
  {
    // Arrange.
    string expected = "The NhsNumber field is required.";
    _model.NhsNumber = null!;

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().BeGreaterThan(0);
    result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
  }

  [Fact]
  public void Invalid_Sex_IsNull()
  {
    // Arrange.
    string expected = "The Sex field is required.";
    _model.Sex = null!;
    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    _model.Sex.Should().BeNull();
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().BeGreaterThan(0);
    result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
  }

  [Fact]
  public void Invalid_Sex_NoValidValue()
  {
    // Arrange.
    _model.Sex = "none";
    string expected = $"The Sex field '{_model.Sex}' is invalid.";

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().BeGreaterThan(0);
    result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
  }

  [Fact]
  public void Invalid_Ethnicity_AllLowerCase()
  {
    // Arrange.
    string expected = "The Ethnicity field 'white' is invalid.";
    _model.Ethnicity = "white"!;

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
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
    // Arrange.
    ReferralCreate referralCreate = new()
    {
      // Act.
      ReferringGpPracticeNumber = code
    };

    // Assert.
    referralCreate.ReferringGpPracticeNumber.Should().Be(expectedValue, because: because);
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
    // Arrange.
    IReferralCreate referralCreate = RandomModelCreator
      .CreateRandomReferralCreate(
        hasDiabetesType1: hasDiabetesType1,
        hasDiabetesType2: hasDiabetesType2,
        hasHypertension: hasHypertension);

    // Act.
    ValidateModelResult result = ValidateModel(referralCreate);

    // Assert.
    result.IsValid.Should().Be(isValid);
  }

  [Fact]
  public void CorrectNumberFields()
  {
    // Arrange.
    FieldNames =
    [
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
      "Mobile",
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
      "ReferralLetterDate",
      "MostRecentAttachmentId",
      "DocumentVersion",
      "SourceSystem",
      "ServiceId",
      "IsMobileValid",
      "IsTelephoneValid"
    ];

    // Act.
    PropertyInfo[] propinfo = CorrectNumberOfFields<ReferralCreate>(out string message);

    // Assert.
    propinfo.Length.Should().Be(FieldNames.Length, message);
  }

  [Fact]
  public void Required_Missing()
  {
    // Arrange.
    ReferralCreate model = new();
    List<string> requiredFields = RequiredFields(model);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Count.Should().Be(requiredFields.Count);
    foreach (ValidationResult message in result.Results)
    {
      if (message.ErrorMessage.Contains("Postcode"))
      {
        message.ErrorMessage.Should().Be("The Postcode field is not a valid postcode.");
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
    // Arrange.
    Dictionary<string, string> rangeFields = RangeFieldsMax(_model);
    foreach (KeyValuePair<string, string> field in rangeFields)
    {
      SetPropertyValue(_model, field.Key, field.Value, 1);
    }

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    foreach (ValidationResult message in result.Results)
    {
      string fieldName = message.MemberNames.ToList()[0];
      if (fieldName == "DateOfBirth")
      {
        message.ErrorMessage.Should().Be($"The field {fieldName} must equate to an age between " +
          $"{Constants.MIN_GP_REFERRAL_AGE} and {Constants.MAX_GP_REFERRAL_AGE}.");
      }
      else
      {
        message.ErrorMessage.Should().StartWith($"The field {fieldName} must be between");
      }
    }

    result.Results.Count.Should().Be(rangeFields.Count);
  }

  [Fact]
  public void Range_Outside_Min()
  {
    // Arrange.
    Dictionary<string, string> rangeFields = RangeFieldsMin(_model);
    foreach (KeyValuePair<string, string> field in rangeFields)
    {
      SetPropertyValue(_model, field.Key, field.Value, 0, 1);
    }

    // Act.
    ValidateModelResult result = ValidateModel(_model);

    // Assert.
    result.IsValid.Should().BeFalse();
    foreach (ValidationResult message in result.Results)
    {
      string fieldName = message.MemberNames.ToList()[0];
      if (fieldName == "DateOfBirth")
      {
        message.ErrorMessage.Should().Be(
          $"The field {fieldName} must equate to an age between " +
          $"{Constants.MIN_GP_REFERRAL_AGE} and " +
          $"{Constants.MAX_GP_REFERRAL_AGE}.");
      }
      else
      {
        message.ErrorMessage.Should().StartWith($"The field {fieldName} must be between");
      }
    }

    result.Results.Count.Should().Be(rangeFields.Count);
  }
}