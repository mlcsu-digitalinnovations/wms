using System;
using System.Collections.Generic;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Common.Helpers;

using static WmsHub.Common.Enums;
using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Business.Helpers
{
  public static class RandomEntityCreator
  {
    public static Call CreateRandomChatBotCall(
      string number = null,
      DateTimeOffset sent = default,
      DateTimeOffset? called = null,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string outcome = null)
    {
      Random random = new Random();
      return new Call()
      {
        Called = called,
        IsActive = isActive,
        Number = number ?? GenerateMobile(random),
        Outcome = outcome,
        Sent = sent == default
          ? DateTimeOffset.Now
          : sent,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId
      };
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
        Email = email ?? GenerateEmail(random),
        Id = id == default ? default(Guid) : id,
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

    public static EthnicityOverride CreateRandomEthnictyOverride(
      Guid ethnicityId,
      string displayName = null,
      string groupName = null,
      bool isActive = true,
      DateTimeOffset? modifiedAt = null,
      Guid? modifiedByUserId = null,
      ReferralSource referralSource = ReferralSource.Msk)
    {
      var ethnicityGrouping = GenerateEthnicityGrouping(new Random());
      return new()
      {
        DisplayName = displayName ?? $"Override:{ethnicityGrouping.Display}",
        EthnicityId = ethnicityId,
        GroupName = groupName ?? $"Override:{ethnicityGrouping.Group}",
        IsActive = isActive,
        ModifiedAt = modifiedAt ?? DateTimeOffset.Now,
        ModifiedByUserId = modifiedByUserId ?? Guid.Empty,
        ReferralSource = referralSource        
      };
    }

    public static Entities.Ethnicity CreateRandomEthnicty(
      string displayName = null,
      int displayOrder = 0,
      string groupName = null,
      int groupOrder = 0,
      bool isActive = true,
      decimal minimumBmi = 27.5m,
      DateTimeOffset? modifiedAt = null,
      Guid? modifiedByUserId = null,
      string oldName = null,
      string triageName = null)
    {
      Random rnd = new();
      var ethnicityGrouping = GenerateEthnicityGrouping(rnd);
      return new()
      {
        DisplayName = displayName ?? ethnicityGrouping.Display,
        DisplayOrder = displayOrder,
        GroupName = groupName ?? ethnicityGrouping.Group,
        GroupOrder = groupOrder,
        IsActive = isActive,
        MinimumBmi = minimumBmi,        
        ModifiedAt = modifiedAt ?? DateTimeOffset.Now,
        ModifiedByUserId = modifiedByUserId ?? Guid.Empty,
        OldName = oldName ?? GenerateName(rnd, 10),
        TriageName = triageName ?? ethnicityGrouping.Triage
      };
    }

    public static Provider CreateRandomProvider(
      string apiKey = null,
      DateTimeOffset apiKeyExpires = default,
      Guid id = default,
      bool isActive = true,
      bool isLevel1 = true,
      bool isLevel2 = true,
      bool isLevel3 = true,
      string logo = null,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = null,
      ProviderAuth providerAuth = null,
      List<ProviderSubmission> providerSubmissions = null,
      List<Referral> referrals = null,
      List<RefreshToken> refreshTokens = null,
      string summary = null,
      string summary2 = null,
      string summary3 = null,
      string website = null)
    {
      Random random = new Random();
      return new Provider()
      {
        ApiKey = apiKey ?? GenerateApiKey("Test"),
        ApiKeyExpires = apiKeyExpires == default
          ? DateTimeOffset.Now.AddYears(1).Date
          : apiKeyExpires,
        Id = id == default
          ? default(Guid)
          : id,
        IsActive = isActive,
        Level1 = isLevel1,
        Level2 = isLevel2,
        Level3 = isLevel3,
        Logo = logo ?? "test logo",
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        Name = name ?? GenerateName(random, 10),
        ProviderAuth = providerAuth,
        ProviderSubmissions = providerSubmissions,
        Referrals = referrals,
        RefreshTokens = refreshTokens,
        Summary = summary ?? GenerateName(random, 50),
        Summary2 = summary2 ?? GenerateName(random, 50),
        Summary3 = summary3 ?? GenerateName(random, 50),
        Website = website ?? GenerateUrl(random)
      };
    }

    public static ProviderSubmission CreateProviderSubmission(
      int coaching = -1,
      DateTimeOffset date = default,
      bool isActive = true,
      int measure = -1,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      Guid providerId = default,
      Guid referralId = default,
      decimal weight = -1)
    {
      Random random = new Random();
      return new ProviderSubmission()
      {
        Coaching = coaching == -1 ? random.Next(1, 101) : coaching,
        Date = date == default
          ? DateTimeOffset.Now.AddDays(random.Next(1, 10))
          : date,
        IsActive = isActive,
        Measure = measure == -1 ? random.Next(1, 101) : measure,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        ProviderId = providerId == default
          ? Guid.Empty
          : providerId,
        ReferralId = referralId == default
          ? Guid.Empty
          : referralId,
        Weight = weight == -1 ? random.Next(75, 126) : weight,
      };
    }

    public static ProviderAuth CreateRandomProviderAuth(
      string accessToken = null,
      string emailContact = null,
      Guid id = default,
      string ipWhiteList = null,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      bool isKeyViaEmail = false,
      bool isKeyViaSms = true,
      string mobileNumber = null,
      Provider provider = null,
      string smsKey = null,
      DateTimeOffset smsKeyExpiry = default)
    {
      Random random = new Random();
      return new ProviderAuth()
      {
        AccessToken = accessToken
          ?? GenerateStringOfDigits(random, 20),
        EmailContact = emailContact ?? GenerateEmail(random),
        Id = id == default
          ? default(Guid)
          : id,
        IpWhitelist = ipWhiteList ?? GenerateIpAddress(random),
        IsActive = isActive,
        KeyViaEmail = isKeyViaEmail,
        KeyViaSms = isKeyViaSms,
        MobileNumber = mobileNumber ?? GenerateMobile(random),
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        Provider = provider,
        SmsKey = smsKey ?? GenerateKey(random),
        SmsKeyExpiry = smsKeyExpiry == default
          ? DateTimeOffset.Now
          : smsKeyExpiry,
      };
    }

    public static StaffRole CreateRandomStaffRole(
      string displayName = null,
      int? displayOrder = null,
      Guid? id = null,
      bool? isActive = null,
      DateTimeOffset? modifiedAt = null,
      Guid? modifiedByUserId = null)
    {
      Random rnd = new();
      return new StaffRole
      {
        DisplayName = displayName ?? GenerateName(rnd, 10),
        DisplayOrder = displayOrder ?? 1,
        Id = id ?? Guid.NewGuid(),
        IsActive = isActive ?? true,
        ModifiedAt = modifiedAt ?? DateTimeOffset.Now,
        ModifiedByUserId = modifiedByUserId ?? Guid.Empty
      };
    }

    public static Referral CreateRandomGeneralReferral(
        string address1 = null,
        string address2 = null,
        string address3 = null,
        decimal calculatedBmiAtRegistration = -1m,
        DateTimeOffset createdDate = default,
        bool? consentForFutureContactForEvaluation = true,
        DateTimeOffset? dateCompletedProgramme = null,
        DateTimeOffset dateOfBirth = default,
        DateTimeOffset dateOfBmiAtRegistration = default,
        DateTimeOffset? dateOfProviderSelection = null,
        DateTimeOffset dateOfReferral = default,
        DateTimeOffset? dateStartedProgramme = null,
        DateTimeOffset? dateToDelayUntil = null,
        DateTimeOffset? dateOfProviderContactedServiceUser = null,
        string deprivation = null,
        string email = null,
        string ethnicity = null,
        string familyName = null,
        string givenName = null,
        bool? hasALearningDisability = null,
        bool? hasAPhysicalDisability = null,
        bool? hasDiabetesType1 = null,
        bool? hasDiabetesType2 = null,
        bool? hasHypertension = null,
        bool? hasRegisteredSeriousMentalIllness = null,
        decimal heightCm = -1m,
        Guid id = default,
        bool isActive = true,
        bool? isMobileValid = null,
        bool? isTelephoneValid = null,
        bool? isVulnerable = null,
        string mobile = null,
        DateTimeOffset modifiedAt = default,
        Guid modifiedByUserId = default,
        long? mostRecentAttachmentId = 234567,
        string nhsLoginClaimEmail = null,
        string nhsLoginClaimFamilyName = null,
        string nhsLoginClaimGivenName = null,
        string nhsLoginClaimMobile = null,
        string nhsNumber = null,
        string offeredCompletionLevel = null,
        string postcode = null,
        string programmeOutcome = null,
        Guid providerId = default,
        long? referralAttachmentId = 123456,
        ReferralSource referralSource = ReferralSource.GpReferral,
        string referringGpPracticeName = "Test Practice",
        string referringGpPracticeNumber = null,
        string sex = null,
        ReferralStatus status = ReferralStatus.New,
        string statusReason = null,
        string telephone = null,
        int traceCount = 0,
        string triagedCompletionLevel = null,
        string triagedWeightedLevel = null,
        string ubrn = null,
        string vulnerableDescription = "Not Vulnerable",
        decimal weightKg = 120m)
    {
      Random rnd = new();
      return CreateRandomReferral(
        address1: address1,
        address2: address2,
        address3: address3,
        calculatedBmiAtRegistration: calculatedBmiAtRegistration,
        createdDate: createdDate,
        consentForFutureContactForEvaluation: 
          consentForFutureContactForEvaluation,
        dateCompletedProgramme: dateCompletedProgramme,
        dateOfBirth: dateOfBirth,
        dateOfBmiAtRegistration: dateOfBmiAtRegistration,
        dateOfProviderSelection: dateOfProviderSelection,
        dateOfReferral: dateOfReferral,
        dateStartedProgramme: dateStartedProgramme,
        dateToDelayUntil: dateToDelayUntil,
        dateOfProviderContactedServiceUser: dateOfProviderContactedServiceUser,
        deprivation: deprivation,
        email: email,
        ethnicity: ethnicity,
        familyName: familyName,
        givenName: givenName,
        hasALearningDisability: hasALearningDisability,
        hasAPhysicalDisability: hasAPhysicalDisability,
        hasDiabetesType1: hasDiabetesType1,
        hasDiabetesType2: hasDiabetesType2,
        hasHypertension: hasHypertension,
        hasRegisteredSeriousMentalIllness: hasRegisteredSeriousMentalIllness,
        heightCm: heightCm,
        id: id,
        isActive: isActive,
        isMobileValid: isMobileValid,
        isTelephoneValid: isTelephoneValid,
        isVulnerable: isVulnerable,
        mobile: mobile,
        modifiedAt: modifiedAt,
        modifiedByUserId: modifiedByUserId,
        mostRecentAttachmentId: mostRecentAttachmentId,
        nhsLoginClaimEmail: nhsLoginClaimEmail ?? GenerateEmail(rnd),
        nhsLoginClaimFamilyName: nhsLoginClaimFamilyName 
          ?? GenerateName(rnd, 12),
        nhsLoginClaimGivenName: nhsLoginClaimGivenName 
          ?? GenerateName(rnd, 8),
        nhsLoginClaimMobile: nhsLoginClaimMobile 
          ?? GenerateMobile(rnd),
        nhsNumber: nhsNumber,
        offeredCompletionLevel: offeredCompletionLevel,
        postcode: postcode,
        programmeOutcome: programmeOutcome,
        providerId: providerId,
        referralAttachmentId: referralAttachmentId,
        referralSource: referralSource,
        referringGpPracticeName: referringGpPracticeName,
        referringGpPracticeNumber: referringGpPracticeNumber,
        sex: sex,
        status: status,
        statusReason: statusReason,
        telephone: telephone,
        traceCount: traceCount,
        triagedCompletionLevel: triagedCompletionLevel,
        triagedWeightedLevel: triagedWeightedLevel,
        ubrn: ubrn,
        vulnerableDescription: vulnerableDescription,
        weightKg: weightKg
      );
    }

    public static Referral CreateRandomReferral(
      string address1 = null,
      string address2 = null,
      string address3 = null,
      decimal calculatedBmiAtRegistration = -1m,
      DateTimeOffset createdDate = default,      
      bool? consentForFutureContactForEvaluation = true,
      bool? consentForGpAndNhsNumberLookup = null,
      bool? consentForReferrerUpdatedWithOutcome = null,
      DateTimeOffset? dateCompletedProgramme = null,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      DateTimeOffset? dateOfProviderSelection = null,
      DateTimeOffset dateOfReferral = default,
      DateTimeOffset? dateLetterSent = null,
      DateTimeOffset? dateStartedProgramme = null,
      DateTimeOffset? dateToDelayUntil = null,
      DateTimeOffset? dateOfProviderContactedServiceUser = null,
      string delayReason = null,
      string deprivation = null,
      decimal documentVersion = 1.0m,
      string email = null,
      string ethnicity = null,
      string familyName = null,
      decimal? firstRecordedWeight = null,
      DateTimeOffset? firstRecordedWeightDate = null,
      string givenName = null,
      bool? hasActiveEatingDisorder = null,
      bool? hasALearningDisability = null,
      bool? hasArthritisOfHip = null,
      bool? hasArthritisOfKnee = null,
      bool? hasAPhysicalDisability = null,
      bool? hasDiabetesType1 = null,
      bool? hasDiabetesType2 = null,
      bool? hasHadBariatricSurgery = null,
      bool? hasHypertension = null,
      bool? hasRegisteredSeriousMentalIllness = null,
      decimal heightCm = -1m,
      Guid id = default,
      bool isActive = true,
      bool? isMobileValid = null,
      bool? isPregnant = null,
      bool? isTelephoneValid = null,
      bool? isVulnerable = null,
      decimal? lastRecordedWeight = null,
      DateTimeOffset? lastRecordedWeightDate = null,
      DateTimeOffset? lastTraceDate = null,
      string mobile = null,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      long? mostRecentAttachmentId = 234567,
      string nhsLoginClaimEmail = null,
      string nhsLoginClaimFamilyName = null,
      string nhsLoginClaimGivenName = null,
      string nhsLoginClaimMobile = null,
      string nhsNumber = null,
      string offeredCompletionLevel = null,
      string postcode = null,
      string programmeOutcome = null,
      Guid providerId = default,
      long? referralAttachmentId = 123456,
      ReferralSource? referralSource = null,
      string referringGpPracticeName = "Test Practice",
      string referringGpPracticeNumber = null,
      string referringPharmacyEmail = null,
      string referringPharmacyODSCode = null,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null,
      string serviceId = "123456",
      string sex = null,
      SourceSystem sourceSystem = SourceSystem.Unidentified,
      string staffRole = null,
      ReferralStatus status = ReferralStatus.New,
      string statusReason = null,
      string telephone = null,
      List<TextMessage> textMessages = null,
      int traceCount = 0,
      string triagedCompletionLevel = null,
      string triagedWeightedLevel = null,
      string ubrn = null,
      string vulnerableDescription = "Not Vulnerable",
      decimal weightKg = 120m)
    {
      Random rnd = new Random();
      var ethnicityGrouping = GenerateEthnicityGrouping(rnd);

      return new Referral()
      {
        Address1 = address1 ?? GenerateAddress1(rnd),
        Address2 = address2 ?? GenerateName(rnd, 10),
        Address3 = address3 ?? GenerateName(rnd, 10),
        CalculatedBmiAtRegistration = calculatedBmiAtRegistration == -1
          ? rnd.Next(30, 90)
          : calculatedBmiAtRegistration,
        ConsentForFutureContactForEvaluation =
          consentForFutureContactForEvaluation
            ?? GenerateNullTrueFalse(rnd),
        ConsentForGpAndNhsNumberLookup = consentForGpAndNhsNumberLookup ??
          GenerateNullTrueFalse(rnd),
        ConsentForReferrerUpdatedWithOutcome =
          consentForReferrerUpdatedWithOutcome
            ?? GenerateNullTrueFalse(rnd),
        CreatedDate = createdDate == default
          ? DateTimeOffset.Now
          : createdDate,
        DateCompletedProgramme = dateCompletedProgramme,
        DateLetterSent = dateLetterSent,
        DateOfBirth = dateOfBirth == default
          ? DateTimeOffset.Now.AddYears(-rnd.Next(18, 100))
          : dateOfBirth,
        DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
          ? DateTimeOffset.Now.AddMonths(rnd.Next(1, 12))
          : dateOfBmiAtRegistration,
        DateOfProviderSelection = dateOfProviderSelection,
        DateOfReferral = dateOfReferral == default
          ? DateTimeOffset.Now.AddDays(-1)
          : dateOfReferral,
        DateStartedProgramme = dateStartedProgramme,
        DateToDelayUntil = dateToDelayUntil,
        DateOfProviderContactedServiceUser = dateOfProviderContactedServiceUser,
        DelayReason = delayReason,
        Deprivation = deprivation ?? $"IMD{rnd.Next(1, 6)}",
        DocumentVersion = documentVersion,
        Email = email ?? GenerateEmail(rnd),
        Ethnicity = ethnicity ?? ethnicityGrouping.Ethnicity,
        FamilyName = familyName ?? GenerateName(rnd, 6),
        FirstRecordedWeight = firstRecordedWeight,
        FirstRecordedWeightDate = firstRecordedWeightDate,
        GivenName = givenName ?? GenerateName(rnd, 8),
        HasActiveEatingDisorder =
          hasActiveEatingDisorder ?? GenerateNullFalse(rnd),
        HasALearningDisability = 
          hasALearningDisability ?? GenerateNullTrueFalse(rnd),
        HasAPhysicalDisability = 
          hasAPhysicalDisability ?? GenerateNullTrueFalse(rnd),
        HasArthritisOfHip =
          hasArthritisOfHip ?? GenerateNullTrueFalse(rnd),
        HasArthritisOfKnee =
          hasArthritisOfKnee ?? GenerateNullTrueFalse(rnd),
        HasDiabetesType1 =
          hasDiabetesType1 ?? GenerateNullTrueFalse(rnd),
        HasDiabetesType2 =
          hasDiabetesType2 ?? GenerateNullTrueFalse(rnd),
        HasHadBariatricSurgery =
          hasHadBariatricSurgery ?? GenerateNullFalse(rnd),
        HasHypertension =
          hasHypertension ?? GenerateNullTrueFalse(rnd),
        HasRegisteredSeriousMentalIllness =
          hasRegisteredSeriousMentalIllness ?? GenerateNullTrueFalse(rnd),
        HeightCm = heightCm == -1
          ? rnd.Next(100, 200)
          : heightCm,
        Id = id == default
          ? default(Guid)
          : id,
        IsActive = isActive,
        IsMobileValid = isMobileValid,
        IsPregnant = isPregnant ?? GenerateNullFalse(rnd),
        IsTelephoneValid = isTelephoneValid,
        IsVulnerable = isVulnerable ?? GenerateNullFalse(rnd),
        LastRecordedWeight = lastRecordedWeight,
        LastRecordedWeightDate = lastRecordedWeightDate,
        LastTraceDate = lastTraceDate ?? GeneratePastDate(rnd),        
        MethodOfContact = (int)MethodOfContact.NoContact,
        Mobile = mobile ?? GenerateMobile(rnd),
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        MostRecentAttachmentId = mostRecentAttachmentId,
        NhsLoginClaimEmail = nhsLoginClaimEmail,
        NhsLoginClaimFamilyName = nhsLoginClaimFamilyName,
        NhsLoginClaimGivenName = nhsLoginClaimGivenName,
        NhsLoginClaimMobile = nhsLoginClaimMobile,
        NhsNumber = nhsNumber ?? GenerateNhsNumber(rnd),
        NumberOfContacts = 0,
        OfferedCompletionLevel = offeredCompletionLevel,
        Postcode = postcode ?? GeneratePostcode(rnd),
        ProgrammeOutcome = programmeOutcome,
        ProviderId = providerId == default
          ? null
          : providerId,
        ReferralAttachmentId = referralAttachmentId,
        ReferralSource = $"{referralSource ?? ReferralSource.GpReferral}",
        ReferringGpPracticeName = referringGpPracticeName,
        ReferringGpPracticeNumber = referringGpPracticeNumber
          ?? GenerateGpPracticeNumber(rnd),
        ReferringOrganisationEmail = 
          referringPharmacyEmail ?? GenerateNhsEmail(rnd),
        ReferringOrganisationOdsCode =
          referringPharmacyODSCode ?? GenerateOdsCode(rnd),        
        ServiceId = serviceId,
        ServiceUserEthnicity = serviceUserEthnicity 
          ?? ethnicityGrouping.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = serviceUserEthnicityGroup
          ?? ethnicityGrouping.ServiceUserEthnicityGroup,
        Sex = sex ?? GenerateSex(rnd),
        SourceSystem = sourceSystem,
        StaffRole = staffRole ?? GenerateStaffRole(rnd),
        Status = status.ToString(),
        StatusReason = statusReason,
        Telephone = telephone ?? GenerateTelephone(rnd),
        TextMessages = textMessages,
        TraceCount = traceCount,
        TriagedCompletionLevel = triagedCompletionLevel,
        TriagedWeightedLevel = triagedWeightedLevel,
        Ubrn = ubrn ?? GenerateUbrn(rnd),
        VulnerableDescription = vulnerableDescription,
        WeightKg = weightKg,        
      };
    }

    public static TextMessage CreateRandomTextMessage(
      string number = null,
      DateTimeOffset sent = default,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string outcome = null,
      DateTimeOffset? received = null)
    {
      Random random = new Random();

      DateTimeOffset sentDate = sent == default ? DateTimeOffset.Now : sent;

      return new TextMessage()
      {
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        Received = received,
        Number = number ?? GenerateMobile(random),
        Outcome = outcome,
        Sent = sentDate,
        Base36DateSent = Base36Converter
          .ConvertDateTimeOffsetToBase36(sentDate)
      };
    }

    public static ReferralCri ReferralCri(
      DateTimeOffset? clinicalInfoLastUpdated = default,
      byte[] clinicalInfoPdfBase64 = null,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      Guid updateOfCriId = default)
    {
      Random random = new Random();

      return new ReferralCri()
      {
        ClinicalInfoLastUpdated = clinicalInfoLastUpdated == default
          ? DateTimeOffset.Now
          : clinicalInfoLastUpdated.Value,
        ClinicalInfoPdfBase64 = clinicalInfoPdfBase64,
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        UpdateOfCriId = updateOfCriId == default
          ? Guid.Empty
          : updateOfCriId
      };
    }

    public static Pharmacy CreateRandomPharmacy(string email = null,
      Guid id = default,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = null,
      string odsCode = null,
      string templateVersion = null
    )
    {
      Random random = new Random();
      return new Pharmacy()
      {
        Email = email ?? GenerateNhsEmail(random),
        Id = id == default ? default(Guid) : id,
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? Guid.Empty
          : modifiedByUserId,
        OdsCode = odsCode ?? GeneratePharmacyOdsCode(random),
        TemplateVersion = templateVersion ?? "1.0"
      };
    }
  }
}
