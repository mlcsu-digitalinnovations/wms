using FluentAssertions;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {

    public class IsNhsNumberInUseAsyncTests : ReferralServiceTests, IDisposable
    {

      public IsNhsNumberInUseAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Theory]      
      [InlineData(null)]
      [InlineData("")]
      [InlineData(" ")]
      public async Task NhsNumberNullOrWhiteSpace_InUseReponseNotFound(
        string nhsNumber)
      {
        // arrange
        string expectedExceptionMessage = new ArgumentException(
          $"'{nameof(nhsNumber)}' cannot be null or whitespace.",
          nameof(nhsNumber))
          .Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.IsNhsNumberInUseAsync(nhsNumber));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task NhsNumberNotInUse_InUseReponseNotFound()
      {
        // arrange
        var nhsNumber = "9996529991";

        // act
        var response = await _service.IsNhsNumberInUseAsync(nhsNumber);

        // assert
        response.Should().NotBeNull();
        response.InUseResult.HasFlag(InUseResult.NotFound).Should().BeTrue();
      }

      [Fact]
      public async Task NhsNumberInUse_InUseReponseFound()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var response = await _service.IsNhsNumberInUseAsync(referral.NhsNumber);

        // assert
        response.Should().NotBeNull();
        response.InUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      }

    }
  }
}
