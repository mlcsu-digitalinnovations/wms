using FluentAssertions;
using System;
using System.Reflection;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Referral.Api.Tests.Models
{
  public class TestWrapper : AReferralPostRequest
  { }

  public class AReferralPostRequestTests : AModelsBaseTests
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
          $"'{nameof(maxLength)}' must be between 1 and {(int.MaxValue - 1)}.",
          nameof(maxLength));
      }

      data.Add(
        fieldName,
        new string('x', (maxLength + 1)), 
        AssertInvalid, 
        string.Format(MAXLENGTH, fieldName, maxLength));
    }


    public static TheoryData<string, object, Action<object, string>, string> 
      MemberSetup()
    {
      var data = new 
        TheoryData<string, object, Action<object, string>, string>();

      AddRequired<string>(data, "FamilyName");
      AddMaxLength(data, "FamilyName", 200);
      AddRequired<string>(data, "GivenName");
      AddMaxLength(data, "GivenName", 200);
      AddRequired<string>(data, "Address1");
      AddMaxLength(data, "Address1", 200);
      AddNotRequired<string>(data, "Address2");
      AddMaxLength(data, "Address2", 200);
      AddNotRequired<string>(data, "Address3");
      AddMaxLength(data, "Address3", 200);
      AddRequired<string>(data, "Postcode");
      // add Postcode RegularExpression
      AddNotRequired<string>(data, "Telephone");
      // add Telephone MaxLength, RegularExpression
      AddRequired<string>(data, "Mobile");
      // add Mobile MaxLength, RegularExpression
      AddRequired<string>(data, "Email");
      // add Email MaxLength, EmailAddress
      AddRequired<DateTimeOffset>(data, "DateOfBirth");
      // add DateOfBirth MaxSecondsAhead, AgeRange
      AddRequired<string>(data, "Sex");
      AddMaxLength(data, "Sex", 200);
      AddRequired<string>(data, "Ethnicity");
      AddMaxLength(data, "Ethnicity", 200);
      AddRequired<string>(data, "ServiceUserEthnicity");
      AddMaxLength(data, "ServiceUserEthnicity", 200);
      AddRequired<string>(data, "ServiceUserEthnicityGroup");
      AddMaxLength(data, "ServiceUserEthnicityGroup", 200);
      AddNotRequired<bool?>(data, "HasAPhysicalDisability");
      AddNotRequired<bool?>(data, "HasALearningDisability");
      AddRange(data, "HeightCm", 50, 250);
      AddRange(data, "WeightKg", 35, 500);
      AddNotRequired<bool?>(data, "HasHypertension");
      AddNotRequired<bool?>(data, "HasDiabetesType1");
      AddNotRequired<bool?>(data, "HasDiabetesType2");
      AddRequired<DateTimeOffset?>(data, "DateOfBmiAtRegistration");
      // DateOfBmiAtRegistration add MaxDaysBehind, MaxSecondsAhead

      return data;
    }

    [Theory]
    [MemberData(nameof(MemberSetup))]
    public void MemberValidation(
      string name,
      object value,
      Action<object, string> method,
      string errorMessage)
    {
      var model = GenerateValidModel();
      model.GetType().InvokeMember(
        name,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
        Type.DefaultBinder,
        model,
        new object[] { value });

      method(model, errorMessage);
    }

    [Theory]
    [MemberData(nameof(SexTheoryData))]
    public void Sex_EnumMatch_Valid(string sex)
    {
      var model = GenerateValidModel();
      model.Sex = sex;

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
    public void Ethnicity_EnumMatch_Valid(string ethnicity)
    {
      var model = GenerateValidModel();
      model.Ethnicity = ethnicity;

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
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Be(errorMessage);
    }

    private static void AssertValid(object model)
    {
      AssertValid(model, null);
    }
    private static void AssertValid(object model, string _)
    {
      // act
      var result = ValidateModel(model);

      // assert
      result.Results.ForEach(r => _testOutput.WriteLine(r.ErrorMessage));
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
      var ethnictyGrouping = GenerateEthnicityGrouping(rnd);

      return new()
      {
        Address1 = address1 ?? GenerateAddress1(rnd),
        Address2 = address2 ?? GenerateName(rnd, 10),
        Address3 = address3 ?? GenerateName(rnd, 10),
        DateOfBirth = dateOfBirth ?? GenerateDateOfBirth(rnd),
        DateOfBmiAtRegistration =
          dateOfBmiAtRegistration ?? GenerateDateOfBmiAtRegistration(rnd),
        Email = email ?? GenerateEmail(rnd),
        Ethnicity = ethnicity ?? ethnictyGrouping.Ethnicity,
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
          serviceUserEthnicity ?? ethnictyGrouping.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnictyGrouping.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(rnd),
        Telephone = telephone ?? GenerateTelephone(rnd),
        WeightKg = weightKg ?? GenerateWeightKg(rnd)
      };
    }
  }
}
