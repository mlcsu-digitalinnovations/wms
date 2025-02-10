using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Services;

[CollectionDefinition("Service collection")]
public class ServiceCollection : ICollectionFixture<ServiceFixture>
{
  // This class has no code, and is never created. Its purpose is simply
  // to be the place to apply [CollectionDefinition] and all the
  // ICollectionFixture<> interfaces.
}

public class ServiceFixture : AServiceFixtureBase
{
  private static readonly string[] CallNumbers = new string[]
  {
    "02080681039",
    "+442080681204",
    "02080681230",
    "+442080681249" ,
    "+442080681330" ,
    "+442080681336" ,
    "+442080681348" ,
    "+442080681349" ,
    "+442080681352" ,
    "+442080681353" ,
    "+442080681360" ,
    "+442080681362"
  };
  private static readonly Guid ChatbotApiUserId =
   new ("eafc7655-89b7-42a3-bdf7-c57c72cd1d41");
  private static readonly Guid ReferralApiUserId =
     new ("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");
  private static readonly string[] StatusReasons = new string[]
{
    "TEST_D5889E4F-334D-4712-841D-5483385F9CF0",
    "TEST_3821AD48-E420-492D-9F48-7A4B4A3402F4",
    "TEST_195BDFB0-89AA-454F-9D33-89383AF2B2A6",
    "TEST_8CB20D4F-2BF1-4E70-BA56-8A2115CD60B5",
    "TEST_38526A64-7936-49F0-BC0E-7B28E7F6AA5C",
    "TEST_5E730241-C55E-4C7B-BEBD-FE386C206D64",
    "TEST_A97C7671-CE71-4CCF-9766-0FA0F8B23284",
    "TEST_A4C31046-1FD7-43F1-9282-8BC249215157",
    "TEST_FFE72F81-285C-4B0C-BD24-F11035DFAFC8",
    "TEST_95DAB9EF-C9DD-458D-9935-49D794FF5834",
    "TEST_F2CFFE8A-215C-4D60-927D-06170428F40F",
    "TEST_6C7E75C2-F13B-4436-B92C-7AB30BED1D11",
    "TEST_C512CE5E-EA19-4312-8A19-4A804FC2C4AA",
    "TEST_00156598-A16B-4EAA-84B7-298A9389DBE4",
    "TEST_C1A05936-4435-4E1C-BFB7-E1CB769C5088",
    "TEST_FAF722AA-6725-4117-8A38-CB9A391D51B3",
    "TEST_1A29B392-C696-4357-A5E6-5CA7ADB47454",
    "TEST_EDC84E52-9FAC-4412-BB0D-3BF4DBD90ECB",
    "TEST_EF089A43-3F18-4C04-ADF5-740FD0C0B594",
    "TEST_5F1EBFF5-8F94-4AE1-8EFA-A3300CFDC477"
};
  private static string Ubrn = "777666555444";
  private static readonly string[] Ubrns = new string[]
{
    "777666555444","777666555443","777666555442","777666555441"
    ,"777666555440","777666555439","777666555438","777666555437"
    ,"777666555436","777666555435","777666555434","777666555433"
    ,"777666555432","777666555431","777666555430","777666555429"
    ,"777666555428","777666555427","777666555426","777666555426"
    ,"777666555424"
};
  private static int UniqueIndex = 1;

  public const string ETHNICITY_GROUPNAME_ASIAN = "Asian or Asian British";
  public const string ETHNICITY_GROUPNAME_BLACK =
    "Black, African, Caribbean or Black British";
  public const string ETHNICITY_GROUPNAME_MIXED =
    "Mixed or Multiple ethnic groups";
  public const string ETHNICITY_GROUPNAME_OTHER = "Other ethnic group";
  public const string ETHNICITY_GROUPNAME_WHITE = "White";
  public const string ETHNICITY_TRIAGENAME_ASIAN = "Asian";
  public const string ETHNICITY_TRIAGENAME_BLACK = "Black";
  public const string ETHNICITY_TRIAGENAME_MIXED = "Mixed";
  public const string ETHNICITY_TRIAGENAME_OTHER = "Other";
  public const string ETHNICITY_TRIAGENAME_WHITE = "White";
  public const string NEVER_USED_TELEPHONE_NUMBER = "+1111111111";
  public const string PROVIDER_API_USER_ID =
    "571342f1-c67d-49bf-a9c6-40a41e6dc702";
  public const string STAFFROLE_AMBULANCEWORKER = "Ambulance Worker";
  public const string STAFFROLE_DOCTOR = "Doctor";
  public const string STAFFROLE_NURSE = "Nurse";
  public const string STAFFROLE_PORTER = "Porter";


  public static Entities.Referral ChatBotCall1Referral { get; private set; }
  public static Entities.Referral ChatBotCall1TransferToPhoneReferral
  { get; private set; }

  public IMapper Mapper { get; set; }

  public static Entities.Referral NewReferral { get; private set; }

  public DbContextOptions<DatabaseContext> Options { get; private set; }

  public static Entities.Referral TextMessage1Referral { get; private set; }

  public ServiceFixture()
  {
    MapperConfiguration mapperConfiguration = new (cfg =>
      cfg.AddMaps(new[]
        {
          "WmsHub.Business"
        })
      );

    Mapper = mapperConfiguration.CreateMapper();
    EnvironmentVariableConfigurator
      .ConfigureEnvironmentVariablesForAlwaysEncrypted();

    Options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(databaseName: "WmsHub")
      .Options;

    CreateDatabase();
  }

  public static Entities.Call CreateCall(
    DateTimeOffset? called = null,
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    string number = "+441100000000",
    string outcome = null,
    Entities.Referral referral = null,
    Guid referralId = default,
    DateTimeOffset sent = default) => new ()
    {
      Called = called,
      IsActive = isActive,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? ReferralApiUserId
        : modifiedByUserId,
      Number = number,
      Outcome = outcome,
      Referral = referral,
      ReferralId = referralId,
      Sent = sent == default
        ? DateTimeOffset.Now
        : sent
    };

  public static Entities.Referral CreateReferral(
    string address1 = "Address1",
    string address2 = "Address2",
    string address3 = "Address3",
    decimal calculatedBmiAtRegistration = 30m,
    bool consentForFutureContactForEvaluation = true,
    DateTimeOffset? dateCompletedProgramme = null,
    DateTimeOffset dateOfBirth = default,
    DateTimeOffset dateOfBmiAtRegistration = default,
    DateTimeOffset? dateOfProviderSelection = null,
    DateTimeOffset dateOfReferral = default,
    DateTimeOffset? dateStartedProgramme = null,
    DateTimeOffset? dateToDelayUntil = null,
    string email = "beaulah.casper37@ethereal.email",
    string ethnicity = "White",
    string familyName = "FamilyName",
    string givenName = "GivenName",
    bool hasALearningDisability = false,
    bool hasAPhysicalDisability = false,
    bool hasDiabetesType1 = true,
    bool hasDiabetesType2 = false,
    bool hasHypertension = true,
    bool hasRegisteredSeriousMentalIllness = false,
    decimal heightCm = 150m,
    bool isActive = true,
    bool? isMobileValid = null,
    bool? isTelephoneValid = null,
    bool isVulnerable = false,
    string mobile = "+447886123456",
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    string nhsNumber = "1111111111",
    string postcode = "TF14NF",
    string programmeOutcome = null,
    string referralAttachmentId = "123456",
    string referringGpPracticeName = "Test Practive",
    string referringGpPracticeNumber = "M11111",
    string sex = "Male",
    Enums.ReferralStatus status = Enums.ReferralStatus.TextMessage1,
    string statusReason = null,
    string telephone = "+441743123456",
    string triagedCompletionLevel = null,
    string triagedWeightedLevel = null,
    string ubrn = "777666555444",
    string vulnerableDescription = "Not Vulnerable",
    decimal weightKg = 120m) => new ()
    {
      Address1 = address1,
      Address2 = address2,
      Address3 = address3,
      CalculatedBmiAtRegistration = calculatedBmiAtRegistration,
      ConsentForFutureContactForEvaluation =
        consentForFutureContactForEvaluation,
      DateCompletedProgramme = dateCompletedProgramme,
      DateOfBirth = dateOfBirth == default
        ? DateTimeOffset.Now.AddYears(-40)
        : dateOfBirth,
      DateOfBmiAtRegistration = dateOfBmiAtRegistration == default
        ? DateTimeOffset.Now.AddYears(-1)
        : dateOfBmiAtRegistration,
      DateOfProviderSelection = dateOfProviderSelection,
      DateOfReferral = dateOfReferral == default
        ? DateTimeOffset.Now
        : dateOfReferral,
      DateStartedProgramme = dateStartedProgramme,
      DateToDelayUntil = dateToDelayUntil,
      Email = email,
      Ethnicity = ethnicity,
      FamilyName = familyName,
      GivenName = givenName,
      HasALearningDisability = hasALearningDisability,
      HasAPhysicalDisability = hasAPhysicalDisability,
      HasDiabetesType1 = hasDiabetesType1,
      HasDiabetesType2 = hasDiabetesType2,
      HasHypertension = hasHypertension,
      HasRegisteredSeriousMentalIllness = hasRegisteredSeriousMentalIllness,
      HeightCm = heightCm,
      IsActive = isActive,
      IsMobileValid = isMobileValid,
      IsTelephoneValid = isTelephoneValid,
      IsVulnerable = isVulnerable,
      Mobile = mobile,
      ModifiedAt = modifiedAt == default
        ? DateTimeOffset.Now
        : modifiedAt,
      ModifiedByUserId = modifiedByUserId == default
        ? ReferralApiUserId
        : modifiedByUserId,
      NhsNumber = nhsNumber,
      Postcode = postcode,
      ProgrammeOutcome = programmeOutcome,
      ReferralAttachmentId = referralAttachmentId,
      ReferringGpPracticeName = referringGpPracticeName,
      ReferringGpPracticeNumber = referringGpPracticeNumber,
      Sex = sex,
      Status = status.ToString(),
      StatusReason = statusReason,
      Telephone = telephone,
      TriagedCompletionLevel = triagedCompletionLevel,
      TriagedWeightedLevel = triagedWeightedLevel,
      Ubrn = ubrn,
      VulnerableDescription = vulnerableDescription,
      WeightKg = weightKg
    };

  public static Business.Models.MessageQueue CreateMessageQueue(
    ApiKeyType apiKeyType = ApiKeyType.None,
    string clientReference = null,
    string contact = null,
    string templateId = null,
    Dictionary<string, dynamic> personalisation = null,
    DateTime? sent = null,
    MessageType type = MessageType.Email,
    string emailReplyToId = null,
    string[] expectedPersonalisationList = null) => new (
      apiKeyType: apiKeyType,
      clientReference: clientReference ?? Guid.NewGuid().ToString(),
      emailTo: type == MessageType.Email
        ? contact ?? Generators.GenerateEmail()
        : string.Empty,
      emailReplyToId: emailReplyToId,
      mobile: type == MessageType.SMS
        ? contact ?? Generators.GenerateMobile(new Random())
        : string.Empty,
      personalisationList: expectedPersonalisationList,
      personalisations: personalisation,
      templateId: string.IsNullOrWhiteSpace(templateId)
        ? null
        : Guid.Parse(templateId),
      type: type
    );

  public static Entities.Provider CreateProviderWithAuth(Guid id) => new ()
  {
    Id = id,
    IsActive = true,
    Level1 = true,
    Level2 = true,
    Level3 = true,
    Logo = "Logo",
    Name = "Test Provider",
    Website = "http://www.test.com",
    ProviderAuth = new Entities.ProviderAuth
    {
      Id = id,
      IsActive = true,
      ModifiedAt = DateTime.Now,
      ModifiedByUserId = Guid.NewGuid(),
      SmsKey = "12345678",
      SmsKeyExpiry = DateTime.Now.AddDays(1),
      KeyViaSms = true,
      KeyViaEmail = false,
      MobileNumber = Generators.GenerateMobile(new Random()),
      IpWhitelist = "**IGNORE_FOR_TESTING**"
    },
    Summary = "Summary",
    Summary2 = "Summary2",
    Summary3 = "Summary3"
  };

  public static Entities.Provider CreateProviderWithNoAuth(Guid id) => new ()
  {
    Id = id,
    IsActive = true,
    Level1 = true,
    Level2 = true,
    Level3 = true,
    Logo = "Logo",
    Name = "Test Provider",
    Website = "http://www.test.com",
    Summary = "Summary",
    Summary2 = "Summary2",
    Summary3 = "Summary3"
  };

  public static Entities.Provider CreateProviderWithReferrals() => new ()
  {
    Id = Guid.Parse(PROVIDER_API_USER_ID),
    IsActive = true,
    Level1 = true,
    Level2 = true,
    Level3 = true,
    Logo = "Logo",
    Name = "Test Provider",
    Website = "http://www.test.com",
    Referrals = ReferralList,
    Summary = "Summary",
    Summary2 = "Summary2",
    Summary3 = "Summary3"
  };

  public static string GetUniqueTelephoneNumber() => $"+441{UniqueIndex++:D9}";

  public static string GetUniqueMobileNumber() => $"+447{UniqueIndex++:D9}";

  public static void LoadDefaultData(DatabaseContext dbContext)
  {
    NewReferral = RandomEntityCreator.CreateRandomReferral();
    dbContext.Referrals.Add(NewReferral);

    CreateChatBotCall1Referral();
    dbContext.Referrals.Add(ChatBotCall1Referral);

    CreateTextMessage1Referral();
    dbContext.Referrals.Add(TextMessage1Referral);

    CreateTelephoneNumberReferral();
    dbContext.Referrals.Add(ChatBotCall1TransferToPhoneReferral);

    PopulateEthnicities(dbContext);

    PopulateStaffRoles(dbContext);

    dbContext.SaveChanges();
  }

  public static readonly Entities.TextMessage ValidTextMessageEntity = new ()
  {
    IsActive = true,
    ModifiedAt = DateTimeOffset.Now,
    ModifiedByUserId = ChatbotApiUserId,
    Sent = DateTimeOffset.Now
  };

  public static readonly Entities.Referral ValidReferralEntity =
    RandomEntityCreator.CreateRandomReferral(
    address1: "Address1",
    address2: "Address2",
    address3: "Address3",
    calculatedBmiAtRegistration: 30m,
    consentForFutureContactForEvaluation: true,
    dateOfBirth: DateTimeOffset.Now.AddYears(-40),
    dateOfReferral: DateTimeOffset.Now,
    email: "beaulah.casper37@ethereal.email",
    ethnicity: "White",
    familyName: "FamilyName",
    givenName: "GivenName",
    hasDiabetesType1: true,
    hasDiabetesType2: false,
    hasHypertension: true,
    hasRegisteredSeriousMentalIllness: false,
    heightCm: 150m,
    isActive: true,
    isVulnerable: false,
    mobile: "+447886123456",
    modifiedAt: DateTimeOffset.Now,
    modifiedByUserId: ReferralApiUserId,
    nhsNumber: "1111111111",
    postcode: "TF14NF",
    referringGpPracticeNumber: "M11111",
    sex: "Male",
    status: Enums.ReferralStatus.TextMessage1,
    statusReason: null,
    telephone: "+441743123456",
    triagedCompletionLevel: null,
    triagedWeightedLevel: null,
    ubrn: Ubrn,
    vulnerableDescription: "Not Vulnerable",
    weightKg: 120m);

  private void CreateDatabase()
  {
    using DatabaseContext dbContext = new (Options);
    LoadDefaultData(dbContext);
  }

  private static void CreateChatBotCall1Referral()
  {
    ChatBotCall1Referral = RandomEntityCreator.CreateRandomReferral();
    ChatBotCall1Referral.Status =
      Enums.ReferralStatus.ChatBotCall1.ToString();
    ChatBotCall1Referral.Calls = new List<Entities.Call>()
    {
      new Entities.Call()
      {
        IsActive = true,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = ChatbotApiUserId,
        Number = ChatBotCall1Referral.Telephone,
        Sent = DateTimeOffset.Now
      },
    };
  }

  private static void CreateTextMessage1Referral()
  {
    TextMessage1Referral = RandomEntityCreator.CreateRandomReferral();
    TextMessage1Referral.Status =
      Enums.ReferralStatus.TextMessage1.ToString();
    TextMessage1Referral.TextMessages = new List<Entities.TextMessage>()
    {
      new Entities.TextMessage()
      {
        IsActive = true,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = ChatbotApiUserId,
        Sent = DateTimeOffset.Now
      },
    };
  }

  private static void CreateTelephoneNumberReferral()
  {
    ChatBotCall1TransferToPhoneReferral =
      RandomEntityCreator.CreateRandomReferral();
    ChatBotCall1TransferToPhoneReferral.Status =
      ReferralStatus.ChatBotCall1.ToString();
    ChatBotCall1TransferToPhoneReferral.Calls = new List<Entities.Call>()
    {
      new Entities.Call()
      {
        IsActive = true,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = ChatbotApiUserId,
        Number = ChatBotCall1TransferToPhoneReferral.Telephone,
        Sent = DateTimeOffset.Now,
        Outcome = ChatBotCallOutcome.TransferredToPhoneNumber.ToString()
      },
    };
  }

  public static void PopulateStaffRoles(DatabaseContext context)
  {
    context.StaffRoles.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: STAFFROLE_AMBULANCEWORKER));
    context.StaffRoles.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: STAFFROLE_DOCTOR));
    context.StaffRoles.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: STAFFROLE_NURSE));
    context.StaffRoles.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: STAFFROLE_PORTER));
  }

  private static List<Entities.Referral> ReferralList
  {
    get
    {
      List<Entities.Referral> referrals = new ();
      int count = 0;
      foreach (string number in CallNumbers)
      {
        count++;
        int nhsnumber = 1111111110 + count;
        bool isMobile = number.StartsWith("07") || number.StartsWith("+447");
        referrals.Add(new Entities.Referral()
        {
          Address1 = $"Address1_{count}",
          Address2 = $"Address2_{count}",
          CalculatedBmiAtRegistration = 30m,
          ConsentForFutureContactForEvaluation = true,
          DateOfBirth = DateTimeOffset.Now.AddYears(-40).AddMonths(count),
          DateOfReferral = DateTimeOffset.Now,
          DateOfBmiAtRegistration = DateTimeOffset.Now.AddYears(-1),
          Email = $"beaulah.casper{count}@ethereal.email",
          Ethnicity = "White",
          FamilyName = $"FamilyName_{count}",
          GivenName = $"GivenName_{count}",
          HasDiabetesType1 = true,
          HasDiabetesType2 = false,
          HasALearningDisability = false,
          HasAPhysicalDisability = false,
          HasHypertension = true,
          HasRegisteredSeriousMentalIllness = false,
          HeightCm = 150m,
          IsActive = true,
          IsMobileValid = true,
          IsTelephoneValid = true,
          IsVulnerable = false,
          Mobile = isMobile ? number : "",
          ModifiedAt = DateTimeOffset.Now,
          ModifiedByUserId = ReferralApiUserId,
          NhsNumber = nhsnumber.ToString(),
          Postcode = $"TF{count}4NF",
          ProviderUbrn = Ubrns[count - 1],
          ReferralAttachmentId = $"123456{count}",
          ReferringGpPracticeName = "Test Practice",
          ReferringGpPracticeNumber = "T112233",
          Sex = "Male",
          Status = Enums.ReferralStatus.ProviderAwaitingStart.ToString(),
          StatusReason = StatusReasons[count - 1],
          Telephone = isMobile ? "" : number,
          TriagedCompletionLevel = null,
          TriagedWeightedLevel = null,
          Ubrn = Ubrns[count - 1],
          VulnerableDescription = "Not Vulnerable",
          WeightKg = 120m
        });
      }
      return referrals;
    }
  }
}
