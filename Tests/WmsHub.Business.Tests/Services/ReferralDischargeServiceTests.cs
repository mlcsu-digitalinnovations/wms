using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Discharge;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ReferralDischargeServiceTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly IReferralDischargeService _service;
  private readonly ProviderOptions _providerOptions = new();
  private readonly ReferralTimelineOptions _referralTimelineOptions = new();

  private const int DISCHARGEAFTERDAYS = 94;
  private const int DISCHARGECOMPLETIONDAYS = 49;
  private const int TERMINATEAFTERDAYS = 42;
  private const int WEIGHTCHANGETHRESHOLD = 25;

  public ReferralDischargeServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    _service = new ReferralDischargeService(
      _context,
      Options.Create(_providerOptions),
      Options.Create(_referralTimelineOptions))
    {
      User = GetClaimsPrincipal()
    };

    ProviderOptions providerOptions = new()
    {
      DischargeAfterDays = DISCHARGEAFTERDAYS,
      DischargeCompletionDays = DISCHARGECOMPLETIONDAYS,
      WeightChangeThreshold = WEIGHTCHANGETHRESHOLD
    };
    ReferralTimelineOptions referralTimelineOptions = new()
    {
      MaxDaysToStartProgrammeAfterProviderSelection = TERMINATEAFTERDAYS
    };

    PreparedDischarge.SetOptions(providerOptions, referralTimelineOptions);
  }

  public class GetPreparedDischargesAsync : ReferralDischargeServiceTests
  {
    private readonly Entities.Provider _provider;

    public GetPreparedDischargesAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
      _context.MskOrganisations.RemoveRange(_context.MskOrganisations);

      _provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(_provider);
    }

    [Theory]
    [InlineData("*NhsNumber*", null, REFERRINGGPPRACTICENUMBER_VALID)]
    [InlineData("*ReferringGpPracticeNumber*", NHSNUMBER_VALID, REFERRINGGPPRACTICENUMBER_NOTKNOWN)]
    public async Task InvalidNhsOrGpPracticeNumber_StatusUpdatedToUnableToDischargeThenComplete(
      string expectedStatusReasonFragment,
      string nhsNumber,
      string referringGpPracticeNumber)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-(_providerOptions.DischargeAfterDays + 1)),
        isActive: true,
        status: ReferralStatus.ProviderCompleted,
        consentForReferrerUpdatedWithOutcome: true);
      referral.DateCompletedProgramme = null;
      referral.DateOfProviderSelection = DateTimeOffset.Now.AddMonths(-1);
      referral.FirstRecordedWeight = null;
      referral.FirstRecordedWeightDate = null;
      referral.LastRecordedWeight = null;
      referral.LastRecordedWeightDate = null;
      referral.NhsNumber = nhsNumber;
      referral.ProgrammeOutcome = null;
      referral.Provider = _provider;
      referral.ReferringGpPracticeNumber = referringGpPracticeNumber;
      _context.Referrals.Add(referral);

      Entities.ProviderSubmission firstSub = RandomEntityCreator.CreateProviderSubmission(
        date: referral.DateStartedProgramme.Value,
        modifiedAt: referral.DateStartedProgramme.Value,
        providerId: _provider.Id,
        referralId: referral.Id,
        weight: 100);

      Entities.ProviderSubmission lastSub = RandomEntityCreator.CreateProviderSubmission(
        date: DateTimeOffset.Now.AddDays(-1),
        modifiedAt: DateTimeOffset.Now.AddDays(-1),
        providerId: _provider.Id,
        referralId: referral.Id,
        weight: 90);

      _context.ProviderSubmissions.Add(firstSub);
      _context.ProviderSubmissions.Add(lastSub);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.PrepareDischarges();

      // Assert.
      Entities.Referral updatedReferral = _context.Referrals
        .Include(r => r.Audits)
        .Single(r => r.Id.Equals(referral.Id));

      updatedReferral.Audits.Should()
        .Contain(x => x.Status == ReferralStatus.ProviderCompleted.ToString())
        .And.Contain(x => x.Status == ReferralStatus.UnableToDischarge.ToString())
        .And.Contain(x => x.Status == ReferralStatus.Complete.ToString());
      
      updatedReferral.DateCompletedProgramme.Should().Be(lastSub.Date);
      updatedReferral.FirstRecordedWeight.Should().Be(firstSub.Weight);
      updatedReferral.FirstRecordedWeightDate.Should().Be(firstSub.Date);
      updatedReferral.LastRecordedWeight.Should().Be(lastSub.Weight);
      updatedReferral.LastRecordedWeightDate.Should().Be(lastSub.Date);
      updatedReferral.ModifiedAt.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      updatedReferral.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      updatedReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
      updatedReferral.StatusReason.Should().Match(expectedStatusReasonFragment);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task TwoSubmissionsSameDateFirstAndLastShouldBeTheSame(
      ReferralSource referralSource)
    {
      // Arrange.
      int expectedFirstRecordedWeight = 90;
      int expectedLastRecordedWeight = 85;
      string odsCode = "ABCDE";

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-(_providerOptions.DischargeAfterDays + 1)),
        isActive: true,
        referralSource: referralSource,
        status: ReferralStatus.ProviderCompleted,
        consentForReferrerUpdatedWithOutcome: true);
      referral.DateCompletedProgramme = null;
      referral.DateOfProviderSelection = DateTimeOffset.Now.AddMonths(-1);
      referral.FirstRecordedWeight = null;
      referral.FirstRecordedWeightDate = null;
      referral.LastRecordedWeight = null;
      referral.LastRecordedWeightDate = null;
      referral.ProgrammeOutcome = null;
      referral.Provider = _provider;
      referral.ReferringOrganisationOdsCode = odsCode;
      _context.Referrals.Add(referral);

      Entities.ProviderSubmission firstSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: referral.DateStartedProgramme.Value,
          modifiedAt: referral.DateStartedProgramme.Value,
          providerId: _provider.Id,
          referralId: referral.Id,
          weight: 250);

      Entities.ProviderSubmission correctedFirstSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: firstSub.Date,
          modifiedAt: firstSub.ModifiedAt.AddDays(10),
          providerId: firstSub.ProviderId,
          referralId: firstSub.ReferralId,
          weight: expectedFirstRecordedWeight);

      Entities.ProviderSubmission lastSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: DateTimeOffset.Now.AddDays(-1),
          modifiedAt: DateTimeOffset.Now.AddDays(-1),
          providerId: _provider.Id,
          referralId: referral.Id,
          weight: 300);

      Entities.ProviderSubmission correctedLastSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: lastSub.Date,
          modifiedAt: lastSub.Date.AddDays(1),
          providerId: lastSub.ProviderId,
          referralId: lastSub.ReferralId,
          weight: expectedLastRecordedWeight);

      Entities.MskOrganisation mskOrganisation =
        new()
        {
          IsActive = true,
          Id = Guid.NewGuid(),
          OdsCode = odsCode,
          SendDischargeLetters = true,
          SiteName = "SiteName"
        };

      _context.ProviderSubmissions.Add(firstSub);
      _context.ProviderSubmissions.Add(correctedFirstSub);
      _context.ProviderSubmissions.Add(lastSub);
      _context.ProviderSubmissions.Add(correctedLastSub);
      _context.MskOrganisations.Add(mskOrganisation);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.PrepareDischarges();

      // Assert.
      using (new AssertionScope())
      {
        Entities.Referral updatedReferral = _context.Referrals
          .Include(r => r.Audits)
          .Include(r => r.Provider)
          .Include(r => r.ProviderSubmissions)
          .Single(r => r.Id.Equals(referral.Id));
        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateCompletedProgramme)
          .Excluding(r => r.FirstRecordedWeight)
          .Excluding(r => r.FirstRecordedWeightDate)
          .Excluding(r => r.LastRecordedWeight)
          .Excluding(r => r.LastRecordedWeightDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.ProgrammeOutcome)
          .Excluding(r => r.Provider)
          .Excluding(r => r.ProviderSubmissions)
          .Excluding(r => r.Status));

        updatedReferral.Audits.Count.Should().Be(2);
        updatedReferral.DateCompletedProgramme.Should()
          .Be(correctedLastSub.Date);
        updatedReferral.FirstRecordedWeight.Should()
          .Be(expectedFirstRecordedWeight);
        updatedReferral.FirstRecordedWeightDate.Should().
          Be(correctedFirstSub.Date);
        updatedReferral.LastRecordedWeight.Should()
          .Be(expectedLastRecordedWeight);
        updatedReferral.LastRecordedWeightDate.Should()
          .Be(correctedLastSub.Date);
        updatedReferral.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.ProgrammeOutcome.Should()
          .Be(ProgrammeOutcome.Complete.ToString());
        updatedReferral.Provider.Should().NotBeNull();
        updatedReferral.ProviderSubmissions.Count.Should().Be(4);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.AwaitingDischarge.ToString());
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task TwoSubmissionsWeightChangeAboveThresholdDischargeOnHold(
      ReferralSource referralSource)
    {
      // Arrange.
      decimal weightChangeThreshold =
        new ProviderOptions().WeightChangeThreshold;
      int expectedFirstRecordedWeight = 90;
      decimal lastRecordedWeight =
        expectedFirstRecordedWeight + weightChangeThreshold + 1;
      string odsCode = "ABCDE";

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-(_providerOptions.DischargeAfterDays + 1)),
        isActive: true,
        referralSource: referralSource,
        status: ReferralStatus.ProviderCompleted,
        consentForReferrerUpdatedWithOutcome: true);
      referral.DateCompletedProgramme = null;
      referral.DateOfProviderSelection = DateTimeOffset.Now.AddMonths(-1);
      referral.FirstRecordedWeight = null;
      referral.FirstRecordedWeightDate = null;
      referral.LastRecordedWeight = null;
      referral.LastRecordedWeightDate = null;
      referral.ProgrammeOutcome = null;
      referral.Provider = _provider;
      referral.ReferringOrganisationOdsCode = odsCode;
      _context.Referrals.Add(referral);

      Entities.ProviderSubmission firstSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: referral.DateStartedProgramme.Value,
          modifiedAt: referral.DateStartedProgramme.Value,
          providerId: _provider.Id,
          referralId: referral.Id,
          weight: expectedFirstRecordedWeight);

      Entities.ProviderSubmission lastSub =
        RandomEntityCreator.CreateProviderSubmission(
          date: DateTimeOffset.Now.AddDays(-1),
          modifiedAt: DateTimeOffset.Now.AddDays(-1),
          providerId: _provider.Id,
          referralId: referral.Id,
          weight: lastRecordedWeight);

      Entities.MskOrganisation mskOrganisation =
        new()
        {
          IsActive = true,
          Id = Guid.NewGuid(),
          OdsCode = odsCode,
          SendDischargeLetters = true,
          SiteName = "SiteName"
        };

      _context.ProviderSubmissions.Add(firstSub);
      _context.ProviderSubmissions.Add(lastSub);
      _context.MskOrganisations.Add(mskOrganisation);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.PrepareDischarges();

      // Assert.
      using (new AssertionScope())
      {
        Entities.Referral updatedReferral = _context.Referrals
        .Include(r => r.Audits)
        .Include(r => r.Provider)
        .Include(r => r.ProviderSubmissions)
        .Single(r => r.Id.Equals(referral.Id));
        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateCompletedProgramme)
          .Excluding(r => r.FirstRecordedWeight)
          .Excluding(r => r.FirstRecordedWeightDate)
          .Excluding(r => r.LastRecordedWeight)
          .Excluding(r => r.LastRecordedWeightDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.ProgrammeOutcome)
          .Excluding(r => r.Provider)
          .Excluding(r => r.ProviderSubmissions)
          .Excluding(r => r.Status)
          .Excluding(r => r.StatusReason));

        updatedReferral.Audits.Count.Should().Be(2);
        updatedReferral.DateCompletedProgramme.Should()
          .Be(lastSub.Date);
        updatedReferral.FirstRecordedWeight.Should()
          .Be(expectedFirstRecordedWeight);
        updatedReferral.FirstRecordedWeightDate.Should().
          Be(firstSub.Date);
        updatedReferral.LastRecordedWeight.Should().BeNull();
        updatedReferral.LastRecordedWeightDate.Should().BeNull();
        updatedReferral.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.ProgrammeOutcome.Should()
          .Be(ProgrammeOutcome.Complete.ToString());
        updatedReferral.Provider.Should().NotBeNull();
        updatedReferral.ProviderSubmissions.Count.Should().Be(2);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.AwaitingDischarge.ToString());
        updatedReferral.StatusReason.Should().NotBeNull();
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DateStartedProgrammeIsNullProviderTerminatedAwaitingDischarge(
      ReferralSource referralSource)
    {
      // Arrange.
      string odsCode = "M12345";
      DateTimeOffset expectedDateCompletedProgramme = DateTimeOffset.Now.AddDays(-1);
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: null,
        isActive: true,
        referralSource: referralSource,
        status: ReferralStatus.ProviderTerminated,
        consentForReferrerUpdatedWithOutcome: true);
      referral.DateCompletedProgramme = null;
      referral.DateOfProviderSelection = expectedDateCompletedProgramme
        .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection);
      referral.FirstRecordedWeight = null;
      referral.FirstRecordedWeightDate = null;
      referral.LastRecordedWeight = null;
      referral.LastRecordedWeightDate = null;
      referral.ProgrammeOutcome = null;
      referral.Provider = _provider;
      referral.ReferringGpPracticeNumber = odsCode;
      referral.ReferringOrganisationOdsCode = odsCode;
      _context.Referrals.Add(referral);

      Entities.MskOrganisation mskOrganisation =
        new()
        {
          IsActive = true,
          Id = Guid.NewGuid(),
          OdsCode = odsCode,
          SendDischargeLetters = true,
          SiteName = "SiteName"
        };

      _context.MskOrganisations.Add(mskOrganisation);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.PrepareDischarges();

      // Assert.
      Entities.Referral updatedReferral = _context.Referrals
        .Include(r => r.Audits)
        .Include(r => r.Provider)
        .Include(r => r.ProviderSubmissions)
        .Single(r => r.Id.Equals(referral.Id));
      updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.DateCompletedProgramme)
        .Excluding(r => r.FirstRecordedWeight)
        .Excluding(r => r.FirstRecordedWeightDate)
        .Excluding(r => r.LastRecordedWeight)
        .Excluding(r => r.LastRecordedWeightDate)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProgrammeOutcome)
        .Excluding(r => r.Provider)
        .Excluding(r => r.ProviderSubmissions)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason));

      updatedReferral.Audits.Count.Should().Be(2);
      updatedReferral.DateCompletedProgramme.Should().Be(expectedDateCompletedProgramme);
      updatedReferral.FirstRecordedWeight.Should().BeNull();
      updatedReferral.FirstRecordedWeightDate.Should().BeNull();
      updatedReferral.LastRecordedWeight.Should().BeNull();
      updatedReferral.LastRecordedWeightDate.Should().BeNull();
      updatedReferral.ModifiedAt.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      updatedReferral.ProgrammeOutcome.Should().Be(ProgrammeOutcome.DidNotCommence.ToString());
      updatedReferral.Provider.Should().NotBeNull();
      updatedReferral.ProviderSubmissions.Count.Should().Be(0);
      updatedReferral.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
    }

    [Fact]
    public async Task ReferralDateStartedProgrammeHasValueReferralDateStartedProgrammeIsNull()
    {
      // Arrange.
      string expectedResult = "Found 2 referrals to prepare.\r\n" +
        "0 did complete the programme, whereas 2 did not.\r\n" +
        "2 are now awaiting discharge, 0 are unable to be discharged, 0 are now on hold, and 0 " + 
        "are now complete.";
      Entities.Referral referralDateStartedProgrammeHasValue = RandomEntityCreator
        .CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now,
          dateStartedProgramme: DateTimeOffset.Now.AddDays(-_providerOptions.DischargeAfterDays),
          isActive: true,
          referralSource: ReferralSource.GpReferral,
          status: ReferralStatus.ProviderTerminated,
          consentForReferrerUpdatedWithOutcome: true);
      Entities.Referral referralDateStartedProgrammeIsNull = RandomEntityCreator
        .CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now,
          dateStartedProgramme: null,
          isActive: true,
          referralSource: ReferralSource.GpReferral,
          status: ReferralStatus.ProviderTerminated,
          consentForReferrerUpdatedWithOutcome: true);
      _context.Referrals.Add(referralDateStartedProgrammeHasValue);
      _context.Referrals.Add(referralDateStartedProgrammeIsNull);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.PrepareDischarges();

      // Assert.
      result.Should().Be(expectedResult);
    }
  }
}
