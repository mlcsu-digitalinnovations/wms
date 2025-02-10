using FluentAssertions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models;
public class PharmacyReferralCreateTests : AModelsBaseTests
{
  public class ValidateTests : PharmacyReferralCreateTests
  {
    [Fact]
    public void InvalidBmiCalculationReturnsInvalidValidationResult()
    {
      // Arrange.
      int height = 180;
      int weight = 150;
      decimal expectedBmi = 46.3m;

      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          calculatedBmiAtRegistration: 30m,
          heightCm: height,
          weightKg: weight);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("CalculatedBmiAtRegistration");
      result.ErrorMessage.Should().Match($"*{expectedBmi}*");
    }

    [Fact]
    public void InvalidEthnicityReturnsInvalidValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(ethnicity: "Invalid ethnicity");

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("Ethnicity");
    }

    [Fact]
    public void InvalidSexReturnsInvalidValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(sex: "Invalid sex");

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("Sex");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void MismatchedEthnicityReturnsInvalidValidationResult(
      bool ethnicityAndGroupNameValid,
      bool ethnicityAndServiceUserEthnicityValid)
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          ethnicityAndGroupNameValid: ethnicityAndGroupNameValid,
          ethnicityAndServiceUserEthnicityValid: ethnicityAndServiceUserEthnicityValid);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("Ethnicity");
    }

    [Fact]
    public void MobileAndTelephoneBothUkMobileSetsTelephoneToEmptyString()
    {
      // Arrange.
      string originalTelephone = Generators.GenerateMobile(new System.Random());
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          telephone: originalTelephone);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      validationResults.Should().BeEmpty();
      pharmacyReferralCreateModel.Telephone.Should().Be(string.Empty);
    }

    [Fact]
    public void NhsNumberIsInUseIsTrueReturnsValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate();
      pharmacyReferralCreateModel.NhsNumberIsInUse = true;

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<ValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("NhsNumber");
    }

    [Fact]
    public void NoDiabetesOrHypertensionReturnsInvalidValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          hasDiabetesType1: false,
          hasDiabetesType2: false,
          hasHypertension: false);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
    }

    [Fact]
    public void NullConsentForGpAndNhsNumberLookupReturnsInvalidValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate();
      pharmacyReferralCreateModel.ConsentForGpAndNhsNumberLookup = null;

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("ConsentForGpAndNhsNumberLookup");
    }

    [Fact]
    public void NullEthnicityReturnsRequiredValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate();
      pharmacyReferralCreateModel.Ethnicity = null;

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<RequiredValidationResult>();
    }
    [Fact]
    public void NullMobileAndUkMobileTelephoneMovesTelephoneToMobile()
    {
      // Arrange.
      string originalTelephone = Generators.GenerateMobile(new System.Random());
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          telephone: originalTelephone);
      pharmacyReferralCreateModel.Mobile = null;


      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      validationResults.Should().BeEmpty();
      pharmacyReferralCreateModel.Mobile.Should().Be(originalTelephone);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", " ")]
    public void NullOrWhiteSpacePhoneNumbersReturnsInvalidValidationResult(
      string mobile,
      string telephone)
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate();
      pharmacyReferralCreateModel.Mobile = mobile;
      pharmacyReferralCreateModel.Telephone = telephone;

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("Telephone, Mobile");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void NullOrWhiteSpaceVulnerableDescriptionReturnsInvalidValidationResult(
      string vulnerableDescription)
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(isVulnerable: true);
      pharmacyReferralCreateModel.VulnerableDescription = vulnerableDescription;

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<InvalidValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("VulnerableDescription");
    }
    [Fact]
    public void ReferringPharmacyEmailIsValidIsFalseReturnsValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          referringPharmacyEmailIsValid: false);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<ValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("ReferringPharmacyEmailIsValid");
    }

    [Fact]
    public void ReferringPharmacyEmailIsWhiteListedIsFalseReturnsValidationResult()
    {
      // Arrange.
      PharmacyReferralCreate pharmacyReferralCreateModel =
        RandomModelCreator.CreateRandomPharmacyReferralCreate(
          referringPharmacyEmailIsWhiteListed: false);

      ValidationContext validationContext = new(pharmacyReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResults =
        pharmacyReferralCreateModel.Validate(validationContext);

      // Assert.
      ValidationResult result = validationResults.Should()
        .ContainSingle()
        .Which
        .Should()
        .BeOfType<ValidationResult>()
        .Subject;
      result.MemberNames.Should().Contain("ReferringPharmacyEmail");
    }
  }
}