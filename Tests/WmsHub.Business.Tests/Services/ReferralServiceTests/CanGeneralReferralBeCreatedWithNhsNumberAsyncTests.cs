using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class CanGeneralReferralBeCreatedWithNhsNumberAsync 
    : ReferralServiceTests, IDisposable
  {

    public CanGeneralReferralBeCreatedWithNhsNumberAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      CleanUp();
    }

    public new void Dispose()
    {
      CleanUp();
    }

    private void CleanUp()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task NhsNumberNullOrWhiteSpace_Exception(string nhsNumber)
    {
      // Arrange.

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.CanGeneralReferralBeCreatedWithNhsNumberAsync(nhsNumber));

      // Assert.
      ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>(
        because: "when NHS number is null or white space an exception " +
          "should be thrown.");
    }

    [Fact]
    public async Task NoExisitingReferrals_CanCreate()
    {
      // Arrange.
      string nhsNumber = "9999999991";

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(nhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.CanCreate,
          because: "there are no existing referrals.");

        response.Reason.Should().Be(
          "No existing referrals match NHS number.");

        response.Referral.Should().BeNull(
          because: "there is no in in progress referral for this NHS number.");
      }
    }

    [Fact]
    public async Task NoActiveExisitingReferrals_CanCreate()
    {
      // Arrange.
      Entities.Referral referralInactive = RandomEntityCreator
        .CreateRandomReferral(isActive: false);
      _context.Referrals.Add(referralInactive);
      _context.SaveChanges();

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(
          referralInactive.NhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.CanCreate,
          because: "there are no active referrals.");

        response.Reason.Should().Be(
          "No existing referrals match NHS number.");

        response.Referral.Should().BeNull(
          because: "there is no in in progress referral for this NHS number.");
      }
    }

    [Theory]
    [MemberData(nameof(CancelledOrCompleteStatus))]
    public async Task DateOfProviderSelectionNull_Exception(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: null,
        providerId: Guid.NewGuid(),
        status: referralStatus);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      Exception ex = await Record.ExceptionAsync(() => 
        _service.CanGeneralReferralBeCreatedWithNhsNumberAsync(
          referral.NhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        ex.Should().BeOfType<InvalidOperationException>(
          because: "when a referral selects a provider then " +
            "DateOfProvderSelection should be set to that date.");

        ex.Message.Should().Contain(
          referral.Id.ToString(),
          because: "the error message should contain the Id of the referral.");
      }
    }

    [Theory]
    [MemberData(nameof(CancelledOrCompleteStatus))]
    public async Task DateOfProviderSelectionTooSoon_ProviderSelected(
      ReferralStatus referralStatus)
    {
      // Arrange.
      DateTimeOffset dateOfProviderSelectionTooSoon = DateTimeOffset.Now.Date
        .AddDays(-Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 1);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: dateOfProviderSelectionTooSoon,
        dateStartedProgramme: null,
        providerId: Guid.NewGuid(),
        status: referralStatus);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(referral.NhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.ProviderSelected,
          because: "a provider was selected less than " +
            $"{Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION} days ago.");

        response.Reason.Should().Be(
          "According to our records you have previously registered for or " +
            "completed the NHS Digital Weight Management Programme. The " +
            "guidance states you can re-register on or after " +
            $"{DateTimeOffset.Now.Date.AddDays(1):dd/MM/yyyy}.");

        response.Referral.Should().BeEquivalentTo(referral, options => options
          .Excluding(x => x.Audits)
          .Excluding(x => x.Cri)
          .Excluding(x => x.DateToDelayUntil)
          .Excluding(x => x.ReferralQuestionnaire)
          .Excluding(x => x.SpellIdentifier)
          .Excluding(x => x.TextMessages)
          .Excluding(x => x.IsErsClosed));
      }
    }

    [Theory]
    [MemberData(nameof(CancelledOrCompleteStatus))]
    public async Task DateStartedProgrammeTooSoon_ProgrammeStarted(
      ReferralStatus referralStatus)
    {
      // Arrange.
      DateTimeOffset dateStartedProgrammeTooSoon = DateTimeOffset.Now.Date
        .AddDays(-Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 1);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: dateStartedProgrammeTooSoon.AddDays(-1),
        dateStartedProgramme: dateStartedProgrammeTooSoon,
        providerId: Guid.NewGuid(),
        status: referralStatus);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(referral.NhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.ProgrammeStarted,
          because: "the programme was started less than " +
            $"{Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME} days ago.");

        response.Reason.Should().Be(
          "According to our records you have previously registered for or " +
            "completed the NHS Digital Weight Management Programme. The " +
            "guidance states you can re-register on or after " +
            $"{DateTimeOffset.Now.Date.AddDays(1):dd/MM/yyyy}.");

        response.Referral.Should().BeEquivalentTo(referral, options => options
          .Excluding(x => x.Audits)
          .Excluding(x => x.Cri)
          .Excluding(x => x.DateToDelayUntil)
          .Excluding(x => x.ReferralQuestionnaire)
          .Excluding(x => x.SpellIdentifier)
          .Excluding(x => x.TextMessages)
          .Excluding(x => x.IsErsClosed));
      }
    }

    [Theory]
    [MemberData(nameof(NotCancelledOrCompleteStatus))]
    public async Task MoreThanOneActiveReferral_Exception(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: referralStatus);
      Entities.Referral referral2 = RandomEntityCreator.CreateRandomReferral(
        nhsNumber: referral1.NhsNumber,
        status: referralStatus);

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      _context.SaveChanges();

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.CanGeneralReferralBeCreatedWithNhsNumberAsync(
          referral1.NhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        ex.Should().BeOfType<InvalidOperationException>(
          because: "there should never be more than one in progress referral " +
            "for a NHS number.");

        ex.Message.Should().Be(
          "There is more than one referral " +
          $"that does not have a status of {ReferralStatus.Complete}, " +
          $"{ReferralStatus.Cancelled} or " +
          $"{ReferralStatus.CancelledByEreferrals} with an NHS number " +
          $"of {referral1.NhsNumber}.");
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
      ReferralSource.GeneralReferral, 
      ReferralSource.ElectiveCare,
      ReferralSource.SelfReferral})]
    public async Task IneligibleReferralSource(
      ReferralSource referralSource)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: ReferralStatus.New);

      string expectedReferralSource;
      switch (referralSource)
      {
        case ReferralSource.GpReferral:
          expectedReferralSource = "General Practice";
          break;
        case ReferralSource.Pharmacy:
          expectedReferralSource = "Community Pharmacy";
          break;
        case ReferralSource.Msk:
          expectedReferralSource = "Physiotherapist";
          break;
        default:
          throw new XunitException($"The ReferralSource {referralSource} " +
            $"should not be tested with this unit test.");
      }


      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(referral.NhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.IneligibleReferralSource,
          because: $"the referral source must be " +
            $"{ReferralSource.GeneralReferral} or" +
            $"{ReferralSource.ElectiveCare}.");

        response.Reason.Should().Be(
          "According to our records you have already been referred for the " +
            "NHS Digital Weight Management Programme by your " +
            $"{expectedReferralSource}. " +
            $"Please refer to the text message sent at the time of referral " +
            $"or alternatively call (01772 660 010) and a member of the team " +
            $"will be able to assist you further.");

        response.Referral.Should().BeEquivalentTo(referral, options => options
          .Excluding(x => x.Audits)
          .Excluding(x => x.Cri)
          .Excluding(x => x.DateToDelayUntil)
          .Excluding(x => x.ReferralQuestionnaire)
          .Excluding(x => x.SpellIdentifier)
          .Excluding(x => x.TextMessages)
          .Excluding(x => x.IsErsClosed));
      }
    }

    [Fact]
    public async Task IneligibleSelfReferralSource()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.SelfReferral,
        status: ReferralStatus.New);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      CanCreateReferralResponse response = await _service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(referral.NhsNumber);

      // Assert.
      using (new AssertionScope())
      {
        response.CanCreateResult.Should().Be(
          CanCreateReferralResult.IneligibleReferralSource,
          because: $"the referral source must be " +
            $"{ReferralSource.GeneralReferral} or" +
            $"{ReferralSource.ElectiveCare}.");

        response.Reason.Should().Be(
          "According to our records you have already registered for this " +
            "programme through the NHS Digital Weight Management Programme " +
            "Staff Self Referral Site. Please refer to the text message sent " +
            "at the time of referral or alternatively call (01772 660 010) " +
            "and a member of the team will be able to assist you further.");

        response.Referral.Should().BeEquivalentTo(referral, options => options
          .Excluding(x => x.Audits)
          .Excluding(x => x.Cri)
          .Excluding(x => x.DateToDelayUntil)
          .Excluding(x => x.ReferralQuestionnaire)
          .Excluding(x => x.SpellIdentifier)
          .Excluding(x => x.TextMessages)
          .Excluding(x => x.IsErsClosed));
      }
    }

    public static TheoryData<ReferralStatus> CancelledOrCompleteStatus()
    {
      return new TheoryData<ReferralStatus> 
      {
        ReferralStatus.Cancelled,
        ReferralStatus.CancelledByEreferrals,
        ReferralStatus.Complete,
      };
    }

    public static TheoryData<ReferralStatus> NotCancelledOrCompleteStatus()
    {
      return EnumTheoryData<ReferralStatus>(new ReferralStatus[]
      {
        ReferralStatus.Cancelled,
        ReferralStatus.CancelledByEreferrals,
        ReferralStatus.Complete,
      });
    }
  }
}
