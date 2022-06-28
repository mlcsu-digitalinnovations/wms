using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class ReferralAdminServiceTests : ServiceTestsBase, IDisposable
  {
    protected readonly DatabaseContext _context;
    protected readonly ReferralAdminService _service;
    protected readonly Mock<ReferralService> _mockReferralService;
    protected readonly Random _random = new Random();

    public ReferralAdminServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _mockReferralService = new Mock<ReferralService>(
        _context,
        null,  // IMapper
        null,  // IProviderService
        null,  // IDeprivationService
        null,  // IPostcodeService
        null,  // IPatientTriageService
        null); // IOdsOrganisationService

      _context = new DatabaseContext(_serviceFixture.Options);
      _service = new ReferralAdminService(
        _context,
        _mockReferralService.Object);
      _service.User = GetClaimsPrincipal();
    }

    public void Dispose()
    {
      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    public class ChangeDateOfBirthAsyncTests : ReferralAdminServiceTests
    {
      public ChangeDateOfBirthAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public async Task UbrnNullOrEmpty_Exception(string ubrn)
      {
        // arrange
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(1);
        var expectedExceptionMessage = new
          ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeDateOfBirthAsync(
            ubrn, originalDateOfBirth, updatedDateOfBirth));

        // assert
        ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task UpdatedAndOriginalDateOfBirthIdentical_Exception()
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(Constants.MAX_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth;
        var expectedExceptionMessage = new ArgumentException(
          $"Value must be different from {nameof(originalDateOfBirth)}.",
          nameof(updatedDateOfBirth)).Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeDateOfBirthAsync(
            ubrn, originalDateOfBirth, updatedDateOfBirth));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Theory]
      [InlineData(Constants.MIN_GP_REFERRAL_AGE - 1)]
      [InlineData(Constants.MAX_GP_REFERRAL_AGE + 1)]
      public async Task UpdatedDateOfBirthOutOfRange_Exception(
        int updatedDateOfBirthAge)
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var updatedDateOfBirth = DateTimeOffset.Now.Date.AddYears(
          -updatedDateOfBirthAge);
        var originalDateOfBirth = updatedDateOfBirth.AddYears(1);
        var expectedExceptionMessage = $"The {nameof(updatedDateOfBirth)} " +
          $"must result in a service user's age between " +
          $"{Constants.MIN_GP_REFERRAL_AGE} and " +
          $"{Constants.MAX_GP_REFERRAL_AGE}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeDateOfBirthAsync(
            ubrn, originalDateOfBirth, updatedDateOfBirth));

        // assert
        ex.Should().BeOfType<AgeOutOfRangeException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferralNotFoundByUbrn_Exception()
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(-1);
        var expectedExceptionMessage = $"Unable to find a referral with a " +
          $"UBRN of {ubrn}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeDateOfBirthAsync(
            ubrn, originalDateOfBirth, updatedDateOfBirth));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferalFoundButNotWithOriginalDateOfBirth_Exception()
      {
        // arrange
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfBirth: originalDateOfBirth.AddYears(-1));
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        var expectedExceptionMessage = $"Unable to find a referral with a " +
          $"UBRN of {referral.Ubrn} and a date of birth of " +
          $"{originalDateOfBirth:yyyy-MM-dd}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeDateOfBirthAsync(
            referral.Ubrn, originalDateOfBirth, updatedDateOfBirth));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferralHasNotBeenTriaged_NotReTriaged()
      {
        // arrange
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          dateOfBirth: originalDateOfBirth,
          offeredCompletionLevel: null,
          triagedCompletionLevel: null);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _mockReferralService.Setup(s => s.UpdateTriage(It.IsAny<Referral>()));

        // act
        string result = await _service.ChangeDateOfBirthAsync(
            createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);

        // assert
        _mockReferralService
          .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Never);

        updatedReferral.Should().BeEquivalentTo(updatedReferral, opts => opts
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateOfBirth)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId));

        updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      [Fact]
      public async Task ReferralHasBeenTriagedProviderSelected_NotReTriaged()
      {
        // arrange
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          dateOfBirth: originalDateOfBirth,
          offeredCompletionLevel: $"{(int)TriageLevel.Low}",
          providerId: Guid.NewGuid(),
          triagedCompletionLevel: $"{(int)TriageLevel.Low}");
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.Entry(createdReferral).State = EntityState.Detached;

        // act
        string result = await _service.ChangeDateOfBirthAsync(
            createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);

        // assert
        _mockReferralService
          .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Never);

        updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateOfBirth)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId));

        updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      [Fact]
      public async Task ReferralHasBeenTriagedProviderNotSelected_ReTriaged()
      {
        // arrange
        var originalDateOfBirth = DateTimeOffset.Now.Date
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
        var updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          dateOfBirth: originalDateOfBirth,
          offeredCompletionLevel: $"{(int)TriageLevel.Low}",
          triagedCompletionLevel: $"{(int)TriageLevel.Low}");
        createdReferral.ProviderId = null;
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.Entry(createdReferral).State = EntityState.Detached;

        // act
        string result = await _service.ChangeDateOfBirthAsync(
            createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);

        // assert
        _mockReferralService
          .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Once);

        updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateOfBirth)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId));

        updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }
    }

    public class ChangeMobileAsyncTests : ReferralAdminServiceTests
    {
      public ChangeMobileAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public async Task UbrnNullOrEmpty_Exception(string ubrn)
      {
        // arrange
        var originalMobile = "+447111111111";
        var updatedMobile = "+447222222222";
        var expectedExceptionMessage = new
          ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeMobileAsync(
            ubrn, originalMobile, updatedMobile));

        // assert
        ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task UpdatedAndOriginalMobileIdentical_Exception()
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var originalMobile = "+447111111111";
        var updatedMobile = "+447111111111";
        var expectedExceptionMessage = new ArgumentException(
          $"Value must be different from {nameof(originalMobile)}.",
          nameof(updatedMobile)).Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeMobileAsync(
            ubrn, originalMobile, updatedMobile));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      [InlineData("+697715427599")]
      [InlineData("+44771542759")]
      [InlineData("+4477154275999")]
      public async Task UpdatedMobileOutOfRange_Exception(
        string updatedMobile)
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var originalMobile = "+447111111111";
        var expectedExceptionMessage = new ArgumentOutOfRangeException(
          "updatedMobile",
          $"Value is not a valid UK mobile number.")
          .Message;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeMobileAsync(
            ubrn, originalMobile, updatedMobile));

        // assert
        ex.Should().BeOfType<ArgumentOutOfRangeException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferralNotFoundByUbrn_Exception()
      {
        // arrange
        var ubrn = Generators.GenerateUbrn(_random);
        var originalMobile = "+447111111111";
        var updatedMobile = "+447222222222";
        var expectedExceptionMessage = $"Unable to find a referral with a " +
          $"UBRN of {ubrn}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeMobileAsync(
            ubrn, originalMobile, updatedMobile));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferalFoundButNotWithOriginalMobile_Exception()
      {
        // arrange
        var originalMobile = "+447111111111";
        var updatedMobile = "+447222222222";

        var referral = RandomEntityCreator.CreateRandomReferral(
          mobile: "+447333333333");
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        var expectedExceptionMessage = $"Unable to find a referral with a " +
          $"UBRN of {referral.Ubrn} and a mobile of {originalMobile}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ChangeMobileAsync(
            referral.Ubrn, originalMobile, updatedMobile));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedExceptionMessage);
      }

      [Fact]
      public async Task ReferralFound_MobileUpdated()
      {
        // arrange
        var originalMobile = "+447111111111";
        var updatedMobile = "+447222222222";

        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          isMobileValid: true,
          mobile: originalMobile);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.Entry(createdReferral).State = EntityState.Detached;

        // act
        string result = await _service.ChangeMobileAsync(
            createdReferral.Ubrn, originalMobile, updatedMobile);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);

        // assert
        updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
          .Excluding(r => r.Audits)
          .Excluding(r => r.IsMobileValid)
          .Excluding(r => r.Mobile)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId));

        updatedReferral.IsMobileValid.Should().BeNull();
        updatedReferral.Mobile.Should().Be(updatedMobile);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

    }

    public class DeleteCancelledGpReferralAsyncTests : ReferralAdminServiceTests
    {
      private const string VALID_UBRN = "123456789012";
      private const string UBRN_NOT_FOUND = "999999999999";
      private const string VALID_REASON = "A valid reason for deletion.";

      public DeleteCancelledGpReferralAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Theory]
      [InlineData("")]
      [InlineData(" ")]
      [InlineData(null)]
      public async Task ReasonNullOrWhiteSpace_ArgumentException(string reason)
      {
        // act
        Task result = _service
          .DeleteCancelledGpReferralAsync(VALID_UBRN, reason);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(() => result);
      }

      [Fact]
      public async Task UbrnNotExists_ReferralNotFoundException()
      {
        // arrange
        CreateAndDetachReferral(
          UBRN_NOT_FOUND,
          ReferralStatus.CancelledByEreferrals,
          ReferralSource.GpReferral);

        // act
        Task result = _service
          .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

        // assert
        await Assert.ThrowsAsync<ReferralNotFoundException>(() => result);
      }

      [Theory]
      [MemberData(nameof(InvalidStatuses))]
      public async Task InvalidStatus_ReferralInvalidStatusException(
        ReferralStatus invalidReferralStatus)
      {
        // arrange
        CreateAndDetachReferral(
          VALID_UBRN,
          invalidReferralStatus,
          ReferralSource.GpReferral);

        // act
        Task result = _service
          .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

        // assert
        await Assert.ThrowsAsync<ReferralInvalidStatusException>(() => result);
      }

      [Theory]
      [MemberData(nameof(InvalidReferralSources))]
      public async Task
        InvalidReferralSource_ReferralInvalidReferralSourceException(
          ReferralSource invalidReferralSource)
      {
        // arrange
        CreateAndDetachReferral(
          VALID_UBRN,
          ReferralStatus.CancelledByEreferrals,
          invalidReferralSource);

        // act
        Task result = _service
          .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

        // assert
        await Assert
          .ThrowsAsync<ReferralInvalidReferralSourceException>(() => result);
      }

      [Fact]
      public async Task Existing_Cancelled_GpReferral_Updated()
      {
        // arrange
        var expectedReferral = CreateAndDetachReferral(
          VALID_UBRN,
          ReferralStatus.CancelledByEreferrals,
          ReferralSource.GpReferral);

        // act
        var result = await _service
          .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

        var updatedReferral = _context.Referrals
          .Where(r => r.Ubrn == expectedReferral.Ubrn)
          .Single();

        // assert
        result.Should().Be($"The referral with a UBRN of " +
          $"'{expectedReferral.Ubrn}' was deleted.");

        updatedReferral.Should().BeEquivalentTo(expectedReferral, opt => opt
          .Excluding(r => r.Audits)
          .Excluding(r => r.IsActive)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.StatusReason));

        updatedReferral.IsActive.Should().BeFalse();
        updatedReferral.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0,0,1));
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.StatusReason.Should().Be(VALID_REASON);
      }

      public static TheoryData<ReferralSource> InvalidReferralSources
      {
        get
        {
          TheoryData<ReferralSource> invalidReferralSources = new();

          foreach (var value in Enum.GetValues(typeof(ReferralSource)))
          {
            if ((ReferralSource)value != ReferralSource.GpReferral)
            {
              invalidReferralSources.Add((ReferralSource)value);
            }
          }
          return invalidReferralSources;
        }
      }

      public static TheoryData<ReferralStatus> InvalidStatuses
      {
        get
        {
          TheoryData<ReferralStatus> invalidStatuses = new();

          foreach (var value in Enum.GetValues(typeof(ReferralStatus)))
          {
            if ((ReferralStatus)value != ReferralStatus.CancelledByEreferrals)
            {
              invalidStatuses.Add((ReferralStatus)value);
            }
          }
          return invalidStatuses;
        }
      }

      private Referral CreateAndDetachReferral(
        string ubrn,
        ReferralStatus referralStatus,
        ReferralSource referralSource)
      {
        var referral = RandomEntityCreator.CreateRandomReferral(
          modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
          referralSource: referralSource,
          status: referralStatus,
          ubrn: ubrn);

        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.Entry(referral).State = EntityState.Detached;

        return referral;
      }

    }

    public class DeleteReferralAsyncTests : ReferralAdminServiceTests
    {
      private const string VALID_UBRN = "123456789012";
      private const string UBRN_NOT_FOUND = "999999999999";
      private const string VALID_REASON = "A valid reason for deletion.";

      public DeleteReferralAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralNull_ArgumentNullException()
      {
        // act
        var ex = await Record
          .ExceptionAsync(() => _service.DeleteReferralAsync(null));

        // assert
        ex.Should().BeOfType<ArgumentNullException>();
      }

      [Fact]
      public async Task UbrnDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToDelete = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = createdReferral.Status,
          Ubrn = "DoesNotMatch"
        };

        await ReferralNotFoundExceptionActAndAssert(referralToDelete);
      }

      [Fact]
      public async Task IdDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToDelete = new Business.Models.Referral()
        {
          Id = Guid.NewGuid(),
          Status = createdReferral.Status,
          Ubrn = createdReferral.Ubrn
        };

        await ReferralNotFoundExceptionActAndAssert(referralToDelete);
      }

      [Fact]
      public async Task StatusDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToDelete = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = ReferralStatus.RmcCall.ToString(),
          Ubrn = createdReferral.Ubrn
        };

        await ReferralNotFoundExceptionActAndAssert(referralToDelete);
      }

      private async Task ReferralNotFoundExceptionActAndAssert(
        Business.Models.Referral referralToDelete)
      {
        string expectedMessage = "Unable to find an active referral with " +
          $"an id of {referralToDelete.Id}, " +
          $"status of {referralToDelete.Status} and ubrn of " +
          $"{referralToDelete.Ubrn}.";

        // act
        var ex = await Record
          .ExceptionAsync(() => _service.DeleteReferralAsync(referralToDelete));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedMessage);
      }

      [Fact]
      public async Task Success()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var referralToDelete = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = createdReferral.Status,
          Ubrn = createdReferral.Ubrn
        };

        string expectedResult =
          $"The referral with an id of {referralToDelete.Id} was deleted.";

        // act
        var result = await _service.DeleteReferralAsync(referralToDelete);
        var updatedReferral = _context.Referrals
          .Single(r => r.Id == referralToDelete.Id);

        // assert
        result.Should().Be(expectedResult);

        updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
          .Excluding(r => r.Audits)
          .Excluding(r => r.IsActive)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId));

        updatedReferral.IsActive.Should().BeFalse();
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }
    }

    public class FixNonGpReferralsWithStatusProviderCompletedAsyncTests
      : ReferralAdminServiceTests
    {
      public FixNonGpReferralsWithStatusProviderCompletedAsyncTests(
        ServiceFixture serviceFixture)
          : base(serviceFixture)
      { }

      /// <summary>
      /// Creates a referral with a status of ProviderCompleted for each 
      /// ReferralSource except Undefined and GpReferral and tests that 
      /// that their status is updated to Complete
      /// </summary>
      [Theory]
      [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.GpReferral})]
      public async Task NonGpReferralSource_ProviderCompleted_UpdatedToComplete(
        ReferralSource referralSource)
      {
        var referral = RandomEntityCreator.CreateRandomReferral(
          referralSource: referralSource,
          status: ReferralStatus.ProviderCompleted);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        _context.Entry(referral).State = EntityState.Detached;

        // act
        var result = await _service
          .FixNonGpReferralsWithStatusProviderCompletedAsync();

        // assert
        result.Should().Be($"1 non-GP referral(s) had " +
        $"their status updated from {ReferralStatus.ProviderCompleted} to " +
        $"{ReferralStatus.Complete}.");

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == referral.Id);

        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Status));

        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.ModifiedByUserId.Should()
          .NotBe(referral.ModifiedByUserId);
        updatedReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
        updatedReferral.Status.Should().NotBe(referral.Status);
      }

      /// <summary>
      /// Creates a referral for each ReferralSource except Undefined and 
      /// GpReferral for all ReferralStatuses except ProviderCompleted and
      /// tests that that they are not updated
      /// </summary>
      [Theory]
      [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.GpReferral})]
      public async Task NonGpReferralSource_NotProviderCompleted_NotUpdated(
          ReferralSource referralSource)
      {
        var referrals = new List<Referral>();

        List<ReferralStatus> referralStatuses = Enum
          .GetValues(typeof(ReferralStatus))
          .Cast<ReferralStatus>()
          .Where(s => s != ReferralStatus.ProviderCompleted)
          .ToList();

        // arrange
        foreach (var status in referralStatuses)
        {
          referrals.Add(RandomEntityCreator.CreateRandomReferral(
            referralSource: referralSource,
            status: status));
        }
        _context.Referrals.AddRange(referrals);
        _context.SaveChanges();

        referrals.ForEach(r => _context.Entry(r).State = EntityState.Detached);

        // act
        var result = await _service
          .FixNonGpReferralsWithStatusProviderCompletedAsync();

        // assert
        result.Should().Be($"0 non-GP referral(s) had " +
        $"their status updated from {ReferralStatus.ProviderCompleted} to " +
        $"{ReferralStatus.Complete}.");

        var updatedReferrals = _context.Referrals
          .Where(r => referrals.Select(x => x.Id).Contains(r.Id))
          .ToList();

        foreach (var updatedReferral in updatedReferrals)
        {
          var referral = referrals.Single(r => r.Id == updatedReferral.Id);
          updatedReferral.Should().BeEquivalentTo(referral, opt => opt
            .Excluding(r => r.Audits));
        }
      }

      /// <summary>
      /// Creates a referral for each ReferralStatus with a ReferralSource of 
      /// GpReferral and tests that none are updated.
      /// </summary>
      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData))]
      public async Task GpReferralSource_NotUpdated(
        ReferralStatus referralStatus)
      {
        var referral = RandomEntityCreator.CreateRandomReferral(
          referralSource: ReferralSource.GpReferral,
          status: referralStatus);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        _context.Entry(referral).State = EntityState.Detached;

        // act
        var result = await _service
          .FixNonGpReferralsWithStatusProviderCompletedAsync();

        // assert
        result.Should().Be($"0 non-GP referral(s) had " +
        $"their status updated from {ReferralStatus.ProviderCompleted} to " +
        $"{ReferralStatus.Complete}.");

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == referral.Id);

        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
      }
    }

    public class FixProviderAwaitingTraceAsyncTests : ReferralAdminServiceTests
    {
      public FixProviderAwaitingTraceAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task NullNhsNumber_NotTraced_ProviderAwaitingTrace()
      {
        // arrange 
        var referral = CreateAndDetachReferral(nhsNumber: null, traceCount: 0);

        // act
        await _service.FixProviderAwaitingTraceAsync();

        // assert
        var updatedReferral = _context.Referrals.Find(referral.Id);
        updatedReferral.Should().BeEquivalentTo(referral, opts => opts
          .Excluding(r => r.Audits));
      }

      [Fact]
      public async Task NullNhsNumber_Traced_ProviderAwaitingStart()
      {
        // arrange 
        var referral = CreateAndDetachReferral(nhsNumber: null, traceCount: 1);

        // act
        await _service.FixProviderAwaitingTraceAsync();

        Assert(
          createdReferral: referral,
          expectedStatus: ReferralStatus.ProviderAwaitingStart,
          expectedStatusReason: "Status not updated to ProviderAwaitingStart " +
            "after first trace failure.");
      }

      [Fact]
      public async Task NhsNumber_NoDupe_Traced_ProviderAwaitingStart()
      {
        // arrange
        var referral = CreateAndDetachReferral(
          nhsNumber: VALID_NHS_NUMBER, traceCount: 1);

        // act
        await _service.FixProviderAwaitingTraceAsync();

        Assert(
          createdReferral: referral,
          expectedStatus: ReferralStatus.ProviderAwaitingStart,
          expectedStatusReason: "Status incorrectly set to " +
            "ProviderAwaitingTrace after provider selection.");
      }

      [Fact]
      public async Task NhsNumber_Dupe_Traced_CancelledDuplicateTextMessage()
      {
        // arrange 
        var referral = CreateAndDetachReferral(
          nhsNumber: VALID_NHS_NUMBER, traceCount: 1);

        var duplicate = CreateAndDetachReferral(
          nhsNumber: referral.NhsNumber, traceCount: 0);

        // act
        await _service.FixProviderAwaitingTraceAsync();

        Assert(
          createdReferral: referral,
          expectedStatus: ReferralStatus.CancelledDuplicateTextMessage,
          expectedStatusReason: "Traced NHS number is a duplicate of " +
            $"existing referral id(s) {string.Join(", ", duplicate.Id)}");
      }

      private Referral CreateAndDetachReferral(string nhsNumber, int traceCount)
      {
        var referral = RandomEntityCreator.CreateRandomReferral(
          modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
          status: ReferralStatus.ProviderAwaitingTrace,
          traceCount: traceCount);
        referral.NhsNumber = nhsNumber;

        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.Entry(referral).State = EntityState.Detached;

        return referral;
      }

      private void Assert(
        Referral createdReferral,
        ReferralStatus expectedStatus,
        string expectedStatusReason)
      {
        var updatedReferral = _context.Referrals.Find(createdReferral.Id);

        createdReferral.ModifiedByUserId = TestUserId;
        createdReferral.Status = expectedStatus.ToString();
        createdReferral.StatusReason = expectedStatusReason;

        updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
          .Excluding(r => r.Audits)
          .Excluding(r => r.ModifiedAt));

        updatedReferral.ModifiedAt.Should()
          .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      }
    }

    public class ResetReferralAsyncTests : ReferralAdminServiceTests
    {
      public ResetReferralAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralNull_ArgumentNullException()
      {
        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ResetReferralAsync(null, ReferralStatus.New));

        // assert
        ex.Should().BeOfType<ArgumentNullException>();
      }

      [Fact]
      public async Task UbrnDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToReset = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = createdReferral.Status,
          Ubrn = "DoesNotMatch"
        };

        await ReferralNotFoundExceptionActAndAssert(referralToReset);
      }

      [Fact]
      public async Task IdDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToReset = new Business.Models.Referral()
        {
          Id = Guid.NewGuid(),
          Status = createdReferral.Status,
          Ubrn = createdReferral.Ubrn
        };

        await ReferralNotFoundExceptionActAndAssert(referralToReset);
      }

      [Fact]
      public async Task StatusDoesNotMatch_ReferralNotFoundException()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToReset = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = ReferralStatus.RmcCall.ToString(),
          Ubrn = createdReferral.Ubrn
        };

        await ReferralNotFoundExceptionActAndAssert(referralToReset);
      }

      private async Task ReferralNotFoundExceptionActAndAssert(
        Business.Models.Referral referralToReset)
      {
        string expectedMessage = "Unable to find an active referral with an " +
          $"id of {referralToReset.Id}, " +
          $"status of {referralToReset.Status} and ubrn of " +
          $"{referralToReset.Ubrn}.";

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ResetReferralAsync(
            referralToReset,
            ReferralStatus.New));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedMessage);
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
        { ReferralStatus.New, ReferralStatus.RmcCall })]
      public async Task UpdateStatusNotSupported_Exception(
        ReferralStatus status)
      {
        // arrange 
        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();

        var referralToReset = new Business.Models.Referral()
        {
          Id = createdReferral.Id,
          Status = ReferralStatus.New.ToString(),
          Ubrn = createdReferral.Ubrn
        };

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.ResetReferralAsync(referralToReset, status));

        // assert
        ex.Should().BeOfType<NotSupportedException>();
        ex.Message.Should().Be("Currently this process will only reset a " +
          $"referral's status to {ReferralStatus.New} or " +
          $"{ReferralStatus.RmcCall}");
      }

      [Theory]
      [InlineData(false, ReferralStatus.New)]
      [InlineData(true, ReferralStatus.RmcCall)]
      public async Task ResetToNew_StatusUpdateAndResetProviderFields(
        bool isVulnerable,
        ReferralStatus expectedStatus)
      {
        Referral existingReferral = CreateReferralWithProviderSubmission(
          isVulnerable: isVulnerable);

        var expectedStatusReason = "Approved reset by NHSE";
        var referralToReset = new Business.Models.Referral()
        {
          Id = existingReferral.Id,
          Status = existingReferral.Status,
          StatusReason = expectedStatusReason,
          Ubrn = existingReferral.Ubrn
        };

        // act
        var result = await _service.ResetReferralAsync(
          referralToReset,
          expectedStatus);

        var updatedReferral = _context.Referrals
          .Include(r => r.ProviderSubmissions)
          .Single(r => r.Id == referralToReset.Id);

        StatusUpdateGenericAsserts(
          existingReferral,
          expectedStatusReason,
          expectedStatus,
          result,
          updatedReferral);
      }

      [Fact]
      public async Task ResetToRmcCall_StatusUpdateAndResetProviderFields()
      {
        Referral existingReferral = CreateReferralWithProviderSubmission(
          isVulnerable: true);

        var expectedStatus = ReferralStatus.RmcCall;
        var expectedStatusReason = "Approved reset by NHSE";
        var referralToReset = new Business.Models.Referral()
        {
          Id = existingReferral.Id,
          Status = existingReferral.Status,
          StatusReason = expectedStatusReason,
          Ubrn = existingReferral.Ubrn
        };

        // act
        var result = await _service.ResetReferralAsync(
          referralToReset,
          expectedStatus);

        var updatedReferral = _context.Referrals
          .Include(r => r.ProviderSubmissions)
          .Single(r => r.Id == referralToReset.Id);

        StatusUpdateGenericAsserts(
          existingReferral,
          expectedStatusReason,
          expectedStatus,
          result,
          updatedReferral);
      }

      private static void StatusUpdateGenericAsserts(
        Referral existingReferral,
        string expectedStatusReason,
        ReferralStatus expectedStatus,
        Referral result,
        Referral updatedReferral)
      {
        // assert
        result.Should().BeOfType<Referral>();
        updatedReferral.Should().BeEquivalentTo(existingReferral, opt => opt
          .Excluding(r => r.Audits)
          .Excluding(r => r.DateCompletedProgramme)
          .Excluding(r => r.DateLetterSent)
          .Excluding(r => r.DateOfProviderContactedServiceUser)
          .Excluding(r => r.DateOfProviderSelection)
          .Excluding(r => r.DateStartedProgramme)
          .Excluding(r => r.DateToDelayUntil)
          .Excluding(r => r.DelayReason)
          .Excluding(r => r.FirstRecordedWeight)
          .Excluding(r => r.FirstRecordedWeightDate)
          .Excluding(r => r.LastRecordedWeight)
          .Excluding(r => r.LastRecordedWeightDate)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.ProgrammeOutcome)
          .Excluding(r => r.ProviderId)
          .Excluding(r => r.ProviderSubmissions)
          .Excluding(r => r.Status)
          .Excluding(r => r.StatusReason));

        result.DateCompletedProgramme.Should().BeNull();
        result.DateLetterSent.Should().BeNull();
        result.DateOfProviderContactedServiceUser.Should().BeNull();
        result.DateOfProviderSelection.Should().BeNull();
        result.DateStartedProgramme.Should().BeNull();
        result.DateToDelayUntil.Should().BeNull();
        result.DelayReason.Should().BeNull();
        result.FirstRecordedWeight.Should().BeNull();
        result.FirstRecordedWeightDate.Should().BeNull();
        result.LastRecordedWeight.Should().BeNull();
        result.LastRecordedWeightDate.Should().BeNull();
        result.ModifiedAt.Should().BeAfter(existingReferral.ModifiedAt);
        result.ModifiedByUserId.Should().Be(TEST_USER_ID);
        result.ProgrammeOutcome.Should().BeNull();
        result.ProviderId.Should().BeNull();
        result.ProviderSubmissions.Any().Should().BeFalse();
        result.Status.Should().Be(expectedStatus.ToString());
        result.StatusReason.Should().Be(expectedStatusReason);
      }

      private Referral CreateReferralWithProviderSubmission(
        bool isVulnerable)
      {
        // arrange 
        var referral = RandomEntityCreator.CreateRandomReferral(
          dateCompletedProgramme: DateTimeOffset.Now,
          dateLetterSent: DateTimeOffset.Now,
          dateOfProviderContactedServiceUser: DateTimeOffset.Now,
          dateOfProviderSelection: DateTimeOffset.Now,
          dateStartedProgramme: DateTimeOffset.Now,
          dateToDelayUntil: DateTimeOffset.Now,
          delayReason: "DelayReason",
          firstRecordedWeight: 100,
          firstRecordedWeightDate: DateTimeOffset.Now,
          isVulnerable: isVulnerable,
          lastRecordedWeight: 95,
          lastRecordedWeightDate: DateTimeOffset.Now,
          programmeOutcome: ReferralStatus.Complete.ToString(),
          providerId: Guid.NewGuid(),
          status: ReferralStatus.Complete);

        referral.ProviderSubmissions = new()
        {
          RandomEntityCreator.CreateProviderSubmission()
        };

        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return referral;
      }
    }
  }
}

