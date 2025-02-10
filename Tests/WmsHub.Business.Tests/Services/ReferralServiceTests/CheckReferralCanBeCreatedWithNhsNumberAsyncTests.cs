using FluentAssertions;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class CheckReferralCanBeCreatedWithNhsNumberAsyncTests 
    : ReferralServiceTests, IDisposable
  {
    public CheckReferralCanBeCreatedWithNhsNumberAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      // Clean up.
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();

      Log.Logger = new LoggerConfiguration()
      .WriteTo.TestOutput(testOutputHelper)
      .CreateLogger();
    }

    public new void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    // No NHS number match
    // Result: create pharmacy referral
    [Fact]
    public async Task NoNhsNumberMatch_Valid()
    {
      // Arrange.
      Random rnd = new Random();
      string nhsNumber = Generators.GenerateNhsNumber(rnd);

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
          _service.CheckReferralCanBeCreatedWithNhsNumberAsync(nhsNumber));

      // Assert.
      ex.Should().BeNull();
    }

    // NHS number match not all Statuses are:
    // Cancelled, CancelledByEreferrals or Complete
    // result: exception
    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[] {
      ReferralStatus.Cancelled,
      ReferralStatus.CancelledByEreferrals,
      ReferralStatus.Complete})]
    public async Task StatusNotCancelledByEreferralsOrComplete_Exception(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: referralStatus);
      _context.Referrals.Add(referral);

      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
        nhsNumber: referral.NhsNumber,
        status: ReferralStatus.Complete);
      _context.Referrals.Add(completeReferral);

      Entities.Referral cancelledByEReferralsReferral = 
        RandomEntityCreator.CreateRandomReferral(
        nhsNumber: referral.NhsNumber,
        status: ReferralStatus.CancelledByEreferrals);
      _context.Referrals.Add(cancelledByEReferralsReferral);

      Entities.Referral cancelledReferral =
        RandomEntityCreator.CreateRandomReferral(
        nhsNumber: referral.NhsNumber,
        status: ReferralStatus.Cancelled);
      _context.Referrals.Add(cancelledReferral);

      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMessage = $"Referral cannot be created because there" +
        $" are in progress referrals with the same NHS number: (UBRN " +
        $"{referral.Ubrn}).";

      // Act.
      ReferralNotUniqueException ex = await Assert
        .ThrowsAsync<ReferralNotUniqueException>(() => _service
          .CheckReferralCanBeCreatedWithNhsNumberAsync(referral.NhsNumber));

      // Assert.
      ex.Should().NotBeNull();
      ex.Message.Should().Be(expectedMessage);
    }

    // NHS number match
    // All Statuses are CancelledByEreferrals or Complete
    // Not all provider selected
    // Result: create pharmacy referral
    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Complete)]
    public async Task StatusCancelledOrComplete_Valid(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: referralStatus
        );
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
          .CheckReferralCanBeCreatedWithNhsNumberAsync(referral.NhsNumber));

      // Assert
      ex.Should().BeNull();
    }

    // NHS number match
    // All Statuses are CancelledByEreferrals or Complete
    // All provider selected
    // All DateStartedProgramme null
    // <42 days since last DateOfProviderSelection
    // Result: exception
    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Complete)]
    public async Task DateStartedProgrammeNullNoReEntry_Exception(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral latestReferral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 2),
        providerId: Guid.NewGuid(),
        status: referralStatus);
      _context.Referrals.Add(latestReferral);

      Entities.Referral earlierReferral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 1),
        nhsNumber: latestReferral.NhsNumber,
        providerId: Guid.NewGuid(),
        status: referralStatus);

      _context.Referrals.Add(earlierReferral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string latestReferralDateOfProviderSelection = latestReferral
        .DateOfProviderSelection
        .Value
        .AddDays(MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 1)
        .Date
        .ToString("yyyy-MM-dd");

      // Act.
      ReferralNotUniqueException ex = await Assert
        .ThrowsAsync<ReferralNotUniqueException>(() => _service
          .CheckReferralCanBeCreatedWithNhsNumberAsync(
            latestReferral.NhsNumber));

      // Assert.
      
      ex.Should().NotBeNull();
      ex.Message.Should().Be("Referral can be created from " +
        latestReferralDateOfProviderSelection + 
        $" as an existing referral for this NHS number (UBRN " + 
        $"{latestReferral.Ubrn}) selected a provider but did not start " +
        $"the programme.");
    }

    // NHS number match
    // all Statuses are CancelledByEreferrals or Complete
    // all provider selected
    // all DateStartedProgramme null
    // 42 or more days since last DateOfProviderSelection
    // result: create pharmacy referral
    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Complete)]
    public async Task DateStartedProgrammeNullReEntry_Valid(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION - 1),
        providerId: Guid.NewGuid(),
        status: referralStatus
        );
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .CheckReferralCanBeCreatedWithNhsNumberAsync(referral.NhsNumber));

      // Assert.
      ex.Should().BeNull();
    }

    // NHS number match
    // all Statuses are CancelledByEreferrals or Complete
    // all provider selected
    // NOT all DateStartedProgramme null
    // <252 days since last DateStartedProgramme selection
    // result: exception
    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Complete)]
    public async Task DateStartedProgrammeNotNullNoReEntry_Exception(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral latestReferral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 2),
        providerId: Guid.NewGuid(),
        status: referralStatus);
      _context.Referrals.Add(latestReferral);

      Entities.Referral earlierReferral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 1),
        providerId: Guid.NewGuid(),
        nhsNumber: latestReferral.NhsNumber,
        status: referralStatus);
      _context.Referrals.Add(earlierReferral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string latestReferralDateStartedProgramme = latestReferral
        .DateStartedProgramme
        .Value
        .AddDays(MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 1)
        .Date.ToString("yyyy-MM-dd");

      // Act.
      ReferralNotUniqueException ex = await Assert
        .ThrowsAsync<ReferralNotUniqueException>(() => _service
          .CheckReferralCanBeCreatedWithNhsNumberAsync(
            latestReferral.NhsNumber));

      // Assert.
      ex.Should().NotBeNull();
      ex.Message.Should().Be("Referral can be created from " +
        latestReferralDateStartedProgramme +
        " as an existing referral for this NHS number (UBRN " +
        $"{latestReferral.Ubrn}) started the programme.");
    }

    // NHS number match
    // all Statuses are CancelledByEreferrals or Complete
    // all provider selected
    // NOT all DateStartedProgramme null
    // 252 or more days since last DateStartedProgramme 
    // result: create pharmacy referral
    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Complete)]
    public async Task DateStartedProgrammeNotNullReEntry_Valid(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-MIN_DAYS_SINCE_DATESTARTEDPROGRAMME - 1),
        providerId: Guid.NewGuid(),
        status: referralStatus
        );
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .CheckReferralCanBeCreatedWithNhsNumberAsync(referral.NhsNumber));

      // Assert.
      ex.Should().BeNull();
    }
  }
}
