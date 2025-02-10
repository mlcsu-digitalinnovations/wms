using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{

  public class GetServiceUserLinkIdAsyncTests : ReferralServiceTests, IDisposable
  {

    public GetServiceUserLinkIdAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      Dispose();
    }

    public override void Dispose()
    {
      _context.TextMessages.RemoveRange(_context.TextMessages);
      _context.SaveChanges();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task MatchedServiceUserLinkIdOfMatchedTextMessage()
    {
      // Arrange.
      Business.Models.IReferral referral = new Business.Models.Referral
      {
        Id = Guid.NewGuid()
      };

      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id);
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.GetServiceUserLinkIdAsync(referral);
      TextMessage loadedTextMessage = _context.TextMessages
        .Find(textMessage.Id);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().Be(textMessage.ServiceUserLinkId);
        textMessage.Should().BeEquivalentTo(loadedTextMessage);
      }
    }

    [Fact]
    public async Task MultipleMatchesServiceUserLinkIdOfFirstMatchedTextMessage()
    {
      // Arrange.
      Business.Models.IReferral referral = new Business.Models.Referral
      {
        Id = Guid.NewGuid()
      };

      TextMessage textMessage1 = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id,
        sent: DateTimeOffset.Now.AddDays(-1));
      _context.TextMessages.Add(textMessage1);

      TextMessage textMessage2 = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id,
        sent: DateTimeOffset.Now);
      _context.TextMessages.Add(textMessage2);

      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.GetServiceUserLinkIdAsync(referral);
      TextMessage loadedTextMessage = _context.TextMessages
        .Find(textMessage1.Id);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().Be(textMessage1.ServiceUserLinkId);
        textMessage1.Should().BeEquivalentTo(loadedTextMessage);
      }
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task MatchedBlankServiceUserLinkIdTextMessageCreated(string serviceUserLinkId)
    {
      // Arrange.
      Business.Models.IReferral referral = new Business.Models.Referral
      {
        Id = Guid.NewGuid(),
        Mobile = Generators.GenerateMobile(new Random())
      };

      _mockLinkIdService.Setup(x => x.GetUnusedLinkIdAsync(It.IsAny<int>()))
        .ReturnsAsync(LinkIdService.GenerateDummyId());

      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id);
      textMessage.ServiceUserLinkId = serviceUserLinkId;
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.GetServiceUserLinkIdAsync(referral);
      TextMessage loadedTextMessage = _context.TextMessages
        .Where(x => x.ServiceUserLinkId == result)
        .Single();

      // Assert.
      using (new AssertionScope())
      {
        DateTimeOffset now = DateTimeOffset.Now;
        TimeSpan fiveSeconds = new(0, 0, 5);
        result.Should().Be(loadedTextMessage.ServiceUserLinkId);
        loadedTextMessage.IsActive.Should().BeTrue();
        loadedTextMessage.ModifiedAt.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.ModifiedByUserId.Should().Be(TEST_USER_ID);
        loadedTextMessage.Number.Should().Be(referral.Mobile);
        loadedTextMessage.ReferralId.Should().Be(referral.Id);
        loadedTextMessage.Outcome.Should()
          .Be(CallbackStatus.GeneratedByRmcCall.GetDescriptionAttributeValue());
        loadedTextMessage.Received.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.Sent.Should().BeCloseTo(now, fiveSeconds);
      }
    }

    [Fact]
    public async Task MatchedInactiveTextMessageCreated()
    {
      // Arrange.
      Business.Models.IReferral referral = new Business.Models.Referral
      {
        Id = Guid.NewGuid(),
        Mobile = Generators.GenerateMobile(new Random())
      };

      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        isActive: false,
        referralId: referral.Id);
      _context.TextMessages.Add(textMessage);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      string result = await _service.GetServiceUserLinkIdAsync(referral);
      TextMessage loadedTextMessage = _context.TextMessages
        .Where(x => x.ServiceUserLinkId == result)
        .Single();

      // Assert.
      using (new AssertionScope())
      {
        DateTimeOffset now = DateTimeOffset.Now;
        TimeSpan fiveSeconds = new(0, 0, 5);
        result.Should().Be(loadedTextMessage.ServiceUserLinkId);
        loadedTextMessage.IsActive.Should().BeTrue();
        loadedTextMessage.ModifiedAt.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.ModifiedByUserId.Should().Be(TEST_USER_ID);
        loadedTextMessage.Number.Should().Be(referral.Mobile);
        loadedTextMessage.ReferralId.Should().Be(referral.Id);
        loadedTextMessage.Outcome.Should()
          .Be(CallbackStatus.GeneratedByRmcCall.GetDescriptionAttributeValue());
        loadedTextMessage.Received.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.Sent.Should().BeCloseTo(now, fiveSeconds);
      }
    }

    [Fact]
    public async Task NoMatchTextMessageCreated()
    {
      // Arrange.
      Business.Models.IReferral referral = new Business.Models.Referral
      {
        Id = Guid.NewGuid(),
        Mobile = Generators.GenerateMobile(new Random())
    };

      // Act.
      string result = await _service.GetServiceUserLinkIdAsync(referral);
      TextMessage loadedTextMessage = _context.TextMessages
        .Where(x => x.ServiceUserLinkId == result)
        .Single();

      // Assert.
      using (new AssertionScope())
      {
        DateTimeOffset now = DateTimeOffset.Now;
        TimeSpan fiveSeconds = new(0, 0, 5);
        result.Should().Be(loadedTextMessage.ServiceUserLinkId);
        loadedTextMessage.IsActive.Should().BeTrue();
        loadedTextMessage.ModifiedAt.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.ModifiedByUserId.Should().Be(TEST_USER_ID);
        loadedTextMessage.Number.Should().Be(referral.Mobile);
        loadedTextMessage.ReferralId.Should().Be(referral.Id);
        loadedTextMessage.Outcome.Should()
          .Be(CallbackStatus.GeneratedByRmcCall.GetDescriptionAttributeValue());
        loadedTextMessage.Received.Should().BeCloseTo(now, fiveSeconds);
        loadedTextMessage.Sent.Should().BeCloseTo(now, fiveSeconds);
      }
    }
  }
}
