using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Models.Tracing;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using Provider = WmsHub.Business.Entities.Provider;
using ProviderSubmission = WmsHub.Business.Entities.ProviderSubmission;
using Referral = WmsHub.Business.Entities.Referral;
using ReferralAudit = WmsHub.Business.Entities.ReferralAudit;

namespace WmsHub.Business.Tests.Services;

public class BusinessIntelligenceFixture
{
  public BusinessIntelligenceFixture()
  {
    EnvironmentVariableConfigurator.ConfigureEnvironmentVariablesForAlwaysEncrypted();
    Context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options);
  }

  public DatabaseContext Context { get; }
}

public class BusinessIntelligenceServiceTests
  : ServiceTestsBase, IClassFixture<BusinessIntelligenceFixture>, IDisposable
{
  protected DatabaseContext _context;

  private readonly Mock<IOptions<BusinessIntelligenceOptions>> _mockOptions = new();
  private readonly Mock<BusinessIntelligenceOptions> _mockOptionsValues = new();

  public BusinessIntelligenceServiceTests(BusinessIntelligenceFixture businessIntelligenceFixture)
  {
    _context ??= businessIntelligenceFixture.Context;

    _mockOptionsValues
      .Setup(t => t.ProviderSubmissionEndedStatusesValue)
      .Returns($"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.ProviderDeclinedByServiceUser}," +
        $"{ReferralStatus.ProviderTerminated}");

    _mockOptions
      .Setup(t => t.Value)
      .Returns(_mockOptionsValues.Object);

    CleanUp();
  }

  public void CleanUp()
  {
    _context.Providers.RemoveRange(_context.Providers);
    _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
    _context.Questionnaires.RemoveRange(_context.Questionnaires);
    _context.ReferralQuestionnaires.RemoveRange(_context.ReferralQuestionnaires);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.ReferralsAudit.RemoveRange(_context.ReferralsAudit);
    _context.SaveChanges();
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    CleanUp();
  }

  protected async Task AddTestReferrals(int offsetOne, int offsetTwo)
  {
    await AddTestReferrals(offsetOne, offsetTwo, "Not Vulnerable", "Vulnerable");
  }

  protected async Task AddTestReferralsStatusReason(
    int offsetOne,
    int offsetTwo,
    string reason)
  {
    await AddTestReferrals(offsetOne, offsetTwo, "Not Vulnerable", "Vulnerable", reason);
  }

  protected async Task AddTestReferrals(
    int offsetOne,
    int offsetTwo,
    string vulnerableDescriptionOne,
    string vulnerableDescriptionTwo,
    string reason = null)
  {
    // Add entities.
    Guid providerId1 = Guid.NewGuid();

    ProviderSubmission providerSub1 = new()
    {
      Coaching = 5,
      Date = DateTime.Now.AddDays(-28),
      Measure = 1,
      Weight = 87,
      ProviderId = providerId1
    };
    ProviderSubmission providerSub2 = new()
    {
      Coaching = 6,
      Date = DateTime.Now.AddDays(-14),
      Measure = 7,
      Weight = 97,
      ProviderId = providerId1
    };

    List<ProviderSubmission> listProviderSubs1 = new()
    {
      providerSub1,
      providerSub2
    };

    Provider provider1 = RandomEntityCreator.CreateRandomProvider(
      id: providerId1,
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Provider One");
    provider1.ProviderSubmissions = listProviderSubs1;

    Guid providerId2 = Guid.NewGuid();

    ProviderSubmission providerSub3 =
      new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId2
      };

    ProviderSubmission providerSub4 =
      new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId2
      };

    List<ProviderSubmission> listProviderSubs2 =
      new()
      {
        providerSub3,
        providerSub4
      };

    Provider provider2 = RandomEntityCreator.CreateRandomProvider(
      id: providerId2,
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Provider Two");
    provider1.ProviderSubmissions = listProviderSubs2;

    _context.Providers.Add(provider1);
    _context.Providers.Add(provider2);

    Referral referral1 = new()
    {
      CalculatedBmiAtRegistration = 30m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-50),
      DateOfReferral = DateTime.Now.AddDays(offsetOne),
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "M11111",
      Sex = "Male",
      Status = "ReferralStatus",
      StatusReason = reason,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = "bb5eba8e-b6d9-47b5-9bb7-2bc36f0d394b",
      VulnerableDescription = vulnerableDescriptionOne,
      WeightKg = 170m,
      Address1 = "Another Address1",
      Address2 = "Another Address2",
      FamilyName = "User 98",
      ProviderId = providerId1,
      ReferringGpPracticeName = "Referrer One",
      Deprivation = "IMD2"
    };

    Referral referral2 = new()
    {
      CalculatedBmiAtRegistration = 33m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-47),
      DateOfReferral = DateTime.Now.AddDays(offsetTwo),
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "J62344",
      Sex = "Female",
      Status = "ReferralStatus",
      StatusReason = reason,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = "cc5eba9f-c6d9-47j3-1tt3-3rt26f0d763e",
      VulnerableDescription = vulnerableDescriptionTwo,
      WeightKg = 160m,
      Address1 = "Another Address1",
      Address2 = "Address2",
      FamilyName = "User 122",
      ProviderId = providerId2,
      ReferringGpPracticeName = "Referrer Two",
      Deprivation = "IMD4"
    };

    _context.Referrals.Add(referral1);
    _context.Referrals.Add(referral2);

    await _context.SaveChangesAsync();
  }

  protected async Task AddTestReferralsWithHistory()
  {
    // Add entities.
    Guid providerId1 = Guid.NewGuid();
    DateTimeOffset startDate = DateTimeOffset.Now.AddMonths(-6);
    Random rnd = new();

    ProviderSubmission providerSub1 = new()
    {
      Coaching = 5,
      Date = DateTime.Now.AddDays(-28),
      Measure = 1,
      Weight = 87,
      ProviderId = providerId1
    };
    ProviderSubmission providerSub2 = new()
    {
      Coaching = 6,
      Date = DateTime.Now.AddDays(-14),
      Measure = 7,
      Weight = 97,
      ProviderId = providerId1
    };

    List<ProviderSubmission> listProviderSubs1 = new()
    {
      providerSub1,
      providerSub2
    };

    Provider provider1 = RandomEntityCreator.CreateRandomProvider(
      id: providerId1,
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Provider One");
    provider1.ProviderSubmissions = listProviderSubs1;

    Guid providerId2 = Guid.NewGuid();

    ProviderSubmission providerSub3 = new()
    {
      Coaching = 5,
      Date = DateTime.Now.AddDays(-28),
      Measure = 1,
      Weight = 87,
      ProviderId = providerId2
    };

    ProviderSubmission providerSub4 = new()
    {
      Coaching = 6,
      Date = DateTime.Now.AddDays(-14),
      Measure = 7,
      Weight = 97,
      ProviderId = providerId2
    };

    List<ProviderSubmission> listProviderSubs2 = new()
    {
      providerSub3,
      providerSub4
    };

    Provider provider2 = RandomEntityCreator.CreateRandomProvider(
      id: providerId2,
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Provider Two");
    provider1.ProviderSubmissions = listProviderSubs2;

    _context.Providers.Add(provider1);
    _context.Providers.Add(provider2);

    Referral referral1 = new()
    {
      CalculatedBmiAtRegistration = 30m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-50),
      DateOfReferral = startDate,
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = startDate,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "M11111",
      Sex = "Male",
      Status = ReferralStatus.New.ToString(),
      StatusReason = null,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = Generators.GenerateUbrn(rnd),
      VulnerableDescription = "Not vulnerable",
      WeightKg = 170m,
      Address1 = "Another Address1",
      Address2 = "Another Address2",
      FamilyName = "User 98",
      ProviderId = providerId1,
      ReferringGpPracticeName = "Referrer One",
      Deprivation = "IMD2"
    };

    Referral referral2 = new()
    {
      CalculatedBmiAtRegistration = 33m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-47),
      DateOfReferral = startDate,
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = startDate,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "J62344",
      Sex = "Female",
      Status = ReferralStatus.Exception.ToString(),
      StatusReason = null,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = Generators.GenerateUbrn(rnd),
      VulnerableDescription = "Not Vulnerable",
      WeightKg = 160m,
      Address1 = "Address1",
      Address2 = "Address2",
      FamilyName = "User 122",
      ProviderId = providerId2,
      ReferringGpPracticeName = "Referrer Two",
      Deprivation = "IMD4"
    };

    _context.Referrals.Add(referral1);
    _context.Referrals.Add(referral2);

    await _context.SaveChangesAsync();

    // Add Provider Rejected.
    Referral entity_1_a =
      _context.Referrals.SingleOrDefault(t => t.Id == referral1.Id);

    entity_1_a.Status = ReferralStatus.ProviderRejected.ToString();
    entity_1_a.StatusReason = "Provider Rejected";
    entity_1_a.ModifiedAt = DateTimeOffset.Now.AddMonths(-5);
    entity_1_a.ModifiedByUserId = entity_1_a.ProviderId.Value;
    await _context.SaveChangesAsync();

    // Add Rejected To EReferrals.
    Referral entity_1_b =
      _context.Referrals.SingleOrDefault(t => t.Id == referral1.Id);
    entity_1_b.Status = ReferralStatus.RejectedToEreferrals.ToString();
    entity_1_b.StatusReason = "Rejected to EReferrals";
    entity_1_b.ModifiedAt = DateTimeOffset.Now.AddMonths(-4);
    entity_1_b.ModifiedByUserId = entity_1_b.ProviderId.Value;
    await _context.SaveChangesAsync();

    // Add back into system.
    Referral referral1_c = new()
    {
      CalculatedBmiAtRegistration = 30m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-50),
      DateOfReferral = DateTimeOffset.Now,
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "M11111",
      Sex = "Male",
      Status = ReferralStatus.New.ToString(),
      StatusReason = null,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = entity_1_b.Ubrn,
      VulnerableDescription = "Not vulnerable",
      WeightKg = 170m,
      Address1 = "Another Address1",
      Address2 = "Another Address2",
      FamilyName = "User 98",
      ProviderId = providerId1,
      ReferringGpPracticeName = "Referrer One",
      Deprivation = "IMD2"
    };

    _context.Referrals.Add(referral1_c);
    await _context.SaveChangesAsync();

    // Add Rejected To EReferrals.
    Referral entity_2_a =
      _context.Referrals.SingleOrDefault(t => t.Id == referral2.Id);
    entity_2_a.Status = ReferralStatus.RejectedToEreferrals.ToString();
    entity_2_a.StatusReason = "Rejected to EReferrals";
    entity_2_a.ModifiedAt = DateTimeOffset.Now.AddMonths(-4);
    entity_2_a.ModifiedByUserId = entity_2_a.ProviderId.Value;
    await _context.SaveChangesAsync();

    // Add back into system.
    Referral referral2_b = new()
    {
      CalculatedBmiAtRegistration = 33m,
      ConsentForFutureContactForEvaluation = false,
      DateOfBirth = DateTime.Now.AddYears(-47),
      DateOfReferral = DateTimeOffset.Now,
      Ethnicity = "White",
      HasDiabetesType1 = true,
      HasDiabetesType2 = false,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 150m,
      IsActive = true,
      IsVulnerable = false,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      ReferringGpPracticeNumber = "J62344",
      Sex = "Female",
      Status = ReferralStatus.New.ToString(),
      StatusReason = null,
      TriagedCompletionLevel = null,
      TriagedWeightedLevel = null,
      Ubrn = entity_2_a.Ubrn,
      VulnerableDescription = "Not Vulnerable",
      WeightKg = 160m,
      Address1 = "Address1",
      Address2 = "Address2",
      FamilyName = "User 122",
      ProviderId = providerId2,
      ReferringGpPracticeName = "Referrer Two",
      Deprivation = "IMD4"
    };

    _context.Referrals.Add(referral2_b);
    await _context.SaveChangesAsync();
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetAnonymisedReferrals : BusinessIntelligenceServiceTests
  {
    private const int OFFSET_ONE = -90;
    private const int OFFSET_TWO = -50;
    protected IBusinessIntelligenceService _service;
    public GetAnonymisedReferrals(BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task NoDates()
    {
      // Arrange.
      const int EXPECTED_COUNT = 2;

      await AddTestReferrals(OFFSET_ONE, OFFSET_TWO);

      // Act.
      IEnumerable<AnonymisedReferral> result = await _service.GetAnonymisedReferrals();

      // Assert.
      result.Should().NotBeNull().And.HaveCount(EXPECTED_COUNT);
    }

    [Fact]
    public async Task WithFromDate()
    {
      // Arrange.
      DateTimeOffset? fromDateTime = DateTime.Now.AddDays(OFFSET_ONE + 10);
      DateTimeOffset expectedDate = DateTime.Now.AddDays(OFFSET_TWO);
      await AddTestReferrals(OFFSET_ONE, OFFSET_TWO);

      // Act.
      IEnumerable<AnonymisedReferral> result = await _service.GetAnonymisedReferrals(fromDateTime);

      // Assert.
      result.Should().ContainSingle().Which.DateOfReferral.Should().BeAfter(expectedDate);
    }

    [Fact]
    public async Task WithToDate()
    {
      // Arrange.
      const int OFFSET_DAYS = -90;
      DateTimeOffset? fromDateTime = null;
      DateTimeOffset? toDateTime = DateTime.Now.AddDays(OFFSET_DAYS + 10);
      DateTimeOffset expectedDate = DateTime.Now.AddDays(OFFSET_DAYS);
      await AddTestReferrals(OFFSET_DAYS, OFFSET_DAYS + 20);

      // Act.
      IEnumerable<AnonymisedReferral> result = await _service
        .GetAnonymisedReferrals(fromDateTime, toDateTime);

      // Assert.
      result.Should().ContainSingle().Which.DateOfReferral.Should().BeAfter(expectedDate);
    }

    [Fact]
    public async Task WithBothDates()
    {
      // Arrange.
      DateTimeOffset? fromDateTime = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset? toDateTime = DateTime.Now.AddDays(OFFSET_TWO + 10);
      DateTimeOffset expectedDate1 = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset expectedDate2 = DateTime.Now.AddDays(OFFSET_TWO + 10);
      const int EXPECTED_COUNT = 2;

      await AddTestReferrals(OFFSET_ONE, OFFSET_TWO);

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

      // Assert.
      result.Should().HaveCount(EXPECTED_COUNT);
      result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
      result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);
    }

    [Fact]
    public async Task Null_andEmpty_VulnerableDescription()
    {
      // Arrange.
      DateTimeOffset? fromDateTime = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset? toDateTime = DateTime.Now.AddDays(OFFSET_TWO + 10);
      DateTimeOffset expectedDate1 = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset expectedDate2 = DateTime.Now.AddDays(OFFSET_TWO + 10);
      const int EXPECTED_COUNT = 2;

      await AddTestReferrals(OFFSET_ONE, OFFSET_TWO, null, string.Empty);

      // Act.
      IEnumerable<AnonymisedReferral> result = await _service
        .GetAnonymisedReferrals(fromDateTime, toDateTime);

      // Assert.
      result.Should().HaveCount(EXPECTED_COUNT);
      result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
      result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);
      result.ElementAt(0).VulnerableDescription.Should().BeNull();
      result.ElementAt(1).VulnerableDescription.Should().BeNull();
    }

    [Fact]
    public async Task Null_And_Value_VulnerableDescription()
    {
      // Arrange.
      const string EXPECTED_VULNERABLE = "Test Vulnerable";
      DateTimeOffset? fromDateTime = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset? toDateTime = DateTime.Now.AddDays(OFFSET_TWO + 10);
      DateTimeOffset expectedDate1 = DateTime.Now.AddDays(OFFSET_ONE - 10);
      DateTimeOffset expectedDate2 = DateTime.Now.AddDays(OFFSET_TWO + 10);
      const int EXPECTED_COUNT = 2;
      await AddTestReferrals(OFFSET_ONE, OFFSET_TWO, null, EXPECTED_VULNERABLE);

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

      // Assert.
      result.Should().HaveCount(EXPECTED_COUNT);
      result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
      result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
      result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);
      result.ElementAt(0).VulnerableDescription.Should().BeNull();
      result.ElementAt(1).VulnerableDescription.Should().Be(EXPECTED_VULNERABLE);
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetAnonymisedReferralsByProviderReasonTests :
    BusinessIntelligenceServiceTests, IDisposable
  {
    private readonly BusinessIntelligenceService _service;

    public GetAnonymisedReferralsByProviderReasonTests(
      BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task InvalidSexMapsAsNull()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        sex: "Invalid",
        status: ReferralStatus.ProviderRejected);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> anonymisedReferral =
        await _service.GetAnonymisedReferralsByProviderReason(
          referral.DateOfReferral.Value.AddDays(-2),
          referral.DateOfReferral.Value.AddDays(2),
          ReferralStatus.ProviderRejected);

      // Assert.
      anonymisedReferral.Single().Sex.Should().BeNull();
    }

    [Fact]
    public async Task ValidSexMapsCorrectly()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.ProviderRejected);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> anonymisedReferral =
        await _service.GetAnonymisedReferralsByProviderReason(
          referral.DateOfReferral.Value.AddDays(-2),
          referral.DateOfReferral.Value.AddDays(2),
          ReferralStatus.ProviderRejected);

      // Assert.
      anonymisedReferral.Single().Sex.Should().Be(referral.Sex);
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetAnonymisedReferralsForUbrnTests :
    BusinessIntelligenceServiceTests, IDisposable
  {
    private readonly BusinessIntelligenceService _service;
    public GetAnonymisedReferralsForUbrnTests(
      BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task InvalidSexMapsAsNull()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(sex: "Invalid");
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> anonymisedReferral =
        await _service.GetAnonymisedReferralsForUbrn(referral.Ubrn);

      // Assert.
      anonymisedReferral.Single().Sex.Should().BeNull();
    }

    [Fact]
    public async Task ValidSexMapsCorrectly()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> anonymisedReferral = 
        await _service.GetAnonymisedReferralsForUbrn(referral.Ubrn);

      // Assert.
      anonymisedReferral.Single().Sex.Should().Be(referral.Sex);
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetAnonymisedReprocessedReferralsBySubmissionDateTests :
    BusinessIntelligenceServiceTests, IDisposable
  {
    private readonly string _testMessage = "TEST MESSAGE";
    protected IBusinessIntelligenceService _service;

    public GetAnonymisedReprocessedReferralsBySubmissionDateTests(
      BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      DateTimeOffset from = DateTimeOffset.Now.AddMonths(-6);
      DateTimeOffset to = DateTimeOffset.Now;

      await AddTestReferralsWithHistory();

      // Act.
      IEnumerable<ReprocessedReferral> result = await _service
        .GetAnonymisedReprocessedReferralsBySubmissionDate(from, to);
      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
      }
    }

    [Theory]
    [MemberData(nameof(AuditStatuses))]
    public async Task InitialStatus_Expected_counts(string id, ReferralAuditData auditStatuses)
    {
      // Arrange.
      Guid referralId = Guid.Parse(id);
      dynamic expectedStatus = await TestSetup(referralId, auditStatuses);
      DateTimeOffset from = DateTimeOffset.Now.AddMonths(-6);
      DateTimeOffset to = DateTimeOffset.Now;

      // Act.
      IEnumerable<ReprocessedReferral> results = await _service
        .GetAnonymisedReprocessedReferralsBySubmissionDate(from, to);

      int reproCount = results.Count(t => t.Reprocessed);
      int SuccessfullyRepro = results.Count(t => t.SuccessfullyReprocessed);
      int uncancelledCount = results.Count(t => t.Uncancelled);
      int cancelledCount = results.Count(t => t.CurrentlyCancelled);

      // Assert.
      using (new AssertionScope())
      {
        results.Should().NotBeNullOrEmpty();
        foreach (ReprocessedReferral result in results)
        {
          List<ReferralAudit> referralAudits = _context.ReferralsAudit
            .Where(t => t.Ubrn == result.Ubrn)
            .OrderBy(t => t.ModifiedAt).ToList();

          ReferralAudit firstAudit = referralAudits.First();
          List<string> statusSteps =
            referralAudits.Select(t => t.Status).ToList();

          // InitialStatus.
          result.InitialStatus.Should()
            .BeOneOf("New", "RmcCall", "Exception");

          // InitialStatusReason if InitialStatus Exception.
          result.InitialStatusReason.Should().Be(firstAudit.StatusReason);
          result.Reprocessed.Should().Be(
            expectedStatus.Reprocessed,
            $"Error with ID {firstAudit.Id}-{result.StatusCsv}.");
          result.SuccessfullyReprocessed.Should().Be(
            expectedStatus.SuccessfullyReprocessed,
            $"Error with ID {firstAudit.Id}-{result.StatusCsv}.");
          result.Uncancelled.Should().Be(
            expectedStatus.Uncancelled,
            $"Error with ID {firstAudit.Id}-{result.StatusCsv}.");
          result.CurrentlyCancelled.Should().Be(
            expectedStatus.CurrentlyCancelled,
            $"Error with ID {firstAudit.Id}-{result.StatusCsv}.");

          if (result.CurrentlyCancelled)
          {
            referralAudits.Last().StatusReason.Should().Be("TestCancelled");
            result.CurrentlyCancelledStatusReason
              .Should().Be("TestCancelled");
          }
          else if (referralAudits.Last().Status ==
                   ReferralStatus.Exception.ToString())
          {
            referralAudits.Last().StatusReason.Should().Be("TestException");
          }
          else
          {
            referralAudits.Last().StatusReason.Should().BeNullOrWhiteSpace();
            result.CurrentlyCancelledStatusReason
              .Should().BeNullOrWhiteSpace();
          }
        }
      }
    }

    private async Task<ExpectedStatus> TestSetup(
      Guid referralId, dynamic data)
    {
      _context.ReferralsAudit.RemoveRange(_context.ReferralsAudit);
      await _context.SaveChangesAsync();

      Random rnd = new();
      string[] status = (string[])data.Status;
      int modified = (int)data.Modified;

      DateTimeOffset dateOfReferral =
        DateTimeOffset.UtcNow.AddDays(-modified);
      string ubrm = Generators.GenerateUbrn(rnd);
      string address1 = Generators.GenerateAddress1(rnd);
      string address2 = Generators.GenerateAddress1(rnd);
      string address3 = Generators.GenerateAddress1(rnd);
      string fname = Generators.GenerateName(rnd, 7);
      string lname = Generators.GenerateName(rnd, 10);

      for (int i = 0; i < status.Length; i++)
      {
        DateTimeOffset modifiedAt =
          DateTimeOffset.UtcNow.AddDays(-modified);
        modified--;

        _context.ReferralsAudit.Add(new ReferralAudit
        {
          Id = referralId,
          Ubrn = ubrm,
          CreatedDate = dateOfReferral,
          ModifiedAt = modifiedAt,
          ProviderId = Guid.Empty,
          DateOfReferral = dateOfReferral,
          Status = status[i],
          FamilyName = lname,
          GivenName = fname,
          Address1 = address1,
          Address2 = address2,
          Address3 = address3,
          AuditAction = i == 0 ? "Insert" : "Update",
          AuditDuration = 11,
          AuditSuccess = true,
          CalculatedBmiAtRegistration = 33m,
          CreatedByUserId = "test",
          AuditResult = 1,
          StatusReason =
            status[i] == ReferralStatus.Exception.ToString()
              ? "TestException"
              :
              status[i] == ReferralStatus.CancelledByEreferrals.ToString()
                ?
                "TestCancelled"
                : "",
          DelayReason = status[i] == "RmcDelayed" ? "TestDelay" : "",
          ReferringGpPracticeNumber = "Test Practice",
          AuditErrorMessage = _testMessage,
          IsActive = true,
          ReferralSource = ReferralSource.GpReferral.ToString()
        });
        await _context.SaveChangesAsync();
      }

      return (ExpectedStatus)data.Expected;
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetQuestionnaires : BusinessIntelligenceServiceTests
  {
    protected IBusinessIntelligenceService _service;
    public GetQuestionnaires(BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task GetQuestionnaire()
    {
      // Arrange.
      Dictionary<QuestionnaireType, Guid> questionnaireGuids = AddRandomQuestionnairesInDatabase();
      Dictionary<Guid, QuestionnaireType> referralGuids = AddRandomReferralsInDatabase();
      AddReferralQuestionnaires(questionnaireGuids, referralGuids);

      // Act.
      List<BiQuestionnaire> questionnaires = await _service
        .GetQuestionnaires(DateTimeOffset.Now.Date.AddDays(-30), DateTimeOffset.Now);

      // Assert.
      questionnaires.Should().BeOfType<List<BiQuestionnaire>>()
        .Which.Count().Should().Be(5);
    }

    protected Dictionary<QuestionnaireType, Guid> 
      AddRandomQuestionnairesInDatabase()
    {
      Dictionary<QuestionnaireType, Guid> guids = new ();
      List<QuestionnaireType> questionnaireTypes = new ()
      {
        { QuestionnaireType.CompleteProT1 },
        { QuestionnaireType.CompleteProT2and3 },
        { QuestionnaireType.CompleteSelfT1 },
        { QuestionnaireType.CompleteSelfT2and3 },
        { QuestionnaireType.NotCompleteProT1and2and3 },
        { QuestionnaireType.NotCompleteSelfT1and2and3 }
      };

      foreach (QuestionnaireType type in questionnaireTypes)
      {
        Guid guid = Guid.NewGuid();

        guids.Add(type, guid);
        
        _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
          type: type,
          id: guid));
      }

      _context.SaveChanges();

      return guids;
    }

    protected Dictionary<Guid, QuestionnaireType> 
      AddRandomReferralsInDatabase()
    {
      Dictionary<Guid, QuestionnaireType> guids = new();

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteProT1);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "07595470000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-70)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteProT2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000000001",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Medium.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-54)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteProT2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000000002",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.High.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-22)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteSelfT1);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000000004",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.SelfReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-35)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteSelfT2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000004292",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.SelfReferral,
        triagedCompletionLevel: TriageLevel.Medium.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-4)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.CompleteSelfT2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447700900003",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.SelfReferral,
        triagedCompletionLevel: TriageLevel.High.ToString(),
        dateOfReferral: DateTimeOffset.Now));

      guids.Add(Guid.NewGuid(), QuestionnaireType.NotCompleteProT1and2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000005001",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Medium.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-10)));

      guids.Add(Guid.NewGuid(), QuestionnaireType.NotCompleteSelfT1and2and3);
      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: guids.Keys.Last(),
        mobile: "+447000004291",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
        referralSource: ReferralSource.SelfReferral,
        triagedCompletionLevel: TriageLevel.High.ToString(),
        dateOfReferral: DateTimeOffset.Now.AddDays(-15)));

      _context.SaveChanges();

      return guids;
    }

    protected void AddReferralQuestionnaires(
      Dictionary<QuestionnaireType, Guid> questionnaireGuids,
      Dictionary<Guid, QuestionnaireType> referrals)
    {
      foreach(Guid guid in referrals.Keys)
      {
        _context.ReferralQuestionnaires.Add(
          RandomEntityCreator.CreateRandomReferralQuestionnaire(
            id: Guid.NewGuid(),
            referralId: guid,
            questionnaireId: 
              questionnaireGuids[referrals[guid]],
            notificationKey: "NotificationKey",
            status: ReferralQuestionnaireStatus.Created));
      }

      _context.SaveChanges();
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetRmcUsersInformationTests : BusinessIntelligenceServiceTests
  {
    protected IBusinessIntelligenceService _service;
    private int _actionCount = 0;
    public GetRmcUsersInformationTests(BusinessIntelligenceFixture businessIntelligenceFixture) 
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
          cfg.AddMaps(new[] { "WmsHub.Business" }))
          .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    private async Task SetupAsync(string queryString = null)
    {
      string[] actions = new string[]
      {
        "AddToRmcCallList",
        "ConfirmDelay",
        "ConfirmEmail",
        "ConfirmEthnicity",
        "ConfirmProvider",
        "EmailProviderListToServiceUser",
        "ForwardAllProviderDetails",
        "ForwardProviderDetails",
        "ReferralView",
        "RejectToEreferrals",
        "SelectProvider",
        "UnableToContact",
        "UpdateDateOfBirth",
        "UpdateMobileNumber"
      };
      _actionCount = actions.Length;
      Entities.UserStore user = RandomEntityCreator.CreateUserStore();
      List<Entities.UserStore> userStores = new() { user };
      List<Entities.UserActionLog> userActionLogs = new();
      for (int i = 1; i <= actions.Length; i++)
      {
        userActionLogs.Add(
          RandomEntityCreator.CreateUserActionLog(
            id: i,
            action: actions[i - 1],
            userId: user.Id,
            queryString: queryString ?? $"Ubrn=GP7400000009" +
            $"&ReferralId={Guid.NewGuid}" +
            $"&ProviderId={Guid.NewGuid}" +
            $"&DelayReason=Test_Delay_reason" +
            $"&StatusReason="
          )
        );
      }

      _context.UsersStore.RemoveRange(_context.UsersStore);
      _context.UserActionLogs.RemoveRange(_context.UserActionLogs);
      await _context.SaveChangesAsync();

      await _context.UsersStore.AddRangeAsync(userStores);
      await _context.UserActionLogs.AddRangeAsync(userActionLogs);
      await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetValid()
    {
      // Arrange.
      await SetupAsync();
      DateTime start = DateTime.Now.AddDays(-10);
      DateTime end = DateTime.Now;

      // Act.     
      IEnumerable<Business.Models.BusinessIntelligence.BiRmcUserInformation>
        result = await _service.GetRmcUsersInformation(start, end);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Should().HaveCount(_actionCount);
        result.Count(t => string.IsNullOrWhiteSpace(t.Ubrn)).Should().Be(0);
      }
    }

    [Fact]
    public async Task Get_QueryString_Missing_Ubrn_Null_Values()
    {
      // Arrange.
      string queryString = $"ReferralId={Guid.NewGuid}" +
        $"&ProviderId={Guid.NewGuid}" +
        $"&DelayReason=Test_Delay_reason" +
        $"&StatusReason=";
      await SetupAsync(queryString: queryString);
      DateTime start = DateTime.Now.AddDays(-10);
      DateTime end = DateTime.Now;

      // Act.     
      IEnumerable<Business.Models.BusinessIntelligence.BiRmcUserInformation>
        result = await _service.GetRmcUsersInformation(start, end);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Should().HaveCount(_actionCount);
        result.Count(t => string.IsNullOrWhiteSpace(t.Ubrn))
          .Should().Be(_actionCount);
      }
    }

    [Fact]
    public async Task Get_QueryString_Ubrn_Empty_Null_Values()
    {
      // Arrange.
      string queryString = $"Ubrn=&ReferralId={Guid.NewGuid}" +
        $"&ProviderId={Guid.NewGuid}" +
        $"&DelayReason=Test_Delay_reason" +
        $"&StatusReason=";
      await SetupAsync(queryString: queryString);
      DateTime start = DateTime.Now.AddDays(-10);
      DateTime end = DateTime.Now;

      // Act.     
      IEnumerable<Business.Models.BusinessIntelligence.BiRmcUserInformation>
        result = await _service.GetRmcUsersInformation(start, end);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Should().HaveCount(_actionCount);
        result.Count(t => string.IsNullOrWhiteSpace(t.Ubrn))
          .Should().Be(_actionCount);
      }
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetTraceIssueReferrals : BusinessIntelligenceServiceTests
  {
    protected IBusinessIntelligenceService _service;
    public GetTraceIssueReferrals(BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.ReferralsAudit.RemoveRange(_context.ReferralsAudit);
      _context.SaveChanges();

      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Fact]
    public async Task MultipleDistinctStatusUpdates_ReturnsTraceIssueReferral()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string gpOdsCode = "M12345";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
          id: referralId,
          isActive: true,
          referringGpPracticeNumber: gpOdsCode);
      _context.Referrals.Add(referral);
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-5));
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.UnableToDischarge,
        DateTimeOffset.Now.AddDays(-4));
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-3));

      // Act.
      IEnumerable<TraceIssueReferral> result =
        await _service.GetTraceIssueReferralsAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().HaveCount(1);
        result.Should().ContainEquivalentOf(
          new TraceIssueReferral()
          {
            Id = referralId,
            ReferringGpPracticeNumber = gpOdsCode,
          });
      }
    }

    [Fact]
    public async Task MultipleAuditEntries_NoIntermediateStatus_ReturnsEmpty()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string gpOdsCode = "M12345";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
          id: referralId,
          isActive: true,
          referringGpPracticeNumber: gpOdsCode);
      _context.Referrals.Add(referral);
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-5));
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-3));

      // Act.
      IEnumerable<TraceIssueReferral> result =
        await _service.GetTraceIssueReferralsAsync();

      // Assert.
      result.Should().HaveCount(0);
    }

    [Fact]
    public async Task SingleAuditEntry_ReturnsEmpty()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string gpOdsCode = "M12345";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
          id: referralId,
          isActive: true,
          referringGpPracticeNumber: gpOdsCode);
      _context.Referrals.Add(referral);
      await AddReferralAuditEntry(
        referral,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-3));

      // Act.
      IEnumerable<TraceIssueReferral> result =
        await _service.GetTraceIssueReferralsAsync();

      // Assert.
      result.Should().HaveCount(0);
    }

    [Fact]
    public async Task
      MultipleAuditEntries_IntermediateEntryDifferentId_ReturnsEmpty()
    {
      // Arrange.
      Guid referralId1 = Guid.NewGuid();
      Guid referralId2 = Guid.NewGuid();
      string gpOdsCode = "M12345";
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
          id: referralId1,
          isActive: true,
          referringGpPracticeNumber: gpOdsCode);
      _context.Referrals.Add(referral1);
      Referral referral2 = RandomEntityCreator.CreateRandomReferral(
          id: referralId2,
          isActive: true,
          referringGpPracticeNumber: gpOdsCode);
      await AddReferralAuditEntry(
        referral1,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-5));
      await AddReferralAuditEntry(
        referral2,
        ReferralStatus.UnableToDischarge,
        DateTimeOffset.Now.AddDays(-4));
      await AddReferralAuditEntry(
        referral1,
        ReferralStatus.DischargeAwaitingTrace,
        DateTimeOffset.Now.AddDays(-3));

      // Act.
      IEnumerable<TraceIssueReferral> result =
        await _service.GetTraceIssueReferralsAsync();

      // Assert.
      result.Should().HaveCount(0);
    }

    private async Task AddReferralAuditEntry(
      Referral referral,
      ReferralStatus status,
      DateTimeOffset modifiedAt)
    {
      _context.ReferralsAudit.Add(
        new ReferralAudit()
        {
          Id = referral.Id,
          Status = status.ToString(),
          ModifiedAt = modifiedAt
        });
      await _context.SaveChangesAsync();
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class GetUntracedNhsNumbers : BusinessIntelligenceServiceTests
  {
    protected IBusinessIntelligenceService _service;
    public GetUntracedNhsNumbers(BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
      _context.SaveChanges();
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData),
      new ReferralSource[] { ReferralSource.GeneralReferral })]
    public async Task Valid(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: referralSource);
      Referral inactiveReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: false,
        referralSource: referralSource);
      Referral nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: referralSource);
      Referral nullReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: referralSource);
      Referral emptyReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: referralSource);
      Referral unknownReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: referralSource);
      nullNhsNoReferral.NhsNumber = null;
      nullNhsNoReferral.LastTraceDate = null;
      nullReferringGpPracticeNoReferral.ReferringGpPracticeNumber = null;
      nullReferringGpPracticeNoReferral.LastTraceDate = null;
      emptyReferringGpPracticeNoReferral.ReferringGpPracticeNumber = string.Empty;
      emptyReferringGpPracticeNoReferral.LastTraceDate = null;
      unknownReferringGpPracticeNoReferral.ReferringGpPracticeNumber =
        Constants.UNKNOWN_GP_PRACTICE_NUMBER;
      unknownReferringGpPracticeNoReferral.LastTraceDate = null;

      int expectedUntracedNhsNumbersCount = 4;

      _context.Referrals.AddRange(
        referral,
        inactiveReferral,
        nullNhsNoReferral,
        nullReferringGpPracticeNoReferral,
        emptyReferringGpPracticeNoReferral,
        unknownReferringGpPracticeNoReferral
        );
      _context.SaveChanges();

      // Act.
      IEnumerable<NhsNumberTrace> untracedNhsNumbers =
        await _service.GetUntracedNhsNumbers();

      // Assert.
      untracedNhsNumbers.Count().Should()
        .Be(expectedUntracedNhsNumbersCount);
      nullNhsNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == nullNhsNoReferral.Id));
      nullReferringGpPracticeNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == nullReferringGpPracticeNoReferral.Id));
      emptyReferringGpPracticeNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == emptyReferringGpPracticeNoReferral.Id));
      unknownReferringGpPracticeNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == unknownReferringGpPracticeNoReferral.Id));
    }

    [Theory]
    [InlineData(ReferralStatus.AwaitingDischarge, 4)]
    [InlineData(ReferralStatus.CancelledByEreferrals, 0)]
    [InlineData(ReferralStatus.CancelledDueToNonContact, 0)]
    [InlineData(ReferralStatus.CancelledDuplicate, 0)]
    [InlineData(ReferralStatus.CancelledDuplicateTextMessage, 0)]
    [InlineData(ReferralStatus.ChatBotCall1, 4)]
    [InlineData(ReferralStatus.ChatBotTransfer, 4)]
    [InlineData(ReferralStatus.Complete, 0)]
    [InlineData(ReferralStatus.DischargeOnHold, 0)]
    [InlineData(ReferralStatus.DischargeAwaitingTrace, 4)]
    [InlineData(ReferralStatus.Exception, 0)]
    [InlineData(ReferralStatus.FailedToContact, 0)]
    [InlineData(ReferralStatus.FailedToContactTextMessage, 0)]
    [InlineData(ReferralStatus.Letter, 0)]
    [InlineData(ReferralStatus.LetterSent, 0)]
    [InlineData(ReferralStatus.New, 4)]
    [InlineData(ReferralStatus.ProviderAccepted, 4)]
    [InlineData(ReferralStatus.ProviderAwaitingStart, 4)]
    [InlineData(ReferralStatus.ProviderAwaitingTrace, 4)]
    [InlineData(ReferralStatus.ProviderCompleted, 4)]
    [InlineData(ReferralStatus.ProviderContactedServiceUser, 4)]
    [InlineData(ReferralStatus.ProviderDeclinedByServiceUser, 4)]
    [InlineData(ReferralStatus.ProviderDeclinedTextMessage, 4)]
    [InlineData(ReferralStatus.ProviderRejected, 4)]
    [InlineData(ReferralStatus.ProviderRejectedTextMessage, 4)]
    [InlineData(ReferralStatus.ProviderStarted, 4)]
    [InlineData(ReferralStatus.ProviderTerminated, 4)]
    [InlineData(ReferralStatus.ProviderTerminatedTextMessage, 4)]
    [InlineData(ReferralStatus.RejectedToEreferrals, 0)]
    [InlineData(ReferralStatus.RmcCall, 4)]
    [InlineData(ReferralStatus.RmcDelayed, 4)]
    [InlineData(ReferralStatus.TextMessage1, 4)]
    [InlineData(ReferralStatus.TextMessage2, 4)]
    public async Task ValidReferralStatusCanTrace(
      ReferralStatus status,
      int expected)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral inactiveReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: false);
      Referral nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral nullReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral emptyReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral unknownReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      nullNhsNoReferral.NhsNumber = null;
      nullNhsNoReferral.LastTraceDate = null;
      nullNhsNoReferral.Status = status.ToString();
      nullReferringGpPracticeNoReferral.ReferringGpPracticeNumber = null;
      nullReferringGpPracticeNoReferral.LastTraceDate = null;
      nullReferringGpPracticeNoReferral.Status = status.ToString();
      emptyReferringGpPracticeNoReferral.ReferringGpPracticeNumber = string.Empty;
      emptyReferringGpPracticeNoReferral.LastTraceDate = null;
      emptyReferringGpPracticeNoReferral.Status = status.ToString();
      unknownReferringGpPracticeNoReferral.ReferringGpPracticeNumber =
        Constants.UNKNOWN_GP_PRACTICE_NUMBER;
      unknownReferringGpPracticeNoReferral.LastTraceDate = null;
      unknownReferringGpPracticeNoReferral.Status = status.ToString();

      _context.Referrals.AddRange(
        referral,
        inactiveReferral,
        nullNhsNoReferral,
        nullReferringGpPracticeNoReferral,
        emptyReferringGpPracticeNoReferral,
        unknownReferringGpPracticeNoReferral);
      _context.SaveChanges();

      // Act.
      IEnumerable<NhsNumberTrace> untracedNhsNumbers =
        await _service.GetUntracedNhsNumbers();

      // Assert.
      untracedNhsNumbers.Count().Should().Be(expected);
      if (untracedNhsNumbers.Any())
      {
        nullNhsNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .Single(n => n.Id == nullNhsNoReferral.Id));
        nullReferringGpPracticeNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == nullReferringGpPracticeNoReferral.Id));
        emptyReferringGpPracticeNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .Single(n => n.Id == emptyReferringGpPracticeNoReferral.Id));
        unknownReferringGpPracticeNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .Single(n => n.Id == unknownReferringGpPracticeNoReferral.Id));
      }
    }

    [Theory]
    [InlineData(ReferralStatus.AwaitingDischarge, 7, 4)]
    [InlineData(ReferralStatus.ChatBotCall1, 7, 4)]
    [InlineData(ReferralStatus.ChatBotTransfer, 7, 4)]
    [InlineData(ReferralStatus.Complete, 30, 0)]
    [InlineData(ReferralStatus.DischargeAwaitingTrace, 7, 4)]
    [InlineData(ReferralStatus.New, null, 4)]
    [InlineData(ReferralStatus.ProviderAccepted, 7, 4)]
    [InlineData(ReferralStatus.ProviderAwaitingStart, 7, 4)]
    [InlineData(ReferralStatus.ProviderAwaitingTrace, 7, 4)]
    [InlineData(ReferralStatus.ProviderCompleted, 7, 4)]
    [InlineData(ReferralStatus.ProviderContactedServiceUser, 7, 4)]
    [InlineData(ReferralStatus.ProviderDeclinedByServiceUser, 7, 4)]
    [InlineData(ReferralStatus.ProviderDeclinedTextMessage, 7, 4)]
    [InlineData(ReferralStatus.ProviderRejected, 7, 4)]
    [InlineData(ReferralStatus.ProviderRejectedTextMessage, 7, 4)]
    [InlineData(ReferralStatus.ProviderStarted, 7, 4)]
    [InlineData(ReferralStatus.ProviderTerminated, 7, 4)]
    [InlineData(ReferralStatus.ProviderTerminatedTextMessage, 7, 4)]
    [InlineData(ReferralStatus.RmcCall, 7, 4)]
    [InlineData(ReferralStatus.RmcDelayed, 7, 4)]
    [InlineData(ReferralStatus.TextMessage1, 7, 4)]
    [InlineData(ReferralStatus.TextMessage2, 7, 4)]
    [InlineData(ReferralStatus.TextMessage3, 7, 4)]
    public async Task ValidReferralStatusLastTraceDate(
      ReferralStatus status,
      int? noOfDays,
      int expectedCount)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral inactiveReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: false);
      Referral nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral nullReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral emptyReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      Referral unknownReferringGpPracticeNoReferral = RandomEntityCreator.CreateRandomReferral(
        isActive: true);
      nullNhsNoReferral.NhsNumber = null;
      nullNhsNoReferral.LastTraceDate = noOfDays == null
        ? null
        : DateTimeOffset.Now.AddDays(-noOfDays.Value);
      nullNhsNoReferral.Status = status.ToString();
      nullReferringGpPracticeNoReferral.ReferringGpPracticeNumber = null;
      nullReferringGpPracticeNoReferral.LastTraceDate = noOfDays == null
        ? null
        : DateTimeOffset.Now.AddDays(-noOfDays.Value);
      nullReferringGpPracticeNoReferral.Status = status.ToString();
      emptyReferringGpPracticeNoReferral.ReferringGpPracticeNumber = string.Empty;
      emptyReferringGpPracticeNoReferral.LastTraceDate = noOfDays == null
        ? null
        : DateTimeOffset.Now.AddDays(-noOfDays.Value);
      emptyReferringGpPracticeNoReferral.Status = status.ToString();
      unknownReferringGpPracticeNoReferral.ReferringGpPracticeNumber =
        Constants.UNKNOWN_GP_PRACTICE_NUMBER;
      unknownReferringGpPracticeNoReferral.LastTraceDate = noOfDays == null
        ? null
        : DateTimeOffset.Now.AddDays(-noOfDays.Value);
      unknownReferringGpPracticeNoReferral.Status = status.ToString();

      _context.Referrals.AddRange(
        referral,
        inactiveReferral,
        nullNhsNoReferral,
        nullReferringGpPracticeNoReferral,
        emptyReferringGpPracticeNoReferral,
        unknownReferringGpPracticeNoReferral);
      _context.SaveChanges();

      // Act.
      IEnumerable<NhsNumberTrace> untracedNhsNumbers = await _service.GetUntracedNhsNumbers();

      // Assert.
      untracedNhsNumbers.Count().Should().Be(expectedCount);

      if (untracedNhsNumbers.Any())
      {
        nullNhsNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .First(n => n.Id == nullNhsNoReferral.Id));
        nullReferringGpPracticeNoReferral.Should()
        .BeEquivalentTo(untracedNhsNumbers
          .Single(n => n.Id == nullReferringGpPracticeNoReferral.Id));
        emptyReferringGpPracticeNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .Single(n => n.Id == emptyReferringGpPracticeNoReferral.Id));
        unknownReferringGpPracticeNoReferral.Should()
          .BeEquivalentTo(untracedNhsNumbers
            .Single(n => n.Id == unknownReferringGpPracticeNoReferral.Id));
      }
    }
  }

  [Collection("BusinessIntelligenceServiceTests")]
  public class UpdateSpineTracedTests :
    BusinessIntelligenceServiceTests,
    IDisposable
  {
    protected IBusinessIntelligenceService _service;
    private readonly Guid _id;
    private readonly Guid _providerId;
    private readonly string _nhsNumber;
    private readonly string _gpPracticeName;
    private readonly string _gpPracticeOdsCode;
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly IEnumerable<SpineTraceResult> _traceResults;

    public UpdateSpineTracedTests(BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(businessIntelligenceFixture)
    {
      if (_service == null)
      {
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg =>
              cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _mockOptions.Object,
          _mockLogger.Object)
        {
          User = GetClaimsPrincipal()
        };
      }

      _id = Guid.NewGuid();
      _providerId = Guid.NewGuid();
      _nhsNumber = Generators.GenerateNhsNumber(new Random());
      _gpPracticeName = Generators.GenerateGpPracticeNumber(new Random());
      _gpPracticeOdsCode = Generators.GenerateOdsCode(new Random());
      _traceResults = new List<SpineTraceResult>()
        {
          new SpineTraceResult
          {
            Id = _id,
            NhsNumber = _nhsNumber,
            GpPracticeName = _gpPracticeName ,
            GpPracticeOdsCode = _gpPracticeOdsCode
          }
        };
    }

    [Fact]
    public async Task SpineTraceResults_AreNullOrEmpty_LogMessage()
    {
      // Arrange.
      string expectedMessage = "IEnumerable<SpineTraceResult> is null.";
      IEnumerable<SpineTraceResult> models = null;

      // Act.
      await _service.UpdateSpineTraced(models);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_Empty_Id_LogMessage()
    {
      // Arrange.
      string expectedMessage = "The Id field cannot be empty.";
      _traceResults.First().Id = Guid.Empty;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_No_Nhsnumber_LogMessage()
    {
      // Arrange.
      string expectedMessage = "The field NhsNumber must be 10 numbers only.";
      _traceResults.First().NhsNumber = string.Empty;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_No_GpPracticeOdsCode_LogMessage()
    {
      // Arrange.
      string expectedMessage = "The GpPracticeOdsCode field must have a" +
        " length of 6 characters.";
      _traceResults.First().GpPracticeOdsCode = string.Empty;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_Spaces_GpPracticeOdsCode_LogMessage()
    {
      // Arrange.
      string expectedMessage = "The GpPracticeOdsCode field must have a" +
        " length of 6 characters.";
      _traceResults.First().GpPracticeOdsCode = "        ";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_No_GpPracticeName_LogMessage()
    {
      // Arrange.
      string expectedMessage =
        "The GpPracticeName field cannot be null or empty if the " +
        "NhsNumber field is provided. Provide a GpPracticeName of " +
        "Unknown if this is intentional.";
      _traceResults.First().GpPracticeName = null;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      _mockLogger.Verify(t => t.Information(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SpineTraceResult_ReferralNotFound_LogMessage()
    {
      // Arrange.
      string expectedMessage =
        $"Referral not found with an id of {_id}.";

      // Act.
      List<SpineTraceResponse> response =
           await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      using (new AssertionScope())
      {
        response.Count.Should().Be(1);
        response.First().Errors.Count.Should().Be(1);
        response.First().Errors.First().Should().Be(expectedMessage);
      }
    }

    [Fact]
    public async Task SpineTraceResult_NhsNumber_Mismatch_UpdateUsingTrace()
    {
      // Arrange.
      string expectedStatus =
        ReferralStatus.ProviderAwaitingStart.ToString();
      Referral referral = await AddReferral(
        NhsNumber: Generators.GenerateNhsNumber(new Random()));

      _traceResults.First().Id = referral.Id;

      // Act.
      List<SpineTraceResponse> response =
        await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals
        .SingleOrDefaultAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        response.Count.Should().Be(1);
        response.First().Errors.Count.Should().Be(0);
        testReferral.Status.Should().Be(expectedStatus);
        testReferral.NhsNumber.Should()
          .Be(_traceResults.First().NhsNumber);
      }
    }

    [Fact]
    public async Task SpineTraceResult_Trace_Not_Successful_NhsNumber_Null()
    {
      // Arrange.
      Referral referral = await AddReferral(id: _id);

      _traceResults.First().NhsNumber = null;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.ReferringGpPracticeName.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NAME);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NUMBER);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task
      SpineTraceResult_Trace_Not_Successful_Status_Not_Changed_LogMessage()
    {
      // Arrange.
      ReferralStatus expectedStatus = ReferralStatus.RmcCall;
      Referral referral = await AddReferral(
        id: _id,
        status: expectedStatus);

      _traceResults.First().NhsNumber = null;

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.ReferringGpPracticeName.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NAME);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NUMBER);
        testReferral.Status.Should().Be(expectedStatus.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task
      SpineTraceResult_DischargeAwaingTrace_Unchanged_Gp_Practice_Number()
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        status: ReferralStatus.DischargeAwaitingTrace,
        referringGpPracticeName: "Referrer One",
        referringGpPracticeNumber: "M11111");

      SpineTraceResult traceResult = _traceResults.First();
      traceResult.GpPracticeName = referral.ReferringGpPracticeName;
      traceResult.GpPracticeOdsCode = referral.ReferringGpPracticeNumber;

      // Act.
      List<SpineTraceResponse> response = await _service
        .UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      List<ReferralAudit> testReferralAudits = _context.ReferralsAudit
            .Where(t => t.Ubrn == testReferral.Ubrn)
            .OrderBy(t => t.AuditId).ToList();
      using (new AssertionScope())
      {
        testReferral.ReferringGpPracticeName.Should()
          .Be(referral.ReferringGpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(referral.ReferringGpPracticeNumber);
        testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
        testReferral.TraceCount.Should().Be(1);
        testReferralAudits.Last().Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        testReferralAudits.Last().StatusReason.Should().BeNull();
        testReferralAudits[testReferralAudits.Count - 2].Status.Should()
          .Be(ReferralStatus.UnableToDischarge.ToString());
        testReferralAudits[testReferralAudits.Count - 2].StatusReason.Should()
          .Be(Constants.UNABLE_TO_TRACE_STATUS_REASON);
      }
    }

    [Theory]
    [MemberData(nameof(UnkownGpPracticeOdsCodeTheoryData))]
    public async Task
      SpineTraceResult_DischargeAwaingTrace_Unknown_Gp_Practice_Number(
        string gpPracticeNumber)
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        status: ReferralStatus.DischargeAwaitingTrace,
        referringGpPracticeName: "Referrer One",
        referringGpPracticeNumber: "M11111");

      SpineTraceResult traceResult = _traceResults.First();
      traceResult.GpPracticeOdsCode = gpPracticeNumber;

      // Act.
      List<SpineTraceResponse> response = await _service
        .UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      List<ReferralAudit> testReferralAudits = _context.ReferralsAudit
           .Where(t => t.Id == testReferral.Id)
           .OrderBy(t => t.AuditId).ToList();
      using (new AssertionScope())
      {
        testReferral.ReferringGpPracticeName.Should()
          .Be(traceResult.GpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should().Be(gpPracticeNumber);
        testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
        testReferral.TraceCount.Should().Be(1);
        testReferralAudits.Last().Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        testReferralAudits.Last().StatusReason.Should().BeNull();
        testReferralAudits[testReferralAudits.Count - 2].Status.Should()
          .Be(ReferralStatus.UnableToDischarge.ToString());
        testReferralAudits[testReferralAudits.Count - 2].StatusReason.Should()
          .Be(Constants.UNABLE_TO_TRACE_STATUS_REASON);
      }
    }

    [Fact]
    public async Task Trace_Successful_Referral_Update()
    {
      // Arrange.
      Referral referral = await AddReferral(id: _id);

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task Trace_Successful_Referral_TraceCount_2_update()
    {
      // Arrange.
      Referral referral = await AddReferral(id: _id, traceCount: 1);

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(2);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral,
ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Pharmacy,
ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.GeneralReferral,
ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Msk, ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
    [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
    public async Task Trace_Successful_Provider_Selected_Date_Null(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        dateProviderSelected: null,
        providerId: _providerId);
      Referral matchedReferral = await AddReferral(
       NhsNumber: _nhsNumber,
       providerId: _providerId,
       source: source,
       status: status);
      string expectedMessage =
        $"The previous referral (UBRN {matchedReferral.Ubrn}) has a " +
        $"selected provider without a matching date of provider selection.";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      using (new AssertionScope())
      {
        _mockLogger.Verify(t => t.Warning(
          It.IsAny<Exception>(),
          expectedMessage),
          Times.Once);
      }
    }

    [Fact]
    public async Task Trace_Referral_Successful_GpReferral_update()
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        source: ReferralSource.GpReferral);

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral)]
    [InlineData(ReferralSource.Pharmacy)]
    [InlineData(ReferralSource.GeneralReferral)]
    [InlineData(ReferralSource.Msk)]
    public async Task Trace_NotGpReferral_ReferralReentry_NoMatches(
      ReferralSource source)
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        source: source);

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Pharmacy,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.GeneralReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Msk, ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
    [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
    public async Task Trace_NotGpReferrallReentry_No_Provider_WithMatches(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        source: source);
      Referral matchedReferral = await AddReferral(
        NhsNumber: _nhsNumber,
        source: source,
        status: status);

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Pharmacy,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.GeneralReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Msk, ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
    [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
    public async Task Trace_NotGpReferrallReentry_WithMatches(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Entities.Provider provider = await AddProvider(id: _providerId);
      Referral referral = await AddReferral(
        id: _id,
        source: source);
      Referral matchedReferral = await AddReferral(
        NhsNumber: _nhsNumber,
        source: source,
        status: status,
        providerId: _providerId,
        dateProviderSelected: DateTime.UtcNow.AddDays(-40));
      string expectedDate =
        matchedReferral.DateOfProviderSelection
        .Value
        .AddDays(43)
        .ToString("yyyy-MM-dd");
      string expectedStatusReason =
        $"Referral can be created from " +
        $"{expectedDate} as an existing referral for this NHS" +
        $" number (UBRN {matchedReferral.Ubrn}) selected a provider but " +
        $"did not start the programme. The selected provider has been removed.";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.CancelledDuplicateTextMessage.ToString());
        testReferral.StatusReason.Should().Be(expectedStatusReason);
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Pharmacy,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.GeneralReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Msk, ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
    [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
    public async Task
      Trace_NotGpReferrallReentry_DateStartedProgram_WithMatches(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Entities.Provider provider = await AddProvider(id: _providerId);
      Referral referral = await AddReferral(
        id: _id,
        source: source);
      Referral matchedReferral = await AddReferral(
        NhsNumber: _nhsNumber,
        source: source,
        status: status,
        providerId: _providerId,
        dateProviderSelected: DateTime.UtcNow.AddDays(-100),
        dateStartedProgramme: DateTime.UtcNow.AddDays(-100));
      string expectedDate =
        matchedReferral.DateStartedProgramme.Value
        .AddDays(Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 1)
        .ToString("yyyy-MM-dd");
      string expectedMessage =
       $"Referral can be created from {expectedDate} as an " +
       $"existing referral for this NHS number (UBRN " +
       $"{matchedReferral.Ubrn}) started the programme. The selected provider" +
       $" has been removed.";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.CancelledDuplicateTextMessage.ToString());
        testReferral.StatusReason.Should().Be(expectedMessage);
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Pharmacy,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.GeneralReferral,
      ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.Msk, ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
    [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
    [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
    public async Task
      Trace_NotGpReferrallReentry_ProviderDateNull_WithMatches(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Entities.Provider provider = await AddProvider(id: _providerId);
      Referral referral = await AddReferral(
        id: _id,
        source: source);
      Referral matchedReferral = await AddReferral(
        NhsNumber: _nhsNumber,
        source: source,
        status: status,
        providerId: _providerId);
      string expectedLogMessage =
        $"The previous referral (UBRN {matchedReferral.Ubrn}) has a " +
        $"selected provider without a matching date of provider selection.";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        testReferral.TraceCount.Should().Be(1);
      }
    }

    [Theory]
    [InlineData(ReferralStatus.New)]
    [InlineData(ReferralStatus.TextMessage1)]
    [InlineData(ReferralStatus.TextMessage2)]
    [InlineData(ReferralStatus.RmcCall)]
    [InlineData(ReferralStatus.ProviderAccepted)]
    [InlineData(ReferralStatus.ProviderAwaitingStart)]
    public async Task
      Trace_StaffReferralReentry_ExistingReferralInProgress(
      ReferralStatus status)
    {
      // Arrange.
      Referral referral = await AddReferral(
        id: _id,
        source: ReferralSource.SelfReferral);
      Referral matchedReferral = await AddReferral(
        NhsNumber: _nhsNumber,
        source: It.IsAny<ReferralSource>(),
        status: status);
      string expectedMessage =
        "Referral cannot be created because there are in progress " +
        $"referrals with the same NHS number: (UBRN {matchedReferral.Ubrn}). " +
        "The selected provider has been removed.";

      // Act.
      await _service.UpdateSpineTraced(_traceResults);

      // Assert.
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      using (new AssertionScope())
      {
        testReferral.NhsNumber.Should().Be(_nhsNumber);
        testReferral.ReferringGpPracticeName.Should().Be(_gpPracticeName);
        testReferral.ReferringGpPracticeNumber.Should()
          .Be(_gpPracticeOdsCode);
        testReferral.Status.Should()
          .Be(ReferralStatus.CancelledDuplicateTextMessage.ToString());
        testReferral.StatusReason.Should().Be(expectedMessage);
        testReferral.TraceCount.Should().Be(1);
      }
    }

    private async Task<Referral> AddReferral(
      Guid? id = null,
      Guid? providerId = null,
      string NhsNumber = null,
      ReferralStatus status = ReferralStatus.ProviderAwaitingTrace,
      ReferralSource source = ReferralSource.SelfReferral,
      DateTime? dateProviderSelected = null,
      DateTime? dateStartedProgramme = null,
      int traceCount = 0,
      string referringGpPracticeNumber = null,
      string referringGpPracticeName = null)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        referralSource: source,
        status: status,
        statusReason: "NHS number is required.");
      if (id != null)
      {
        referral.Id = id.Value;
      }

      referral.NhsNumber = NhsNumber;
      referral.ReferringGpPracticeNumber = referringGpPracticeNumber;
      referral.ReferringGpPracticeName = referringGpPracticeName;
      referral.TraceCount = traceCount;
      referral.ProviderId = providerId;
      referral.DateOfProviderSelection = dateProviderSelected;
      referral.DateStartedProgramme = dateStartedProgramme;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();
      return referral;
    }

    private async Task<Entities.Provider> AddProvider(
      Guid? id = null)
    {
      Entities.Provider provider1 = RandomEntityCreator.CreateRandomProvider(
      id: id.Value,
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Provider One");
      _context.Providers.Add(provider1);
      await _context.SaveChangesAsync();
      return provider1;
    }
  }
}
