using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class UpdateDateOfBirthTests : ReferralServiceTests, IDisposable
    {
      private readonly DateTimeOffset _validDateOfBirth;
      private readonly IEnumerable<Provider> _providers = new List<Provider>()
      {
        RandomModelCreator.CreateRandomProvider()
      };
      private new readonly ReferralService _service;
      private readonly Mock<IProviderService> _mockProviderService = new();
      private readonly string _referralNotFoundExceptionMessage =
        "Unable to find a referral with an id of {referralId}.";

      public UpdateDateOfBirthTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      {
        _validDateOfBirth = DateTimeOffset.Now
          .AddYears(-Constants.MIN_GP_REFERRAL_AGE - 1);

        _mockProviderService
          .Setup(p => p.GetProvidersAsync(It.IsAny<TriageLevel>()))
          .ReturnsAsync(_providers);

        _service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _mockProviderService.Object,
          _mockDeprivationService.Object,
          _mockLinkIdService.Object,
          _mockPostcodeIoService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object,
          _mockGpDocumentProxyOptions.Object,
          _mockReferralTimelineOptions.Object,
          null,
          _log)
        {
          User = GetClaimsPrincipal()
        };
      }

      public new void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Theory]
      [InlineData(-Constants.MIN_GP_REFERRAL_AGE + 1)]
      [InlineData(-Constants.MAX_GP_REFERRAL_AGE - 1)]
      public async Task AgeIsOutOfRange_AgeOutOfRangeException(int age)
      {
        // arrange
        var dateOfBirth = DateTimeOffset.Now.AddYears(age);

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateDateOfBirth(Guid.Empty, dateOfBirth));

        // assert
        ex.Should().BeOfType<AgeOutOfRangeException>();
        ex.Message.Should().Be($"The {nameof(dateOfBirth)} " +
          $"must result in a service user's age between " +
          $"{Constants.MIN_GP_REFERRAL_AGE} and " +
          $"{Constants.MAX_GP_REFERRAL_AGE}.");
      }

      [Fact]
      public async Task ReferralDoesNotExist_ReferralNotFoundException()
      {
        // arrange
        var id = Guid.NewGuid();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateDateOfBirth(id, _validDateOfBirth));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", id.ToString()));
      }

      [Fact]
      public async Task ReferralIsInactive_ReferralNotFoundException()
      {
        // arrange
        var inactiveReferral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        _context.Referrals.Add(inactiveReferral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateDateOfBirth(inactiveReferral.Id, _validDateOfBirth));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", inactiveReferral.Id.ToString()));
      }

      [Fact]
      public async Task ProviderSet_TriageReferralNotRun()
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          providerId: Guid.NewGuid());
        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()));

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateDateOfBirth(createdReferral.Id, _validDateOfBirth);

        // assert
        _mockPatientTriageService.Invocations.Count.Should().Be(0);
        UniversalAsserts(createdReferral, result);
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async Task SexNotSet_TriageReferralNotRun(string sex)
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        createdReferral.Sex = sex;
        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()));

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateDateOfBirth(createdReferral.Id, _validDateOfBirth);

        // assert
        _mockPatientTriageService.Invocations.Count.Should().Be(0);
        UniversalAsserts(createdReferral, result);
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async Task EthnicityNotSet_TriageReferralNotRun(string ethnicity)
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        createdReferral.Ethnicity = ethnicity;
        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()));

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateDateOfBirth(createdReferral.Id, _validDateOfBirth);

        // assert
        _mockPatientTriageService.Invocations.Count.Should().Be(0);
        UniversalAsserts(createdReferral, result);
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async Task DepriviationNotSet_TriageReferralNotRun(
        string deprivation)
      {
        // arrange
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        createdReferral.Deprivation = deprivation;
        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()));

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateDateOfBirth(createdReferral.Id, _validDateOfBirth);

        // assert
        _mockPatientTriageService.Invocations.Count.Should().Be(0);
        UniversalAsserts(createdReferral, result);
      }

      [Fact]
      public async Task Valid()
      {
        // arrange
        const TriageLevel expectedTriageCompletionLevel = TriageLevel.Low;
        const TriageLevel expectedTriageWeightedLevel = TriageLevel.Medium;
        var createdReferral = RandomEntityCreator.CreateRandomReferral();
        createdReferral.ProviderId = null;
        Mock<CourseCompletionResult> courseCompletionResult = new();
        courseCompletionResult.Setup(t => t.TriagedCompletionLevel)
          .Returns(expectedTriageCompletionLevel);
        courseCompletionResult.Setup(t => t.TriagedWeightedLevel)
          .Returns(expectedTriageWeightedLevel);
        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()))
          .Returns(courseCompletionResult.Object);

        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .UpdateDateOfBirth(createdReferral.Id, _validDateOfBirth);

        // assert
        _mockPatientTriageService.Invocations.Count.Should().Be(1);
        result.TriagedCompletionLevel.Should()
          .Be(expectedTriageCompletionLevel.ToString("d"));
        result.TriagedWeightedLevel.Should()
          .Be(expectedTriageWeightedLevel.ToString("d"));
        result.Providers.Count.Should().Be(1);
        UniversalAsserts(createdReferral, result);
      }

      private void UniversalAsserts(
        Entities.Referral createdReferral,
        IReferral returnedReferral)
      {
        returnedReferral.Id.Should().Be(createdReferral.Id);

        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);
        updatedReferral.Should()
          .BeEquivalentTo(createdReferral, options => options
            .Excluding(r => r.Audits)
            .Excluding(r => r.DateOfBirth)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.TriagedCompletionLevel)
            .Excluding(r => r.TriagedWeightedLevel)
            .Excluding(r => r.OfferedCompletionLevel));
        updatedReferral.DateOfBirth.Should().Be(returnedReferral.DateOfBirth);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }
    }

  }
}
