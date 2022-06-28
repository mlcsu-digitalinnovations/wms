using FluentAssertions;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests.Models.MskReferral
{
  public class PostRequestTests : AModelsBaseTests
  {
    public PostRequestTests(ITestOutputHelper testOutput)
    {
      _testOutput = testOutput;
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ValidModel_IsValid(
      bool hasArthritisOfHip,
      bool hasArthritisOfKnee)
    {
      // arrange      
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        hasArthritisOfHip: hasArthritisOfHip,
        hasArthritisOfKnee: hasArthritisOfKnee);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ConsentForGpAndNhsNumberLookup_False_NotValid()
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        consentForGpAndNhsNumberLookup: false);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidNhsNumberTheoryData))]
    public void NhsNumber_NotAnNhsNumber_NotValid(string nhsNumber)
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest();
      model.NhsNumber = nhsNumber;

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidGpPracticeOdsCodeTheoryData))]
    public void ReferringGpPracticeNumber_Invalid_NotValid(
      string referringGpPracticeNumber)
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest();
      model.ReferringGpPracticeNumber = referringGpPracticeNumber;

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidEmailAddressTheoryData))]
    public void ReferringMskClinicianEmailAddress_Invalid_NotValid(
      string referringMskClinicianEmailAddress)
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest();
      model.ReferringMskClinicianEmailAddress = 
        referringMskClinicianEmailAddress;

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Arthritis_BothFalse_NotValid()
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        hasArthritisOfHip: false,
        hasArthritisOfKnee: false);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsPregnant_False_NotValid()
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        isPregnant: true);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void HasActiveEatingDisorder_True_NotValid()
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        hasActiveEatingDisorder: true);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void HasHadBariatricSurgery_True_NotValid()
    {
      // arrange 
      var model = RandomModelCreator.CreateRandomMskReferralPostRequest(
        hasHadBariatricSurgery: true);

      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.Results.Should().HaveCount(1);
      result.IsValid.Should().BeFalse();
    }
  }
}
