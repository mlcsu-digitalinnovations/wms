using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;

namespace WmsHub.Utilities.Seeds;

public class ReferralSeeder : SeederBase<Referral>
{
  public static readonly Guid ReferralApiUserId = new("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");
  public const string SeededServiceIdPrefix = "SeededReferral";

  public static async Task DeleteTestData()
  {
    Referral[] referralsToDelete = await DatabaseContext.Referrals
      .Where(x => x.ServiceId.StartsWith(SeededServiceIdPrefix))
      .ToArrayAsync();

    if (referralsToDelete.Length > 0)
    {
      DatabaseContext.Referrals.RemoveRange([.. referralsToDelete]);
    }

    Serilog.Log.Information(
      "{DeletedReferrals} referrals set to be deleted",
      referralsToDelete.Length);
  }

  internal static void SeedQueryPerformanceData(int noOfRecords)
  {
    Stopwatch sw = new Stopwatch();
    sw.Start();

    ReferralStatus[] referralStatuses = new ReferralStatus[]
    {
      ReferralStatus.New,
      ReferralStatus.TextMessage1,
      ReferralStatus.TextMessage2,
      ReferralStatus.ChatBotCall1,
      ReferralStatus.TextMessage3,
      ReferralStatus.ChatBotTransfer,
      ReferralStatus.RmcCall
    };


    string TEST_UBRN_PREFIX = Config["SeedSettings:Ubrn_Prefix"];
    Random random = new();

    List<Referral> referrals = new(noOfRecords);

    for (int i = 0; i < noOfRecords; i++)
    {
      //dates setup per referral
      DateTimeOffset dateOfBirth =
        DateTime.Now.AddYears(-1 * random.Next(30, 71));
      DateTimeOffset dateOfReferral =
        dateOfBirth.AddYears(random.Next(1, 31));
      DateTimeOffset dateOfBmiAtReg =
        dateOfReferral.AddDays(random.Next(1, 5));

      //setup referral
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        address1: "Address1",
        address2: "Address2",
        calculatedBmiAtRegistration: (decimal)random.Next(30, 41),
        consentForFutureContactForEvaluation: true,
        dateOfBirth: dateOfBirth,
        dateOfReferral: dateOfReferral,
        email: $"query.perf{i}@test.xom",
        ethnicity: Business.Enums.Ethnicity.White.ToString(),
        givenName: Generators.GenerateName(random, 6),
        familyName: Generators.GenerateName(random, 8),
        hasDiabetesType1: Convert.ToBoolean(random.Next(0, 2)),
        hasDiabetesType2: Convert.ToBoolean(random.Next(0, 2)),
        hasHypertension: Convert.ToBoolean(random.Next(0, 2)),
        hasRegisteredSeriousMentalIllness:
          Convert.ToBoolean(random.Next(0, 2)),
        heightCm: Convert.ToDecimal(random.Next(130, 230)),
        isVulnerable: Convert.ToBoolean(random.Next(0, 2)),
        mobile: $"+447886{i.ToString().PadLeft(6, '0')}",
        nhsNumber: Generators.GenerateNhsNumber(random),
        postcode: Generators.GeneratePostcode(random),
        referringGpPracticeNumber: Generators.GenerateName(random, 1)
          + Generators.GenerateStringOfDigits(random, 5),
        sex: random.Next(0, 2) == 0 ? "Male" : "Female",
        status: referralStatuses[random.Next(0, referralStatuses.Length)],
        statusReason: "QPerf - " + Generators.GenerateName(random, 8),
        telephone: $"+441743{i.ToString().PadLeft(6, '0')}",
        serviceId: $"{SeededServiceIdPrefix}{i:0000000000}",
        triagedCompletionLevel: null,
        triagedWeightedLevel: null,
        ubrn: ((Guid.NewGuid().ToString()).Remove(0, 4))
          .Insert(0, TEST_UBRN_PREFIX),
        vulnerableDescription: random.Next(0, 2) == 0 ?
          "Not Vulnerable" : "Vulnerable",
        weightKg: Convert.ToDecimal(random.Next(60, 121)),
        hasAPhysicalDisability: Convert.ToBoolean(random.Next(0, 2)),
        referringGpPracticeName: Generators.GenerateName(random, 7)
          + " Practice Name",
        dateOfBmiAtRegistration: dateOfBmiAtReg,
        hasALearningDisability: Convert.ToBoolean(random.Next(0, 2)),
        programmeOutcome: "Programme Outcome " +
          Generators.GenerateName(random, 7),
        isMobileValid: Convert.ToBoolean(random.Next(0, 2)),
        isTelephoneValid: Convert.ToBoolean(random.Next(0, 2)),
        referralAttachmentId: "0");

      referral.Id = Guid.NewGuid();

      //hold list of Provider Guids - used to link to new Providers
      Guid tempGuid;
      referral.ProviderId = tempGuid = Guid.NewGuid();

      referrals.Add(referral);
    }

    // LOCALHOST
    //string[] columns = new string[] { "Id", "IsActive", "ModifiedAt",
    //  "ModifiedByUserId", "Status", "StatusReason", 
    //  "ReferringGpPracticeNumber", "Sex", "DateOfReferral", 
    //  "ConsentForFutureContactForEvaluation", "Ethnicity", 
    //  "HasAPhysicalDisability", "ReferringGpPracticeName", 
    //  "HasHypertension", "HasDiabetesType1",
    //  "HasDiabetesType2", "HeightCm", "WeightKg", 
    //  "CalculatedBmiAtRegistration", "IsVulnerable", 
    //  "HasRegisteredSeriousMentalIllness", 
    //  "TriagedCompletionLevel", "TriagedWeightedLevel", 
    //  "NhsNumber", "Ubrn", "FamilyName", "GivenName", 
    //  "Postcode", "Address1", "Address2", "Address3", "Telephone", "Mobile", 
    //  "Email", "DateOfBirth", 
    //  "VulnerableDescription", "DateCompletedProgramme", 
    //  "DateOfBmiAtRegistration", "DateOfProviderSelection", 
    //  "DateStartedProgramme", "HasALearningDisability", 
    //  "ProgrammeOutcome", "ProviderId", "DateToDelayUntil",
    //  "IsMobileValid", "IsTelephoneValid", "ReferralAttachmentId" };

    // STAGING
    string[] columns = new string[] { "Id", "IsActive", "ModifiedAt",
      "ModifiedByUserId", "Status", "StatusReason", "NhsNumber",
      "ReferringGpPracticeNumber",
      "Ubrn", "FamilyName", "GivenName",
      "Postcode", "Address1", "Address2", "Address3", "Telephone", "Mobile",
      "Email", "DateOfBirth", "Sex", "DateOfReferral",
      "ConsentForFutureContactForEvaluation", "Ethnicity",
      "HasAPhysicalDisability", "ReferringGpPracticeName",
      "HasHypertension", "HasDiabetesType1",
      "HasDiabetesType2", "HeightCm", "WeightKg",
      "CalculatedBmiAtRegistration", "IsVulnerable", "VulnerableDescription",
      "HasRegisteredSeriousMentalIllness",
      "TriagedCompletionLevel", "TriagedWeightedLevel",
      "DateCompletedProgramme",
      "DateOfBmiAtRegistration", "DateOfProviderSelection",
      "DateStartedProgramme", "HasALearningDisability",
      "ProgrammeOutcome", "ProviderId", "DateToDelayUntil",
      "IsMobileValid", "IsTelephoneValid", "ReferralAttachmentId" };

    using (IDataReader reader = ObjectReader.Create(referrals, columns))
    using (SqlConnection connection =
      new SqlConnection(Config.GetConnectionString("WmsHub")))
    using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
    {
      connection.Open();
      bcp.BulkCopyTimeout = 0;
      bcp.BatchSize = 1000;
      bcp.DestinationTableName = "[Referrals]";
      bcp.WriteToServer(reader);
      connection.Close();
    }

    sw.Stop();
    Serilog.Log.Information("Added {count} Referrals - took {seconds}s.",
      noOfRecords,
      sw.Elapsed.TotalSeconds);
  }

  internal static int CreateDischargeReferrals(Provider provider)
  {
    string testUbrnPrefix = Config["SeedSettings:Ubrn_Prefix"];
    testUbrnPrefix += new string('0', 11 - testUbrnPrefix.Length);
    DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-94);
    DateTimeOffset lastSubmissionDate = dateStartedProgramme.AddDays(49);
    List<Referral> referrals = new();

    // GP referral, no submissions
    // Outcome = Did not commence, Status = AwaitingDischarge
    referrals.Add(RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme.AddDays(-10),
      offeredCompletionLevel: "1",
      providerId: provider.Id,
      referralSource: ReferralSource.GpReferral,
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "1",
      weightKg: 90.55m));

    // Non-GP referral, no submissions
    // Outcome = Did not commence, Status = Complete
    referrals.Add(RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme,
      offeredCompletionLevel: "2",
      providerId: provider.Id,
      referralSource: ReferralSource.Pharmacy,
      serviceId: $"{SeededServiceIdPrefix}0000000002",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "2",
      weightKg: 90.55m));

    // GP referral, last submission before last engagment threshold
    // Outcome = Did not complete, Status = AwaitingDischarge
    Referral referral3 = RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme,
      offeredCompletionLevel: "1",
      providerId: provider.Id,
      referralSource: ReferralSource.GpReferral,
      serviceId: $"{SeededServiceIdPrefix}0000000003",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "3",
      weightKg: 90.55m);
    referral3.ProviderSubmissions = new()
    {
      RandomEntityCreator.CreateProviderSubmission(
        date: referral3.DateStartedProgramme.Value.AddDays(10),
        providerId: provider.Id)
    };
    referrals.Add(referral3);

    // Non-GP referral, last submission before last engagment threshold
    // Outcome = Did not complete, Status = Complete
    Referral referral4 = RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme,
      offeredCompletionLevel: "2",
      providerId: provider.Id,
      referralSource: ReferralSource.SelfReferral,
      serviceId: $"{SeededServiceIdPrefix}0000000004",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "4",
      weightKg: 90.55m);
    referral4.ProviderSubmissions = new()
    {
      RandomEntityCreator.CreateProviderSubmission(
        date: referral4.DateStartedProgramme.Value.AddDays(10),
        providerId: provider.Id)
    };
    referrals.Add(referral4);


    // GP referral, last submission after last engagment threshold
    // Outcome = Complete, Status = AwaitingDischarge
    Referral referral5 = RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme,
      offeredCompletionLevel: "1",
      providerId: provider.Id,
      referralSource: ReferralSource.GpReferral,
      serviceId: $"{SeededServiceIdPrefix}0000000005",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "5",
      weightKg: 90.55m);
    referral5.ProviderSubmissions = new()
    {
      RandomEntityCreator.CreateProviderSubmission(
        date: referral5.DateStartedProgramme.Value.AddDays(10),
        providerId: provider.Id,
        weight: 95.25m),
      RandomEntityCreator.CreateProviderSubmission(
        date: referral5.DateStartedProgramme.Value.AddDays(49),
        providerId: provider.Id,
        weight: 90.25m)
    };
    referrals.Add(referral5);

    // Non-GP referral, last submission after last engagment threshold
    // Outcome = Complete, Status = Complete
    Referral referral6 = RandomEntityCreator.CreateRandomReferral(
      dateStartedProgramme: dateStartedProgramme,
      offeredCompletionLevel: "2",
      providerId: provider.Id,
      referralSource: ReferralSource.GeneralReferral,
      serviceId: $"{SeededServiceIdPrefix}0000000006",
      status: ReferralStatus.ProviderStarted,
      ubrn: testUbrnPrefix + "6",
      weightKg: 90.55m);
    referral4.ProviderSubmissions = new()
    {
      RandomEntityCreator.CreateProviderSubmission(
        date: referral5.DateStartedProgramme.Value.AddDays(10),
        providerId: provider.Id,
        weight: 95.25m),
      RandomEntityCreator.CreateProviderSubmission(
        date: referral5.DateStartedProgramme.Value.AddDays(49),
        providerId: provider.Id,
        weight: 90.25m)
    };
    referrals.Add(referral6);


    DatabaseContext.Referrals.AddRange(referrals);
    DatabaseContext.SaveChanges();

    return referrals.Count;
  }

  internal static int CreateOneReferralForEachReferralSourceForTheTestProvider()
  {
    int referralsCreated = 0;

    Guid testProviderId = DatabaseContext
      .Providers
      .Single(x => x.Name == "Test")
      .Id;

    Random random = new Random();

    int i = 0;
    foreach (ReferralSource rs in Enum.GetValues(typeof(ReferralSource)))
    {
      i++;
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now,
        providerId: testProviderId,
        referralSource: rs,
        serviceId: $"{SeededServiceIdPrefix}{i:0000000000}",
        status: ReferralStatus.ProviderAwaitingStart,
        triagedCompletionLevel: "1");

      switch (rs)
      {
        case ReferralSource.GpReferral:
          referral.Ubrn = "000000000001";
          referral.ProviderUbrn = "GP0000000001";
          break;
        case ReferralSource.SelfReferral:
          referral.Ubrn = "SR0000000001";
          referral.ProviderUbrn = referral.Ubrn;
          break;
        case ReferralSource.Pharmacy:
          referral.Ubrn = "PR0000000001";
          referral.ProviderUbrn = referral.Ubrn;
          break;
        case ReferralSource.GeneralReferral:
          referral.Ubrn = "GR0000000001";
          referral.ProviderUbrn = referral.Ubrn;
          break;
        case ReferralSource.Msk:
          referral.Ubrn = "MSK000000001";
          referral.ProviderUbrn = referral.Ubrn;
          break;
        case ReferralSource.ElectiveCare:
          referral.Ubrn = "EC0000000001";
          referral.ProviderUbrn = referral.Ubrn;
          break;
      }


      DatabaseContext.Referrals.Add(referral);
      referralsCreated++;
    }

    DatabaseContext.SaveChanges();
    return referralsCreated;
  }

  internal static int CreateOneReferralForEachStatus()
  {
    int referralsCreated = 0;
    foreach (ReferralStatus rs in Enum.GetValues(typeof(ReferralStatus)))
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        serviceId: $"{SeededServiceIdPrefix}{referralsCreated:0000000000}",
        status: rs, 
        triagedCompletionLevel: "1");

      List<string> providerStatuses = Enum.GetNames<ReferralStatus>()
        .Where(n => n.StartsWith("Provider"))
        .ToList();

      if (providerStatuses.Contains(referral.Status))
      {
        referral.ProviderId = DatabaseContext.Providers.First().Id;
      }

      DatabaseContext.Referrals.Add(referral);
      referralsCreated++;
    }
    DatabaseContext.SaveChanges();
    return referralsCreated;
  }

  internal static List<Referral> CreateReferralsWithProviderStartedStatus(
    int noOfRecords,
    List<Provider> providers,
    DateTimeOffset? dateOfReferral = null)
  {
    List<Referral> referrals = new(noOfRecords);
    Random random = new();
    dateOfReferral = dateOfReferral ?? DateTimeOffset.Now.AddDays(-90);

    string testUbrnPrefix = Config["SeedSettings:Ubrn_Prefix"];
    testUbrnPrefix += new string('0', 12 - testUbrnPrefix.Length);

    for (int i = 0; i < noOfRecords; i++)
    {
      int idxLength = i.ToString().Length;
      string ubrn = string
        .Concat(testUbrnPrefix.AsSpan(0, testUbrnPrefix.Length - idxLength), (i + 1).ToString());

      referrals.Add(RandomEntityCreator.CreateRandomReferral(
        providerId: providers[random.Next(0, providers.Count)].Id,
        dateOfReferral: dateOfReferral.Value,
        dateOfProviderSelection: dateOfReferral.Value,
        dateStartedProgramme: dateOfReferral.Value,
        modifiedAt: dateOfReferral.Value,
        offeredCompletionLevel: random.Next(1, 4).ToString(),
        serviceId: $"{SeededServiceIdPrefix}{i:0000000000}",
        status: ReferralStatus.ProviderStarted,
        ubrn: ubrn
      ));
    }

    DatabaseContext.Referrals.AddRange(referrals);
    DatabaseContext.SaveChanges();
    Serilog.Log.Information("Created {CreatedReferrals} referrals.", referrals.Count);

    return referrals;
  }

  public static void SeedDevelopmentData()
  {
    AddNew();

    AddInvalid();

    AddTextMessage1();

    AddTextMessage2();

    AddTextMessage3();

    AddChatBotCall1();

    AddRmcCallVulnerable();

    AddRmcCallTwoTextMessagesOneCall();
  }

  private static void AddNew()
  {
    Referral newReferral = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001");

    DatabaseContext.Referrals.Add(newReferral);
    DatabaseContext.SaveChanges();
  }

  private static void AddInvalid()
  {
    Referral invalid = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000002",
      sex: null,
      status: ReferralStatus.Exception,
      statusReason: "The Sex field is required");

    DatabaseContext.Referrals.Add(invalid);
    DatabaseContext.SaveChanges();
  }

  private static void AddTextMessage1()
  {
    Referral referralTriageLevel1 = RandomEntityCreator.CreateRandomReferral(
      dateOfBirth: new DateTime(DateTime.Now.AddYears(-65).Year, 1, 1),
      ethnicity: "White",
      postcode: "B755AR",
      serviceId: $"{SeededServiceIdPrefix}0000000003",
      sex: "Male",
      status: ReferralStatus.TextMessage1,
      statusReason: "Triage Level 1");

    TextMessage textMessageTriageLevel1 = RandomEntityCreator.CreateRandomTextMessage(

      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID,
      number: referralTriageLevel1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-1),
      serviceUserLinkId: "TriageLvl001");

    referralTriageLevel1.TextMessages = new List<TextMessage>() { textMessageTriageLevel1 };

    Referral referralTriageLevel2 = RandomEntityCreator.CreateRandomReferral(
      dateOfBirth: new DateTime(DateTime.Now.AddYears(-30).Year, 1, 1),
      ethnicity: "White",
      postcode: "B743AA",
      serviceId: $"{SeededServiceIdPrefix}0000000003",
      sex: "Male",
      status: ReferralStatus.TextMessage1,
      statusReason: "Triage Level 2");

    TextMessage textMessageTriageLevel2 = RandomEntityCreator.CreateRandomTextMessage(
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID,
      number: referralTriageLevel1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-1),
      serviceUserLinkId: "TriageLvl002");

    referralTriageLevel2.TextMessages = new List<TextMessage>() { textMessageTriageLevel2 };

    Referral referralTriageLevel3 = RandomEntityCreator.CreateRandomReferral(
      dateOfBirth: new DateTime(DateTime.Now.AddYears(-30).Year, 1, 1),
      ethnicity: "Black",
      postcode: "B771BN",
      serviceId: $"{SeededServiceIdPrefix}0000000004",
      sex: "Female",
      status: ReferralStatus.TextMessage1,
      statusReason: "Triage Level 3");

    TextMessage textMessageTriageLevel3 = RandomEntityCreator.CreateRandomTextMessage(
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID,
      number: referralTriageLevel1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-1),
      serviceUserLinkId: "TriageLvl003");

    referralTriageLevel3.TextMessages = new List<TextMessage>() { textMessageTriageLevel3 };

    DatabaseContext.Referrals.Add(referralTriageLevel1);
    DatabaseContext.Referrals.Add(referralTriageLevel2);
    DatabaseContext.Referrals.Add(referralTriageLevel3);
    DatabaseContext.SaveChanges();
  }

  private static void AddTextMessage2()
  {
    Referral textMessage2 = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.TextMessage2);

    TextMessage txt1 = RandomEntityCreator.CreateRandomTextMessage(
      number: textMessage2.Mobile,
      sent: DateTimeOffset.Now.AddHours(-98),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    TextMessage txt2 = RandomEntityCreator.CreateRandomTextMessage(
      number: textMessage2.Mobile,
      sent: DateTimeOffset.Now.AddHours(-49),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    textMessage2.TextMessages = new List<TextMessage>()
    { txt1, txt2 };

    DatabaseContext.Referrals.Add(textMessage2);
    DatabaseContext.SaveChanges();
  }

  private static void AddTextMessage3()
  {
    Referral textMessage3 = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.TextMessage2);

    TextMessage txt1 = RandomEntityCreator.CreateRandomTextMessage(
      number: textMessage3.Mobile,
      sent: DateTimeOffset.Now.AddHours(-265),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    TextMessage txt2 = RandomEntityCreator.CreateRandomTextMessage(
      number: textMessage3.Mobile,
      sent: DateTimeOffset.Now.AddHours(-217),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
      number: textMessage3.Mobile,
      sent: DateTimeOffset.Now.AddHours(-169),
      modifiedByUserId: CallSeeder.CHATBOT_API_USER_ID);

    textMessage3.Calls = new List<Call>() { call1 };

    TextMessage txt3 = RandomEntityCreator.CreateRandomTextMessage(
      number: textMessage3.Mobile,
      sent: DateTimeOffset.Now.AddHours(-1),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    textMessage3.TextMessages = new List<TextMessage>()
    { txt1, txt2, txt3 };

    DatabaseContext.Referrals.Add(textMessage3);
    DatabaseContext.SaveChanges();
  }

  private static void AddChatBotCall1()
  {
    Referral chatBotCall1 = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.ChatBotCall1);

    TextMessage txt1 = RandomEntityCreator.CreateRandomTextMessage(
      number: chatBotCall1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-98),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    TextMessage txt2 = RandomEntityCreator.CreateRandomTextMessage(
      number: chatBotCall1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-49),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    chatBotCall1.TextMessages = new List<TextMessage>()
    { txt1, txt2 };

    Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
      number: chatBotCall1.Mobile,
      sent: DateTimeOffset.Now.AddHours(-49),
      modifiedByUserId: CallSeeder.CHATBOT_API_USER_ID);

    chatBotCall1.Calls = new List<Call>() { call1 };

    DatabaseContext.Referrals.Add(chatBotCall1);
    DatabaseContext.SaveChanges();
  }

  private static void AddRmcCallVulnerable()
  {
    Referral rmcCallVulnerable = RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.RmcCall,
      isVulnerable: true,
      vulnerableDescription: "Vulnerable");

    DatabaseContext.Referrals.Add(rmcCallVulnerable);
    DatabaseContext.SaveChanges();
  }

  private static void AddRmcCallTwoTextMessagesOneCall()
  {
    Referral rmcCallTwoTextMessagesOneCall =RandomEntityCreator.CreateRandomReferral(
      serviceId: $"{SeededServiceIdPrefix}0000000001",
      status: ReferralStatus.RmcCall);

    TextMessage txt1 = RandomEntityCreator.CreateRandomTextMessage(
      number: rmcCallTwoTextMessagesOneCall.Mobile,
      sent: DateTimeOffset.Now.AddHours(-49),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    TextMessage txt2 = RandomEntityCreator.CreateRandomTextMessage(
      number: rmcCallTwoTextMessagesOneCall.Mobile,
      sent: DateTimeOffset.Now.AddHours(-1),
      modifiedByUserId: TextMessageSeeder.GOVUKNOTIFY_API_USER_ID);

    rmcCallTwoTextMessagesOneCall.TextMessages = new List<TextMessage>()
    { txt1, txt2 };

    Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
      called: DateTimeOffset.Now.AddHours(-40),
      number: rmcCallTwoTextMessagesOneCall.Mobile,
      outcome: ChatBotCallOutcome.HungUp.ToString(),
      sent: DateTimeOffset.Now.AddHours(-49),
      modifiedByUserId: CallSeeder.CHATBOT_API_USER_ID);

    rmcCallTwoTextMessagesOneCall.Calls = new List<Call>()
    { call1 };

    DatabaseContext.Referrals.Add(rmcCallTwoTextMessagesOneCall);
    DatabaseContext.SaveChanges();
  }
}
