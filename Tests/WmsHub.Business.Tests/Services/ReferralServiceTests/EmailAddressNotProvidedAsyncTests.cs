using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class EmailAddressNotProvidedAsyncTests : ReferralServiceTests, IDisposable
  {
    public EmailAddressNotProvidedAsyncTests(
      ServiceFixture fixture,
      ITestOutputHelper testOutputHelper)
      : base(fixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNotFoundThrowsException()
    {
      // Arrange.
      Guid id = Guid.NewGuid();

      // Act.
      Func<Task<Business.Models.IReferral>> output = () => _service.EmailAddressNotProvidedAsync(id);

      // Assert.
      await output.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ReferralProviderSelectedThrowsException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        providerId: Guid.NewGuid());

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<Business.Models.IReferral>> output =
        () => _service.EmailAddressNotProvidedAsync(referral.Id);

      // Assert.
      await output.Should().ThrowAsync<ReferralProviderSelectedException>();
    }

    [Fact]
    public async Task ValidReferralSetsPropertiesAndReturnsReferral()
    {
      // Arrange.
      string expectedStatusReason = "Service user did not want to provide an email address.";

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        consentForFutureContactForEvaluation: true,
        email: "test@email.com",
        status: ReferralStatus.TextMessage1);

      TextMessage textMessage = RandomEntityCreator.CreateRandomTextMessage(
        referralId: referral.Id);

      referral.TextMessages = new() { textMessage };

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Business.Models.IReferral output = await _service.EmailAddressNotProvidedAsync(referral.Id);

      // Assert.
      Referral updatedReferral = _context.Referrals
        .Where(r => r.Id == referral.Id)
        .Include(r => r.TextMessages)
        .SingleOrDefault();

      ReferralAudit referralAudit = _context.ReferralsAudit
        .Where(r => r.Id == referral.Id)
        .Where(r => r.Status == ReferralStatus.Exception.ToString())
        .SingleOrDefault();

      updatedReferral.Should().NotBeNull();
      referralAudit.Should().NotBeNull();
      updatedReferral.ConsentForFutureContactForEvaluation.Should().BeFalse();
      updatedReferral.Email.Should().Be(WmsHub.Common.Helpers.Constants.DO_NOT_CONTACT_EMAIL);
      updatedReferral.IsErsClosed.Should().BeNull();
      updatedReferral.Status.Should().Be(ReferralStatus.FailedToContact.ToString());
      updatedReferral.StatusReason.Should().Be(expectedStatusReason);
      updatedReferral.TextMessages.SingleOrDefault().Should().NotBeNull().And.BeOfType<TextMessage>()
        .Subject.Outcome.Should().Be(WmsHub.Common.Helpers.Constants.DO_NOT_CONTACT_EMAIL);
    }
  }
}

