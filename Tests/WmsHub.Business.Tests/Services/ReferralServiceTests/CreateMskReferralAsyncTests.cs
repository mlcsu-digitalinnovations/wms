using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using System;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class CreateMskReferralAsyncTests
      : ReferralServiceTests, IDisposable
    {
      public CreateMskReferralAsyncTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture)
      {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async void Param_Null_Exception()
      {
        // arrange
        IMskReferralCreate mskReferralCreate = null;

        // act
        Exception ex = await Record.ExceptionAsync(() =>
          _service.CreateMskReferralAsync(mskReferralCreate));

        // assert
        ex.Should().BeOfType<ArgumentNullException>();
      }

      [Fact]
      public async void NhsNumberInUseWithCurrentReferral_Exception()
      {
        // arrange
        Entities.Referral referral = RandomEntityCreator
          .CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var create = RandomModelCreator.CreateRandomMskReferralCreate(
          nhsNumber: referral.NhsNumber);

        // act
        Exception ex = await Record.ExceptionAsync(() =>
          _service.CreateMskReferralAsync(create));

        // assert
        ex.Should().BeOfType<ReferralNotUniqueException>();

        var savedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
        savedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
      }

      //[Fact]
      //public async void InvalidModel_Exception()
      //{
      //  // arrange
      //  var create = new MskReferralCreate();
        
      //  // act
      //  var ex = await Assert
      //    .ThrowsAsync<MskReferralValidationException>(() =>
      //      _service.CreateMskReferralAsync(create));

      //  // assert
      //  ex.ValidationResults.Keys.Should().ContainSingle()
      //    .Which.Should().Be(nameof(Referral.CalculatedBmiAtRegistration));
      //}

      [Fact]
      public async void InvalidServiceUserEthnicityGroup_Exception()
      {
        // arrange
        var create = RandomModelCreator.CreateRandomMskReferralCreate(
          serviceUserEthnicityGroup: "InvalidGroup");

        // act
        var ex = await Assert
          .ThrowsAsync<MskReferralValidationException>(() =>
            _service.CreateMskReferralAsync(create));

        // assert
        ex.ValidationResults.Keys.Should()
          .Contain(nameof(Referral.ServiceUserEthnicityGroup));
      }

      [Fact]
      public async void InvalidServiceUserEthnicity_Exception()
      {
        // arrange
        var create = RandomModelCreator.CreateRandomMskReferralCreate(
          serviceUserEthnicity: "InvalidEthnicity");

        // act
        var ex = await Assert
          .ThrowsAsync<MskReferralValidationException>(() =>
            _service.CreateMskReferralAsync(create));

        // assert
        ex.ValidationResults.Keys.Should()
          .Contain(nameof(Referral.ServiceUserEthnicity));
      }

      [Fact]
      public async void InvalidBmi_Exception()
      {
        // arrange
        var create = RandomModelCreator.CreateRandomMskReferralCreate(
          heightCm: Constants.MAX_HEIGHT_CM,
          weightKg: Constants.MIN_WEIGHT_KG);

        // act
        var ex = await Assert
          .ThrowsAsync<MskReferralValidationException>(() =>
            _service.CreateMskReferralAsync(create));

        // assert
        ex.ValidationResults.Keys.Should().ContainSingle()
          .Which.Should().Be(nameof(Referral.CalculatedBmiAtRegistration));
      }

      [Fact]
      public async void Valid()
      {
        // arrange
        DateTimeOffset testStart = DateTimeOffset.Now;

        // arrange - UpdateReferringGpPracticeName
        string mockReferringGpPracticeName = "Test Practice Name";
        string mockReferringGpPracticeNumber = "X12345";

        _mockOdsOrganisationService
          .Setup(x => x.GetOdsOrganisationAsync(It.IsAny<string>()))
          .ReturnsAsync(new OdsOrganisation(
            mockReferringGpPracticeNumber,
            mockReferringGpPracticeName));

        // arrange - UpdateDeprivation
        int mockImdDecile = 10;
        string mockLsoa = "X99999999";
        string expectedDeprivation = "IMD5";

        _mockPostcodeService
          .Setup(x => x.GetLsoa(It.IsAny<string>()))
          .ReturnsAsync(mockLsoa);

        _mockDeprivationService
          .Setup(x => x.GetByLsoa(It.IsAny<string>()))
          .ReturnsAsync(new Business.Models.Deprivation(
            mockImdDecile, mockLsoa));

        // arrange - TriageReferral
        Enums.TriageLevel mockTriagedCompletionLevel = Enums.TriageLevel.High;
        Enums.TriageLevel mockTriagedWeightedLevel = Enums.TriageLevel.Low;
        string expectedTriagedCompletionLevel = "3";
        string expectedTriagedWeightedLevel = "1";

        Mock<CourseCompletionResult> mockCourseCompletionResult = new();
        mockCourseCompletionResult
          .Setup(x => x.TriagedCompletionLevel)
          .Returns(mockTriagedCompletionLevel);
        mockCourseCompletionResult
          .Setup(x => x.TriagedWeightedLevel)
          .Returns(mockTriagedWeightedLevel);

        _mockPatientTriageService
          .Setup(x => x.GetScores(It.IsAny<CourseCompletionParameters>()))
          .Returns(mockCourseCompletionResult.Object);

        // arrange - MskReferralCreate
        var create = RandomModelCreator.CreateRandomMskReferralCreate(
          referringGpPracticeNumber: mockReferringGpPracticeNumber);

        // arrange - CalculatedBmiAtRegistration
        decimal expectedBmi = BmiHelper.CalculateBmi(
          create.WeightKg, create.HeightCm);

        // arrange - Ubrn
        var expectedUbrn = "MSK000000001";

        // act
        await _service.CreateMskReferralAsync(create);

        // assert - SaveChangesAsync
        var referral = _context.Referrals
          .Include(r => r.Audits)
          .Include(r => r.Calls)
          .Include(r => r.Provider)
          .Include(r => r.ProviderSubmissions)
          .Include(r => r.TextMessages)
          .Single(r => r.NhsNumber == create.NhsNumber);

        referral.Should().BeEquivalentTo(create, options => options
          .WithMapping<Entities.Referral>(
            c => c.ReferringMskHubOdsCode,
            r => r.ReferringOrganisationOdsCode)
          .WithMapping<Entities.Referral>(
            c => c.ReferringMskClinicianEmailAddress,
            r => r.ReferringClinicianEmail));

        // assert - UpdateBmiAndValidateAsync
        referral.CalculatedBmiAtRegistration.Should().Be(expectedBmi);

        // assert - UpdateReferringGpPracticeName
        referral.ReferringGpPracticeName.Should()
          .Be(mockReferringGpPracticeName);
        _mockOdsOrganisationService
          .Verify(x => x.GetOdsOrganisationAsync(It.IsAny<string>()),
            Times.Once);

        // assert - UpdateDeprivation
        referral.Deprivation.Should().Be(expectedDeprivation);
        _mockPostcodeService
          .Verify(x => x.GetLsoa(It.IsAny<string>()), Times.Once);        
        _mockDeprivationService
          .Verify(x => x.GetByLsoa(It.IsAny<string>()), Times.Once);

        // assert - TriageReferral
        referral.TriagedCompletionLevel.Should()
          .Be(expectedTriagedCompletionLevel);
        referral.OfferedCompletionLevel.Should()
          .Be(expectedTriagedCompletionLevel);
        referral.TriagedWeightedLevel.Should()
          .Be(expectedTriagedWeightedLevel);
        _mockPatientTriageService
          .Verify(x => x.GetScores(It.IsAny<CourseCompletionParameters>()), 
            Times.Once);

        // assert - UpdateModified
        referral.ModifiedAt.Should().BeAfter(testStart);
        referral.ModifiedByUserId.Should().Be(TEST_USER_ID);

        // assert - Ubrn
        referral.Ubrn.Should().Be(expectedUbrn);

        // assert - Generated
        referral.Audits.Should().HaveCount(2);
        referral.Id.Should().NotBeEmpty();

        // assert MskReferralCreate Defaults
        referral.ConsentForGpAndNhsNumberLookup.Should().BeTrue();
        referral.IsActive.Should().BeTrue();
        referral.CreatedDate.Should()
          .BeCloseTo(testStart, new TimeSpan(0, 0, 1));
        referral.DateOfReferral.Should()
          .BeCloseTo(testStart, new TimeSpan(0, 0, 1));
        referral.ReferralSource.Should().Be(ReferralSource.Msk.ToString());
        referral.Status.Should().Be(ReferralStatus.New.ToString());

        AssertMskReferralEquivalentOfReferral(referral, create);

        AssertEmptysAndNulls(referral);
      }

      private static void AssertEmptysAndNulls(Entities.Referral referral)
      {
        referral.Calls.Should().BeEmpty();
        referral.ProviderSubmissions.Should().BeEmpty();
        referral.TextMessages.Should().BeEmpty();

        referral.Cri.Should().BeNull();
        referral.ConsentForFutureContactForEvaluation.Should().BeNull();
        referral.DateCompletedProgramme.Should().BeNull();
        referral.DateLetterSent.Should().BeNull();
        referral.DateOfProviderContactedServiceUser.Should().BeNull();
        referral.DateOfProviderSelection.Should().BeNull();
        referral.DateStartedProgramme.Should().BeNull();
        referral.DateToDelayUntil.Should().BeNull();
        referral.DelayReason.Should().BeNull();
        referral.DocumentVersion.Should().BeNull();
        referral.FirstRecordedWeight.Should().BeNull();
        referral.FirstRecordedWeightDate.Should().BeNull();        
        referral.IsMobileValid.Should().BeNull();
        referral.IsTelephoneValid.Should().BeNull();
        referral.IsVulnerable.Should().BeNull();
        referral.LastRecordedWeight.Should().BeNull();
        referral.LastRecordedWeightDate.Should().BeNull();
        referral.LastTraceDate.Should().BeNull();
        referral.MethodOfContact.Should().BeNull();
        referral.MostRecentAttachmentId.Should().BeNull();
        referral.NhsLoginClaimEmail.Should().BeNull();
        referral.NhsLoginClaimFamilyName.Should().BeNull();
        referral.NhsLoginClaimGivenName.Should().BeNull();
        referral.NhsLoginClaimMobile.Should().BeNull();
        referral.NumberOfContacts.Should().BeNull();
        referral.ProgrammeOutcome.Should().BeNull();
        referral.Provider.Should().BeNull();
        referral.ProviderId.Should().BeNull();        
        referral.ReferralAttachmentId.Should().BeNull();
        referral.ServiceId.Should().BeNull();
        referral.SourceSystem.Should().BeNull();
        referral.StaffRole.Should().BeNull();
        referral.StatusReason.Should().BeNull();        
        referral.TraceCount.Should().BeNull();
        referral.VulnerableDescription.Should().BeNull();
      }

      private static void AssertMskReferralEquivalentOfReferral(
        Entities.Referral referral, 
        MskReferralCreate create)
      {
        create.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.CalculatedBmiAtRegistration)
          .Excluding(r => r.Calls)
          .Excluding(r => r.ConsentForFutureContactForEvaluation)
          .Excluding(r => r.Cri)
          .Excluding(r => r.DateCompletedProgramme)
          .Excluding(r => r.DateLetterSent)
          .Excluding(r => r.DateOfProviderContactedServiceUser)
          .Excluding(r => r.DateOfProviderSelection)
          .Excluding(r => r.DateStartedProgramme)
          .Excluding(r => r.DateToDelayUntil)
          .Excluding(r => r.DelayReason)
          .Excluding(r => r.Deprivation)
          .Excluding(r => r.DocumentVersion)
          .Excluding(r => r.FirstRecordedWeight)
          .Excluding(r => r.FirstRecordedWeightDate)
          .Excluding(r => r.Id)
          .Excluding(r => r.IsMobileValid)
          .Excluding(r => r.IsTelephoneValid)
          .Excluding(r => r.IsVulnerable)
          .Excluding(r => r.LastRecordedWeight)
          .Excluding(r => r.LastRecordedWeightDate)
          .Excluding(r => r.LastTraceDate)
          .Excluding(r => r.MethodOfContact)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.MostRecentAttachmentId)
          .Excluding(r => r.NhsLoginClaimEmail)
          .Excluding(r => r.NhsLoginClaimFamilyName)
          .Excluding(r => r.NhsLoginClaimGivenName)
          .Excluding(r => r.NhsLoginClaimMobile)
          .Excluding(r => r.NumberOfContacts)
          .Excluding(r => r.OfferedCompletionLevel)
          .Excluding(r => r.ProgrammeOutcome)
          .Excluding(r => r.Provider)
          .Excluding(r => r.ProviderSubmissions)
          .Excluding(r => r.ProviderId)
          .Excluding(r => r.ReferralAttachmentId)
          .Excluding(r => r.ReferringClinicianEmail)
          .Excluding(r => r.ReferringGpPracticeName)
          .Excluding(r => r.ReferringOrganisationEmail)
          .Excluding(r => r.ReferringOrganisationOdsCode)
          .Excluding(r => r.ServiceId)
          .Excluding(r => r.SourceSystem)
          .Excluding(r => r.StaffRole)
          .Excluding(r => r.StatusReason)
          .Excluding(r => r.TextMessages)
          .Excluding(r => r.TraceCount)
          .Excluding(r => r.TriagedCompletionLevel)
          .Excluding(r => r.TriagedWeightedLevel)
          .Excluding(r => r.Ubrn)
          .Excluding(r => r.VulnerableDescription));
      }
    }
  }
}
