using System;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Common.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class RejectAfterProviderSelectionTests
    : ReferralServiceTests, IDisposable
  {
    public static TheoryData<ReferralStatus> ValidStatusTheoryData()
    {
      return new()
      {
        { ReferralStatus.ProviderDeclinedByServiceUser },
        { ReferralStatus.ProviderRejected },
        { ReferralStatus.ProviderTerminated }
      };
    }

    private readonly string[] _validStatuses = ValidStatusTheoryData()
      .SelectMany(x => x)
      .Select(x => x.ToString())
      .ToArray();

    public RejectAfterProviderSelectionTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      // Clean up.
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    public new void Dispose()
    {
      // Remove all created referrals.
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task Id_NotFound_Exception()
    {
      // Arrange.
      Guid id = Guid.NewGuid();
      string reason = "Rejected";
      string expectedMessage = new ReferralNotFoundException(id).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.RejectAfterProviderSelectionAsync(id, reason));

      // Assert.
      ex.Should().BeOfType<ReferralNotFoundException>()
        .Subject.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task Reason_NullOrWhiteSpace_Exception(string reason)
    {
      // Arrange.
      Guid id = Guid.NewGuid();
      string expectedMessage = new
        ArgumentNullOrWhiteSpaceException(nameof(reason)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.RejectAfterProviderSelectionAsync(id, reason));

      // Assert.
      ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>()
        .Subject.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[]
      {
        ReferralSource.GpReferral
      })]
    public async Task ReferralSource_Unexpected_Exception(ReferralSource source)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: source);
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMessage =
        $"Referral {referral.Id} has an unexpected referral source of " +
        $"{referral.ReferralSource}. The only valid referral source " +
        $"is {ReferralSource.GpReferral}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.RejectAfterProviderSelectionAsync(
          referral.Id,
          "Rejected"));

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);

      using (new AssertionScope())
      {
        ex.Should().BeOfType<ReferralInvalidReferralSourceException>()
          .Subject.Message.Should().Be(expectedMessage);

        referral.Should().BeEquivalentTo(updatedReferral, options => options
          .Excluding(x => x.Audits));
      }
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      {
        ReferralStatus.ProviderDeclinedByServiceUser,
        ReferralStatus.ProviderRejected,
        ReferralStatus.ProviderTerminated
      })]
    public async Task Status_Unexpected_Exception(ReferralStatus status)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: status);
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMessage = $"Referral {referral.Id} has an unexpected " +
        $"status of {status}. Valid statuses are: " +
        $"{string.Join(",", _validStatuses)}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.RejectAfterProviderSelectionAsync(
          referral.Id,
          "Rejected"));

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);

      using (new AssertionScope())
      {
        ex.Should().BeOfType<ReferralInvalidStatusException>()
          .Subject.Message.Should().Be(expectedMessage);

        referral.Should().BeEquivalentTo(updatedReferral, options => options
          .Excluding(x => x.Audits));
      }
    }

    [Theory]
    [MemberData(nameof(ValidStatusTheoryData))]
    public async Task ProviderId_Null_Exception(ReferralStatus status)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: status);
      referral.ProviderId = null;
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMessage = $"Referral {referral.Id} has a status of " +
        $"{referral.Status}, and therefore should have a selected provider, " +
        $"but it does not.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.RejectAfterProviderSelectionAsync(
          referral.Id,
          "Rejected"));

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);

      using (new AssertionScope())
      {
        ex.Should().BeOfType<ReferralProviderSelectedException>()
          .Subject.Message.Should().StartWith(expectedMessage);

        referral.Should().BeEquivalentTo(updatedReferral, options => options
          .Excluding(x => x.Audits));
      }
    }

    [Theory]
    [MemberData(nameof(ValidStatusTheoryData))]
    public async Task Valid(ReferralStatus status)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        providerId: Guid.NewGuid(),
        referralSource: ReferralSource.GpReferral,
        status: status);
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedStatusReason = $"Rejected";

      // Act.
      Business.Models.IReferral result = await _service
        .RejectAfterProviderSelectionAsync(referral.Id, expectedStatusReason);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeEquivalentTo(referral, options => options
          .ExcludingMissingMembers()
          .Excluding(x => x.DateCompletedProgramme)
          .Excluding(x => x.ModifiedAt)
          .Excluding(x => x.ModifiedByUserId)
          .Excluding(x => x.ProgrammeOutcome)
          .Excluding(x => x.Status)
          .Excluding(x => x.StatusReason)
          .Excluding(x => x.TextMessages));

        result.DateCompletedProgramme.Value.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        result.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        result.ModifiedByUserId.Should().Be(TEST_USER_ID);
        result.ProgrammeOutcome.Should()
          .Be(ProgrammeOutcome.RejectedAfterProviderSelection.ToString());
        result.Status.Should()
          .Be(ReferralStatus.AwaitingDischarge.ToString());
        result.StatusReason.Should().Be(expectedStatusReason);

        Referral updatedReferral = _context.Referrals.Find(referral.Id);

        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(x => x.Audits)
          .Excluding(x => x.DateCompletedProgramme)
          .Excluding(x => x.ModifiedAt)
          .Excluding(x => x.ModifiedByUserId)
          .Excluding(x => x.ProgrammeOutcome)
          .Excluding(x => x.Status)
          .Excluding(x => x.StatusReason));

        updatedReferral.Audits.Should().HaveCount(1);
        updatedReferral.DateCompletedProgramme.Value.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        updatedReferral.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.ProgrammeOutcome.Should()
          .Be(ProgrammeOutcome.RejectedAfterProviderSelection.ToString());
        updatedReferral.Status.Should()
          .Be(ReferralStatus.AwaitingDischarge.ToString());
        updatedReferral.StatusReason.Should().Be(expectedStatusReason);
      }
    }
  }
}
