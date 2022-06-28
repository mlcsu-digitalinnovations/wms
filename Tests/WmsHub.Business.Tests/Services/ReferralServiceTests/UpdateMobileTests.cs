using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class UpdateMobileTests : ReferralServiceTests, IDisposable
    {
      private const string VALID_MOBILE = "+447012345678";
      private new readonly ReferralService _service;
      private readonly Mock<IProviderService> _mockProviderService = new();
      private readonly string _referralNotFoundExceptionMessage =
        "Unable to find a referral with an id of {referralId}.";

      private readonly Provider _mockProviderTriageLow = RandomModelCreator
        .CreateRandomProvider(level1: true, level2: false, level3: false);

      private readonly Provider _mockProviderTriageHigh = RandomModelCreator
        .CreateRandomProvider(level1: false, level2: false, level3: true);

      public UpdateMobileTests(ServiceFixture serviceFixture) :
        base(serviceFixture)
      {
        _service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _mockProviderService.Object,
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object)
        {
          User = GetClaimsPrincipal()
        };

        _mockProviderService
          .Setup(p => p.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == TriageLevel.Low)))
          .ReturnsAsync(new List<Provider>() { _mockProviderTriageLow });

        _mockProviderService
          .Setup(p => p.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == TriageLevel.High)))
          .ReturnsAsync(new List<Provider>() { _mockProviderTriageHigh });
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      [InlineData("+44701234567")]
      [InlineData("+4470123456789")]
      [InlineData("+446012345678")]
      [InlineData("446012345678")]
      public async Task InvalidMobile_ArgumentException(string mobile)
      {
        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateMobile(Guid.Empty, mobile));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be("Mobile is invalid (Parameter 'mobile')");
      }

      [Fact]
      public async Task ReferralDoesNotExist_ReferralNotFoundException()
      {
        // arrange
        var id = Guid.NewGuid();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateMobile(id, VALID_MOBILE));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", id.ToString()));
      }

      [Fact]
      public async Task ReferralIsInactive_ReferralNotFoundException()
      {
        // arrange
        var inactiveReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        _context.Referrals.Add(inactiveReferral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateMobile(inactiveReferral.Id, VALID_MOBILE));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", inactiveReferral.Id.ToString()));
      }

      [Fact]
      public async Task TriageLevelHigh_ProviderAvailable_OfferredHigh()
      {
        // arrange
        var expectedTriageLevel = TriageLevel.High.ToString("d");
        var expectedProvider = _mockProviderTriageHigh;

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          triagedCompletionLevel: expectedTriageLevel);

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateMobile(createdReferral.Id, VALID_MOBILE);

        // assert
        result.Should().BeEquivalentTo(createdReferral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.Cri)
          .Excluding(r => r.DateToDelayUntil)
          .Excluding(r => r.Mobile)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.OfferedCompletionLevel)
          .Excluding(r => r.TextMessages));
        result.DelayUntil.Should().Be(createdReferral.DateToDelayUntil);
        result.Mobile.Should().Be(VALID_MOBILE);
        result.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        result.ModifiedByUserId.Should().Be(TEST_USER_ID);
        result.OfferedCompletionLevel.Should().Be(expectedTriageLevel);
        result.TextMessages.Should().BeEmpty();
        result.TriagedCompletionLevel.Should().Be(expectedTriageLevel);

        result.Providers.Should().HaveCount(1);
        result.Providers.Single().Should().BeEquivalentTo(expectedProvider);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);
        updatedReferral.Should()
          .BeEquivalentTo(createdReferral, options => options
            .Excluding(r => r.Audits)
            .Excluding(r => r.Mobile)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.OfferedCompletionLevel));
        updatedReferral.Audits.Should()
          .HaveCount(createdReferral.Audits.Count + 1);
        updatedReferral.Mobile.Should().Be(VALID_MOBILE);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.OfferedCompletionLevel.Should()
          .Be(expectedTriageLevel);
      }

      [Fact]
      public async Task TriageLevelHigh_ProviderNotAvailable_OfferredLow()
      {
        // arrange
        var triagedCompletionLevel = TriageLevel.High.ToString("d");
        var expectedOfferredTriageLevel = TriageLevel.Low.ToString("d");
        var expectedProvider = _mockProviderTriageLow;

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          triagedCompletionLevel: triagedCompletionLevel);

        _mockProviderService
          .Setup(p => p.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == TriageLevel.High)))
          .ReturnsAsync(new List<Provider>());

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateMobile(createdReferral.Id, VALID_MOBILE);

        // assert
        result.Should().BeEquivalentTo(createdReferral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.Cri)
          .Excluding(r => r.DateToDelayUntil)
          .Excluding(r => r.Mobile)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.OfferedCompletionLevel)
          .Excluding(r => r.TextMessages));
        result.DelayUntil.Should().Be(createdReferral.DateToDelayUntil);
        result.Mobile.Should().Be(VALID_MOBILE);
        result.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        result.ModifiedByUserId.Should().Be(TEST_USER_ID);
        result.OfferedCompletionLevel.Should().Be(expectedOfferredTriageLevel);
        result.TextMessages.Should().BeEmpty();
        result.TriagedCompletionLevel.Should().Be(triagedCompletionLevel);

        result.Providers.Should().HaveCount(1);
        result.Providers.Single().Should().BeEquivalentTo(
          _mockProviderTriageLow);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);
        updatedReferral.Should()
          .BeEquivalentTo(createdReferral, options => options
            .Excluding(r => r.Audits)
            .Excluding(r => r.Mobile)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.OfferedCompletionLevel));
        updatedReferral.Audits.Should()
          .HaveCount(createdReferral.Audits.Count + 1);
        updatedReferral.Mobile.Should().Be(VALID_MOBILE);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.OfferedCompletionLevel.Should()
          .Be(expectedOfferredTriageLevel);
      }
    }
  }
}
