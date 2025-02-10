using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Enums;
using WmsHub.Common.Helpers;
using static WmsHub.Common.Helpers.Generators;
using MskReferral = WmsHub.Referral.Api.Models.MskReferral;

namespace WmsHub.Referral.Api.Tests;

public static class RandomModelCreator
{
  public static ActiveReferralAndExceptionUbrn ActiveReferralAndExceptionUbrn(
    DateTimeOffset? criLastUpdated = null,
    DateTimeOffset? mostRecentAttachmentDate = null,
    string referralAttachmentId = "0694b1c3-f0be-4d56-ba1a-564f6acbb765",
    ReferralStatus referralStatus = ReferralStatus.RejectedToEreferrals,
    string serviceId = "123456",
    string ubrn = null)
  {
    ActiveReferralAndExceptionUbrn activeReferralAndExceptionUbrn = new()
    {
      CriLastUpdated = criLastUpdated,
      MostRecentAttachmentDate = mostRecentAttachmentDate,
      ReferralAttachmentId = referralAttachmentId,
      ReferralStatus = referralStatus.ToString(),
      ServiceId = serviceId,
      Ubrn = ubrn ?? GenerateUbrn()
    };

    return activeReferralAndExceptionUbrn;
  }

  public static Business.Models.Ethnicity CreateRandomEthnicity(
    string displayName = null,
    int? displayOrder = null,
    string groupName = null,
    int? groupOrder = null,
    Guid? id = null,
    bool? isActive = null,
    int? minimumBmi = null,
    DateTimeOffset? modifiedAt = null,
    Guid? modifiedByUserId = null,
    string oldName = null,
    string triageName = null,
    string nhsDataDictionary2001Code = null,
    string nhsDataDictionary2001Description = null)
  {
    Random rnd = new();
    var ethnictyGrouping = GenerateEthnicityGrouping(rnd);

    return new Business.Models.Ethnicity
    {
      DisplayName = displayName ?? ethnictyGrouping.Display,
      DisplayOrder = displayOrder ?? rnd.Next(1, 10),
      GroupName = groupName ?? ethnictyGrouping.Group,
      GroupOrder = groupOrder ?? rnd.Next(1, 10),
      Id = id ?? Guid.NewGuid(),
      IsActive = isActive ?? true,
      MinimumBmi = minimumBmi ?? rnd.Next(20, 30),
      ModifiedAt = modifiedAt ?? DateTimeOffset.Now,
      ModifiedByUserId = modifiedByUserId ?? Guid.NewGuid(),
      Census2001 = oldName ?? ethnictyGrouping.Ethnicity,
      NhsDataDictionary2001Code = nhsDataDictionary2001Code,
      NhsDataDictionary2001Description = nhsDataDictionary2001Description,
      TriageName = triageName ?? ethnictyGrouping.Triage
    };
  }

  public static Business.Models.ReferralCreate CreateRandomReferralCreate(
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
    DateTimeOffset mostRecentAttachmentDate = default,
    string nhsNumber = null,
    string postcode = null,
    string referralAttachmentId = "123456",
    string referringGpPracticeName = "Test Practice",
    string referringGpPracticeNumber = null,
    string sex = null,
    string telephone = null,
    string ubrn = null,
    string vulnerableDescription = "Not Vulnerable",
    decimal weightKg = 120m)
  {
    Random random = new();

    return new Business.Models.ReferralCreate()
    {
      Address1 = address1 ?? GenerateAddress1(random),
      Address2 = address2 ?? GenerateName(random, 10),
      Address3 = address3 ?? GenerateName(random, 10),
      CalculatedBmiAtRegistration = calculatedBmiAtRegistration == -1
        ? random.Next(30, 90)
        : calculatedBmiAtRegistration,
      DateOfBirth = dateOfBirth == default
        ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
        : dateOfBirth,
      DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
        ? DateTimeOffset.Now.AddMonths(random.Next(1, 12))
        : dateOfBmiAtRegistration,
      DateOfReferral = dateOfReferral == default
       ? DateTimeOffset.Now.AddDays(-1)
       : dateOfReferral,
      Email = email ?? GenerateEmail(),
      Ethnicity = ethnicity ?? GenerateEthnicity(random),
      FamilyName = familyName ?? GenerateName(random, 6),
      GivenName = givenName ?? GenerateName(random, 8),
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
      Mobile = mobile ?? GenerateMobile(random),
      MostRecentAttachmentDate = mostRecentAttachmentDate == default
        ? DateTimeOffset.Now
        : mostRecentAttachmentDate,
      NhsNumber = nhsNumber ?? GenerateNhsNumber(random),
      Postcode = postcode ?? GeneratePostcode(random),
      ReferralAttachmentId = referralAttachmentId,
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber
        ?? GenerateGpPracticeNumber(random),
      Sex = sex ?? GenerateSex(random),
      Telephone = telephone ?? GenerateTelephone(random),
      Ubrn = ubrn ?? GenerateUbrn(random),
      VulnerableDescription = vulnerableDescription,
      WeightKg = weightKg
    };
  }

  public static MskReferral.PostRequest CreateRandomMskReferralPostRequest(
    string address1 = null,
    string address2 = null,
    string address3 = null,
    bool? consentForGpAndNhsNumberLookup = null,
    bool? consentForReferrerUpdatedWithOutcome = null,
    string createdByUserId = null,
    DateTimeOffset? dateOfBirth = null,
    DateTimeOffset? dateOfBmiAtRegistration = null,
    string email = null,
    string ethnicity = null,
    string familyName = null,
    string givenName = null,
    bool? hasActiveEatingDisorder = false,
    bool? hasALearningDisability = null,
    bool? hasAPhysicalDisability = null,
    bool? hasArthritisOfHip = true,
    bool? hasArthritisOfKnee = true,
    bool? hasDiabetesType1 = null,
    bool? hasDiabetesType2 = null,
    bool? hasHadBariatricSurgery = false,
    bool? hasHypertension = null,
    decimal? heightCm = null,
    bool? isPregnant = null,
    string mobile = null,
    string nhsNumber = null,
    string postcode = null,
    string referringGpPracticeNumber = null,
    string referringMskClinicianEmailAddress = null,
    string referringMskHubOdsCode = null,
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

    MskReferral.PostRequest result = new()
    {
      Address1 = address1 ?? GenerateAddress1(rnd),

      Address2 = address2 ?? GenerateName(rnd, 10),

      Address3 = address3 ?? GenerateName(rnd, 10),

      ConsentForGpAndNhsNumberLookup = consentForGpAndNhsNumberLookup ?? true,

      ConsentForReferrerUpdatedWithOutcome =
        consentForReferrerUpdatedWithOutcome
        ?? GenerateTrueFalse(rnd),

      CreatedByUserId = createdByUserId ?? GenerateCreatedByUserId(),

      DateOfBirth = dateOfBirth ?? GenerateDateOfBirth(rnd),

      DateOfBmiAtRegistration = dateOfBmiAtRegistration
        ?? GenerateDateOfBmiAtRegistration(rnd),

      Email = email ?? GenerateEmail(),

      Ethnicity = ethnicity ?? ethnicityGrouping.Ethnicity,

      FamilyName = familyName ?? GenerateName(rnd, 6),

      GivenName = givenName ?? GenerateName(rnd, 8),

      HasActiveEatingDisorder = hasActiveEatingDisorder,

      HasALearningDisability = hasALearningDisability
        ?? GenerateNullTrueFalse(rnd),

      HasAPhysicalDisability = hasAPhysicalDisability
        ?? GenerateNullTrueFalse(rnd),

      HasArthritisOfHip = hasArthritisOfHip,

      HasArthritisOfKnee = hasArthritisOfKnee,

      HasDiabetesType1 = hasDiabetesType1 ?? GenerateNullTrueFalse(rnd),

      HasDiabetesType2 = hasDiabetesType2 ?? GenerateNullTrueFalse(rnd),

      HasHadBariatricSurgery = hasHadBariatricSurgery,

      HasHypertension = hasHypertension ?? GenerateNullTrueFalse(rnd),

      HeightCm = heightCm ?? GenerateHeightCm(rnd),

      IsPregnant = isPregnant,

      Mobile = mobile ?? GenerateMobile(rnd),

      NhsNumber = nhsNumber ?? GenerateNhsNumber(rnd),

      Postcode = postcode ?? GeneratePostcode(rnd),

      ReferringGpPracticeNumber = referringGpPracticeNumber
        ?? GenerateGpPracticeNumber(rnd),

      ReferringMskClinicianEmailAddress = referringMskClinicianEmailAddress
        ?? GenerateEmail(),

      ReferringMskHubOdsCode = referringMskHubOdsCode
        ?? GenerateMskHubOdsCode(rnd),

      ServiceUserEthnicity = serviceUserEthnicity
        ?? ethnicityGrouping.ServiceUserEthnicity,

      ServiceUserEthnicityGroup = serviceUserEthnicityGroup
        ?? ethnicityGrouping.ServiceUserEthnicityGroup,

      Sex = sex ?? GenerateSex(rnd),

      Telephone = telephone ?? GenerateTelephone(rnd),

      WeightKg = weightKg ?? GenerateWeightKg(rnd)
    };

    return result;
  }

  public static Api.Models.PharmacyReferralPostRequest
    CreatePharmacyReferralPostRequest(
      string referringPharmacyEmail = null,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null
    )
  {
    Random rnd = new();
    return new Api.Models.PharmacyReferralPostRequest()
    {
      ReferringGpPracticeNumber = GenerateGpPracticeNumber(rnd),
      ReferringGpPracticeName = "Test",
      ReferringPharmacyEmail = referringPharmacyEmail ?? "pharma01@nhs.net",
      ReferringPharmacyOdsCode = "FA111",
      NhsNumber = GenerateNhsNumber(rnd),
      ConsentForGpAndNhsNumberLookup = true,
      ConsentForReferrerUpdatedWithOutcome = true,
      Address1 = "Address1",
      Address2 = "Address2",
      Address3 = "Address3",
      DateOfBirth = DateTimeOffset.Now.AddYears(-40),
      DateOfBmiAtRegistration = DateTimeOffset.Now,
      Email = GenerateNhsEmail(),
      Ethnicity = Business.Enums.Ethnicity.White.ToString(),
      FamilyName = "FamilyName",
      GivenName = "GivenName",
      HasALearningDisability = null,
      HasAPhysicalDisability = null,
      HasDiabetesType1 = false,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HeightCm = 181m,
      Mobile = GenerateMobile(rnd),
      Postcode = "TF1 4NF",
      ServiceUserEthnicity = serviceUserEthnicity ?? "Irish",
      ServiceUserEthnicityGroup = serviceUserEthnicityGroup ?? "White",
      Sex = "Male",
      Telephone = GenerateTelephone(rnd),
      WeightKg = 110m,
      CalculatedBmiAtRegistration = Business.Helpers.BmiHelper
        .CalculateBmi(110m, 181m),
      IsVulnerable = false
    };
  }

  public static ReferralPost ReferralPost(
    string address1 = null,
    string address2 = null,
    string address3 = null,
    decimal calculatedBmiAtRegistration = -1m,
    string criDocument = null,
    DateTimeOffset? criLastUpdated = null,
    DateTimeOffset dateOfBirth = default,
    DateTimeOffset dateOfBmiAtRegistration = default,
    DateTimeOffset dateOfReferral = default,
    decimal? documentVersion = null,
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
    DateTimeOffset? mostRecentAttachmentDate = default,
    string mobile = null,
    string nhsNumber = null,
    string pdfParseLog = null,
    string postcode = null,
    string referralAttachmentId = "123456",
    DateTimeOffset? referralLetterDate = null,
    string referringGpPracticeName = "Test Practice",
    string referringGpPracticeNumber = null,
    string serviceId = null,
    string sex = null,
    SourceSystem sourceSystem = SourceSystem.Unidentified,
    string telephone = null,
    string ubrn = null,
    string vulnerableDescription = "Not Vulnerable",
    decimal weightKg = 120m)
  {
    Random random = new();

    ReferralPost referralPost = new()
    {
      Address1 = address1 ?? GenerateAddress1(random),
      Address2 = address2 ?? GenerateName(random, 10),
      Address3 = address3 ?? GenerateName(random, 10),
      CalculatedBmiAtRegistration = calculatedBmiAtRegistration == -1
        ? random.Next(30, 90)
        : calculatedBmiAtRegistration,
      CriDocument = criDocument,
      CriLastUpdated = criLastUpdated,
      DateOfBirth = dateOfBirth == default
        ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
        : dateOfBirth,
      DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
        ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
        : dateOfBmiAtRegistration,
      DateOfReferral = dateOfReferral == default
        ? DateTimeOffset.Now.AddDays(-1)
        : dateOfReferral,
      DocumentVersion = documentVersion ?? GenerateDocumentVersion(random),
      Email = email ?? GenerateEmail(),
      Ethnicity = ethnicity ?? GenerateEthnicity(random),
      FamilyName = familyName ?? GenerateName(random, 6),
      GivenName = givenName ?? GenerateName(random, 8),
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
      Mobile = mobile ?? GenerateMobile(random),
      MostRecentAttachmentDate = mostRecentAttachmentDate == default
        ? DateTimeOffset.Now
        : mostRecentAttachmentDate,
      NhsNumber = nhsNumber ?? GenerateNhsNumber(random),
      PdfParseLog = pdfParseLog,
      Postcode = postcode ?? GeneratePostcode(random),
      ReferralAttachmentId = referralAttachmentId,
      ReferralLetterDate = referralLetterDate,
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber ?? GenerateGpPracticeNumber(random),
      ServiceId = serviceId ?? GenerateStringOfDigits(random, 7),
      Sex = sex ?? GenerateSex(random),
      SourceSystem = sourceSystem,
      Telephone = telephone ?? GenerateTelephone(random),
      Ubrn = ubrn ?? GenerateUbrn(random),
      VulnerableDescription = vulnerableDescription,
      WeightKg = weightKg,
    };

    return referralPost;
  }

  public static ReferralPut ReferralPut(
    string address1 = null,
    string address2 = null,
    string address3 = null,
    decimal calculatedBmiAtRegistration = -1m,
    DateTimeOffset dateOfBirth = default,
    DateTimeOffset dateOfBmiAtRegistration = default,
    DateTimeOffset dateOfReferral = default,
    decimal? documentVersion = null,
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
    DateTimeOffset? mostRecentAttachmentDate = default,
    string mobile = null,
    string nhsNumber = null,
    string pdfParseLog = null,
    string postcode = null,
    string referralAttachmentId = "123456",
    DateTimeOffset? referralLetterDate = null,
    string referringGpPracticeName = "Test Practice",
    string referringGpPracticeNumber = null,
    string serviceId = null,
    string sex = null,
    SourceSystem sourceSystem = SourceSystem.Unidentified,
    string telephone = null,
    string vulnerableDescription = "Not Vulnerable",
    decimal weightKg = 120m)
  {
    Random random = new();

    ReferralPut referralPut = new()
    {

      Address1 = address1 ?? GenerateAddress1(random),
      Address2 = address2 ?? GenerateName(random, 10),
      Address3 = address3 ?? GenerateName(random, 10),
      CalculatedBmiAtRegistration = calculatedBmiAtRegistration == -1
        ? random.Next(30, 90)
        : calculatedBmiAtRegistration,
      DateOfBirth = dateOfBirth == default
        ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
        : dateOfBirth,
      DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
        ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
        : dateOfBmiAtRegistration,
      DateOfReferral = dateOfReferral == default
        ? DateTimeOffset.Now.AddDays(-1)
        : dateOfReferral,
      DocumentVersion = documentVersion ?? GenerateDocumentVersion(random),
      Email = email ?? GenerateEmail(),
      Ethnicity = ethnicity ?? GenerateEthnicity(random),
      FamilyName = familyName ?? GenerateName(random, 6),
      GivenName = givenName ?? GenerateName(random, 8),
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
      Mobile = mobile ?? GenerateMobile(random),
      MostRecentAttachmentDate = mostRecentAttachmentDate == default
        ? DateTimeOffset.Now
        : mostRecentAttachmentDate,
      NhsNumber = nhsNumber ?? GenerateNhsNumber(random),
      PdfParseLog = pdfParseLog,
      Postcode = postcode ?? GeneratePostcode(random),
      ReferralAttachmentId = referralAttachmentId,
      ReferralLetterDate = referralLetterDate,
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber ?? GenerateGpPracticeNumber(random),
      ServiceId = serviceId ?? GenerateStringOfDigits(random, 7),
      Sex = sex ?? GenerateSex(random),
      SourceSystem = sourceSystem,
      Telephone = telephone ?? GenerateTelephone(random),
      VulnerableDescription = vulnerableDescription,
      WeightKg = weightKg,
    };

    return referralPut;
  }
}