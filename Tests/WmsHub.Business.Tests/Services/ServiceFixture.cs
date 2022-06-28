using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [CollectionDefinition("Service collection")]
  public class ServiceCollection : ICollectionFixture<ServiceFixture>
  {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }

  public class ServiceFixture
  {
    public static readonly Guid REFERRAL_API_USER_ID =
       new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");

    public static readonly Guid PROVIDER_API_USER_ID =
       new Guid("571342f1-c67d-49bf-a9c6-40a41e6dc702");

    public static readonly Guid CHATBOT_API_USER_ID =
       new Guid("eafc7655-89b7-42a3-bdf7-c57c72cd1d41");

    public static string UBRN = "777666555444";

    public const string NEVER_USED_TELEPHONE_NUMBER = "+1111111111";

    public const string STAFFROLE_AMBULANCEWORKER = "Ambulance Worker";
    public const string STAFFROLE_DOCTOR = "Doctor";
    public const string STAFFROLE_NURSE = "Nurse";
    public const string STAFFROLE_PORTER = "Porter";

    private static int UniqueIndex = 1;

    public static string GetUniqueTelephoneNumber()
    {
      return $"+441{UniqueIndex++:D9}";
    }

    public static string GetUniqueMobileNumber()
    {
      return $"+447{UniqueIndex++:D9}";
    }

    public static readonly Entities.Referral VALID_REFERRAL_ENTITY =
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
      modifiedByUserId: REFERRAL_API_USER_ID,
      nhsNumber: "1111111111",
      postcode: "TF14NF",
      referringGpPracticeNumber: "M11111",
      sex: "Male",
      status: Enums.ReferralStatus.TextMessage1,
      statusReason: null,
      telephone: "+441743123456",
      triagedCompletionLevel: null,
      triagedWeightedLevel: null,
      ubrn: UBRN,
      vulnerableDescription: "Not Vulnerable",
      weightKg: 120m);

    public static readonly Entities.TextMessage VALID_TEXTMESSAGE_ENTITY =
      new Entities.TextMessage()
      {
        IsActive = true,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = CHATBOT_API_USER_ID,
        Sent = DateTimeOffset.Now
      };

    public const string ETHNICITY_GROUPNAME_WHITE = "White";
    public const string ETHNICITY_GROUPNAME_MIXED =
      "Mixed or Multiple ethnic groups";
    public const string ETHNICITY_GROUPNAME_ASIAN = "Asian or Asian British";
    public const string ETHNICITY_GROUPNAME_BLACK =
      "Black, African, Caribbean or Black British";
    public const string ETHNICITY_GROUPNAME_OTHER = "Other ethnic group";
    public const string ETHNICITY_TRIAGENAME_WHITE = "White";
    public const string ETHNICITY_TRIAGENAME_MIXED = "Mixed";
    public const string ETHNICITY_TRIAGENAME_ASIAN = "Asian";
    public const string ETHNICITY_TRIAGENAME_BLACK = "Black";
    public const string ETHNICITY_TRIAGENAME_OTHER = "Other";

    public static Entities.Referral NewReferral { get; private set; }
    public static Entities.Referral ChatBotCall1Referral { get; private set; }
    public static Entities.Referral TextMessage1Referral { get; private set; }

    public static Entities.Referral ChatBotCall1TransferToPhoneReferral
    { get; private set; }

    public IMapper Mapper { get; set; }
    public DbContextOptions<DatabaseContext> Options { get; private set; }

    public ServiceFixture()
    {
      MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business"
        })
      );

      Mapper = mapperConfiguration.CreateMapper();

      Options = new DbContextOptionsBuilder<DatabaseContext>()
        .UseInMemoryDatabase(databaseName: "WmsHub")
        .Options;

      CreateDatabase();
    }

    private void CreateDatabase()
    {
      using DatabaseContext dbContext = new DatabaseContext(Options);

      NewReferral = RandomEntityCreator.CreateRandomReferral() ;
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

    private static void PopulateEthnicities(DatabaseContext context)
    {
      context.Ethnicities.Add(CreateEthnicity(
        id: "D15B2787-7926-1EF6-704E-1012F9298AE1",
        displayName: "Any other ethnic group", groupName: "Other ethnic group",
        oldName: "Other - ethnic category", triageName: "Other",
        minimumBmi: 27.50M, groupOrder: 5, displayOrder: 2));
      context.Ethnicities.Add(CreateEthnicity(
        id: "EFC61F30-F872-FA71-9709-1A416A51982F", displayName: "Chinese",
        groupName: "Asian or Asian British", oldName: "Chinese",
        triageName: "Asian", minimumBmi: 27.50M, groupOrder: 3,
        displayOrder: 4));
      context.Ethnicities.Add(CreateEthnicity(
        id: "3C69F5AE-073F-F180-3CAC-2197EB73E369", displayName: "Indian",
        groupName: "Asian or Asian British",
        oldName: "Indian or British Indian", triageName: "Asian",
        minimumBmi: 27.50M, groupOrder: 3, displayOrder: 1));
      context.Ethnicities.Add(CreateEthnicity(
        id: "4E84EFCD-3DBA-B459-C302-29BCBD9E8E64",
        displayName: "Any other Mixed or Multiple ethnic background",
        groupName: "Mixed or Multiple ethnic groups",
        oldName: "Other Mixed background", triageName: "Mixed",
        minimumBmi: 27.50M, groupOrder: 2, displayOrder: 4));
      context.Ethnicities.Add(CreateEthnicity(
        id: "F6C29207-A3FC-163B-94BC-2CE840AF9396", displayName: "African",
        groupName: "Black, African, Caribbean or Black British",
        oldName: "African", triageName: "Black", minimumBmi: 27.50M,
        groupOrder: 4, displayOrder: 1));
      context.Ethnicities.Add(CreateEthnicity(
        id: "A1B8C48B-FA12-E001-9F8E-3C9BA9D3D065",
        displayName: "Gypsy or Irish Traveller", groupName: "White",
        oldName: "Other White background", triageName: "White",
        minimumBmi: 30.00M, groupOrder: 1, displayOrder: 3));
      context.Ethnicities.Add(CreateEthnicity(
        id: "3185A21D-2FD4-4313-4A59-43DB28A2E89A",
        displayName: "White and Black Caribbean",
        groupName: "Mixed or Multiple ethnic groups",
        oldName: "White and Black Caribbean", triageName: "Mixed",
        minimumBmi: 27.50M, groupOrder: 2, displayOrder: 1));
      context.Ethnicities.Add(CreateEthnicity(
        id: "934A2FA6-F541-60F1-D08D-46F5E647A28D", displayName: "Arab",
        groupName: "Other ethnic group", oldName: "Other - ethnic category",
        triageName: "Other", minimumBmi: 27.50M, groupOrder: 5,
        displayOrder: 1));
      context.Ethnicities.Add(CreateEthnicity(
        id: "E0694F9A-2D9E-BEF6-2F46-6EB9FB7891AD",
        displayName: "Any other Black, African or Caribbean background",
        groupName: "Black, African, Caribbean or Black British",
        oldName: "Other Black background", triageName: "Black",
        minimumBmi: 27.50M, groupOrder: 4, displayOrder: 3));
      context.Ethnicities.Add(CreateEthnicity(
        id: "CB5CA465-C397-A34F-F32B-729A38932E0E",
        displayName: "Any other Asian background",
        groupName: "Asian or Asian British", oldName: "Other Asian background",
        triageName: "Asian", minimumBmi: 27.50M, groupOrder: 3,
        displayOrder: 5));
      context.Ethnicities.Add(CreateEthnicity(
        id: "36FE1D6A-3B04-5A31-FBD9-8D378C2CB86A", displayName: "Caribbean",
        groupName: "Black, African, Caribbean or Black British",
        oldName: "Caribbean", triageName: "Black", minimumBmi: 27.50M,
        groupOrder: 4, displayOrder: 2));
      context.Ethnicities.Add(CreateEthnicity(
        id: "5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F", displayName: "Bangladeshi",
        groupName: "Asian or Asian British",
        oldName: "Pakistani or British Pakistani", triageName: "Asian",
        minimumBmi: 27.50M, groupOrder: 3, displayOrder: 3));
      context.Ethnicities.Add(CreateEthnicity(
        id: "5DC90D60-F03C-3CE6-72F6-A34D4E6F163B",
        displayName: "English, Welsh, Scottish, Northern Irish or British",
        groupName: "White", oldName: "British or mixed British",
        triageName: "White", minimumBmi: 30.00M, groupOrder: 1,
        displayOrder: 1));
      context.Ethnicities.Add(CreateEthnicity(
        id: "76D69A87-D9A7-EAC6-2E2D-A6017D02E04F", displayName: "Pakistani",
        groupName: "Asian or Asian British",
        oldName: "Pakistani or British Pakistani", triageName: "Asian",
        minimumBmi: 27.50M, groupOrder: 3, displayOrder: 2));
      context.Ethnicities.Add(CreateEthnicity(
        id: "279DC2CB-6F4B-96BC-AE72-B96BF7A2579A",
        displayName: "White and Asian",
        groupName: "Mixed or Multiple ethnic groups",
        oldName: "White and Asian", triageName: "Mixed", minimumBmi: 27.50M,
        groupOrder: 2, displayOrder: 3));

      context.Ethnicities.Add(CreateEthnicity(
        id: "5D2B37FD-24C4-7572-4AEA-D437C6E17318", displayName: "Irish",
        groupName: "White", oldName: "Irish", triageName: "White",
        minimumBmi: 30.00M, groupOrder: 1, displayOrder: 2));
      context.Ethnicities.Add(CreateEthnicity(
        id: "75E8313C-BFDF-5ABF-B6DA-D6CA64138CF4",
        displayName: "Any other White background", groupName: "White",
        oldName: "Other White background", triageName: "White",
        minimumBmi: 30.00M, groupOrder: 1, displayOrder: 4));
      context.Ethnicities.Add(CreateEthnicity(
        id: "EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7",
        displayName: "White and Black African",
        groupName: "Mixed or Multiple ethnic groups",
        oldName: "White and Black African", triageName: "Mixed",
        minimumBmi: 27.50M, groupOrder: 2, displayOrder: 2));
      context.Ethnicities.Add(CreateEthnicity(
        id: "95b0feb5-5ece-98ed-1269-c71e327e98c5",
        displayName: "I do not wish to Disclose my Ethnicity",
        groupName: "I do not wish to Disclose my Ethnicity",
        oldName: null,
        triageName: ETHNICITY_TRIAGENAME_OTHER,
        minimumBmi: 30.00M,
        6, 1
      ));
    }

    private static void PopulateStaffRoles(DatabaseContext context)
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

      private static Entities.Ethnicity CreateEthnicity(
      string id,
      string displayName,
      string groupName,
      string oldName,
      string triageName,
      decimal minimumBmi,
      int groupOrder,
      int displayOrder)
    {
      return new Entities.Ethnicity()
      {
        Id = new Guid(id),
        IsActive = true,
        DisplayName = displayName,
        GroupName = groupName,
        OldName = oldName,
        TriageName = triageName,
        GroupOrder = groupOrder,
        DisplayOrder = displayOrder,
        MinimumBmi = minimumBmi
      };
    }

    private void CreateChatBotCall1Referral()
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
          ModifiedByUserId = CHATBOT_API_USER_ID,
          Number = ChatBotCall1Referral.Telephone,
          Sent = DateTimeOffset.Now
        },
      };
    }

    private void CreateTextMessage1Referral()
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
          ModifiedByUserId = CHATBOT_API_USER_ID,
          Sent = DateTimeOffset.Now
        },
      };
    }

    private void CreateTelephoneNumberReferral()
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
          ModifiedByUserId = CHATBOT_API_USER_ID,
          Number = ChatBotCall1TransferToPhoneReferral.Telephone,
          Sent = DateTimeOffset.Now,
          Outcome = ChatBotCallOutcome.TransferredToPhoneNumber.ToString()
        },
      };
    }

    public static Entities.Call CreateCall(
      DateTimeOffset? called = null,
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string number = NEVER_USED_TELEPHONE_NUMBER,
      string outcome = null,
      Entities.Referral referral = null,
      Guid referralId = default,
      DateTimeOffset sent = default)
    {
      return new Entities.Call()
      {
        Called = called,
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? REFERRAL_API_USER_ID
          : modifiedByUserId,
        Number = number,
        Outcome = outcome,
        Referral = referral,
        ReferralId = referralId,
        Sent = sent == default
          ? DateTimeOffset.Now
          : sent
      };
    }

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
        long? referralAttachmentId = 123456,
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
        decimal weightKg = 120m)
    {
      return new Entities.Referral()
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
          ? REFERRAL_API_USER_ID
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
    }

    public static Entities.Provider CreateProviderWithReferrals()
    {
      return new Entities.Provider
      {
        Id = PROVIDER_API_USER_ID,
        IsActive = true,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Logo = "Logo",
        Name = "Test Provider",
        Website = "http://www.test.com",
        Referrals = REFERRAL_LIST,
        Summary = "Summary",
        Summary2 = "Summary2",
        Summary3 = "Summary3"
      };
    }


    public static Entities.Provider CreateProviderWithAuth()
    {
      return new Entities.Provider
      {
        Id = PROVIDER_API_USER_ID,
        IsActive = true,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Logo = "Logo",
        Name = "Test Provider",
        Website = "http://www.test.com",
        ProviderAuth = new ProviderAuth
        {
          Id = Guid.NewGuid()
        },
        Summary = "Summary",
        Summary2 = "Summary2",
        Summary3 = "Summary3"
      };
    }

    public static Entities.Provider CreateProviderWithAuth(Guid id)
    {
      return new Entities.Provider
      {
        Id = id,
        IsActive = true,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Logo = "Logo",
        Name = "Test Provider",
        Website = "http://www.test.com",
        ProviderAuth = new ProviderAuth
        {
          Id = Guid.NewGuid()
        },
        Summary = "Summary",
        Summary2 = "Summary2",
        Summary3 = "Summary3"
      };
    }

    public static Entities.Provider CreateProviderWithNoAuth()
    {
      return new Entities.Provider
      {
        Id = PROVIDER_API_USER_ID,
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
    }

    public static Entities.Provider CreateProviderWithNoAuth(Guid id)
    {
      return new Entities.Provider
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
    }

    private static readonly string[] UBRN_LIST = new string[]
    {
      "777666555444","777666555443","777666555442","777666555441"
      ,"777666555440","777666555439","777666555438","777666555437"
      ,"777666555436","777666555436","777666555434","777666555433"
      ,"777666555432","777666555431","777666555430","777666555429"
      ,"777666555428","777666555427","777666555426","777666555426"
      ,"777666555424"
    };

    private static readonly string[] STATUS_REASON = new string[]
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

    private static readonly string[] CALL_NUMBERS = new string[]
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
    public static List<Entities.Referral> REFERRAL_LIST
    {
      get
      {
        var referrals = new List<Entities.Referral>();
        var count = 0;
        foreach (var number in CALL_NUMBERS)
        {
          count++;
          var nhsnumber = 1111111110 + count;
          var isMobile = number.StartsWith("07") || number.StartsWith("+447");
          referrals.Add(new Business.Entities.Referral()
          {
            Address1 = $"Address1_{count}",
            Address2 = $"Address2_{count}",
            CalculatedBmiAtRegistration = 30m,
            ConsentForFutureContactForEvaluation = true,
            DateOfBirth = DateTimeOffset.Now.AddYears(-40).AddMonths(count),
            DateOfReferral = DateTimeOffset.Now,
            DateOfBmiAtRegistration =  DateTimeOffset.Now.AddYears(-1),
            Email = $"pagambar+notify{count}@gmail.com",
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
            ModifiedByUserId =REFERRAL_API_USER_ID,
            NhsNumber = nhsnumber.ToString(),
            Postcode = $"TF{count}4NF",
            ReferralAttachmentId = 123456 + count,
            ReferringGpPracticeName = "Test Practice",
            ReferringGpPracticeNumber = "T112233",
            Sex = "Male",
            Status = Enums.ReferralStatus.ProviderAwaitingStart.ToString(),
            StatusReason = STATUS_REASON[count - 1],
            Telephone = isMobile ? "" : number,
            TriagedCompletionLevel = null,
            TriagedWeightedLevel = null,
            Ubrn = UBRN_LIST[count - 1],
            VulnerableDescription = "Not Vulnerable",
            WeightKg = 120m
          });
        }
        return referrals;
      }
    }

  }
}
