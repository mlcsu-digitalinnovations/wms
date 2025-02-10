using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests
{
  public class UpdateReferralWithProviderTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : ReferralServiceTests(serviceFixture, testOutputHelper), IDisposable
  {
    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.SaveChanges();
      base.Dispose();
      GC.SuppressFinalize(this);
    }

    public static TheoryData<ReferralSource, ReferralStatus> InvalidReferralStatusTheoryData()
    {
      TheoryData<ReferralSource, ReferralStatus> theoryData = [];

      List<ReferralStatus> validGeneralReferralStatuses =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.New,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage1,
        ReferralStatus.TextMessage2,
        ReferralStatus.TextMessage3
      ];

      Array referralSources = Enum.GetValues(typeof(ReferralSource));
      Array referralStatuses = Enum.GetValues(typeof(ReferralStatus));

      foreach (ReferralSource referralSource in referralSources)
      {
        if (referralSource == ReferralSource.GeneralReferral)
        {
          foreach (ReferralStatus referralStatus in referralStatuses)
          {
            if (!validGeneralReferralStatuses.Contains(referralStatus))
            {
              theoryData.Add(referralSource, referralStatus);
            }
          }
        }
        else
        {
          foreach (ReferralStatus referralStatus in referralStatuses)
          {
            if (referralStatus != ReferralStatus.New)
            {
              theoryData.Add(referralSource, referralStatus);
            }
          }
        }
      }

      return theoryData;
    }

    [Fact]
    public async Task NoMatchingProviderThrowsProviderSelectionMismatchException()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New,
        triagedCompletionLevel: "1");

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      Guid providerId = Guid.NewGuid();

      // Act.
      Func<Task<object>> result = async () => await _service.UpdateReferralWithProviderAsync(
        referral.Id,
        providerId);

      // Assert.
      await result.Should().ThrowAsync<ProviderSelectionMismatch>();
    }

    [Fact]
    public async Task NoMatchingReferralThrowsReferralNotFoundException()
    {
      // Arrange.
      Guid invalidReferralId = Guid.NewGuid();
      Guid providerId = Guid.NewGuid();

      // Act.
      Func<Task<object>> result = async () => await _service.UpdateReferralWithProviderAsync(
        invalidReferralId,
        providerId);

      // Assert.
      await result.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ProviderAlreadySelectedThrowsReferralProviderSelectedException()
    {
      // Arrange.
      Entities.Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        providerId: provider.Id);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<object>> result = async () => await _service.UpdateReferralWithProviderAsync(
        referral.Id,
        provider.Id);

      // Assert.
      await result.Should().ThrowAsync<ReferralProviderSelectedException>()
        .WithMessage($"*{referral.Id}*");
    }

    [Theory]
    [MemberData(nameof(InvalidReferralStatusTheoryData))]
    public async Task ReferralWithInvalidStatusThrowsReferralInvalidStatusException(
      ReferralSource source,
      ReferralStatus status)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: source,
        status: status);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      Guid providerId = Guid.NewGuid();

      // Act.
      Func<Task<object>> result = async () => await _service.UpdateReferralWithProviderAsync(
        referral.Id,
        providerId,
        source);

      // Assert.
      await result.Should().ThrowAsync<ReferralInvalidStatusException>()
        .WithMessage($"*{referral.Id}*{status}*");
    }

    [Theory]
    [MemberData(nameof(ValidReferralTheoryData))]
    public async Task ValidReferralHasProviderSelectedAndStatusUpdated(
      ReferralStatus expectedStatus,
      string expectedStatusReason,
      string nhsNumber,
      ReferralSource referralSource,
      ReferralSource referralSourceParameter,
      ReferralStatus referralStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: referralStatus,
        triagedCompletionLevel: "1");
      referral.NhsNumber = nhsNumber;
      _context.Referrals.Add(referral);

      Entities.Provider provider = RandomEntityCreator.CreateRandomProvider(
        isLevel1: true,
        isLevel2: true,
        isLevel3: true);
      _context.Providers.Add(provider);

      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act.
      Business.Models.IReferral result = await _service.UpdateReferralWithProviderAsync(
        referral.Id,
        provider.Id,
        referralSourceParameter);

      // Assert.
      Entities.Referral storedReferral = _context.Referrals.Single(x => x.Id == referral.Id);
      storedReferral.ProviderId.Should().Be(provider.Id);
      storedReferral.Status.Should().Be(expectedStatus.ToString());
      storedReferral.StatusReason.Should().Be(expectedStatusReason);
      storedReferral.DateOfProviderSelection.Should().HaveValue()
        .And.Subject.Value.Date.Should().Be(DateTimeOffset.Now.Date);

      result.Should().BeEquivalentTo(storedReferral, options => options
        .ExcludingMissingMembers()
        .Excluding(x => x.TextMessages));

      result.TextMessages.Should().BeEmpty();
    }

    public static TheoryData<ReferralStatus,
      string,
      string,
      ReferralSource,
      ReferralSource,
      ReferralStatus>
      ValidReferralTheoryData()
    {
      TheoryData<ReferralStatus, string, string, ReferralSource, ReferralSource, ReferralStatus>
        theoryData = [];

      List<ReferralStatus> validGeneralReferralStatuses =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.New,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage1,
        ReferralStatus.TextMessage2,
        ReferralStatus.TextMessage3
      ];

      string[] nhsNumberValues = [" ", null, "9991234567"];

      Array referralSources = Enum.GetValues(typeof(ReferralSource));

      foreach (string nhsNumber in nhsNumberValues)
      {
        ReferralStatus expectedReferralStatus;
        string expectedReferralStatusReason = null;

        if (string.IsNullOrWhiteSpace(nhsNumber))
        {
          expectedReferralStatus = ReferralStatus.ProviderAwaitingTrace;
          expectedReferralStatusReason = "NHS number awaiting trace.";
        }
        else
        {
          expectedReferralStatus = ReferralStatus.ProviderAwaitingStart;
        }

        foreach (ReferralSource referralSource in referralSources)
        {
          if (referralSource == ReferralSource.GeneralReferral
            || referralSource == ReferralSource.ElectiveCare)
          {
            // Elective Care referrals are updated by General Referral controller.
            foreach (ReferralStatus status in validGeneralReferralStatuses)
            {
              theoryData.Add(
                expectedReferralStatus,
                expectedReferralStatusReason,
                nhsNumber,
                referralSource,
                ReferralSource.GeneralReferral,
                status);
            }
          }
          else
          {
            theoryData.Add(
              expectedReferralStatus,
              expectedReferralStatusReason,
              nhsNumber,
              referralSource,
              referralSource,
              ReferralStatus.New);
          }
        }
      }

      return theoryData;
    }
  }
}