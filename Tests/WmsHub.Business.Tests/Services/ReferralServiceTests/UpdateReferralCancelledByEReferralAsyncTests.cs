using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {

    public class UpdateReferralCancelledByEReferralAsyncTests
      : ReferralServiceTests, IDisposable
    {

      public UpdateReferralCancelledByEReferralAsyncTests(
        ServiceFixture serviceFixture)
        : base(serviceFixture)
      {

      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task UbrnDoesNotExist_ReferralNotFoundException()
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(new Random());

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateReferralCancelledByEReferralAsync(ubrn));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be("An active referral was not found with a " +
          $"ubrn of {ubrn}.");
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData), 
        ReferralStatus.RejectedToEreferrals)]
      public async Task ReferralHasInvalidStatus_ReferralInvalidStatusException(
        ReferralStatus status)
      {
        // arrange
        var existingReferral = RandomEntityCreator.CreateRandomReferral(
          status: status);
        _context.Referrals.Add(existingReferral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateReferralCancelledByEReferralAsync(existingReferral.Ubrn));

        // assert
        ex.Should().BeOfType<ReferralInvalidStatusException>();
        ex.Message.Should().Be("Unable to cancel the referral because its " +
          $"status is {existingReferral.Status}.");
      }

      [Fact]
      public async Task ReferralRejected_Returns_1()
      {
        // arrange
        var rejectedReferral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.RejectedToEreferrals);
        _context.Referrals.Add(rejectedReferral);
        
        var cancelledReferral = RandomEntityCreator.CreateRandomReferral(
          ubrn: rejectedReferral.Ubrn,
          status: ReferralStatus.CancelledByEreferrals);
        _context.Referrals.Add(cancelledReferral);

        _context.SaveChanges();
        _context.Entry(rejectedReferral).State = EntityState.Detached;

        // act
        var result = await _service.UpdateReferralCancelledByEReferralAsync(
          rejectedReferral.Ubrn);

        // assert
        result.Should().Be(1);

        var updatedReferral = _context.Referrals
          .Where(r => r.Id == rejectedReferral.Id)
          .Single();

        updatedReferral.Should()
          .BeEquivalentTo(rejectedReferral, options => options
            .Excluding(r => r.Audits)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.Status));

        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.ModifiedAt.Should()
          .BeAfter(rejectedReferral.ModifiedAt);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.CancelledByEreferrals.ToString());
      }
    }
  }
}
