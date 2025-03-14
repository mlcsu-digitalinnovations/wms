﻿using System;
using System.Collections.Generic;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Services;
using WmsHub.Common.Enums;
using WmsHub.Common.Helpers;
using static WmsHub.Common.Helpers.Constants;
using static WmsHub.Common.Helpers.Generators;

namespace WmsHub.Business.Helpers;

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
    Random random = new();
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
    string systemName = null)
  {
    Random random = new();
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

  public static EthnicityOverride CreateRandomEthnicityOverride(
    Guid ethnicityId,
    string displayName = null,
    string groupName = null,
    bool isActive = true,
    DateTimeOffset? modifiedAt = null,
    Guid? modifiedByUserId = null,
    ReferralSource referralSource = ReferralSource.Msk)
  {
    EthnicityGrouping ethnicityGrouping = 
      GenerateEthnicityGrouping(new Random());
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

  public static Entities.Ethnicity CreateRandomEthnicity(
    Guid? id = null,
    string displayName = null,
    int displayOrder = 0,
    string groupName = null,
    int groupOrder = 0,
    bool isActive = true,
    decimal minimumBmi = 27.5m,
    DateTimeOffset? modifiedAt = null,
    Guid? modifiedByUserId = null,
    string census2001 = null,
    string nhsDataDictionary2001Code = null,
    string nhsDataDictionary2001Description = null,
    string triageName = null)
  {
    Random rnd = new();
    EthnicityGrouping ethnicityGrouping = GenerateEthnicityGrouping(rnd);
    return new()
    {
      Id = id ?? Guid.NewGuid(),
      DisplayName = displayName ?? ethnicityGrouping.Display,
      DisplayOrder = displayOrder,
      GroupName = groupName ?? ethnicityGrouping.Group,
      GroupOrder = groupOrder,
      IsActive = isActive,
      MinimumBmi = minimumBmi,
      ModifiedAt = modifiedAt ?? DateTimeOffset.Now,
      ModifiedByUserId = modifiedByUserId ?? Guid.Empty,
      Census2001 = census2001 ?? GenerateName(rnd, 10),
      NhsDataDictionary2001Code =
        nhsDataDictionary2001Code ?? GenerateName(rnd, 10),
      NhsDataDictionary2001Description =
        nhsDataDictionary2001Description ?? GenerateName(rnd, 10),
      TriageName = triageName ?? ethnicityGrouping.Triage
    };
  }

  public static Provider CreateRandomProvider(
    string apiKey = null,
    DateTimeOffset apiKeyExpires = default,
    List<ProviderDetail> details = null,
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
    Random random = new();
    return new Provider()
    {
      ApiKey = apiKey ?? GenerateApiKey("Test"),
      ApiKeyExpires = apiKeyExpires == default ? DateTimeOffset.Now.AddYears(1).Date : apiKeyExpires,
      Details = details,
      Id = id == default ? default : id,
      IsActive = isActive,
      Level1 = isLevel1,
      Level2 = isLevel2,
      Level3 = isLevel3,
      Logo = logo ?? "test logo",
      ModifiedAt = modifiedAt == default ? DateTimeOffset.Now : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default ? Guid.Empty : modifiedByUserId,
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
    Random random = new();
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
    Random random = new();
    return new ProviderAuth()
    {
      AccessToken = accessToken
        ?? GenerateStringOfDigits(random, 20),
      EmailContact = emailContact ?? GenerateEmail(),
      Id = id == default
        ? default
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
      DateTimeOffset mostRecentAttachmentDate = default,
      string nhsLoginClaimEmail = null,
      string nhsLoginClaimFamilyName = null,
      string nhsLoginClaimGivenName = null,
      string nhsLoginClaimMobile = null,
      string nhsNumber = null,
      string offeredCompletionLevel = null,
      string postcode = null,
      string programmeOutcome = null,
      Guid providerId = default,
      string referralAttachmentId = "123456",
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
      mostRecentAttachmentDate: mostRecentAttachmentDate,
      nhsLoginClaimEmail: nhsLoginClaimEmail ?? GenerateEmail(),
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
    Entities.ReferralCri cri = null,
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
    bool? isErsClosed = null,
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
    DateTimeOffset? mostRecentAttachmentDate = default,
    string nhsLoginClaimEmail = null,
    string nhsLoginClaimFamilyName = null,
    string nhsLoginClaimGivenName = null,
    string nhsLoginClaimMobile = null,
    string nhsNumber = null,
    string offeredCompletionLevel = null,
    string postcode = null,
    string programmeOutcome = null,
    Guid providerId = default,
    string providerUbrn = null,
    string referralAttachmentId = "123456",
    ReferralSource? referralSource = null,
    string referringGpPracticeName = "Test Practice",
    string referringGpPracticeNumber = null,
    string referringOrganisationEmail = null,
    string referringOrganisationOdsCode = null,
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
    Random rnd = new();
    EthnicityGrouping ethnicityGrouping = GenerateEthnicityGrouping(rnd);

    referralSource ??= ReferralSource.GpReferral;
    if (referralSource == ReferralSource.GpReferral)
    {
      providerUbrn ??= GenerateUbrnGp(rnd);
      ubrn ??= GenerateUbrn(rnd);
    }

    providerUbrn ??= GenerateUbrn(rnd);

    textMessages ??= [];

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
      Cri = cri,
      DateCompletedProgramme = dateCompletedProgramme,
      DateLetterSent = dateLetterSent,
      DateOfBirth = dateOfBirth == default
        ? DateTimeOffset.Now.AddYears(-rnd.Next(18, 100))
        : dateOfBirth,
      DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
        ? DateTimeOffset.Now.AddMonths(-rnd.Next(1, 12))
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
      Email = email ?? GenerateEmail(),
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
        ? default
        : id,
      IsActive = isActive,
      IsErsClosed = isErsClosed,
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
      MostRecentAttachmentDate = mostRecentAttachmentDate == default
        ? DateTimeOffset.Now
        : mostRecentAttachmentDate,
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
      ProviderUbrn = providerUbrn,
      ReferralAttachmentId = referralAttachmentId,
      ReferralSource = $"{referralSource ?? ReferralSource.GpReferral}",
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber
        ?? GenerateGpPracticeNumber(rnd),
      ReferringOrganisationEmail =
        referringOrganisationEmail ?? GenerateNhsEmail(),
      ReferringOrganisationOdsCode =
        referringOrganisationOdsCode ?? GenerateOdsCode(rnd),
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

  public static ReferralAudit CreateRandomReferralAudit(
    string auditAction = "Update",
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
    bool? isErsClosed = null,
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
    DateTimeOffset? mostRecentAttachmentDate = default,
    string nhsLoginClaimEmail = null,
    string nhsLoginClaimFamilyName = null,
    string nhsLoginClaimGivenName = null,
    string nhsLoginClaimMobile = null,
    string nhsNumber = null,
    string offeredCompletionLevel = null,
    string postcode = null,
    string programmeOutcome = null,
    Guid providerId = default,
    string providerUbrn = null,
    string referralAttachmentId = "123456",
    ReferralSource? referralSource = null,
    string referringGpPracticeName = "Test Practice",
    string referringGpPracticeNumber = null,
    string referringOrganisationEmail = null,
    string referringOrganisationOdsCode = null,
    string serviceUserEthnicity = null,
    string serviceUserEthnicityGroup = null,
    string serviceId = "123456",
    string sex = null,
    SourceSystem sourceSystem = SourceSystem.Unidentified,
    string staffRole = null,
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
    EthnicityGrouping ethnicityGrouping = GenerateEthnicityGrouping(rnd);

    referralSource ??= ReferralSource.GpReferral;
    if (referralSource == ReferralSource.GpReferral)
    {
      providerUbrn ??= GenerateUbrnGp(rnd);
      ubrn ??= GenerateUbrn(rnd);
    }

    providerUbrn ??= GenerateUbrn(rnd);

    return new ReferralAudit()
    {
      AuditAction = auditAction,
      AuditDuration = 36,
      AuditResult = 1,
      AuditSuccess = true,
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
      Email = email ?? GenerateEmail(),
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
        ? default
        : id,
      IsActive = isActive,
      IsErsClosed = isErsClosed,
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
      MostRecentAttachmentDate = mostRecentAttachmentDate == default
        ? DateTimeOffset.Now
        : mostRecentAttachmentDate,
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
      ProviderUbrn = providerUbrn,
      ReferralAttachmentId = referralAttachmentId,
      ReferralSource = $"{referralSource ?? ReferralSource.GpReferral}",
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber
        ?? GenerateGpPracticeNumber(rnd),
      ReferringOrganisationEmail =
        referringOrganisationEmail ?? GenerateNhsEmail(),
      ReferringOrganisationOdsCode =
        referringOrganisationOdsCode ?? GenerateOdsCode(rnd),
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
    DateTimeOffset? received = null,
    Guid referralId = default,
    string referralStatus = null,
    string serviceUserLinkId = null)
  {
    Random random = new();

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
      Number = number ?? GenerateMobile(random),
      Outcome = outcome,
      Received = received,
      ReferralId = referralId == default
        ? Guid.Empty
        : referralId,
      ReferralStatus = referralStatus,
      Sent = sentDate,
      ServiceUserLinkId = serviceUserLinkId ?? LinkIdService.GenerateDummyId(),
    };
  }

  public static MessageQueue CreateRandomMessageQueue(
    ApiKeyType apiKeyType = ApiKeyType.TextMessage1,
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
     string personalisation = null,
    string sendResult = null,
    DateTime sent = default,
    string sendTo = null,
    string serviceUserLinkId = null,
    Guid templateId = default)
  {
    DateTime sentDate = sent == default ? DateTime.UtcNow : sent;

    return new MessageQueue()
    {
      ApiKeyType = apiKeyType,
      ServiceUserLinkId = serviceUserLinkId
        ?? LinkIdService.GenerateDummyId(),
      Id = Guid.NewGuid(),
      IsActive = isActive,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? Guid.Empty
        : modifiedByUserId,
      PersonalisationJson = personalisation ?? "{\"givenName\":\"test\"}",
      SendResult = sendResult,
      SendTo = sendTo,
      SentDate = sentDate,
      TemplateId = templateId,
      Type = MessageType.SMS,
    };
  }

  public static MskOrganisation CreateRandomMskOrganisation(
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    string odsCode = null,
    bool sendDischargeLetters = true,
    string siteName = null)
  {
    Random random = new();
    return new MskOrganisation()
    {
      IsActive = isActive,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? Guid.Empty
        : modifiedByUserId,
      OdsCode = odsCode ?? GenerateMskHubOdsCode(random),
      SendDischargeLetters = sendDischargeLetters,
      SiteName = siteName ?? GenerateName(random, 10),
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
    string odsCode = null,
    string templateVersion = null
  )
  {
    Random random = new();
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
      TemplateVersion = templateVersion ?? "1.0"
    };
  }

  public static Questionnaire CreateRandomQuestionnaire(
    QuestionnaireType type,
    DateTimeOffset endDate = default,
    Guid id = default,
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    Guid notificationTemplateId = default,
    DateTimeOffset startDate = default
  )
  {
    return new Questionnaire
    {
      Id = id == default ? default : id,
      IsActive = isActive,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? Guid.Empty
        : modifiedByUserId,
      Type = type,
      EndDate = endDate == default
        ? DateTimeOffset.Now
        : endDate,
      StartDate = startDate == default
        ? DateTimeOffset.Now
        : startDate,
      NotificationTemplateId = notificationTemplateId == default
        ? Guid.Empty
        : notificationTemplateId
    };
  }

  public static ReferralQuestionnaire CreateRandomReferralQuestionnaire(
    Guid id = default,
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    Guid referralId = default,
    Guid questionnaireId = default,
    string notificationKey = null,
    DateTimeOffset created = default,
    DateTimeOffset sending = default,
    DateTimeOffset delivered = default,
    DateTimeOffset temporaryFailure = default,
    DateTimeOffset technicalFailure = default,
    DateTimeOffset permanentFailure = default,
    DateTimeOffset started = default,
    DateTimeOffset completed = default,
    int failureCount = 0,
    string answers = null,
    string familyName = null,
    string givenName = null,
    string mobile = null,
    string email = null,
    ReferralQuestionnaireStatus status = default
  )
  {
    return new ReferralQuestionnaire
    {
      Id = id == default ? default : id,
      IsActive = isActive,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? Guid.Empty
        : modifiedByUserId,
      ReferralId = referralId == default
        ? default
        : referralId,
      QuestionnaireId = questionnaireId == default
        ? default
        : questionnaireId,
      NotificationKey = notificationKey,
      Created = created == default
        ? DateTimeOffset.Now
        : created,
      Sending = sending == default
        ? DateTimeOffset.Now
        : sending,
      Delivered = delivered == default
        ? DateTimeOffset.Now
        : delivered,
      TemporaryFailure = temporaryFailure == default
        ? DateTimeOffset.Now
        : temporaryFailure,
      TechnicalFailure = technicalFailure == default
        ? DateTimeOffset.Now
        : technicalFailure,
      PermanentFailure = permanentFailure == default
        ? DateTimeOffset.Now
        : permanentFailure,
      Started = started == default
        ? DateTimeOffset.Now
        : started,
      Completed = completed == default
        ? DateTimeOffset.Now
        : completed,
      FailureCount = failureCount,
      Answers = answers,
      Email = email,
      Mobile = mobile,
      GivenName = givenName,
      FamilyName = familyName,
      Status = status
    };
  }

  public static UserActionLog CreateUserActionLog(
    int? id = null,
    string action = "RejectToEreferrals",
    string controller = WebUi.RMC_CONTROLLER,
    Guid? userId = null,
    DateTime? requestedAt = null,
    string request = null,
    string ipAddress = "127.0.0.1",
    string queryString = null)
  {
    if (string.IsNullOrEmpty(queryString))
    {
      queryString =
        $"StatusReason=Example Test&DelayReason=Other" +
        $"&Ubrn={GenerateUbrn(new Random())}";
    }
    return new UserActionLog
    {
      Id = id ?? 1,
      Action = action,
      Controller = controller,
      IpAddress = ipAddress,
      Method = HttpMethod.POST,
      UserId = userId ?? Guid.NewGuid(),
      RequestAt = requestedAt ?? DateTimeOffset.Now,
      Request = request ??
        $"https://{ipAddress}/{controller}/{action}|{queryString}"
    };
  }

  public static UserStore CreateUserStore()
  {
    return new UserStore
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.NewGuid(),
      ApiKey = "none",
      Domain = "Rmc.Ui,||**TEST**||",
      ForceExpiry = false,
      OwnerName = "Test User 1"
    };
  }
}
