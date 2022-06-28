using FluentAssertions;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {

    public class IsEmailInUseAsyncTests : ReferralServiceTests, IDisposable
    {

      public IsEmailInUseAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task EmailNotInUse_FalseNull()
      {
        // arrange
        var email = "unit.test@microsoft.com";

        // act
        var response = await _service.IsEmailInUseAsync(email);

        // assert
        response.WasFound.Should().BeFalse();
        response.WasNotFound.Should().BeTrue();
        response.IsCompleteAndProviderNotSelected.Should().BeFalse();
      }

      [Fact]
      public async Task EmailInUse_True_StatusNew()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var response = await _service.IsEmailInUseAsync(referral.Email);

        // assert
        response.WasFound.Should().BeTrue();
        response.WasNotFound.Should().BeFalse();
        response.IsCompleteAndProviderNotSelected.Should().BeFalse();
      }

      [Fact]
      public async Task EmailInUse_True_StatusComplete_ProviderNotSelected()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: Enums.ReferralStatus.Complete);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var response = await _service.IsEmailInUseAsync(referral.Email);

        // assert
        response.WasFound.Should().BeTrue();
        response.WasNotFound.Should().BeFalse();
        response.IsCompleteAndProviderNotSelected.Should().BeTrue();
      }

      [Fact]
      public async Task EmailInUse_True_StatusComplete_ProviderSelected()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: Enums.ReferralStatus.Complete,
          providerId: Guid.NewGuid());
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var response = await _service.IsEmailInUseAsync(referral.Email);

        // assert
        response.WasFound.Should().BeTrue();
        response.WasNotFound.Should().BeFalse();
        response.IsCompleteAndProviderNotSelected.Should().BeFalse();
      }

    }
  }
}
