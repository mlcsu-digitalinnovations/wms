using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Reflection;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Referral.Api.Tests.Models;

public class TestWrapper : AReferralPostRequest
{ }

public class AReferralPostRequestTests : AModelsBaseTests, IDisposable
{
  private static TestWrapper _model;

  public AReferralPostRequestTests(ITestOutputHelper testOutput)
  {
    _model = GenerateValidModel();
    _testOutput = testOutput;
  }

  private static void AddNotRequired<T>(
    TheoryData<string, object, Action<object, string>, string> data,
    string fieldName)
  {
    if (data is null)
    {
      throw new ArgumentNullException(nameof(data));
    }

    if (string.IsNullOrEmpty(fieldName))
    {
      throw new ArgumentException(
        $"'{nameof(fieldName)}' cannot be null or empty.",
        nameof(fieldName));
    }

    if (typeof(T) == typeof(string))
    {
      data.Add(fieldName, null, AssertValid, null);
      data.Add(fieldName, "", AssertValid, null);
    }
    else if (typeof(T) == typeof(bool?))
    {
      data.Add(fieldName, null, AssertValid, null);
      data.Add(fieldName, true, AssertValid, null);
      data.Add(fieldName, false, AssertValid, null);
    }
    else
    {
      throw new ArgumentException($"Unknown type {typeof(T)}");
    }

  }

  private static void AddRange(
    TheoryData<string, object, Action<object, string>, string> data,
    string fieldName,
    decimal min,
    decimal max)
  {
    if (data is null)
    {
      throw new ArgumentNullException(nameof(data));
    }

    if (string.IsNullOrEmpty(fieldName))
    {
      throw new ArgumentException(
        $"'{nameof(fieldName)}' cannot be null or empty.",
        nameof(fieldName));
    }

    if (min < (decimal.MinValue + 1) || min > (decimal.MaxValue - 1))
    {
      throw new ArgumentException(
        $"'{nameof(min)}' must be between {decimal.MinValue + 1} " +
          $"and {decimal.MaxValue - 1}.",
        nameof(min));
    }

    if (max <= min || max > (decimal.MaxValue - 1))
    {
      throw new ArgumentException(
        $"'{nameof(max)}' must be between {min} " +
          $"and {decimal.MaxValue - 1}.",
        nameof(max));
    }

    data.Add(
      fieldName,
      min - 1,
      AssertInvalid,
      string.Format(RANGE, fieldName, min, max));

    data.Add(
      fieldName,
      max + 1,
      AssertInvalid,
      string.Format(RANGE, fieldName, min, max));

  }

  private static void AddRequired<T>(
  TheoryData<string, object, Action<object, string>, string> data,
  string fieldName)
  {
    if (data is null)
    {
      throw new ArgumentNullException(nameof(data));
    }

    if (string.IsNullOrEmpty(fieldName))
    {
      throw new ArgumentException(
        $"'{nameof(fieldName)}' cannot be null or empty.",
        nameof(fieldName));
    }

    data.Add(
      fieldName,
      null,
      AssertInvalid,
      string.Format(REQUIRED, fieldName));

    if (typeof(T) == typeof(string))
    {
      data.Add(
        fieldName,
        "",
        AssertInvalid,
        string.Format(REQUIRED, fieldName));
    }
  }

  private static void AddMaxLength(
    TheoryData<string, object, Action<object, string>, string> data,
    string fieldName,
    int maxLength)
  {
    if (data is null)
    {
      throw new ArgumentNullException(nameof(data));
    }

    if (string.IsNullOrEmpty(fieldName))
    {
      throw new ArgumentException(
        $"'{nameof(fieldName)}' cannot be null or empty.",
        nameof(fieldName));
    }

    if (maxLength < 1 || maxLength > (int.MaxValue - 1))
    {
      throw new ArgumentException(
        $"'{nameof(maxLength)}' must be between 1 and {int.MaxValue - 1}.",
        nameof(maxLength));
    }

    data.Add(
      fieldName,
      new string('x', maxLength + 1),
      AssertInvalid,
      string.Format(MAXLENGTH, fieldName, maxLength));
  }

  [Theory]
  [MemberData(nameof(SexTheoryData))]
  public void Sex_EnumMatch_Valid(Business.Enums.Sex sex)
  {
    TestWrapper model = GenerateValidModel();
    model.Sex = sex.GetDescriptionAttributeValue();

    AssertValid(model);
  }

  [Fact]
  public void Sex_NoEnumMatch_Invalid()
  {
    var model = GenerateValidModel();
    model.Sex = "NotMaleOrFemale";

    AssertInvalid(model, $"The Sex field '{model.Sex}' is invalid.");
  }

  [Theory]
  [MemberData(nameof(EthnicityTheoryData))]
  public void Ethnicity_EnumMatch_Valid(Business.Enums.Ethnicity ethnicity)
  {
    TestWrapper model = GenerateValidModel();
    model.Ethnicity = ethnicity.ToString();

    AssertValid(model);
  }


  [Fact]
  public void Ethnicity_NoEnumMatch_Invalid()
  {
    var model = GenerateValidModel();
    model.Ethnicity = "NotWhiteOrBlankOrAsianOrMixedOrOther";

    AssertInvalid(
      model,
      $"The Ethnicity field '{model.Ethnicity}' is invalid.");
  }

  private static void AssertInvalid(object model, string errorMessage)
  {
    // act
    Common.Validation.ValidateModelResult result = ValidateModel(model);

    // assert
    model.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
    result.Results[0].ErrorMessage.Should().Be(errorMessage);
  }

  private static void AssertValid(object model) => AssertValid(model, null);
  private static void AssertValid(object model, string _)
  {
    // act
    var result = ValidateModel(model);

    // assert
    model.Should().NotBeNull();
    result.IsValid.Should().BeTrue();
    result.Results.Should().HaveCount(0);
  }

  private static TestWrapper GenerateValidModel(
    string address1 = null,
    string address2 = null,
    string address3 = null,
    DateTimeOffset? dateOfBirth = null,
    DateTimeOffset? dateOfBmiAtRegistration = null,
    string email = null,
    string ethnicity = null,
    string familyName = null,
    string givenName = null,
    bool? hasRegisteredSeriousMentalIllness = null,
    bool? hasALearningDisability = null,
    bool? hasAPhysicalDisability = null,
    bool? hasDiabetesType1 = null,
    bool? hasDiabetesType2 = null,
    bool? hasHypertension = null,
    decimal? heightCm = null,
    string mobile = null,
    string postcode = null,
    string serviceUserEthnicity = null,
    string serviceUserEthnicityGroup = null,
    string sex = null,
    string telephone = null,
    decimal? weightKg = null)
  {
    Random rnd = new();
    EthnicityGrouping ethnicityGrouping = GenerateEthnicityGrouping(rnd);
    if (ethnicityGrouping == null)
    {
      throw new ArgumentNullException(nameof(ethnicityGrouping));
    }

    return new()
    {
      Address1 = address1 ?? GenerateAddress1(rnd),
      Address2 = address2 ?? GenerateName(rnd, 10),
      Address3 = address3 ?? GenerateName(rnd, 10),
      DateOfBirth = dateOfBirth ?? GenerateDateOfBirth(rnd),
      DateOfBmiAtRegistration =
        dateOfBmiAtRegistration ?? GenerateDateOfBmiAtRegistration(rnd),
      Email = email ?? GenerateEmail(),
      Ethnicity = ethnicity ?? ethnicityGrouping.Ethnicity,
      FamilyName = familyName ?? GenerateName(rnd, 6),
      GivenName = givenName ?? GenerateName(rnd, 8),
      HasALearningDisability =
        hasALearningDisability ?? GenerateNullTrueFalse(rnd),
      HasAPhysicalDisability =
        hasAPhysicalDisability ?? GenerateNullTrueFalse(rnd),
      HasDiabetesType1 = hasDiabetesType1 ?? GenerateNullTrueFalse(rnd),
      HasDiabetesType2 = hasDiabetesType2 ?? GenerateNullTrueFalse(rnd),
      HasHypertension = hasHypertension ?? GenerateNullTrueFalse(rnd),
      HasRegisteredSeriousMentalIllness =
        hasRegisteredSeriousMentalIllness ?? GenerateNullTrueFalse(rnd),
      HeightCm = heightCm ?? GenerateHeightCm(rnd),
      Mobile = mobile ?? GenerateMobile(rnd),
      Postcode = postcode ?? GeneratePostcode(rnd),
      ServiceUserEthnicity =
        serviceUserEthnicity ?? ethnicityGrouping.ServiceUserEthnicity,
      ServiceUserEthnicityGroup = serviceUserEthnicityGroup
        ?? ethnicityGrouping.ServiceUserEthnicityGroup,
      Sex = sex ?? GenerateSex(rnd),
      Telephone = telephone ?? GenerateTelephone(rnd),
      WeightKg = weightKg ?? GenerateWeightKg(rnd)
    };
  }

  public void Dispose()
  {
    _model = null;
    _testOutput = null;
    GC.SuppressFinalize(this);
  }
}
