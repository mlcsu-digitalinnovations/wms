using FluentAssertions;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {

    public class IsEmailInUseAsyncTests : ReferralServiceTests, IDisposable
    {

      public IsEmailInUseAsyncTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      public new void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task EmailNotInUse_NoException()
      {
        // arrange
        var email = "unit.test@microsoft.com";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(email));

        // assert
        ex.Should().BeNull();
      }

      [Fact]
      public async Task EmailInUse_ReferralNotUniqueException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(referral.Email));

        // assert
        ex.Should().BeOfType(typeof(ReferralNotUniqueException));
        ex.Message.Should().Be(
          $"Referral cannot be created because there are in progress " +
          $"referrals with the same Email Address: (UBRN {referral.Ubrn}).");
      }

      [Fact]
      public async Task EmailInUse_ProvSelLT42Days_ReferralNotUniqueException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(-30),
          providerId: Guid.NewGuid(),
          status: ReferralStatus.Complete);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(referral.Email));

        // assert
        ex.Should().BeOfType(typeof(ReferralNotUniqueException));
        ex.Message.Should().Contain("did not start the programme.");
      }

      [Fact]
      public async Task EmailInUse_ProvSelGT42Days_NoException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(-43),
          providerId: Guid.NewGuid(),
          status: ReferralStatus.Complete);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(referral.Email));

        // assert
        ex.Should().BeNull();
      }

      [Fact]
      public async Task EmailInUse_ProvStLT252Days_ReferralNotUniqueException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(-250),
          dateStartedProgramme: DateTimeOffset.Now.AddDays(-250),
          providerId: Guid.NewGuid(),
          status: ReferralStatus.Complete);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(referral.Email));

        // assert
        ex.Should().BeOfType(typeof(ReferralNotUniqueException));
        ex.Message.Should().Contain("started the programme.");
      }

      [Fact]
      public async Task EmailInUse_ProvStGT252Days_NoException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(-253),
          dateStartedProgramme: DateTimeOffset.Now.AddDays(-253),
          providerId: Guid.NewGuid(),
          status: ReferralStatus.Complete);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsEmailInUseAsync(referral.Email));

        // assert
        ex.Should().BeNull();
      }

    }
  }
}
