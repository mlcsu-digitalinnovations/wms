using FluentAssertions;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WmsHub.Business.Helpers;
using WmsHub.Common.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class UbrnToIdTests : ReferralServiceTests
  {
    private readonly Entities.Referral _referral;

    public UbrnToIdTests(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    {
      Log.Logger = new LoggerConfiguration()
      .WriteTo
      .TestOutput(testOutputHelper, formatProvider: CultureInfo.InvariantCulture)
      .CreateLogger();

      _referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000001",
        providerUbrn: "GP0000000001");

      AddReferral(_referral);
    }

    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
      base.Dispose();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task DuplicateReferralsWithSameUbrn_ThrowInvalidOperationException()
    {
      // Arrange.
      Entities.Referral duplicateReferral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000001",
        providerUbrn: "GP0000000001");
      AddReferral(duplicateReferral);

      // Act.
      Func<Task> funcUbrn = async () => await _service.GetIdFromUbrn(_referral.Ubrn);
      Func<Task> funcProviderUbrn = async () => await _service
        .GetIdFromUbrn(_referral.ProviderUbrn);

      // Assert.
      await funcUbrn.Should().ThrowExactlyAsync<InvalidOperationException>();
      await funcProviderUbrn.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task IgnoreDisabledReferral_ReturnNull()
    {
      // Arrange.
      Entities.Referral disabledReferral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000002",
        providerUbrn: "GP0000000002",
        isActive: false);

      AddReferral(disabledReferral);

      // Act.
      string ubrnResult = await _service.GetIdFromUbrn(disabledReferral.Ubrn);
      string providerUbrnResult = await _service.GetIdFromUbrn(disabledReferral.ProviderUbrn);

      // Assert.
      ubrnResult.Should().BeNull();
      providerUbrnResult.Should().BeNull();
    }

    [Fact]
    public async Task IgnoreDuplicateDisabledReferral_ReturnId()
    {
      // Arrange.
      Entities.Referral duplicateDisabledReferral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: _referral.Ubrn,
        providerUbrn: _referral.ProviderUbrn,
        isActive: false);

      AddReferral(duplicateDisabledReferral);

      // Act.
      string ubrnResult = await _service.GetIdFromUbrn(duplicateDisabledReferral.Ubrn);
      string providerUbrnResult = await _service
        .GetIdFromUbrn(duplicateDisabledReferral.ProviderUbrn);

      // Assert.
      ubrnResult.Should().Be(_referral.Id.ToString());
      providerUbrnResult.Should().Be(_referral.Id.ToString());
    }

    [Theory]
    [InlineData("000000000009")]
    [InlineData("GP0000000009")]
    [InlineData("abc123")]
    public async Task ReferralNotFound_ReturnNull(string badUbrn)
    {
      // Arrange.

      // Act.
      string res = await _service.GetIdFromUbrn(badUbrn);

      // Assert.
      res.Should().BeNull();
    }

    [Fact]
    public async Task ReferralWithProviderUbrnExists_ReturnId()
    {
      // Arrange.

      // Act.
      string res = await _service.GetIdFromUbrn(_referral.ProviderUbrn);

      // Assert.
      res.Should().Be(_referral.Id.ToString());
    }

    [Fact]
    public async Task ReferralWithUbrnExists_ReturnId()
    {
      // Arrange.

      // Act.
      string res = await _service.GetIdFromUbrn(_referral.Ubrn);

      // Assert.
      res.Should().Be(_referral.Id.ToString());
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task UbrnIsNullOrWhitespace_ThrowArgumentNullOrWhiteSpaceException(
      string nullOrWhitespaceValue)
    {
      // Arrange.

      // Act.
      Func<Task> func = async () => await _service.GetIdFromUbrn(nullOrWhitespaceValue);

      // Assert.
      await func.Should().ThrowExactlyAsync<ArgumentNullOrWhiteSpaceException>();
    }

    private void AddReferral(Entities.Referral referral)
    {
      _context.Referrals.Add(referral);
      _context.SaveChanges();
    }
  }
}
