using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class DischargeReferralAsyncTests : ReferralServiceTests
    {
      public DischargeReferralAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        // clean up
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralNotFound_Exception()
      {
        // arrange
        Guid id = Guid.NewGuid();
        string expectedMessage = new ReferralNotFoundException(id).Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.DischargeReferralAsync(id));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedMessage);
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData),
        ReferralStatus.AwaitingDischarge)]
      public async Task ReferralStatusNotAwaitingDischarge_Exception(
        ReferralStatus referralStatus)
      {
        // arrange
        Referral referral = RandomEntityCreator.CreateRandomReferral(
          status: referralStatus);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        string expectedMessage = $"Referral {referral.Id} has an " +
          $"invalid status of {referral.Status}, expected " +
          $"{ReferralStatus.AwaitingDischarge}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.DischargeReferralAsync(referral.Id));

        // assert
        ex.Should().BeOfType<ReferralInvalidStatusException>();
        ex.Message.Should().Be(expectedMessage);
      }

      [Fact]
      public async Task ReferralStatusAwaitingDischarge_StatusComplete()
      {
        // arrange
        Referral referral = RandomEntityCreator.CreateRandomReferral(
          modifiedAt: DateTimeOffset.Now,
          status: ReferralStatus.AwaitingDischarge);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.Entry(referral).State = EntityState.Detached;

        // act
        await _service.DischargeReferralAsync(referral.Id);

        // assert
        Referral updatedReferral = _context.Referrals.Find(referral.Id);
        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Status));
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
      }
    }
  }
}
