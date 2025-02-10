using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Business.Services;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class CreateMskReferralAsyncTests
    : ReferralServiceTests, IDisposable
  {
    public CreateMskReferralAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      Log.Logger = new LoggerConfiguration()
      .WriteTo.TestOutput(testOutputHelper)
      .CreateLogger();

      AServiceFixtureBase.PopulateEthnicities(_context);
    }

    public new void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.MskReferrals.RemoveRange(_context.MskReferrals);
      _context.SaveChanges();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EthnicityCorrectlyOverrides()
    {
      // Arrange.
      string ethnicityToBeOverridden = "The patient does not want to disclose their ethnicity";
      string expectedEthnicity = "Other";
      string expectedServiceUserEthnicity = "I do not wish to Disclose my Ethnicity";
      string expectedServiceUserEthnicityGroup = "I do not wish to Disclose my Ethnicity";

      Entities.EthnicityOverride ethnicityOverride = new()
      {
        DisplayName = ethnicityToBeOverridden,
        EthnicityId = Guid.Parse("95b0feb5-5ece-98ed-1269-c71e327e98c5"),
        GroupName = ethnicityToBeOverridden,
        Id = Guid.NewGuid(),
        IsActive = true,
        ReferralSource = ReferralSource.Msk,
        ModifiedAt = DateTime.UtcNow,
        ModifiedByUserId = Guid.NewGuid(),
      };

      _context.EthnicityOverrides.Add(ethnicityOverride);
      await _context.SaveChangesAsync();

      IMskReferralCreate mskReferralCreate = RandomModelCreator.CreateRandomMskReferralCreate(
        serviceUserEthnicity: ethnicityToBeOverridden,
        serviceUserEthnicityGroup: ethnicityToBeOverridden,
        ethnicity: expectedEthnicity);

      // Arrange. - UpdateReferringGpPracticeName
      string mockReferringGpPracticeName = "Test Practice Name";
      string mockReferringGpPracticeNumber = "X12345";

      _mockOdsOrganisationService
        .Setup(x => x.GetOdsOrganisationAsync(It.IsAny<string>()))
        .ReturnsAsync(new OdsOrganisation(
          mockReferringGpPracticeNumber,
          mockReferringGpPracticeName));

      int mockImdDecile = 10;
      string mockLsoa = "X99999999";

      _mockPostcodeIoService
        .Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
        .ReturnsAsync(mockLsoa);

      _mockDeprivationService
        .Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .ReturnsAsync(new Business.Models.Deprivation(
          mockImdDecile, mockLsoa));

      Enums.TriageLevel mockTriagedCompletionLevel = Enums.TriageLevel.High;
      Enums.TriageLevel mockTriagedWeightedLevel = Enums.TriageLevel.Low;

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

      // Act.
      await _service.CreateMskReferralAsync(mskReferralCreate);

      // Assert.
      Entities.Referral createdReferral =  _context.Referrals
        .Where(r => r.NhsNumber == mskReferralCreate.NhsNumber)
        .SingleOrDefault();

      createdReferral.Should().NotBeNull();
      createdReferral.Ethnicity.Should().Be(expectedEthnicity);
      createdReferral.ServiceUserEthnicity.Should().Be(expectedServiceUserEthnicity);
      createdReferral.ServiceUserEthnicityGroup.Should().Be(expectedServiceUserEthnicityGroup);
    }

    [Fact]
    public async Task Param_Null_Exception()
    {
      // Arrange.
      IMskReferralCreate mskReferralCreate = null;

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.CreateMskReferralAsync(mskReferralCreate));

      // Assert.
      ex.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task NhsNumberInUseWithCurrentReferral_Exception()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator
        .CreateRandomReferral();
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      MskReferralCreate create = RandomModelCreator.CreateRandomMskReferralCreate(
        nhsNumber: referral.NhsNumber);

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.CreateMskReferralAsync(create));

      // Assert.
      ex.Should().BeOfType<ReferralNotUniqueException>();

      Entities.Referral savedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
      savedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits));
    }

    [Fact]
    public async Task InvalidServiceUserEthnicityGroup_Exception()
    {
      // Arrange.
      MskReferralCreate create = RandomModelCreator.CreateRandomMskReferralCreate(
        serviceUserEthnicityGroup: "InvalidGroup");

      // Act.
      MskReferralValidationException ex = await Assert
        .ThrowsAsync<MskReferralValidationException>(() =>
          _service.CreateMskReferralAsync(create));

      // Assert.
      ex.ValidationResults.Keys.Should()
        .Contain(nameof(Referral.ServiceUserEthnicityGroup));
    }

    [Fact]
    public async Task InvalidServiceUserEthnicity_Exception()
    {
      // Arrange.
      MskReferralCreate create = RandomModelCreator.CreateRandomMskReferralCreate(
        serviceUserEthnicity: "InvalidEthnicity");

      // Act.
      MskReferralValidationException ex = await Assert
        .ThrowsAsync<MskReferralValidationException>(() =>
          _service.CreateMskReferralAsync(create));

      // Assert.
      ex.ValidationResults.Keys.Should()
        .Contain(nameof(Referral.ServiceUserEthnicity));
    }

    [Fact]
    public async Task InvalidBmi_Exception()
    {
      // Arrange.
      MskReferralCreate create = RandomModelCreator.CreateRandomMskReferralCreate(
        heightCm: Constants.MAX_HEIGHT_CM,
        weightKg: Constants.MIN_WEIGHT_KG);

      // Act.
      MskReferralValidationException ex = await Assert
        .ThrowsAsync<MskReferralValidationException>(() =>
          _service.CreateMskReferralAsync(create));

      // Assert.
      ex.ValidationResults.Keys.Should().ContainSingle()
        .Which.Should().Be(nameof(Referral.CalculatedBmiAtRegistration));
    }

    [Fact]
    public async Task Valid()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.MskReferrals.RemoveRange(_context.MskReferrals);
      await _context.SaveChangesAsync();

      // Arrange.
      DateTimeOffset testStart = DateTimeOffset.Now;

      // Arrange. - UpdateReferringGpPracticeName
      string mockReferringGpPracticeName = "Test Practice Name";
      string mockReferringGpPracticeNumber = "X12345";

      _mockOdsOrganisationService
        .Setup(x => x.GetOdsOrganisationAsync(It.IsAny<string>()))
        .ReturnsAsync(new OdsOrganisation(
          mockReferringGpPracticeNumber,
          mockReferringGpPracticeName));

      // Arrange. - UpdateDeprivation
      int mockImdDecile = 10;
      string mockLsoa = "X99999999";
      string expectedDeprivation = "IMD5";

      _mockPostcodeIoService
        .Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
        .ReturnsAsync(mockLsoa);

      _mockDeprivationService
        .Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .ReturnsAsync(new Business.Models.Deprivation(
          mockImdDecile, mockLsoa));

      // Arrange. - TriageReferral
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

      // Arrange. - MskReferralCreate
      MskReferralCreate create = RandomModelCreator.CreateRandomMskReferralCreate(
        referringGpPracticeNumber: mockReferringGpPracticeNumber);

      // Arrange. - CalculatedBmiAtRegistration
      decimal expectedBmi = BmiHelper.CalculateBmi(
        create.WeightKg, create.HeightCm);

      // Act.
      await _service.CreateMskReferralAsync(create);

      // Assert. - SaveChangesAsync
      Entities.Referral referral = _context.Referrals
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

      // Assert. - UpdateBmiAndValidateAsync
      referral.CalculatedBmiAtRegistration.Should().Be(expectedBmi);

      // Assert. - UpdateReferringGpPracticeName
      referral.ReferringGpPracticeName.Should()
        .Be(mockReferringGpPracticeName);
      _mockOdsOrganisationService
        .Verify(x => x.GetOdsOrganisationAsync(It.IsAny<string>()),
          Times.Once);

      // Assert. - UpdateDeprivation
      referral.Deprivation.Should().Be(expectedDeprivation);
      _mockPostcodeIoService
        .Verify(x => x.GetLsoaAsync(It.IsAny<string>()), Times.Once);
      _mockDeprivationService.Verify(x => x.GetByLsoa(It.IsAny<string>()), Times.Once);

      // Assert. - TriageReferral
      referral.TriagedCompletionLevel.Should()
        .Be(expectedTriagedCompletionLevel);
      referral.OfferedCompletionLevel.Should()
        .Be(expectedTriagedCompletionLevel);
      referral.TriagedWeightedLevel.Should()
        .Be(expectedTriagedWeightedLevel);
      _mockPatientTriageService
        .Verify(x => x.GetScores(It.IsAny<CourseCompletionParameters>()),
          Times.Once);

      // Assert. - UpdateModified
      referral.ModifiedAt.Should().BeAfter(testStart);
      referral.ModifiedByUserId.Should().Be(TEST_USER_ID);

      // Assert. - Generated
      referral.Audits.Should().HaveCount(2);
      referral.Id.Should().NotBeEmpty();

      // Assert. MskReferralCreate Defaults
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

    [Fact]
    public async Task UpdateMskReferralUbrnAsyncError_DeactivatesReferral()
    {
      // Arrange.
      string mockReferringGpPracticeName = "Test Practice Name";
      string mockReferringGpPracticeNumber = "X12345";

      _mockOdsOrganisationService
        .Setup(x => x.GetOdsOrganisationAsync(It.IsAny<string>()))
        .ReturnsAsync(new OdsOrganisation(
          mockReferringGpPracticeNumber,
          mockReferringGpPracticeName));

      int mockImdDecile = 10;
      string mockLsoa = "X99999999";

      _mockPostcodeIoService
        .Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
        .ReturnsAsync(mockLsoa);

      _mockDeprivationService
        .Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .ReturnsAsync(new Business.Models.Deprivation(
          mockImdDecile, mockLsoa));

      TriageLevel mockTriagedCompletionLevel = TriageLevel.High;
      TriageLevel mockTriagedWeightedLevel = TriageLevel.Low;

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

      Entities.Ethnicity ethnicity = RandomEntityCreator.CreateRandomEthnicity(
        minimumBmi: 0);

      DatabaseContextOriginReferralsException testContext = new(_serviceFixture.Options);
      testContext.Referrals.RemoveRange(testContext.Referrals);

      testContext.Ethnicities.Add(ethnicity);
      await testContext.SaveChangesAsync();

      MskReferralCreate model = RandomModelCreator.CreateRandomMskReferralCreate(
        referringGpPracticeNumber: mockReferringGpPracticeNumber,
        ethnicity: ethnicity.TriageName,
        serviceUserEthnicity: ethnicity.DisplayName,
        serviceUserEthnicityGroup: ethnicity.GroupName);

        ReferralService testService = new(
          testContext,
          _serviceFixture.Mapper,
          _providerService,
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

      try
      {
        // Act.
        await testService.CreateMskReferralAsync(model);
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().NotBeNull();
        Entities.Referral storedReferral = testContext.Referrals.SingleOrDefault();
        storedReferral.Should().NotBeNull();
        storedReferral.IsActive.Should().BeFalse();
        storedReferral.Status.Should().Be("Exception");
        storedReferral.StatusReason.Should().Be("Error adding Ubrn to Referral.");
      }
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
      referral.IsErsClosed.Should().BeNull();
      referral.IsMobileValid.Should().BeNull();
      referral.IsTelephoneValid.Should().BeNull();
      referral.IsVulnerable.Should().BeNull();
      referral.LastRecordedWeight.Should().BeNull();
      referral.LastRecordedWeightDate.Should().BeNull();
      referral.LastTraceDate.Should().BeNull();
      referral.MethodOfContact.Should().BeNull();
      referral.MostRecentAttachmentDate.Should().BeNull();
      referral.NhsLoginClaimEmail.Should().BeNull();
      referral.NhsLoginClaimFamilyName.Should().BeNull();
      referral.NhsLoginClaimGivenName.Should().BeNull();
      referral.NhsLoginClaimMobile.Should().BeNull();
      referral.NumberOfContacts.Should().Be(0);
      referral.ProgrammeOutcome.Should().BeNull();
      referral.Provider.Should().BeNull();
      referral.ProviderId.Should().BeNull();
      referral.ReferralAttachmentId.Should().BeNull();
      referral.ReferralLetterDate.Should().BeNull();
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
        .Excluding(r => r.DatePlacedOnWaitingList)
        .Excluding(r => r.DateStartedProgramme)
        .Excluding(r => r.DateToDelayUntil)
        .Excluding(r => r.DelayReason)
        .Excluding(r => r.Deprivation)
        .Excluding(r => r.DocumentVersion)
        .Excluding(r => r.FirstRecordedWeight)
        .Excluding(r => r.FirstRecordedWeightDate)
        .Excluding(r => r.HeightFeet)
        .Excluding(r => r.HeightInches)
        .Excluding(r => r.HeightUnits)
        .Excluding(r => r.Id)
        .Excluding(r => r.IsErsClosed)
        .Excluding(r => r.IsMobileValid)
        .Excluding(r => r.IsTelephoneValid)
        .Excluding(r => r.IsVulnerable)
        .Excluding(r => r.LastRecordedWeight)
        .Excluding(r => r.LastRecordedWeightDate)
        .Excluding(r => r.LastTraceDate)
        .Excluding(r => r.MethodOfContact)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.MostRecentAttachmentDate)
        .Excluding(r => r.NhsLoginClaimEmail)
        .Excluding(r => r.NhsLoginClaimFamilyName)
        .Excluding(r => r.NhsLoginClaimGivenName)
        .Excluding(r => r.NhsLoginClaimMobile)
        .Excluding(r => r.NumberOfContacts)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.OpcsCodes)
        .Excluding(r => r.ProgrammeOutcome)
        .Excluding(r => r.Provider)
        .Excluding(r => r.ProviderId)
        .Excluding(r => r.ProviderSubmissions)
        .Excluding(r => r.ProviderUbrn)
        .Excluding(r => r.ReferralAttachmentId)
        .Excluding(r => r.ReferralLetterDate)
        .Excluding(r => r.ReferralQuestionnaire)
        .Excluding(r => r.ReferringClinicianEmail)
        .Excluding(r => r.ReferringGpPracticeName)
        .Excluding(r => r.ReferringOrganisationEmail)
        .Excluding(r => r.ReferringOrganisationOdsCode)
        .Excluding(r => r.ServiceId)
        .Excluding(r => r.SourceEthnicity)
        .Excluding(r => r.SourceSystem)
        .Excluding(r => r.SpellIdentifier)
        .Excluding(r => r.StaffRole)
        .Excluding(r => r.StatusReason)
        .Excluding(r => r.SurgeryInLessThanEighteenWeeks)
        .Excluding(r => r.TextMessages)
        .Excluding(r => r.TraceCount)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel)
        .Excluding(r => r.Ubrn)
        .Excluding(r => r.VulnerableDescription)
        .Excluding(r => r.WeeksOnWaitingList)
        .Excluding(r => r.WeightPounds)
        .Excluding(r => r.WeightStones)
        .Excluding(r => r.WeightUnits));
    }
  }
}
