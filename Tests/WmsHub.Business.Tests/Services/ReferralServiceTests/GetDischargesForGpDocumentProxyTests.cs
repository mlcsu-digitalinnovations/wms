using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class GetDischargesForGpDocumentProxyTests : ReferralServiceTests, IDisposable
  {
    private new readonly GpDocumentProxyOptions _gpDocumentProxyOptions = new();
    private readonly Mock<ILogger> _mockLogger;
    private readonly ReferralTimelineOptions _referralTimelineOptions = new();
    private new readonly ReferralService _service;

    public GetDischargesForGpDocumentProxyTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeCompleteTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeDidNotCommenceTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeDidNotCompleteTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeFailedToContactTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeInvalidContactDetailsTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId = 
        Guid.NewGuid();
      _gpDocumentProxyOptions.Gp.ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId = 
        Guid.NewGuid();

      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeCompleteTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeDidNotCommenceTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeDidNotCompleteTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeFailedToContactTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeInvalidContactDetailsTemplateId = Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId =
        Guid.NewGuid();
      _gpDocumentProxyOptions.Msk.ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId =
        Guid.NewGuid();

      _mockLogger = new Mock<ILogger>();

      _service = new ReferralService(
        _context,
        _serviceFixture.Mapper,
        null, // provider service
        _mockDeprivationService.Object,
        _mockLinkIdService.Object,
        _mockPostcodeIoService.Object,
        _mockPatientTriageService.Object,
        _mockOdsOrganisationService.Object,
        Options.Create(_gpDocumentProxyOptions),
        Options.Create(_referralTimelineOptions),
        null, // httpclient
        _mockLogger.Object)
      {
        User = GetClaimsPrincipal()
      };
    }

    public override void Dispose()
    {
      _context.MskOrganisations.RemoveRange(_context.MskOrganisations);
      _context.Providers.RemoveRange(_context.Providers);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task ReferralInactive_NoDischarges()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(isActive: false);
      _context.AddSaveAndDetachEntity(referral);

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Any().Should().BeFalse();
    }

    [Fact]
    public async Task ReferralDateOfBirthIsNull_NoDischarges()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      referral.DateOfBirth = null;
      _context.AddSaveAndDetachEntity(referral);

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Any().Should().BeFalse();
    }

    [Fact]
    public async Task ReferralDateOfReferralIsNull_NoDischarges()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      referral.DateOfReferral = null;
      _context.AddSaveAndDetachEntity(referral);

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Any().Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), 
      new ReferralStatus[] { ReferralStatus.AwaitingDischarge })]
    public async Task ReferralStatusNotAwaitingDischarge_NoDischarges(ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        consentForReferrerUpdatedWithOutcome: true,
        status: referralStatus);
      _context.AddSaveAndDetachEntity(referral);

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Any().Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralSourceMskOrganisationInactiveSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation(isActive: false);
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralSourceMskOrganisationSendDischargeLettersFalseSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation(
        sendDischargeLetters: false);
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralSourceMskConsentForReferrerUpdatedWithOutcomeNullSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation();
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        provider.Id);
      referral.ConsentForReferrerUpdatedWithOutcome = null;
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralSourceMskConsentForReferrerUpdatedWithOutcomeFalseSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation();
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        provider.Id);
      referral.ConsentForReferrerUpdatedWithOutcome = false;
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralSourceMskOrganisationUnmatchedSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation();
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        "INVALIDODSCODE",
        programmeOutcome.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Fact]
    public async Task GetTemplateIdArgumentExceptionTemplateIdNull()
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomGpReferral(
        ProgrammeOutcome.NotSet.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      string messageTemplate = "Get Discharge for referral {Id} failed: {message}";
      string message = $"Programme Outcome {referral.ProgrammeOutcome} does not match any " + 
        "discharge template Ids.";

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().BeNull();
      _mockLogger.Verify(l => l.Error(
        It.IsAny<Exception>(),
        messageTemplate,
        referral.Id,
        message));
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralGpReferralSingleDischarge(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomGpReferral(programmeOutcome.ToString(), provider.Id);
      _context.AddSaveAndDetachEntity(referral);

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      Guid expectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());
      result.Count.Should().Be(1);
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralMskTwoDischarges(string message, ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation();
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      Guid expectedGpTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());
      Guid expectedMskTemplateId = _gpDocumentProxyOptions.Msk
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result[0].ReferringOrganisationOdsCode.Should().Be(referral.ReferringGpPracticeNumber);
      result[0].TemplateId.Should().Be(expectedGpTemplateId);
      result[1].ReferringOrganisationOdsCode.Should().Be(referral.ReferringOrganisationOdsCode);
      result[1].TemplateId.Should().Be(expectedMskTemplateId);
      ReferralAssertions(message, provider, referral, result[0]);
      ReferralAssertions(message, provider, referral, result[1]);
    }

    [Theory]
    [MemberData(nameof(MessageAndProgrammeOutcomeTheoryData))]
    public async Task ReferralGpReferralMskThreeDischarges(
      string message,
      ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      MskOrganisation mskOrg = RandomEntityCreator.CreateRandomMskOrganisation();
      _context.AddSaveAndDetachEntity(mskOrg);
      Provider gpProvider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(gpProvider);
      Provider mskProvider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(mskProvider);
      Referral gpReferral = CreateRandomGpReferral(programmeOutcome.ToString(), gpProvider.Id);
      _context.AddSaveAndDetachEntity(gpReferral);
      Referral mskReferral = CreateRandomMskReferral(
        mskOrg.OdsCode,
        programmeOutcome.ToString(),
        mskProvider.Id);
      _context.AddSaveAndDetachEntity(mskReferral);
      Guid gpExpectedTemplateId = _gpDocumentProxyOptions.Gp
        .GetTemplateId(programmeOutcome.ToString());
      Guid mskExpectedTemplateId = _gpDocumentProxyOptions.Msk
        .GetTemplateId(programmeOutcome.ToString());

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert
      GpDocumentProxyReferralDischarge gpResult = result
        .Single(r => r.ReferralSource == ReferralSource.GpReferral.ToString());
      GpDocumentProxyReferralDischarge mskResult1 = result
        .FirstOrDefault(r => r.ReferralSource == ReferralSource.Msk.ToString());
      GpDocumentProxyReferralDischarge mskResult2 = result
        .LastOrDefault(r => r.ReferralSource == ReferralSource.Msk.ToString());
      result.Count.Should().Be(3);
      gpResult.ReferringOrganisationOdsCode.Should().Be(gpReferral.ReferringGpPracticeNumber);
      gpResult.TemplateId.Should().Be(gpExpectedTemplateId);
      ReferralAssertions(message, gpProvider, gpReferral, gpResult);
      mskResult1.ReferringOrganisationOdsCode.Should().Be(mskReferral.ReferringGpPracticeNumber);
      mskResult1.TemplateId.Should().Be(gpExpectedTemplateId);
      ReferralAssertions(message, mskProvider, mskReferral, mskResult1);
      mskResult2.ReferringOrganisationOdsCode.Should().Be(mskReferral.ReferringOrganisationOdsCode);
      mskResult2.TemplateId.Should().Be(mskExpectedTemplateId);
      ReferralAssertions(message, mskProvider, mskReferral, mskResult2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ReferralGpReferralPostDischargesLimit(int postDischargesLimit)
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral referral = CreateRandomGpReferral(
        ProgrammeOutcome.DidNotCommence.ToString(),
        provider.Id);
      _context.AddSaveAndDetachEntity(referral);
      _gpDocumentProxyOptions.PostDischargesLimit = postDischargesLimit;

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      ReferralAssertions(null, provider, referral, result[0]);
    }

    [Fact]
    public async Task ReferralGpReferralPostDischargesLimitOneDischarge()
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.AddSaveAndDetachEntity(provider);
      Referral olderReferral = CreateRandomGpReferral(
        ProgrammeOutcome.DidNotCommence.ToString(),
        provider.Id);
      olderReferral.DateOfReferral = DateTime.UtcNow.AddDays(-2);
      Referral newerReferral = CreateRandomGpReferral(
        ProgrammeOutcome.DidNotCommence.ToString(),
        provider.Id);
      newerReferral.DateOfReferral = DateTime.UtcNow.AddDays(-1);
      _context.AddSaveAndDetachEntity(olderReferral);
      _context.AddSaveAndDetachEntity(newerReferral);
      _gpDocumentProxyOptions.PostDischargesLimit = 1;

      // Act.
      List<GpDocumentProxyReferralDischarge> result = await _service
        .GetDischargesForGpDocumentProxy();

      // Assert.
      result.Count.Should().Be(1);
      ReferralAssertions(null, provider, olderReferral, result[0]);
    }

    private Referral CreateRandomGpReferral(string programmeOutcome, Guid providerId)
    {
      return RandomEntityCreator.CreateRandomReferral(
        consentForReferrerUpdatedWithOutcome: false,
        dateCompletedProgramme: DateTime.UtcNow,
        firstRecordedWeight: Constants.MAX_WEIGHT_KG,
        lastRecordedWeight: Constants.MIN_WEIGHT_KG,
        lastRecordedWeightDate: DateTime.UtcNow,
        programmeOutcome: programmeOutcome,
        providerId: providerId,
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.AwaitingDischarge,
        statusReason: $"{programmeOutcome}MESSAGE");
    }

    private Referral CreateRandomMskReferral(
      string odsCode,
      string programmeOutcome,
      Guid providerId)
    {
      return RandomEntityCreator.CreateRandomReferral(
        consentForReferrerUpdatedWithOutcome: true,
        dateCompletedProgramme: DateTime.UtcNow,
        firstRecordedWeight: Constants.MAX_WEIGHT_KG,
        lastRecordedWeight: Constants.MIN_WEIGHT_KG,
        lastRecordedWeightDate: DateTime.UtcNow,
        programmeOutcome: programmeOutcome,
        providerId: providerId,
        referralSource: ReferralSource.Msk,
        referringOrganisationOdsCode: odsCode,
        status: ReferralStatus.AwaitingDischarge,
        statusReason: $"{programmeOutcome}MESSAGE");
    }

    private void ReferralAssertions(
      string message,
      Provider provider, 
      Referral referral,
      GpDocumentProxyReferralDischarge result)
    {
      result.Should().BeEquivalentTo(referral, options => options
        .ExcludingMissingMembers()
        .Excluding(r => r.ReferringOrganisationOdsCode)
        .Excluding(r => r.StatusReason)
        .Excluding(r => r.Sex)
        .WithMapping<GpDocumentProxyReferralDischarge>(
          e => e.FirstRecordedWeight, s => s.WeightOnReferral));
      result.Message.Should().Be(message);
      result.ProviderName.Should().Be(provider.Name);
      result.ReferralSourceDescription.Should().Be(
        ((ReferralSource)Enum.Parse(typeof(ReferralSource), referral.ReferralSource))
          .GetDescriptionAttributeValue());
      result.Sex.Should().Be(GetExpectedSexValue(referral.Sex));
    }

    private static string GetExpectedSexValue(string sex)
    {
      if (sex.TryParseSex(out Sex sexEnum))
      {
        return sexEnum switch
        {
          Sex.NotKnown => "Unspecified",
          Sex.NotSpecified => "Unspecified",
          _ => sex
        };
      }

      return null;
    }

    public static TheoryData<string, ProgrammeOutcome> MessageAndProgrammeOutcomeTheoryData()
    {
      TheoryData<string, ProgrammeOutcome> theoryData = new();

      IEnumerable<ProgrammeOutcome> enums = Enum.GetValues<ProgrammeOutcome>()
        .Where(e => e != ProgrammeOutcome.NotSet);

      foreach (ProgrammeOutcome programmeOutcome in enums)
      {
        string expectedMessage = programmeOutcome switch
        {
          ProgrammeOutcome.RejectedBeforeProviderSelection => 
            "RejectedBeforeProviderSelectionMESSAGE",
          ProgrammeOutcome.RejectedAfterProviderSelection => 
            "RejectedAfterProviderSelectionMESSAGE",
          _ => null
        };
        theoryData.Add(expectedMessage, programmeOutcome);
      }
      
      return theoryData;
    }
  }
}
