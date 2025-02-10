using FluentAssertions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using WmsHub.Business.Helpers;
using WmsHub.Common.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class UbrnsToIdsTests : ReferralServiceTests
  {
    private readonly Entities.Referral _referral1;
    private readonly Entities.Referral _referral2;
    private readonly Entities.Referral _referral3;

    public UbrnsToIdsTests(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    {
      Log.Logger = new LoggerConfiguration()
      .WriteTo
      .TestOutput(testOutputHelper, formatProvider: CultureInfo.InvariantCulture)
      .CreateLogger();

      _referral1 = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000001",
        providerUbrn: "GP0000000001");

      _referral2 = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000002",
        providerUbrn: "GP0000000002");

      _referral3 = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        ubrn: "000000000003",
        providerUbrn: "GP0000000003");

      AddReferral(_referral1);
      AddReferral(_referral2);
      AddReferral(_referral3);
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
      Func<Task> funcUbrn = async () => await _service
        .GetIdsFromUbrns([duplicateReferral.Ubrn]);

      Func<Task> funcProviderUbrn = async () => await _service
        .GetIdsFromUbrns([duplicateReferral.ProviderUbrn]);

      // Assert.
      await funcUbrn.Should().ThrowExactlyAsync<InvalidOperationException>();
      await funcProviderUbrn.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task IdsShouldBeInOrderOfUbrnInput_ReturnIdsInCorrectOrder()
    {
      // Arrange.
      string[] ubrns =
      [
        _referral3.Ubrn,
        _referral1.Ubrn,
        _referral2.Ubrn,
      ];

      string[] expectedIds =
      [
        _referral3.Id.ToString(),
        _referral1.Id.ToString(),
        _referral2.Id.ToString(),
      ];

      // Act.
      IEnumerable<string> res = await _service.GetIdsFromUbrns(ubrns);

      // Assert.
      res.Should().Equal(expectedIds);
    }

    [Theory]
    [InlineData("000000000009")]
    [InlineData("GP0000000009")]
    public async Task ReferralNotFound_ReturnArrayWithNullValue(string badUbrn)
    {
      // Arrange.
      string[] ubrns =
      [
        _referral1.Ubrn,
        badUbrn,
        _referral3.Ubrn,
      ];

      string[] expectedIds =
      [
        _referral1.Id.ToString(),
        null,
        _referral3.Id.ToString(),
      ];

      // Act.
      IEnumerable<string> res = await _service.GetIdsFromUbrns(ubrns);

      // Assert.
      res.Should().Equal(expectedIds);
    }

    [Fact]
    public async Task ReferralWithProviderUbrnExists_ReturnIds()
    {
      // Arrange.
      string[] providerUrns =
      [
        _referral1.ProviderUbrn,
        _referral2.ProviderUbrn,
        _referral3.ProviderUbrn,
      ];

      string[] expectedIds =
      [
        _referral1.Id.ToString(),
        _referral2.Id.ToString(),
        _referral3.Id.ToString(),
      ];

      // Act.
      IEnumerable<string> res = await _service.GetIdsFromUbrns(providerUrns);

      // Assert.
      res.Should().Equal(expectedIds);
    }

    [Fact]
    public async Task ReferralWithUbrnsAndProviderUbrnsExists_ReturnIds()
    {
      // Arrange.
      string[] providerUrns =
      [
        _referral1.ProviderUbrn,
        _referral2.ProviderUbrn,
        _referral3.ProviderUbrn,
        _referral1.Ubrn,
        _referral2.Ubrn,
        _referral3.Ubrn,
      ];

      string[] expectedIds =
      [
        _referral1.Id.ToString(),
        _referral2.Id.ToString(),
        _referral3.Id.ToString(),
        _referral1.Id.ToString(),
        _referral2.Id.ToString(),
        _referral3.Id.ToString(),
      ];

      // Act.
      IEnumerable<string> res = await _service.GetIdsFromUbrns(providerUrns);

      // Assert.
      res.Should().Equal(expectedIds);
    }

    [Fact]
    public async Task ReferralWithUbrnsExists_ReturnIds()
    {
      // Arrange.
      string[] ubrns =
      [
        _referral1.Ubrn,
        _referral2.Ubrn,
        _referral3.Ubrn,
      ];

      string[] expectedIds =
      [
        _referral1.Id.ToString(),
        _referral2.Id.ToString(),
        _referral3.Id.ToString(),
      ];

      // Act.
      IEnumerable<string> res = await _service.GetIdsFromUbrns(ubrns);

      // Assert.
      res.Should().Equal(expectedIds);
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task UbrnsContainsNullOrEmptyValues_ThrowArgumentNullOrWhiteSpaceException(
      string nullOrWhitespaceValue)
    {
      // Arrange.
      string[] nullOrWhiteSpaceUbrns = [nullOrWhitespaceValue];

      // Act.
      Func<Task> func = async () => await _service.GetIdsFromUbrns(nullOrWhiteSpaceUbrns);

      // Assert.
      await func.Should().ThrowExactlyAsync<ArgumentNullOrWhiteSpaceException>();
    }

    [Fact]
    public async Task UbrnsIsEmpty_ThrowArgumentException()
    {
      // Arrange.
      string[] emptyUbrns = [];

      // Act.
      Func<Task> func = async () => await _service.GetIdsFromUbrns(emptyUbrns);

      // Assert.
      await func.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Fact]
    public async Task UbrnsIsNull_ThrowArgumentNullException()
    {
      // Arrange.

      // Act.
      Func<Task> func = async () => await _service.GetIdsFromUbrns(null);

      // Assert.
      await func.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    private void AddReferral(Entities.Referral referral)
    {
      _context.Referrals.Add(referral);
      _context.SaveChanges();
    }
  }
}
