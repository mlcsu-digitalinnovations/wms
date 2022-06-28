using System;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Api.Tests
{
  public static class RandomModelCreator
  {
    public static ReferralPost CreateRandomReferralCreate(
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
