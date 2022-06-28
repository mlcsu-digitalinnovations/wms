using FluentAssertions;
using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class InUseResponseTests : ATheoryData
  {
    // method names after _ describes the flags/properties that should be true

    [Fact]
    public void Default_NotFound()
    {
      // arrange
      var inUseResponse = new InUseResponse();

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeFalse();
      inUseResponse.WasNotFound.Should().BeTrue();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void NullReferral_NotFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = null
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeFalse();
      inUseResponse.WasNotFound.Should().BeTrue();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void NoProvider_FoundIsNotGeneralReferralProviderNotSelectedWasFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = null,
          ReferralSource = ReferralSource.GpReferral.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void NoProviderGeneralReferral_FoundIsGeneralReferralProviderNotSelectedWasFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = null,
          ReferralSource = ReferralSource.GeneralReferral.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void ProviderGeneralReferral_FoundIsGeneralReferralProviderSelectedWasFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = Guid.NewGuid(),
          ReferralSource = ReferralSource.GeneralReferral.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeTrue();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
        { ReferralStatus.CancelledByEreferrals, 
          ReferralStatus.CancelledDuplicate,
          ReferralStatus.CancelledDuplicateTextMessage,
          ReferralStatus.Complete})]
    public void NoProviderNotCancelledStatuses_FoundProviderNotSelectedWasFound(
      ReferralStatus referralStatus)
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = null,
          ReferralSource = ReferralSource.GeneralReferral.ToString(),
          Status = referralStatus.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Theory]
    [InlineData(ReferralStatus.CancelledByEreferrals)]
    [InlineData(ReferralStatus.CancelledDuplicate)]
    [InlineData(ReferralStatus.CancelledDuplicateTextMessage)]
    public void ProviderCancelledStatuses_CancelledFoundProviderSelectedWasFound(
      ReferralStatus referralStatus)
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = null,
          ReferralSource = ReferralSource.GeneralReferral.ToString(),
          Status = referralStatus.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void ProviderStatusCompleted_CompleteFoundIsGeneralReferralProviderSelectedWasFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ProviderId = Guid.NewGuid(),
          ReferralSource = ReferralSource.GeneralReferral.ToString(),
          Status = ReferralStatus.Complete.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeTrue();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeFalse();
    }

    [Fact]
    public void NoProvider_CompleteFoundIsGeneralReferralProviderSelectedWasNotFound()
    {
      // arrange
      var inUseResponse = new InUseResponse
      {
        Referral = new Referral
        {
          ReferralSource = ReferralSource.GeneralReferral.ToString(),
          Status = ReferralStatus.Complete.ToString()
        }
      };

      // act
      var inUseResult = inUseResponse.InUseResult;

      // assert            
      inUseResult.HasFlag(InUseResult.Cancelled).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.Complete).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.Found).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsGeneralReferral).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.IsNotGeneralReferral).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.NotFound).Should().BeFalse();
      inUseResult.HasFlag(InUseResult.ProviderNotSelected).Should().BeTrue();
      inUseResult.HasFlag(InUseResult.ProviderSelected).Should().BeFalse();
      inUseResponse.WasFound.Should().BeTrue();
      inUseResponse.WasNotFound.Should().BeFalse();
      inUseResponse.IsCompleteAndProviderNotSelected.Should().BeTrue();
    }
  }
}
