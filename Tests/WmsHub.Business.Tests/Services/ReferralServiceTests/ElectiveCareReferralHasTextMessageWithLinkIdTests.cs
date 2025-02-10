using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class ElectiveCareReferralHasTextMessageWithLinkIdTests : ReferralServiceTests, IDisposable
  {
    private new readonly ReferralService _service;

    public ElectiveCareReferralHasTextMessageWithLinkIdTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) : base(serviceFixture, testOutputHelper)
    {
      _service = new ReferralService(
        _context,
        _serviceFixture.Mapper,
        null,
        _mockDeprivationService.Object,
        _mockLinkIdService.Object,
        _mockPostcodeIoService.Object,
        _mockPatientTriageService.Object,
        _mockOdsOrganisationService.Object,
        _mockGpDocumentProxyOptions.Object,
        _mockReferralTimelineOptions.Object,
        null,
        _log)
      {
        User = GetClaimsPrincipal()
      };
    }

    public new void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.TextMessages.RemoveRange(_context.TextMessages);
      _context.SaveChanges();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task InactiveReferralReturnsFalse()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      Referral referral = RandomEntityCreator.CreateRandomReferral(isActive: false);
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        serviceUserLinkId: linkId,
        referralId: referral.Id);
      textMessage.Referral = referral;
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task LinkIdIsNullOrWhitespaceThrowsException(string linkId)
    {
      // Arrange.

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId));

      // Assert.
      ex.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task NoActiveTextMessagesReturnsFalse()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        isActive: false,
        serviceUserLinkId: linkId);
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public async Task NonMatchingLinkIdReturnsFalse()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      string textMessageLinkId = LinkIdService.GenerateDummyId();
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        serviceUserLinkId: textMessageLinkId);
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData),
      new ReferralSource[] { ReferralSource.ElectiveCare })]
    public async Task NonMatchingReferralSourceReturnsFalse(ReferralSource referralSource)
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      Referral referral = RandomEntityCreator.CreateRandomReferral(referralSource: referralSource);
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        serviceUserLinkId: linkId,
        referralId: referral.Id);
      textMessage.Referral = referral;
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[] {
      ReferralStatus.ChatBotCall1,ReferralStatus.ChatBotTransfer, ReferralStatus.New,
      ReferralStatus.RmcCall, ReferralStatus.RmcDelayed, ReferralStatus.TextMessage1,
      ReferralStatus.TextMessage2, ReferralStatus.TextMessage3 })]
    public async Task NonMatchingReferralStatusReturnsFalse(ReferralStatus referralStatus)
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.ElectiveCare,
        status: referralStatus);
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        serviceUserLinkId: linkId,
        referralId: referral.Id);
      textMessage.Referral = referral;
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public async Task NoReferralReturnsFalse()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        serviceUserLinkId: linkId);
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public async Task NoTextMessagesReturnsFalse()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeFalse();
    }

    [Theory]
    [InlineData(ReferralStatus.ChatBotCall1)]
    [InlineData(ReferralStatus.ChatBotTransfer)]
    [InlineData(ReferralStatus.New)]
    [InlineData(ReferralStatus.RmcCall)]
    [InlineData(ReferralStatus.RmcDelayed)]
    [InlineData(ReferralStatus.TextMessage1)]
    [InlineData(ReferralStatus.TextMessage2)]
    [InlineData(ReferralStatus.TextMessage3)]
    public async Task ValidReturnsTrue(ReferralStatus referralStatus)
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.ElectiveCare,
        status: referralStatus);
      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id,
        serviceUserLinkId: linkId);
      textMessage.Referral = referral;
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
      _context.Entry(textMessage).State = EntityState.Detached;

      // Act.
      bool result = await _service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      // Assert.
      result.Should().BeTrue();
    }
  }
}