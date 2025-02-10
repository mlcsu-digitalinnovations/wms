using FluentAssertions;
using FluentAssertions.Execution;
using System;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests.Models.MskReferral;

public class PostRequestTests : AModelsBaseTests, IDisposable
{
  public PostRequestTests(ITestOutputHelper testOutput)
  {
    _testOutput = testOutput;
  }

  public void Dispose() => GC.SuppressFinalize(this);

  [Theory]
  [InlineData(true, false)]
  [InlineData(false, true)]
  [InlineData(true, true)]
  public void ValidModel_IsValid(bool hasArthritisOfHip, bool hasArthritisOfKnee)
  {
    // Arrange.     
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest(
      hasArthritisOfHip: hasArthritisOfHip,
      hasArthritisOfKnee: hasArthritisOfKnee);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeTrue();
  }

  [Fact]
  public void ConsentForGpAndNhsNumberLookup_False_NotValid()
  {
    // Arrange.
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest(
      consentForGpAndNhsNumberLookup: false);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.Results.Should().HaveCount(1);
    result.IsValid.Should().BeFalse();
  }

  [Theory]
  [MemberData(nameof(InvalidNhsNumberTheoryData))]
  public void NhsNumber_NotAnNhsNumber_NotValid(string nhsNumber)
  {
    // Arrange.
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest();
    model.NhsNumber = nhsNumber;

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
  }

  [Theory]
  [MemberData(nameof(InvalidGpPracticeOdsCodeTheoryData))]
  public void ReferringGpPracticeNumber_Invalid_NotValid(string referringGpPracticeNumber)
  {
    // Arange. 
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest();
    model.ReferringGpPracticeNumber = referringGpPracticeNumber;

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse(because: result.GetErrorMessage());
    result.Results.Should().NotBeNull().And.HaveCount(1);
  }

  [Theory]
  [MemberData(nameof(InvalidEmailAddressTheoryData))]
  public void ReferringMskClinicianEmailAddress_Invalid_NotValid(
    string referringMskClinicianEmailAddress)
  {
    // Arrange.
    PostRequest model = 
      RandomModelCreator.CreateRandomMskReferralPostRequest();
    model.ReferringMskClinicianEmailAddress =
      referringMskClinicianEmailAddress;

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }
  }

  [Fact]
  public void Arthritis_BothFalse_NotValid()
  {
    // Arrange.
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest(
      hasArthritisOfHip: false,
      hasArthritisOfKnee: false);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.Results.Should().HaveCount(1);
    result.IsValid.Should().BeFalse();
  }

  [Fact]
  public void IsPregnant_False_NotValid()
  {
    // Arrange.
    PostRequest model = RandomModelCreator
      .CreateRandomMskReferralPostRequest(isPregnant: true);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.Should().NotBeNull();
    result.Results.Should().NotBeNull().And.HaveCount(1);
    result.IsValid.Should().BeFalse();
  }

  [Fact]
  public void HasActiveEatingDisorder_True_NotValid()
  {
    // Arrange.
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest(
      hasActiveEatingDisorder: true);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.Results.Should().HaveCount(1);
    result.IsValid.Should().BeFalse();
  }

  [Fact]
  public void HasHadBariatricSurgery_True_NotValid()
  {
    // Arrange.
    PostRequest model = RandomModelCreator.CreateRandomMskReferralPostRequest(
      hasHadBariatricSurgery: true);

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.Results.Should().HaveCount(1);
    result.IsValid.Should().BeFalse();
  }
}
