using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Reflection;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests
{
  public class CanGpReferralsIsErsClosedSetTests 
    : ReferralServiceTests, IDisposable
  {
    public CanGpReferralsIsErsClosedSetTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    {
      CleanUp();
    }

    public new void Dispose()
    {
      CleanUp();
      GC.SuppressFinalize(this);
    }

    private void CleanUp()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Theory]
    [MemberData(nameof(NotCancelledOrRejectedStatus))]
    public async Task Valid_GpReferral_IsErsClosed_SetTrue(
      ReferralStatus status)
    {
      // Arrange.
      bool? expectedInitialIsErsClosed = null;
      bool expectedIsErsClosed = true;
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: status);
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      referral.IsErsClosed.Should().Be(expectedInitialIsErsClosed);

      // Act.
      try
      {
        await _service.CloseErsReferral(referral.Id);

        // Assert.
        using (new AssertionScope())
        {
          referral.Status.Should().Be(
            status.ToString(),
            because: "Ensure this hasn't changed");
          referral.IsErsClosed.Should().Be(
            expectedIsErsClosed,
            because: "Ensure this has changed");
        }
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
      finally
      {
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }
    }

    [Fact]
    public void CloseErsReferral_ShouldHaveCorrectSignature()
    {
      // Arrange
      MethodInfo methodInfo = typeof(ReferralService)
        .GetMethod(
          "CloseErsReferral",
          new[] {
            typeof(Guid),
            typeof(ReferralSource),
            typeof(ReferralStatus)
          });
      ReferralSource expectedSourceFlags = ReferralSource.GpReferral;
      ReferralStatus expectedStatusFlags = 
        ReferralStatus.CancelledByEreferrals 
        | ReferralStatus.RejectedToEreferrals;

      // Assert
      using (new AssertionScope())
      {
        methodInfo.Should().NotBeNull();
        methodInfo.ReturnType.Should().Be(typeof(Task));

        ParameterInfo[] parameters = methodInfo.GetParameters();
        parameters.Length.Should().Be(3);

        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(ReferralSource));
        parameters[2].ParameterType.Should().Be(typeof(ReferralStatus));

        parameters[1].HasDefaultValue.Should().BeTrue();
        parameters[1].DefaultValue.Should().Be(expectedSourceFlags);

        parameters[2].HasDefaultValue.Should().BeTrue();
        ReferralStatus defaultValue = (ReferralStatus)parameters[2].DefaultValue;
        defaultValue.Should().Be(expectedStatusFlags);
        defaultValue.Should().HaveFlag(expectedStatusFlags);
      }
    }

    [Fact]
    public async Task WithEmptyId_Throws_ArgumentOutOfRangeException()
    {
      // Arrange.
      Guid emptyId = Guid.Empty;

      string expectedErrorMessage = new
        ArgumentOutOfRangeException("id", "Cannot be empty.")
        .Message;

      // Act.
      Func<Task> act = async () => await _service.CloseErsReferral(emptyId);

      // Assert.
      await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
        .WithMessage(expectedErrorMessage);
    }

    [Fact]
    public async Task ReferralNotFound_Throws_ReferralNotFoundException()
    {
      // Arrange.
      Guid newId = Guid.NewGuid();
      string expectedErrorMessage =
        $"An active referral was not found with an id of {newId}.";

      // Act.
      Func<Task> act = async () => await _service.CloseErsReferral(newId);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>()
        .WithMessage(expectedErrorMessage);
    }

    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.Exception)]
    [InlineData(ReferralStatus.RejectedToEreferrals)]
    public async Task
      WithInvalidReferralStatus_Throws_ReferralInvalidStatusException(
        ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: referralStatus);
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      string expectedErrorMessage = $"Referral Id {referral.Id} " +
        $"has a unexpected Status of {referral.Status}. " +
        $"A referral cannot have its eRS record closed if it has one of " +
        $"the following statuses: " +
        $"{ReferralStatus.Exception}, " +
        $"{ReferralStatus.RejectedToEreferrals}, " +
        $"{ReferralStatus.CancelledByEreferrals}.";

      // Act.
      Func<Task> act = async () => await _service.CloseErsReferral(referral.Id);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidStatusException>()
        .WithMessage(expectedErrorMessage);
    }

    [Theory]
    [MemberData(nameof(AllReferralSourcesExcluding), ReferralSource.GpReferral)]
    public async Task WithValidReferralId_InvalidSource_NotValueUpdateMade(
      ReferralSource referralSource)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: ReferralStatus.CancelledByEreferrals);
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      string expectedErrorMessage = $"Referral Id {referral.Id} " +
        $"has a unexpected ReferralSource of {referral.ReferralSource}. " +
        $"A referral can only have its eRS record closed it has one of the " +
        $"following referral sources: {ReferralSource.GpReferral}.";

      // Act.
      Func<Task> act = async () => await _service.CloseErsReferral(referral.Id);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidReferralSourceException>()
        .WithMessage(expectedErrorMessage);
    }

    public static TheoryData<ReferralStatus> NotCancelledOrRejectedStatus()
    {
      TheoryData<ReferralStatus> data = new();

      foreach (ReferralStatus status in Enum.GetValues(typeof(ReferralStatus)))
      {
        if (status is not ReferralStatus.CancelledByEreferrals and
            not ReferralStatus.RejectedToEreferrals and
            not ReferralStatus.Exception)
        {
          data.Add(status);
        }
      }

      return data;
    }

    public static TheoryData<ReferralSource>AllReferralSourcesExcluding(
      ReferralSource excludedReferralSource)
    {
      TheoryData<ReferralSource> data = new();
      foreach (ReferralSource status in Enum.GetValues(typeof(ReferralSource)))
      {
        if (status != excludedReferralSource)
        {
          data.Add(status);
        }
      }

      return data;
    }
  }
}
