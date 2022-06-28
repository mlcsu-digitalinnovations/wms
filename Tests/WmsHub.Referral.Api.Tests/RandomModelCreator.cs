using System;
using MskReferral = WmsHub.Referral.Api.Models.MskReferral;

using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Referral.Api.Tests
{
  public static class RandomModelCreator
  {
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
      string triageName = null)
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
        OldName = oldName ?? ethnictyGrouping.Ethnicity,
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
      long? mostRecentAttachmentId = 234567,
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
        Email = email ?? GenerateEmail(random),
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
        MostRecentAttachmentId = mostRecentAttachmentId,
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
        DateOfBirth = dateOfBirth ?? GenerateDateOfBirth(rnd),
        DateOfBmiAtRegistration = 
          dateOfBmiAtRegistration ?? GenerateDateOfBmiAtRegistration(rnd),
        Email = email ?? GenerateEmail(rnd),
        Ethnicity = ethnicity ?? ethnictyGrouping.Ethnicity,
        FamilyName = familyName ?? GenerateName(rnd, 6),
        GivenName = givenName ?? GenerateName(rnd, 8),
        HasActiveEatingDisorder = hasActiveEatingDisorder,
        HasALearningDisability = 
          hasALearningDisability ?? GenerateNullTrueFalse(rnd),
        HasAPhysicalDisability = 
          hasAPhysicalDisability ?? GenerateNullTrueFalse(rnd),
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
        ReferringGpPracticeNumber = 
          referringGpPracticeNumber ?? GenerateGpPracticeNumber(rnd),
        ReferringMskClinicianEmailAddress =
          referringMskClinicianEmailAddress ?? GenerateEmail(rnd),
        ReferringMskHubOdsCode =
          referringMskHubOdsCode ?? GenerateMskHubOdsCode(rnd),
        ServiceUserEthnicity = 
          serviceUserEthnicity ?? ethnictyGrouping.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup 
          ?? ethnictyGrouping.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(rnd),
        Telephone = telephone ?? GenerateTelephone(rnd),
        WeightKg = weightKg ?? GenerateWeightKg(rnd),
      };
    }
  }
}