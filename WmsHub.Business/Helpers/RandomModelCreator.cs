using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Migrations;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Common.Enums;
using WmsHub.Common.Helpers;
using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Business.Helpers
{
  public static class RandomModelCreator
  {
    public static ReferralCreate CreateRandomReferralCreate(
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
      string postcode = null,
      string referralAttachmentId = "123456",
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
      Random random = new Random();

      return new ReferralCreate()
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
        DateOfBmiAtRegistration =
          dateOfBmiAtRegistration == default
              ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
              : dateOfBmiAtRegistration,
        DateOfReferral = dateOfReferral == default
              ? DateTimeOffset.Now.AddDays(-1)
              : dateOfReferral,
        DocumentVersion = documentVersion ?? Generators
          .GenerateDocumentVersion(random),
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
        ServiceId = serviceId ?? GenerateStringOfDigits(random, 7),
        Sex = sex ?? GenerateSex(random),
        SourceSystem = sourceSystem,
        Telephone = telephone ?? GenerateTelephone(random),
        Ubrn = ubrn ?? GenerateUbrn(random),
        VulnerableDescription = vulnerableDescription,
        WeightKg = weightKg
      };
    }

    public static ReferralUpdate CreateRandomReferralUpdate(
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
      DateTimeOffset? mostRecentAttachmentDate = default,
      string nhsNumber = null,
      string postcode = null,
      string referralAttachmentId = "123456",
      string referringGpPracticeName = "Test Practice",
      string referringGpPracticeNumber = null,
      string sex = null,
      string telephone = null,
      string ubrn = null,
      string vulnerableDescription = "Not Vulnerable",
      decimal weightKg = 120m,
      decimal? documentVersion = null,
      Common.Enums.SourceSystem? sourceSystem = null,
      string serviceId = "1234567")
    {
      Random random = new Random();

      return new ReferralUpdate()
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
        DateOfBmiAtRegistration =
          dateOfBmiAtRegistration == default
              ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
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
        WeightKg = weightKg,
        ServiceId = serviceId,
        DocumentVersion = documentVersion,
        SourceSystem = sourceSystem
      };
    }

    public static IReferralExceptionCreate CreateRandomReferralExceptionCreate(
      CreateReferralException exceptionType,
      DateTimeOffset? mostRecentAttachmentDate = null,
      string nhsNumberAttachment = null,
      string nhsNumberWorkList = null,
      string referralAttachmentId = null,
      string ubrn = null)
    {
      Random random = new Random();
      var referralExceptionCreate = new ReferralExceptionCreate()
      {        
        ExceptionType = exceptionType,
        MostRecentAttachmentDate = mostRecentAttachmentDate,
        ReferralAttachmentId = referralAttachmentId,
        Ubrn = ubrn ?? GenerateUbrn(random)
      };

      switch (exceptionType)
      {
        case CreateReferralException.NhsNumberMismatch:

        referralExceptionCreate.NhsNumberAttachment =
          nhsNumberAttachment ?? GenerateNhsNumber(random);

        referralExceptionCreate.NhsNumberWorkList =
          nhsNumberWorkList ?? GenerateNhsNumber(
              random,
              mustNotBeEqualTo: referralExceptionCreate.NhsNumberAttachment);
        break;
      }

      return referralExceptionCreate;
    }

    public static Practice CreateRandomPractice(
      string email = null,
      Guid id = default,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = null,
      string odsCode = null,
      string systemName = null
      )
    {
      Random random = new Random();
      return new Practice
      {
        Email = email ?? GenerateEmail(),
        Id = id == default ? default : id,
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        Name = name ?? GenerateName(random, 10),
        OdsCode = odsCode ?? GenerateGpPracticeNumber(random),
        SystemName = systemName ?? PracticeSystemName.Emis.ToString()
      };
    }

    public static Provider CreateRandomProvider(
      Guid id = default,
      bool isActive = true,
      bool level1 = false,
      bool level2 = false,
      bool level3 = false,
      string logo = null,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = null,
      ProviderAuth providerAuth = null,
      string summary = null,
      string summary2 = null,
      string summary3 = null,
      string website = null)
    {
      Random rnd = new();
      return new Provider()
      {
        Id = id == default ? default : id,
        IsActive = isActive,
        Level1 = level1,
        Level2 = level2,
        Level3 = level3,
        Logo = logo,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        Name = name ?? GenerateName(rnd, 10),
        ProviderAuth = providerAuth,
        Summary = summary,
        Summary2 = summary2,
        Summary3 = summary3,
        Website = website
      };
    }

    public static ProviderSubmission CreateRandomProviderSubmission(
      int? coaching = null,
      DateTimeOffset? date = null,
      int? measure = null,
      DateTimeOffset? submissionDate = null,
      decimal? weight = null)
    {
      Random rnd = new();
      return new ProviderSubmission
      {
        Coaching = coaching ?? rnd.Next(0, 51),
        Date = date ?? DateTimeOffset.Now,
        Measure = measure ?? rnd.Next(0, 11),
        SubmissionDate = submissionDate ?? DateTimeOffset.Now,
        Weight = weight ?? rnd.Next(90, 131),
      };
    }

    public static SelfReferralCreate CreateRandomSelfReferralCreate(
      string address1 = null,
      string address2 = null,
      string address3 = null,
      bool? consentForFutureContactForEvaluation = null,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      string email = null,
      string ethnicity = null,
      string familyName = null,
      string givenName = null,
      bool? hasDiabetesType1 = true,
      bool? hasDiabetesType2 = false,
      bool? hasHypertension = true,
      bool? hasALearningDisability = false,
      bool? hasAPhysicalDisability = false,
      bool? hasRegisteredSeriousMentalIllness = false,
      decimal heightCm = -1m,
      string mobile = null,
      string postcode = null,
      string sex = null,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null,
      string staffRole = null,
      string telephone = null,
      decimal weightKg = -1m)
    {
      Random random = new Random();
      var ethnicityGroup = GenerateEthnicityGrouping(random);

      return new SelfReferralCreate()
      {

        Address1 = address1 ?? GenerateAddress1(random),
        Address2 = address2 ?? GenerateAddress1(random),
        Address3 = address3 ?? GenerateAddress1(random),
        ConsentForFutureContactForEvaluation =
          consentForFutureContactForEvaluation ?? true,
        DateOfBirth = dateOfBirth == default
          ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
          : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
          ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
          : dateOfBmiAtRegistration,
        Email = email ?? GenerateNhsEmail(),
        Ethnicity = ethnicity ?? ethnicityGroup.Ethnicity,
        FamilyName = familyName ?? GenerateName(random, 6),
        GivenName = givenName ?? GenerateName(random, 8),
        HasDiabetesType1 = hasDiabetesType1,
        HasDiabetesType2 = hasDiabetesType2,
        HasHypertension = hasHypertension,
        HasRegisteredSeriousMentalIllness = hasRegisteredSeriousMentalIllness,
        HasALearningDisability = hasALearningDisability,
        HasAPhysicalDisability = hasAPhysicalDisability,
        HeightCm = heightCm == -1 ? random.Next(100, 200) : heightCm,
        Mobile = mobile ?? GenerateMobile(random),
        Postcode = postcode ?? GeneratePostcode(random),
        ServiceUserEthnicity = serviceUserEthnicity
          ?? ethnicityGroup.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnicityGroup.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(random),
        StaffRole = staffRole ?? GenerateName(random, 10),
        Telephone = telephone ?? GenerateTelephone(random),
        WeightKg = weightKg == -1 ? random.Next(100, 300) : weightKg,
      };
    }

    public static Pharmacy CreateRandomPharmacy(string email = null,
      Guid id = default,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = null,
      string odsCode = null,
      string version = null
    )
    {
      Random random = new Random();
      return new Pharmacy()
      {
        Email = email ?? GenerateNhsEmail(),
        Id = id == default ? default : id,
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        OdsCode = odsCode ?? GeneratePharmacyOdsCode(random),
        TemplateVersion = version ?? "1.0"
      };
    }

    public static GeneralReferralCreate CreateRandomGeneralReferralCreate(
      string nhsNumber = null,
      string address1 = null,
      string address2 = null,
      string address3 = null,
      string postcode = null,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      string email = null,
      string sex = null,
      string ethnicity = null,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null,
      string familyName = null,
      string givenName = null,
      bool? hasDiabetesType1 = null,
      bool? hasDiabetesType2 = null,
      bool? hasHypertension = null,
      bool? hasALearningDisability = null,
      bool? hasAPhysicalDisability = null,
      bool? hasArthritisOfKnee = null,
      bool? hasArthritisOfHip = null,
      bool? isPregnant = null,
      bool? hasActiveEatingDisorder = null,
      bool? hasHadBariatricSurgery = null,
      bool? consentForGpAndNhsNumberLookup = null,
      bool? consentForReferrerUpdatedWithOutcome = null,
      bool? consentForFutureContactForEvaluation = null,
      decimal heightCm = -1m,
      string mobile = null,
      string telephone = null,
      decimal weightKg = -1m,
      string referringGpPracticeNumber = null,
      string nhsLoginClaimEmail = null,
      string nhsLoginClaimFamilyName = null,
      string nhsLoginClaimGivenName = null,
      string nhsLoginClaimMobile = null)
    {
      Random random = new Random();

      var ethnicityGroup = GenerateEthnicityGrouping(random);

      return new GeneralReferralCreate()
      {
        Address1 = address1 ?? GenerateAddress1(random),
        Address2 = address2 ?? GenerateAddress1(random),
        Address3 = address3 ?? GenerateAddress1(random),
        Postcode = postcode ?? GeneratePostcode(random),
        DateOfBirth = dateOfBirth == default
          ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
          : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
          ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
          : dateOfBmiAtRegistration,
        Email = email ?? GenerateNhsEmail(),
        Sex = sex ?? GenerateSex(random),
        Ethnicity = ethnicity ?? ethnicityGroup.Ethnicity,
        ServiceUserEthnicity = serviceUserEthnicity
          ?? ethnicityGroup.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnicityGroup.ServiceUserEthnicityGroup,
        FamilyName = familyName ?? GenerateName(random, 6),
        GivenName = givenName ?? GenerateName(random, 8),
        HasDiabetesType1 = hasDiabetesType1,
        HasDiabetesType2 = hasDiabetesType2,
        HasHypertension = hasHypertension,
        HeightCm = heightCm == -1
          ? random.Next(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM + 1)
          : heightCm,
        HasALearningDisability = hasALearningDisability,
        HasAPhysicalDisability = hasAPhysicalDisability,
        Mobile = mobile ?? GenerateMobile(random),
        Telephone = telephone ?? GenerateTelephone(random),
        WeightKg = weightKg == -1
          ? random.Next(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG + 1)
          : weightKg,
        ConsentForGpAndNhsNumberLookup = consentForGpAndNhsNumberLookup ?? true,
        ConsentForReferrerUpdatedWithOutcome =
          consentForReferrerUpdatedWithOutcome ?? false,
        ConsentForFutureContactForEvaluation =
          consentForFutureContactForEvaluation,
        HasActiveEatingDisorder = hasActiveEatingDisorder,
        HasArthritisOfHip = hasArthritisOfHip,
        HasArthritisOfKnee = hasArthritisOfKnee,
        HasHadBariatricSurgery = hasHadBariatricSurgery,
        IsPregnant = isPregnant,
        NhsNumber = nhsNumber ?? GenerateNhsNumber(random),
        ReferringGpPracticeNumber = referringGpPracticeNumber ??
          GenerateGpPracticeNumber(random),
        NhsLoginClaimEmail = nhsLoginClaimEmail ?? GenerateEmail(),
        NhsLoginClaimFamilyName = nhsLoginClaimFamilyName ??
          GenerateName(random, 6),
        NhsLoginClaimGivenName = nhsLoginClaimGivenName ??
          GenerateName(random, 8),
        NhsLoginClaimMobile = nhsLoginClaimMobile ??
          GenerateMobile(random)
      };
    }
    public static GeneralReferralUpdate CreateRandomGeneralReferralUpdate(
      string nhsNumber = null,
      string address1 = null,
      string address2 = null,
      string address3 = null,
      string postcode = null,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      string email = null,
      string sex = null,
      string ethnicity = null,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null,
      string familyName = null,
      string givenName = null,
      bool? hasDiabetesType1 = null,
      bool? hasDiabetesType2 = null,
      bool? hasHypertension = null,
      bool? hasALearningDisability = null,
      bool? hasAPhysicalDisability = null,
      bool? hasArthritisOfKnee = null,
      bool? hasArthritisOfHip = null,
      bool? isPregnant = null,
      bool? hasActiveEatingDisorder = null,
      bool? hasHadBariatricSurgery = null,
      bool? consentForGpAndNhsNumberLookup = null,
      bool? consentForReferrerUpdatedWithOutcome = null,
      bool? consentForFutureContactForEvaluation = null,
      decimal heightCm = -1m,
      Guid id = default,
      string mobile = null,
      string telephone = null,
      decimal weightKg = -1m,
      string referringGpPracticeNumber = null,
      string nhsLoginClaimEmail = null,
      string nhsLoginClaimFamilyName = null,
      string nhsLoginClaimGivenName = null,
      string nhsLoginClaimMobile = null)
    {
      Random random = new Random();

      var ethnicityGroup = GenerateEthnicityGrouping(random);

      return new GeneralReferralUpdate()
      {
        Address1 = address1 ?? GenerateAddress1(random),
        Address2 = address2 ?? GenerateAddress1(random),
        Address3 = address3 ?? GenerateAddress1(random),
        Postcode = postcode ?? GeneratePostcode(random),
        DateOfBirth = dateOfBirth == default
          ? DateTimeOffset.Now.AddYears(-random.Next(18, 100))
          : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
          ? DateTimeOffset.Now.AddMonths(-random.Next(1, 12))
          : dateOfBmiAtRegistration,
        Email = email ?? GenerateNhsEmail(),
        Sex = sex ?? GenerateSex(random),
        Ethnicity = ethnicity ?? ethnicityGroup.Ethnicity,
        ServiceUserEthnicity = serviceUserEthnicity
          ?? ethnicityGroup.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnicityGroup.ServiceUserEthnicityGroup,
        FamilyName = familyName ?? GenerateName(random, 6),
        GivenName = givenName ?? GenerateName(random, 8),
        HasDiabetesType1 = hasDiabetesType1,
        HasDiabetesType2 = hasDiabetesType2,
        HasHypertension = hasHypertension,
        HeightCm = heightCm == -1
          ? random.Next(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM + 1)
          : heightCm,
        HasALearningDisability = hasALearningDisability,
        HasAPhysicalDisability = hasAPhysicalDisability,
        Mobile = mobile ?? GenerateMobile(random),
        Telephone = telephone ?? GenerateTelephone(random),
        WeightKg = weightKg == -1
          ? random.Next(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG + 1)
          : weightKg,
        ConsentForGpAndNhsNumberLookup = consentForGpAndNhsNumberLookup ?? true,
        ConsentForReferrerUpdatedWithOutcome =
          consentForReferrerUpdatedWithOutcome ?? false,
        ConsentForFutureContactForEvaluation =
          consentForFutureContactForEvaluation ?? true,
        HasActiveEatingDisorder = hasActiveEatingDisorder,
        HasArthritisOfHip = hasArthritisOfHip,
        HasArthritisOfKnee = hasArthritisOfKnee,
        HasHadBariatricSurgery = hasHadBariatricSurgery,
        IsPregnant = isPregnant,
        Id = id == default ? Guid.NewGuid() : id,
        NhsNumber = nhsNumber ?? GenerateNhsNumber(random),
        ReferringGpPracticeNumber = referringGpPracticeNumber ??
          GenerateGpPracticeNumber(random),
        NhsLoginClaimEmail = nhsLoginClaimEmail ?? GenerateEmail(),
        NhsLoginClaimFamilyName = nhsLoginClaimFamilyName ??
          GenerateName(random, 6),
        NhsLoginClaimGivenName = nhsLoginClaimGivenName ??
          GenerateName(random, 8),
        NhsLoginClaimMobile = nhsLoginClaimMobile ??
          GenerateMobile(random)
      };
    }

    public static MskReferralCreate CreateRandomMskReferralCreate(
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
      bool? hasRegisteredSeriousMentalIllness = null,
      decimal? heightCm = 190,
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
      decimal? weightKg = 150)
    {
      Random rnd = new();
      var ethnictyGrouping = GenerateEthnicityGrouping(rnd);

      return new()
      {
        Address1 = address1 ?? GenerateAddress1(rnd),
        Address2 = address2 ?? GenerateName(rnd, 10),
        Address3 = address3 ?? GenerateName(rnd, 10),
        ConsentForGpAndNhsNumberLookup =
          consentForGpAndNhsNumberLookup ?? true,
        ConsentForReferrerUpdatedWithOutcome =
          consentForReferrerUpdatedWithOutcome ?? GenerateTrueFalse(rnd),
        CreatedByUserId = createdByUserId ?? GenerateCreatedByUserId(),
        DateOfBirth = (dateOfBirth ?? GenerateDateOfBirth(rnd)).Value,
        DateOfBmiAtRegistration =
          (dateOfBmiAtRegistration ?? GenerateDateOfBmiAtRegistration(rnd))
          .Value,
        Email = email ?? GenerateEmail(),
        Ethnicity = ethnicity ?? ethnictyGrouping.Ethnicity,
        FamilyName = familyName ?? GenerateName(rnd, 6),
        GivenName = givenName ?? GenerateName(rnd, 8),
        HasActiveEatingDisorder =
          hasActiveEatingDisorder ?? GenerateNullTrueFalse(rnd),
        HasALearningDisability =
          hasALearningDisability ?? GenerateNullTrueFalse(rnd),
        HasAPhysicalDisability =
          hasAPhysicalDisability ?? GenerateNullTrueFalse(rnd),
        HasArthritisOfHip =
          hasArthritisOfHip ?? GenerateNullTrueFalse(rnd),
        HasArthritisOfKnee =
          hasArthritisOfKnee ?? GenerateNullTrueFalse(rnd),
        HasDiabetesType1 = hasDiabetesType1 ?? GenerateNullTrueFalse(rnd),
        HasDiabetesType2 = hasDiabetesType2 ?? GenerateNullTrueFalse(rnd),
        HasHadBariatricSurgery =
          hasHadBariatricSurgery ?? GenerateNullTrueFalse(rnd),
        HasHypertension = hasHypertension ?? GenerateNullTrueFalse(rnd),
        HasRegisteredSeriousMentalIllness =
          hasRegisteredSeriousMentalIllness ?? GenerateNullTrueFalse(rnd),
        HeightCm = heightCm ?? GenerateHeightCm(rnd),
        IsPregnant = isPregnant,
        Mobile = mobile ?? GenerateMobile(rnd),
        NhsNumber = nhsNumber ?? GenerateNhsNumber(rnd),
        Postcode = postcode ?? GeneratePostcode(rnd),
        ReferringGpPracticeNumber =
          referringGpPracticeNumber ?? GenerateGpPracticeNumber(rnd),
        ReferringMskClinicianEmailAddress =
          referringMskClinicianEmailAddress ?? GenerateEmail(),
        ReferringMskHubOdsCode =
          referringMskHubOdsCode ?? GenerateMskHubOdsCode(rnd),
        ServiceUserEthnicity =
          serviceUserEthnicity ?? ethnictyGrouping.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnictyGrouping.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(rnd),
        Telephone = telephone ?? GenerateTelephone(rnd),
        WeightKg = weightKg ?? GenerateWeightKg(rnd)
      };
    }

    public static PharmacyReferralCreate
      CreateRandomPharmacyReferralCreate(
        string referringGpPracticeNumber = null,
        string referringGpPracticeName = null,
        string referringPharmacyEmail = null,
        string referringPharmacyOdsCode = null,
        string nhsNumber = null,
        bool? consentForGpAndNhsNumberLookup = null,
        bool? consentForReferrerUpdatedWithOutcome = null,
        string address1 = null,
        string address2 = null,
        string address3 = null,
        DateTimeOffset dateOfBirth = default,
        DateTimeOffset dateOfBmiAtRegistration = default,
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
        string mobile = null,
        string postcode = null,
        string serviceUserEthnicity = null,
        string serviceUserEthnicityGroup = null,
        string sex = null,
        string telephone = null,
        decimal weightKg = 110m,
        decimal calculatedBmiAtRegistration = -1m,
        bool isVulnerable = false,
        bool referringPharmacyEmailIsValid = true,
        bool referringPharmacyEmailIsWhiteListed = true,
        bool ethnicityAndGroupNameValid = true,
        bool ethnicityAndServiceUserEthnicityValid = true
      )
    {
      Random rnd = new Random();
      var ethnictyGrouping = GenerateEthnicityGrouping(rnd);

      PharmacyReferralCreate pharmacyReferralCreate = new()
      {
        Address1 = address1 ?? GenerateAddress1(rnd),
        Address2 = address2 ?? GenerateName(rnd, 10),
        Address3 = address3 ?? GenerateName(rnd, 10),
        ConsentForGpAndNhsNumberLookup = consentForGpAndNhsNumberLookup ?? true,
        ConsentForReferrerUpdatedWithOutcome = consentForReferrerUpdatedWithOutcome ?? true,
        DateOfBirth = dateOfBirth == default ? DateTimeOffset.Now.AddYears(-40) : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default 
          ? DateTimeOffset.Now 
          : dateOfBmiAtRegistration,
        Email = email ?? GenerateNhsEmail(),
        Ethnicity = ethnicity ?? ethnictyGrouping.Ethnicity,
        EthnicityAndGroupNameValid = ethnicityAndGroupNameValid,
        EthnicityAndServiceUserEthnicityValid = ethnicityAndServiceUserEthnicityValid,
        FamilyName = familyName ?? GenerateName(rnd, 6),
        GivenName = givenName ?? GenerateName(rnd, 8),
        HasALearningDisability = hasALearningDisability,
        HasAPhysicalDisability = hasAPhysicalDisability,
        HasDiabetesType1 = hasDiabetesType1,
        HasDiabetesType2 = hasDiabetesType2,
        HasHypertension = hasHypertension,
        HasRegisteredSeriousMentalIllness = hasRegisteredSeriousMentalIllness,
        HeightCm = heightCm == -1 ? rnd.Next(100, 190) : heightCm,
        IsVulnerable = isVulnerable,
        Mobile = mobile ?? GenerateMobile(rnd),
        NhsNumber = nhsNumber ?? GenerateNhsNumber(rnd),
        Postcode = postcode ?? GeneratePostcode(rnd),
        ReferringGpPracticeName = referringGpPracticeName ?? "Test",
        ReferringGpPracticeNumber = referringGpPracticeNumber ?? GenerateGpPracticeNumber(rnd),
        ReferringPharmacyEmail = referringPharmacyEmail ?? GenerateNhsEmail(),
        ReferringPharmacyEmailIsValid = referringPharmacyEmailIsValid,
        ReferringPharmacyEmailIsWhiteListed = referringPharmacyEmailIsWhiteListed,
        ReferringPharmacyOdsCode = referringPharmacyOdsCode ?? GeneratePharmacyOdsCode(rnd),
        ServiceUserEthnicity = serviceUserEthnicity ?? ethnictyGrouping.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup 
          ?? ethnictyGrouping.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(rnd),
        Telephone = telephone ?? GenerateTelephone(rnd),
        WeightKg = weightKg,
      };
      
      if (calculatedBmiAtRegistration == -1m)
      {
        pharmacyReferralCreate.CalculatedBmiAtRegistration = BmiHelper.CalculateBmi(
          pharmacyReferralCreate.WeightKg,
          pharmacyReferralCreate.HeightCm);
      }
      
      return pharmacyReferralCreate;
    }

    public static BiQuestionnaire CreateBiQuestionnaire(
      string answers,
      bool? consentToShare,
      Guid? id,
      QuestionnaireType? questionnaireType,
      string ubrn)
    {
      Random random = new();

      return new BiQuestionnaire()
      {
        Answers = answers ??
          "[{\"QuestionId\":1,\"a\":\"Agree\",\"b\":\"Agree\"}]",
        ConsentToShare = consentToShare ?? false,
        Id = id ?? Guid.NewGuid(),
        QuestionnaireType =
          questionnaireType ?? QuestionnaireType.CompleteSelfT1,
        Ubrn = ubrn ?? GenerateUbrn(random)
      };
    }
  }
}
