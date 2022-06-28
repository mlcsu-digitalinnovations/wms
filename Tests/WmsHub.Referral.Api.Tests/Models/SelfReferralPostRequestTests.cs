using System;
using System.Linq;
using FluentAssertions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class SelfReferralPostRequestTests: AModelsBaseTests
  {

    private const string ETHNICITY__IRISH = "Irish";
    private const string ETHNICITY_GROUP__WHITE = "White";
    private const string STAFF_ROLE__AMBULANCE_STAFF = "Ambulance staff";
    private SelfReferralPostRequest _classToTest;

    public SelfReferralPostRequestTests()
    {
      _classToTest = CreateSelfReferralPostRequest();
    }

    [Theory]
    [InlineData("mock.test@nhs.net")]
    [InlineData("Mock.test@nhs.net")]
    [InlineData("Mock.Test@nhs.net")]
    [InlineData("Mock.Test@NHS.net")]
    [InlineData("MOCK.TEST@NHS.NET")]
    [InlineData("mock.test@nhs.net ")]
    [InlineData(" Mock.test@nhs.net")]
    [InlineData("")]
    [InlineData(null)]
    public void Email_Trimmed_And_Converted_To_Lower(string email)
    {
      // ARRANGE
      SelfReferralPostRequest selfReferralPostRequest = new();

      // ACT
      selfReferralPostRequest.Email = email;

      // ASSERT
      selfReferralPostRequest.Email.Should().Be(email?.Trim().ToLower());
    }

    [Fact]
    public void Validate_ConsentForFutureContactForEvaluation_IsNull()
    {
      // arrange
      string expected = 
        "The ConsentForFutureContactForEvaluation field is required.";
      _classToTest.ConsentForFutureContactForEvaluation = null;
      // act
      ValidateModelResult result = ValidateModel(_classToTest);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results.FirstOrDefault().ErrorMessage.Should().Be(expected);
    }

    private SelfReferralPostRequest CreateSelfReferralPostRequest()
    {
      Random random = new Random();
      return new SelfReferralPostRequest()
      {
        Address1 = "Address1",
        Address2 = "Address2",
        Address3 = "Address3",
        DateOfBirth = DateTimeOffset.Now.AddYears(-40),
        DateOfBmiAtRegistration = DateTimeOffset.Now,
        Email = Generators.GenerateNhsEmail(random),
        Ethnicity = Business.Enums.Ethnicity.White.ToString(),
        FamilyName = "FamilyName",
        GivenName = "GivenName",
        HasALearningDisability = null,
        HasAPhysicalDisability = null,
        HasDiabetesType1 = null,
        HasDiabetesType2 = null,
        HasHypertension = null,
        HeightCm = 150m,
        Mobile = "+447886123456",
        Postcode = "TF1 4NF",
        ServiceUserEthnicity = ETHNICITY__IRISH,
        ServiceUserEthnicityGroup = ETHNICITY_GROUP__WHITE,
        Sex = "Male",
        StaffRole = STAFF_ROLE__AMBULANCE_STAFF,
        Telephone = "+441743123456",
        WeightKg = 120m,
        ConsentForFutureContactForEvaluation = false
      };
    }
  }
}
