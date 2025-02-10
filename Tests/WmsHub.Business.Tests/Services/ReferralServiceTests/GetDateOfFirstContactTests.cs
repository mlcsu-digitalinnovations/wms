using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests
{
  public class GetDateOfFirstContactTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : ReferralServiceTests(serviceFixture, testOutputHelper), IDisposable
  {
    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.TextMessages.RemoveRange(_context.TextMessages);
      _context.Calls.RemoveRange(_context.Calls);
      _context.SaveChanges();
      base.Dispose();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EmptyGuidThrowsArgumentException()
    {
      // Arrange.

      // Act.
      Func<Task<DateTimeOffset?>> result = () => _service.GetDateOfFirstContact(Guid.Empty);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task NoContactsReturnsNull()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      DateTimeOffset? dateOfFirstContact = await _service.GetDateOfFirstContact(referral.Id);

      // Assert.
      dateOfFirstContact.Should().BeNull();
    }


    [Fact]
    public async Task ReferralNotFoundReturnsNull()
    {
      // Arrange.

      // Act.
      DateTimeOffset? dateOfFirstContact = await _service.GetDateOfFirstContact(Guid.NewGuid());

      // Assert.
      dateOfFirstContact.Should().BeNull();
    }

    [Fact]
    public async Task ValidChatBotCallsOnlyReturnsLatestChatBotCall1Date()
    {
      // Arrange.
      DateTimeOffset? expectedDateOfFirstContact = DateTimeOffset.Now.AddDays(-10);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
      Entities.Call initialChatBotCall1 = RandomEntityCreator.CreateRandomChatBotCall(
        sent: expectedDateOfFirstContact.Value.AddDays(-20));
      Entities.Call resetChatBotCall1 = RandomEntityCreator.CreateRandomChatBotCall(
        sent: expectedDateOfFirstContact.Value);


      referral.Calls = [initialChatBotCall1, resetChatBotCall1];
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      DateTimeOffset? dateOfFirstContact = await _service.GetDateOfFirstContact(referral.Id);

      // Assert.
      dateOfFirstContact.Should().Be(expectedDateOfFirstContact);
    }

    [Fact]
    public async Task ValidTextMessagesAndChatBotCallsReturnsLatestTextMessage1SentDate()
    {
      // Arrange.
      DateTimeOffset? expectedDateOfFirstContact = DateTimeOffset.Now.AddDays(-10);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
      Entities.TextMessage initialTextMessage1 = RandomEntityCreator.CreateRandomTextMessage(
        sent: expectedDateOfFirstContact.Value.AddDays(-10),
        referralStatus: ReferralStatus.TextMessage1.ToString());
      Entities.TextMessage resetTextMessage1 = RandomEntityCreator.CreateRandomTextMessage(
        sent: expectedDateOfFirstContact.Value,
        referralStatus: ReferralStatus.TextMessage1.ToString());
      Entities.TextMessage textMessage2 = RandomEntityCreator.CreateRandomTextMessage(
        sent: expectedDateOfFirstContact.Value.AddDays(2),
        referralStatus: ReferralStatus.TextMessage2.ToString());
      Entities.Call chatBotCall1 = RandomEntityCreator.CreateRandomChatBotCall(
        sent: expectedDateOfFirstContact.Value.AddDays(4));

      referral.TextMessages = [initialTextMessage1, resetTextMessage1, textMessage2];
      referral.Calls = [chatBotCall1];
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      DateTimeOffset? dateOfFirstContact = await _service.GetDateOfFirstContact(referral.Id);

      // Assert.
      dateOfFirstContact.Should().Be(expectedDateOfFirstContact);
    }
  }
}

