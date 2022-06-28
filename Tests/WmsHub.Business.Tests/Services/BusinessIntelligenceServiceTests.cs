using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Tests.Services
{
  public class BusinessIntelligenceFixture
  {
    public DatabaseContext Context { get; private set; }

    public BusinessIntelligenceFixture()
    {
      Context = new DatabaseContext(
        new DbContextOptionsBuilder<DatabaseContext>()
          .UseInMemoryDatabase(databaseName: "WmsHub_BIServiceTest")
          .Options);
    }
  }

  public class BusinessIntelligenceServiceTests
    : ServiceTestsBase, IClassFixture<BusinessIntelligenceFixture>
  {
    protected readonly DatabaseContext _context;
    protected readonly IBusinessIntelligenceService _service;

    public BusinessIntelligenceServiceTests(
      ITestOutputHelper testOutputHelper,
      BusinessIntelligenceFixture businessIntelligenceFixture)
      : base(testOutputHelper)
    {
      if (_context == null)
        _context = businessIntelligenceFixture.Context;

      if (_service == null)
        _service = new BusinessIntelligenceService(
          _context,
          new MapperConfiguration(cfg => cfg.AddMaps(new[] { "WmsHub.Business" }))
            .CreateMapper(),
          _log)
        {
          User = GetClaimsPrincipal()
        };
    }

    public void CleanUp()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
      _context.SaveChanges();
    }

    [Collection("BusinessIntelligenceServiceTests")]
    public class GetAnonymisedReferrals : BusinessIntelligenceServiceTests,
      IClassFixture<BusinessIntelligenceFixture>
    {
      public GetAnonymisedReferrals(
        ITestOutputHelper testOutputHelper,
        BusinessIntelligenceFixture businessIntelligenceFixture)
        : base(testOutputHelper, businessIntelligenceFixture)
      { }

      [Theory]
      [InlineData("=HYPERLINK", "HYPERLINK")]
      [InlineData("==HYPERLINK", "HYPERLINK")]
      [InlineData("@=HYPERLINK", "HYPERLINK")]
      [InlineData("-=HYPERLINK", "HYPERLINK")]
      [InlineData("@HYPERLINK", "HYPERLINK")]
      [InlineData("+=HYPERLINK", "HYPERLINK")]
      public async Task ValidInjectionRemoval(string injected, string expected)
      {
        // ARRANGE
        // ARRANGE
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = null;
        DateTimeOffset? toDateTime = null;
        int expectedCount = 2;

        await AddTestReferralsStatusReason(offsetOne, offsetTwo, injected);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Equals(expectedCount);

        foreach (AnonymisedReferral r in result)
        {
          r.StatusReason.Should().Be(expected);
        }

        CleanUp();
      }

      [Fact]
      public async void NoDates()
      {
        // ARRANGE
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = null;
        DateTimeOffset? toDateTime = null;
        int expectedCount = 2;

        await AddTestReferrals(offsetOne, offsetTwo);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Equals(expectedCount);

        CleanUp();
      }

      [Fact]
      public async void WithFromDate()
      {
        // ARRANGE
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = DateTime.Now.AddDays(offsetOne + 10);
        DateTimeOffset? toDateTime = null;
        DateTimeOffset expectedDate = DateTime.Now.AddDays(offsetTwo);
        int expectedCount = 1;

        await AddTestReferrals(offsetOne, offsetTwo);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount,
          $"total count is {_context.Referrals.Count()}");
        result.FirstOrDefault().DateOfReferral.Should().BeAfter(expectedDate);

        CleanUp();
      }

      [Fact]
      public async void WithToDate()
      {
        // ARRANGE
        int offsetDays = -90;
        DateTimeOffset? fromDateTime = null;
        DateTimeOffset? toDateTime = DateTime.Now.AddDays(offsetDays + 10);
        DateTimeOffset expectedDate = DateTime.Now.AddDays(offsetDays);
        int expectedCount = 1;

        await AddTestReferrals(offsetDays, offsetDays + 20);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.FirstOrDefault().DateOfReferral.Should().BeAfter(expectedDate);

        CleanUp();
      }

      [Fact]
      public async void WithBothDates()
      {
        // ARRANGE
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset? toDateTime = DateTime.Now.AddDays(offsetTwo + 10); ;
        DateTimeOffset expectedDate1 = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset expectedDate2 = DateTime.Now.AddDays(offsetTwo + 10);
        int expectedCount = 2;

        await AddTestReferrals(offsetOne, offsetTwo);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
        result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);

        CleanUp();
      }

      [Fact]
      public async void Null_andEmpty_VulnerableDescription()
      {
        // ARRANGE
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset? toDateTime = DateTime.Now.AddDays(offsetTwo + 10);
        ;
        DateTimeOffset expectedDate1 = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset expectedDate2 = DateTime.Now.AddDays(offsetTwo + 10);
        int expectedCount = 2;

        await AddTestReferrals(offsetOne, offsetTwo, null, string.Empty);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
        result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);
        result.ElementAt(0).VulnerableDescription.Should().BeNull();
        result.ElementAt(1).VulnerableDescription.Should().BeNull();

        CleanUp();
      }

      [Fact]
      public async void Null_And_Value_VulnerableDescription()
      {
        // ARRANGE
        string expectedVulnerable = "Test Vulnerable";
        int offsetOne = -90;
        int offsetTwo = -50;
        DateTimeOffset? fromDateTime = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset? toDateTime = DateTime.Now.AddDays(offsetTwo + 10);
        ;
        DateTimeOffset expectedDate1 = DateTime.Now.AddDays(offsetOne - 10);
        DateTimeOffset expectedDate2 = DateTime.Now.AddDays(offsetTwo + 10);
        int expectedCount = 2;

        await AddTestReferrals(offsetOne, offsetTwo, null, expectedVulnerable);

        // ACT
        IEnumerable<AnonymisedReferral> result =
          await _service.GetAnonymisedReferrals(fromDateTime, toDateTime);

        // ASSERT
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ElementAt(0).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(0).DateOfReferral.Should().BeBefore(expectedDate2);
        result.ElementAt(1).DateOfReferral.Should().BeAfter(expectedDate1);
        result.ElementAt(1).DateOfReferral.Should().BeBefore(expectedDate2);
        result.ElementAt(0).VulnerableDescription.Should().BeNull();
        result.ElementAt(1).VulnerableDescription.Should()
          .Be(expectedVulnerable);

        CleanUp();
      }
    }

    [Collection("BusinessIntelligenceServiceTests")]
    public class GetAnonymisedReprocessedReferralsBySubmissionDateTests :
      BusinessIntelligenceServiceTests
    {
      public GetAnonymisedReprocessedReferralsBySubmissionDateTests(
        ITestOutputHelper testOutputHelper,
        BusinessIntelligenceFixture businessIntelligenceFixture)
        : base(testOutputHelper, businessIntelligenceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        //Arrange
        var from = DateTimeOffset.Now.AddMonths(-6);
        var to = DateTimeOffset.Now;

        await AddTestReferralsWithHistory();

        //Act
        var result =
          await _service.GetAnonymisedReprocessedReferralsBySubmissionDate(
            from, to);
      }

    }

    [Collection("BusinessIntelligenceServiceTests")]
    public class GetUntracedNhsNumbers : BusinessIntelligenceServiceTests
    {
      public GetUntracedNhsNumbers(
        ITestOutputHelper testOutputHelper,
        BusinessIntelligenceFixture businessIntelligenceFixture)
        : base(testOutputHelper, businessIntelligenceFixture)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.Providers.RemoveRange(_context.Providers);
        _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        var inactiveReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        var nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        nullNhsNoReferral.NhsNumber = null;
        nullNhsNoReferral.LastTraceDate = null;

        int expectedUntracedNhsNumbersCount = 1;

        _context.Referrals.AddRange(new Entities.Referral[]
          { referral, inactiveReferral, nullNhsNoReferral });
        _context.SaveChanges();

        // act
        var untracedNhsNumbers = await _service.GetUntracedNhsNumbers();

        // assert
        untracedNhsNumbers.Count().Should().Be(expectedUntracedNhsNumbersCount);
        nullNhsNoReferral.Should().BeEquivalentTo(untracedNhsNumbers.Single());
      }

      [Theory]
      [InlineData(ReferralStatus.Exception, 0)]
      [InlineData(ReferralStatus.CancelledByEreferrals, 0)]
      [InlineData(ReferralStatus.RejectedToEreferrals, 0)]
      [InlineData(ReferralStatus.FailedToContact, 0)]
      [InlineData(ReferralStatus.FailedToContactTextMessage, 0)]
      [InlineData(ReferralStatus.New, 1)]
      [InlineData(ReferralStatus.TextMessage1, 1)]
      [InlineData(ReferralStatus.TextMessage2, 1)]
      [InlineData(ReferralStatus.ChatBotCall1, 1)]
      [InlineData(ReferralStatus.ChatBotCall2, 1)]
      [InlineData(ReferralStatus.ChatBotTransfer, 1)]
      [InlineData(ReferralStatus.RmcCall, 1)]
      [InlineData(ReferralStatus.RmcDelayed, 1)]
      [InlineData(ReferralStatus.Letter, 1)]
      [InlineData(ReferralStatus.ProviderAwaitingStart, 1)]
      [InlineData(ReferralStatus.ProviderAccepted, 1)]
      [InlineData(ReferralStatus.ProviderDeclinedByServiceUser, 1)]
      [InlineData(ReferralStatus.ProviderRejected, 1)]
      [InlineData(ReferralStatus.ProviderRejectedTextMessage, 1)]
      [InlineData(ReferralStatus.ProviderContactedServiceUser, 1)]
      [InlineData(ReferralStatus.ProviderStarted, 1)]
      [InlineData(ReferralStatus.ProviderCompleted, 1)]
      [InlineData(ReferralStatus.ProviderTerminated, 1)]
      [InlineData(ReferralStatus.ProviderTerminatedTextMessage, 1)]
      [InlineData(ReferralStatus.LetterSent, 1)]
      [InlineData(ReferralStatus.Complete, 1)]
      public async Task Valid_ReferralStatus_CanTrace(ReferralStatus status,
        int expected)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        var inactiveReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        var nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        nullNhsNoReferral.NhsNumber = null;
        nullNhsNoReferral.LastTraceDate = null;
        nullNhsNoReferral.Status = status.ToString();

        _context.Referrals.AddRange(new Entities.Referral[]
          { referral, inactiveReferral, nullNhsNoReferral });
        _context.SaveChanges();

        // act
        var untracedNhsNumbers = await _service.GetUntracedNhsNumbers();

        // assert
        untracedNhsNumbers.Count().Should().Be(expected);
        if (untracedNhsNumbers.Any())
        {
          nullNhsNoReferral.Should()
            .BeEquivalentTo(untracedNhsNumbers.Single());
        }
      }

      [Theory]
      [InlineData(ReferralStatus.New, null)]
      [InlineData(ReferralStatus.TextMessage1, 1)]
      [InlineData(ReferralStatus.TextMessage2, 1)]
      [InlineData(ReferralStatus.ChatBotCall1, 1)]
      [InlineData(ReferralStatus.ChatBotCall2, 1)]
      [InlineData(ReferralStatus.ChatBotTransfer, 1)]
      [InlineData(ReferralStatus.RmcCall, 1)]
      [InlineData(ReferralStatus.RmcDelayed, 1)]
      [InlineData(ReferralStatus.Letter, 1)]
      [InlineData(ReferralStatus.ProviderAwaitingStart, 7)]
      [InlineData(ReferralStatus.ProviderAccepted, 7)]
      [InlineData(ReferralStatus.ProviderDeclinedByServiceUser, 7)]
      [InlineData(ReferralStatus.ProviderRejected, 7)]
      [InlineData(ReferralStatus.ProviderRejectedTextMessage, 7)]
      [InlineData(ReferralStatus.ProviderContactedServiceUser, 7)]
      [InlineData(ReferralStatus.ProviderStarted, 7)]
      [InlineData(ReferralStatus.ProviderCompleted, 7)]
      [InlineData(ReferralStatus.ProviderTerminated, 7)]
      [InlineData(ReferralStatus.ProviderTerminatedTextMessage, 7)]
      [InlineData(ReferralStatus.LetterSent, 7)]
      [InlineData(ReferralStatus.Complete, 30)]
      public async Task Valid_ReferralSTatus_LastTraceDate(
        ReferralStatus status, int? noOfDays)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        var inactiveReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        var nullNhsNoReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        nullNhsNoReferral.NhsNumber = null;
        nullNhsNoReferral.LastTraceDate = noOfDays == null ?
          null :
          DateTimeOffset.Now.AddDays(-noOfDays.Value);
        nullNhsNoReferral.Status = status.ToString();

        _context.Referrals.AddRange(new Entities.Referral[]
          {referral, inactiveReferral, nullNhsNoReferral});
        _context.SaveChanges();

        // act
        var untracedNhsNumbers = await _service.GetUntracedNhsNumbers();

        // assert
        untracedNhsNumbers.Count().Should().Be(1);
        nullNhsNoReferral.Should().BeEquivalentTo(untracedNhsNumbers.Single());
      }


    }

    [Collection("BusinessIntelligenceServiceTests")]
    public class UpdateSpineTraced : BusinessIntelligenceServiceTests
    {
      Random _random = new Random();

      public UpdateSpineTraced(
        ITestOutputHelper testOutputHelper,
        BusinessIntelligenceFixture businessIntelligenceFixture)
        : base(testOutputHelper, businessIntelligenceFixture)
      { }

      [Fact]
      public async Task NullSpineTraceResults_Exception()
      {
        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(async
          () => await _service.UpdateSpineTraced(null));
      }

      [Fact]
      public async Task SuccessfulTrace_SelfReferralDuplicateNhsNumber()
      {
        // arrange
        Referral gpReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          referralSource: ReferralSource.GpReferral);
        _context.Referrals.Add(gpReferral);

        Referral selfReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          referralSource: ReferralSource.SelfReferral,
          ubrn: Generators.GenerateSelfUbrn(_random));
        selfReferral.NhsNumber = null;
        selfReferral.ReferringGpPracticeNumber = null;
        selfReferral.ReferringGpPracticeName = null;
        _context.Referrals.Add(selfReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var expectedStatusReason =
          $"Duplicate NHS number found in UBRN {gpReferral.Ubrn}.";

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = selfReferral.Id,
            NhsNumber = gpReferral.NhsNumber,
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals
          .FindAsync(selfReferral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(selfReferral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.Status)
          .Excluding(r => r.StatusReason)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(selfReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().Be(spineTraceResults[0].NhsNumber);
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(spineTraceResults[0].GpPracticeName);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(spineTraceResults[0].GpPracticeOdsCode);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.CancelledDuplicateTextMessage.ToString());
        updatedReferral.StatusReason.Should().Be(expectedStatusReason);
        updatedReferral.TraceCount.Should().Be(1);

        CleanUp();
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData),
        ReferralStatus.ProviderAwaitingTrace)]
      public async Task SuccessfulTrace_StatusNotProviderAwaitingTrace(
        ReferralStatus referralStatus)
      {
        // arrange        
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          status: referralStatus);
        referral.NhsNumber = null;
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = Generators.GenerateNhsNumber(_random),
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals.FindAsync(referral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().Be(spineTraceResults[0].NhsNumber);
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(spineTraceResults[0].GpPracticeName);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(spineTraceResults[0].GpPracticeOdsCode);
        updatedReferral.TraceCount.Should().Be(1);
        CleanUp();
      }

      [Fact]
      public async Task SuccessfulTrace_StatusProviderAwaitingStart()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          status: ReferralStatus.ProviderAwaitingTrace);
        referral.NhsNumber = null;
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = Generators.GenerateNhsNumber(_random),
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals.FindAsync(referral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.Status)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().Be(spineTraceResults[0].NhsNumber);
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(spineTraceResults[0].GpPracticeName);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(spineTraceResults[0].GpPracticeOdsCode);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        updatedReferral.TraceCount.Should().Be(1);
        CleanUp();
      }

      [Fact]
      public async Task SuccessfulTrace_SelfReferralStatusProviderAwaitingStart()
      {
        // arrange        
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          referralSource: ReferralSource.SelfReferral,
          status: ReferralStatus.ProviderAwaitingTrace,
          statusReason: "NHS number is required.");
        referral.NhsNumber = null;
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = Generators.GenerateNhsNumber(_random),
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals.FindAsync(referral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.Status)
          .Excluding(r => r.StatusReason)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().Be(spineTraceResults[0].NhsNumber);
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(spineTraceResults[0].GpPracticeName);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(spineTraceResults[0].GpPracticeOdsCode);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        updatedReferral.StatusReason.Should().BeNullOrWhiteSpace();
        updatedReferral.TraceCount.Should().Be(1);

        CleanUp();
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData),
        ReferralStatus.ProviderAwaitingTrace)]
      public async Task UnsuccessfulTrace_StatusNotProviderAwaitingTrace(
        ReferralStatus referralStatus)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          status: referralStatus);
        referral.NhsNumber = null;
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = null,
            GpPracticeOdsCode = null,
            GpPracticeName = null
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals.FindAsync(referral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().BeNullOrWhiteSpace();
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NAME);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NUMBER);
        updatedReferral.TraceCount.Should().Be(1);
        CleanUp();
      }

      [Fact]
      public async Task UnsuccessfulTrace_StatusProviderAwaitingStart()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          status: ReferralStatus.ProviderAwaitingTrace);
        referral.NhsNumber = null;
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = null,
            GpPracticeOdsCode = null,
            GpPracticeName = null
          }
        };

        // act
        await _service.UpdateSpineTraced(spineTraceResults);

        var updatedReferral = await _context.Referrals.FindAsync(referral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, option => option
          .Excluding(r => r.Audits)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.NhsNumber)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringGpPracticeNumber)
          .Excluding(r => r.Status)
          .Excluding(r => r.TraceCount));

        updatedReferral.LastTraceDate.Should()
          .BeOnOrAfter(DateTimeOffset.Now.AddSeconds(-30));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.NhsNumber.Should().BeNullOrWhiteSpace();
        updatedReferral.ReferringGpPracticeName.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NAME);
        updatedReferral.ReferringGpPracticeNumber.Should()
          .Be(Constants.UNKNOWN_GP_PRACTICE_NUMBER);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.ProviderAwaitingStart.ToString());
        updatedReferral.TraceCount.Should().Be(1);
        CleanUp();
      }

      [Theory]
      [InlineData("", "M12345", "Practice", "NhsNumber")]
      [InlineData("123", "M12345", "Practice", "NhsNumber")]
      [InlineData("12345678901", "M12345", "Practice", "NhsNumber")]
      [InlineData("9999999999", null, "Practice", "GpPracticeOdsCode")]
      [InlineData("9999999999", "", "Practice", "GpPracticeOdsCode")]
      [InlineData("9999999999", "M1234", "Practice", "GpPracticeOdsCode")]
      [InlineData("9999999999", "M123456", "Practice", "GpPracticeOdsCode")]
      [InlineData("9999999999", "Z12345", "Practice", "GpPracticeOdsCode")]
      [InlineData("9999999999", "M12345", null, "GpPracticeName")]
      [InlineData("9999999999", "M12345", "", "GpPracticeName")]
      public async Task ValidationException(
        string nhsNumber,
        string gpPracticeCode,
        string gpPracticeName,
        string expectedMessage)
      {
        // arrange
        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = Guid.Empty,
            NhsNumber = nhsNumber,
            GpPracticeOdsCode = gpPracticeCode,
            GpPracticeName = gpPracticeName
          }
        };

        // act & assert
        var ex = await Assert.ThrowsAsync<ValidationException>(async
          () => await _service.UpdateSpineTraced(spineTraceResults));

        ex.Message.Should().Contain(expectedMessage);
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = Guid.NewGuid(),
            NhsNumber = Generators.GenerateNhsNumber(_random),
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act & assert
        await Assert.ThrowsAsync<ReferralNotFoundException>(async
          () => await _service.UpdateSpineTraced(spineTraceResults));
      }

      [Fact]
      public async Task NhsNumberTraceMismatchException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: true);
        referral.NhsNumber = Generators.GenerateNhsNumber(_random);
        referral.ReferringGpPracticeNumber = null;
        referral.ReferringGpPracticeName = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        var spineTraceResults = new List<SpineTraceResult>
        {
          new SpineTraceResult
          {
            Id = referral.Id,
            NhsNumber = Generators
              .GenerateNhsNumber(_random, referral.NhsNumber),
            GpPracticeOdsCode = Generators.GenerateOdsCode(_random),
            GpPracticeName = Generators.GenerateName(_random, 10)
          }
        };

        // act & assert
        await Assert.ThrowsAsync<NhsNumberTraceMismatchException>(async
          () => await _service.UpdateSpineTraced(spineTraceResults));

        CleanUp();
      }
    }

    protected async Task AddTestReferrals(int offsetOne, int offsetTwo)
      => await AddTestReferrals(offsetOne, offsetTwo,
        "Not Vulnerable", "Vulnerable");

    protected async Task AddTestReferralsStatusReason(int offsetOne,
      int offsetTwo, string reason)
      => await AddTestReferrals(offsetOne, offsetTwo,
        "Not Vulnerable", "Vulnerable", reason);

    protected async Task AddTestReferrals(int offsetOne,
      int offsetTwo,
      string vulnerableDescriptionOne,
      string vulnerableDescriptionTwo,
      string reason = null)
    {
      //add entities
      Guid providerId1 = Guid.NewGuid();

      Entities.ProviderSubmission providerSub1 = new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId1
      };
      Entities.ProviderSubmission providerSub2 = new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId1
      };

      List<Entities.ProviderSubmission> listProviderSubs1 = new()
      {
        providerSub1,
        providerSub2
      };

      Entities.Provider provider1 = RandomEntityCreator.CreateRandomProvider(
        id: providerId1,
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider One");
      provider1.ProviderSubmissions = listProviderSubs1;

      Guid providerId2 = Guid.NewGuid();

      Entities.ProviderSubmission providerSub3 =
      new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId2
      };

      Entities.ProviderSubmission providerSub4 =
        new()
        {
          Coaching = 6,
          Date = DateTime.Now.AddDays(-14),
          Measure = 7,
          Weight = 97,
          ProviderId = providerId2
        };

      List<Entities.ProviderSubmission> listProviderSubs2 =
        new()
        {
          providerSub3,
          providerSub4
        };

      Entities.Provider provider2 = RandomEntityCreator.CreateRandomProvider(
        id: providerId2,
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider Two");
      provider1.ProviderSubmissions = listProviderSubs2;

      _context.Providers.Add(provider1);
      _context.Providers.Add(provider2);

      Entities.Referral referral1 = new()
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

      Entities.Referral referral2 = new()
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
      //add entities
      Guid providerId1 = Guid.NewGuid();
      var startDate = DateTimeOffset.Now.AddMonths(-6);
      Random rnd = new Random();

      Entities.ProviderSubmission providerSub1 = new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId1
      };
      Entities.ProviderSubmission providerSub2 = new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId1
      };

      List<Entities.ProviderSubmission> listProviderSubs1 = new()
      {
        providerSub1,
        providerSub2
      };

      Entities.Provider provider1 = RandomEntityCreator.CreateRandomProvider(
        id: providerId1,
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider One");
      provider1.ProviderSubmissions = listProviderSubs1;

      Guid providerId2 = Guid.NewGuid();

      Entities.ProviderSubmission providerSub3 = new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId2
      };

      Entities.ProviderSubmission providerSub4 = new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId2
      };

      List<Entities.ProviderSubmission> listProviderSubs2 = new()
      {
        providerSub3,
        providerSub4
      };

      Entities.Provider provider2 = RandomEntityCreator.CreateRandomProvider(
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

      Entities.Referral referral2 = new()
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

      //Add Provider Rejected
      var entity_1_a =
        _context.Referrals.SingleOrDefault(t => t.Id == referral1.Id);

      entity_1_a.Status = ReferralStatus.ProviderRejected.ToString();
      entity_1_a.StatusReason = "Provider Rejected";
      entity_1_a.ModifiedAt = DateTimeOffset.Now.AddMonths(-5);
      entity_1_a.ModifiedByUserId = entity_1_a.ProviderId.Value;
      await _context.SaveChangesAsync();

      //Add Rejected To EREferrals
      var entity_1_b =
        _context.Referrals.SingleOrDefault(t => t.Id == referral1.Id);
      entity_1_b.Status = ReferralStatus.RejectedToEreferrals.ToString();
      entity_1_b.StatusReason = "Rejected to EReferrals";
      entity_1_b.ModifiedAt = DateTimeOffset.Now.AddMonths(-4);
      entity_1_b.ModifiedByUserId = entity_1_b.ProviderId.Value;
      await _context.SaveChangesAsync();

      // Add back into system
      Entities.Referral referral1_c = new()
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


      //Add Rejected To EREferrals
      var entity_2_a =
        _context.Referrals.SingleOrDefault(t => t.Id == referral2.Id);
      entity_2_a.Status = ReferralStatus.RejectedToEreferrals.ToString();
      entity_2_a.StatusReason = "Rejected to EReferrals";
      entity_2_a.ModifiedAt = DateTimeOffset.Now.AddMonths(-4);
      entity_2_a.ModifiedByUserId = entity_2_a.ProviderId.Value;
      await _context.SaveChangesAsync();


      // Add back into system
      Entities.Referral referral2_b = new()
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
  }
}
