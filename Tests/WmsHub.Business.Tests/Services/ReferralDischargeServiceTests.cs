using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ReferralDischargeServiceTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly IReferralDischargeService _service;
  private readonly ProviderOptions _options = new();

  public ReferralDischargeServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    _service = new ReferralDischargeService(
      _context,
      Options.Create(_options))
    {
      User = GetClaimsPrincipal()
    };
  }

  public class GetPreparedDischargesAsync : ReferralDischargeServiceTests
  {
    private readonly Entities.Referral _referral;
    private readonly Entities.Provider _provider;

    public GetPreparedDischargesAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      _provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(_provider);

      _referral = RandomEntityCreator.CreateRandomReferral(
        dateStartedProgramme: DateTimeOffset.Now
          .AddDays(-(_options.DischargeAfterDays + 1)),
        isActive: true,
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.ProviderCompleted);
      _referral.DateCompletedProgramme = null;
      _referral.FirstRecordedWeight = null;
      _referral.FirstRecordedWeightDate = null;
      _referral.LastRecordedWeight = null;
      _referral.LastRecordedWeightDate = null;
      _referral.ProgrammeOutcome = null;
      _referral.Provider = _provider;
      _context.Referrals.Add(_referral);

      _context.SaveChanges();
    }

    [Fact]
    public async void TwoSubmissionsSameDate_FirstAndLastShouldBeTheSame()
    {
      var expectedFirstRecordedWeight = 90;
      var expectedLastRecordedWeight = 85;

      // arrange
      var firstSub = RandomEntityCreator.CreateProviderSubmission(
        date: _referral.DateStartedProgramme.Value,
        modifiedAt: _referral.DateStartedProgramme.Value,
        providerId: _provider.Id,
        referralId: _referral.Id,
        weight: 250);

      var correctedFirstSub = RandomEntityCreator.CreateProviderSubmission(
        date: firstSub.Date,
        modifiedAt: firstSub.ModifiedAt.AddDays(10),
        providerId: firstSub.ProviderId,
        referralId: firstSub.ReferralId,
        weight: expectedFirstRecordedWeight);

      var lastSub = RandomEntityCreator.CreateProviderSubmission(
        date: DateTimeOffset.Now.AddDays(-1),
        modifiedAt: DateTimeOffset.Now.AddDays(-1),
        providerId: _provider.Id,
        referralId: _referral.Id,
        weight: 300);

      var correctedLastSub = RandomEntityCreator.CreateProviderSubmission(
        date: lastSub.Date,
        modifiedAt: lastSub.Date.AddDays(1),
        providerId: lastSub.ProviderId,
        referralId: lastSub.ReferralId,
        weight: expectedLastRecordedWeight);

      _context.ProviderSubmissions.Add(firstSub);
      _context.ProviderSubmissions.Add(correctedFirstSub);
      _context.ProviderSubmissions.Add(lastSub);
      _context.ProviderSubmissions.Add(correctedLastSub);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // act
      var result = _service.PrepareDischarges();

      // assert
      var referral = _context.Referrals
        .Include(r => r.Provider)
        .Include(r => r.ProviderSubmissions)
        .Single(r => r.Id.Equals(_referral.Id));
      referral.Should().BeEquivalentTo(_referral, options => options
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

      referral.Audits.Count.Should().Be(1);
      referral.DateCompletedProgramme.Should().Be(correctedLastSub.Date);
      referral.FirstRecordedWeight.Should().Be(expectedFirstRecordedWeight);
      referral.FirstRecordedWeightDate.Should().Be(correctedFirstSub.Date);
      referral.LastRecordedWeight.Should().Be(expectedLastRecordedWeight);
      referral.LastRecordedWeightDate.Should().Be(correctedLastSub.Date);
      referral.ModifiedAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      referral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      referral.ProgrammeOutcome.Should()
        .Be(ProgrammeOutcome.Complete.ToString());
      referral.Provider.Should().NotBeNull();
      referral.ProviderSubmissions.Count.Should().Be(4);
      referral.Status.Should()
        .Be(ReferralStatus.AwaitingDischarge.ToString());
    }
  }
}
