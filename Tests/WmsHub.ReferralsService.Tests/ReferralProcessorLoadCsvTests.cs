using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;
using FluentAssertions;
using Moq;
using Serilog;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Mappings;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ReferralsService.Tests
{
  public class ReferralProcessorLoadCsvTests : AModelsBaseTests
  {
    private ReferralCSVProcessor _processor;
    private readonly Mock<IReferralsDataProvider> _mockProvider =
      new Mock<IReferralsDataProvider>();
    private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();

    private Config _config;

    public ReferralProcessorLoadCsvTests()
    {
      _config = new Config
      {
        Data = new DataConfiguration()
      };

      _mockLogger.Setup(t => t.Information(It.IsAny<string>())).Verifiable();

      _processor = new ReferralCSVProcessor(_mockProvider.Object, 
        _config, _mockLogger.Object);
    }

    public List<ReferralPost> LoadCsvCreate(string document)
    {
      List<ReferralPost> result;
      var config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        PrepareHeaderForMatch = args => args.Header.ToLower(),
      };
      using (TextReader reader = new StringReader(document))
      {
        using (CsvReader csv = new CsvReader(reader, 
          CultureInfo.InvariantCulture))
        {
          csv.Context.RegisterClassMap<ReferralPostMap>();
          result = csv.GetRecords<ReferralPost>().ToList();
        }
      }

      return result;
    }

    [Fact]
    public void ValidCsvModel()
    {
      //Arrange
      ReferralPost post = CreateRandomReferralCreate();
      List<ReferralPost> posts = new List<ReferralPost> { post };
      string csv = GenerateCsv(posts);
      csv = csv.Replace("NhsNumber", "NHSNumber");
      csv = csv.Replace("Ubrn",
        "UBRN,CalculatedBmiAtRegistration,DateOfBirth," +
        "DateOfBMIAtRegistration,DateOfReferral,HasALearningDisability," +
        "HasAPhysicalDisability,HasDiabetesType1,HasDiabetesType2," +
        "HasHypertension,HasRegisteredSeriousMentalIllness,HeightCm," +
        "IsVulnerable,ReferralAttachmentId,WeightKg");
      csv = csv.Replace(post.Ubrn.ToString(),
        $"{post.Ubrn},{post.CalculatedBmiAtRegistration}," +
        $"{post.DateOfBirth:s},{post.DateOfBmiAtRegistration:s}," +
        $"{post.DateOfReferral:s},{post.HasALearningDisability}," +
        $"{post.HasAPhysicalDisability},{post.HasDiabetesType1}," +
        $"{post.HasDiabetesType2},{post.HasHypertension}," +
        $"{post.HasRegisteredSeriousMentalIllness},{post.HeightCm}," +
        $"{post.IsVulnerable},{post.ReferralAttachmentId},{post.WeightKg}");
      csv = csv.Replace("null", "")
        .Replace("True", "true")
        .Replace("False", "false");

      //Act
      List<ReferralPost> result = LoadCsvCreate(csv);
      //assert
      PropertyInfo[] properties = typeof(ReferralPost).GetProperties();
      foreach (PropertyInfo propInfo in properties)
      {
        object inputValue = propInfo.GetValue(post);
        object expected = propInfo.GetValue(result.First());
        if (!propInfo.PropertyType.FullName.Contains("DateTimeOffset")  &&
            expected != null)
        {
          inputValue.Should().Be(expected);
        }
      }
    }


    private ReferralPost CreateRandomReferralCreate(
      string address1 = null,
      string address2 = null,
      string address3 = null,
      decimal calculatedBmiAtRegistration = -1m,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      DateTimeOffset dateOfReferral = default,
      string email = null,
      string ethnicity = null,
      string familyName = null,
      string givenName = null,
      bool hasALearningDisability = false,
      bool hasAPhysicalDisability = false,
      bool hasDiabetesType1 = true,
      bool hasDiabetesType2 = false,
      bool hasHypertension = true,
      bool hasRegisteredSeriousMentalIllness = false,
      decimal heightCm = -1m,
      bool isVulnerable = false,
      string mobile = null,
      string nhsNumber = null,
      string postcode = null,
      long? referralAttachmentId = 123456,
      string referringGpPracticeName = "Test Practice",
      string referringGpPracticeNumber = null,
      string sex = null,
      string telephone = null,
      string ubrn = null,
      string vulnerableDescription = "Not Vulnerable",
      decimal weightKg = 120m)
    {
      Random random = new Random();

      return new ReferralPost
      {
        Address1 = address1 ?? Generators.GenerateAddress1(random),
        Address2 = address2 ?? Generators.GenerateName(random, 10),
        Address3 = address3 ?? Generators.GenerateName(random, 10),
        CalculatedBmiAtRegistration =
          calculatedBmiAtRegistration == -1
            ? random.Next(30, 90)
            : calculatedBmiAtRegistration,
        DateOfBirth = dateOfBirth == default
          ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
          : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
          ? DateTimeOffset.Now.AddMonths(
            random.Next(1, 12))
          : dateOfBmiAtRegistration,
        DateOfReferral = dateOfReferral == default
          ? DateTimeOffset.Now.AddDays(-1)
          : dateOfReferral,
        Email = email ?? Generators.GenerateEmail(random),
        Ethnicity = ethnicity ?? Generators.GenerateEthnicity(random),
        FamilyName = familyName ?? Generators.GenerateName(random, 6),
        GivenName = givenName ?? Generators.GenerateName(random, 8),
        HasALearningDisability = hasALearningDisability,
        HasAPhysicalDisability = hasAPhysicalDisability,
        HasDiabetesType1 = hasDiabetesType1,
        HasDiabetesType2 = hasDiabetesType2,
        HasHypertension = hasHypertension,
        HasRegisteredSeriousMentalIllness = hasRegisteredSeriousMentalIllness,
        HeightCm = heightCm == -1
          ? random.Next(100, 200)
          : heightCm,
        IsVulnerable = isVulnerable,
        Mobile = mobile ?? Generators.GenerateMobile(random),
        NhsNumber = nhsNumber ?? Generators.GenerateNhsNumber(random),
        Postcode = postcode ?? Generators.GeneratePostcode(random),
        ReferralAttachmentId = referralAttachmentId,
        ReferringGpPracticeName = referringGpPracticeName,
        ReferringGpPracticeNumber = referringGpPracticeNumber
                                    ?? Generators.GenerateGpPracticeNumber(
                                      random),
        Sex = sex ?? Generators.GenerateSex(random),
        Telephone = telephone ?? Generators.GenerateTelephone(random),
        Ubrn = ubrn ?? Generators.GenerateUbrn(random),
        VulnerableDescription = vulnerableDescription,
        WeightKg = weightKg
      };
    }
  }
}