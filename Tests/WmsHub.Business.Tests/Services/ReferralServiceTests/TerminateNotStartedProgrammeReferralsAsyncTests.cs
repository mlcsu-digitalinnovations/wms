using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class TerminateNotStartedProgrammeReferralsAsyncTests : ReferralServiceTests, IDisposable
  {
    private readonly string _expectedStatusReason;
    private readonly Mock<IOptions<GpDocumentProxyOptions>> _gpDocumentProxyOptionsMock = new();
    private readonly ReferralTimelineOptions _referralTimelineOptions = new();
    private new readonly ReferralService _service;

    public TerminateNotStartedProgrammeReferralsAsyncTests(
      ServiceFixture fixture,
      ITestOutputHelper testOutputHelper)
      : base(fixture, testOutputHelper)
    {
      _expectedStatusReason = $"Service user did not start the programme within " +
          _referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection +
          " days of selecting a provider. Referral automatically terminated.";

      _service = new ReferralService(
        _context,
        _serviceFixture.Mapper,
        null,
        _mockDeprivationService.Object,
        _mockLinkIdService.Object,
        _mockPostcodeIoService.Object,
        _mockPatientTriageService.Object,
        _mockOdsOrganisationService.Object,
        _gpDocumentProxyOptionsMock.Object,
        Options.Create(_referralTimelineOptions),
        null,
        null)
      {
        User = GetClaimsPrincipal()
      };
    }

    [Fact]
    public async Task IsActiveFalseReturnsZero()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: false);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _service.TerminateNotStartedProgrammeReferralsAsync();

      // Assert.
      result.Should().Be(0);
    }

    [Fact]
    public async Task DateOfProviderSelectionNotExpiredReturnsZero()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.UtcNow
          .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection + 1)
      );
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _service.TerminateNotStartedProgrammeReferralsAsync();

      // Assert.
      result.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[] 
      { ReferralStatus.ProviderAccepted, ReferralStatus.ProviderContactedServiceUser })]
    public async Task StatusNotValidReturnsZero(ReferralStatus status)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.UtcNow
          .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection),
        status: status
      );
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _service.TerminateNotStartedProgrammeReferralsAsync();

      // Assert.
      result.Should().Be(0);
    }

    [Theory]
    [InlineData(ReferralStatus.ProviderAccepted)]
    [InlineData(ReferralStatus.ProviderContactedServiceUser)]
    public async Task StatusValidGpReferralReturnsOne(ReferralStatus status)
    {
      // Arrange.
      string expectedStatus = ReferralStatus.ProviderTerminated.ToString();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.UtcNow
          .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection - 1),
        status: status
      );
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _service.TerminateNotStartedProgrammeReferralsAsync();

      // Assert.
      result.Should().Be(1);
      referral.Status.Should().Be(expectedStatus);
      referral.StatusReason.Should().Be(_expectedStatusReason);
    }

    [Theory]
    [MemberData(nameof(TerminateNotStartedProgrammeReferralsTheoryData))]
    public async Task StatusValidOtherReferralSourceReturnsOne(
      ReferralStatus status,
      ReferralSource referralSource)
    {
      // Arrange.
      string expectedStatus = ReferralStatus.ProviderTerminatedTextMessage.ToString();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.UtcNow
          .AddDays(-_referralTimelineOptions.MaxDaysToStartProgrammeAfterProviderSelection - 1),
        referralSource: referralSource,
        status: status
      );
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _service.TerminateNotStartedProgrammeReferralsAsync();

      // Assert.
      result.Should().Be(1);
      referral.Status.Should().Be(expectedStatus);
      referral.StatusReason.Should().Be(_expectedStatusReason);
    }
  }
}

