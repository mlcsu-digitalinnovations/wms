using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Enums;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Deprivation = WmsHub.Business.Models.Deprivation;
using IReferral = WmsHub.Business.Models.IReferral;
using IStaffRole = WmsHub.Business.Models.ReferralService.IStaffRole;
using Provider = WmsHub.Business.Entities.Provider;
using Referral = WmsHub.Business.Models.Referral;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public partial class ReferralServiceTests : ServiceTestsBase, IDisposable
{
  private readonly GeneralReferralCreate _validGeneralReferralCreate;
  private readonly SelfReferralCreate _validSelfReferralCreate;
  private readonly DatabaseContext _context;
  private readonly ReferralService _service;
  private readonly ProviderOptions _options = new() 
  { 
    CompletionDays = 84, NumDaysPastCompletedDate = 10 
  };
  private readonly GpDocumentProxyOptions _gpDocumentProxyOptions = new();
  private readonly Mock<IOptions<ProviderOptions>> _mockOptions = new();
  private readonly Mock<ICsvExportService> _mockCsvExport = new();

  private readonly Deprivation _mockDeprivationValue = new()
  {
    ImdDecile = 6,
    Lsoa = "E00000001"
  };
  private readonly Mock<IDeprivationService> _mockDeprivationService = new();
  private readonly Mock<ILinkIdService> _mockLinkIdService = new();
  private readonly Mock<IPostcodesIoService> _mockPostcodeIoService = new();
  private readonly Mock<CourseCompletionResult> _mockScoreResult = new();
  private readonly Mock<IPatientTriageService> _mockPatientTriageService = new();
  private readonly Mock<IOdsOrganisationService> _mockOdsOrganisationService = new();
  private readonly ProviderService _providerService;
  private readonly Mock<IOptions<GpDocumentProxyOptions>> _mockGpDocumentProxyOptions = new();
  private readonly Mock<IOptions<ReferralTimelineOptions>> _mockReferralTimelineOptions = new();

  public ReferralServiceTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _mockOptions.Setup(x => x.Value).Returns(_options);

    _context = new DatabaseContext(_serviceFixture.Options);

    _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue);

    _mockPostcodeIoService.Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue.Lsoa);

    _mockPostcodeIoService
      .Setup(x => x.IsEnglishPostcodeAsync(It.IsAny<string>()))
      .ReturnsAsync(true);

    _providerService = new ProviderService(
        _context,
        _serviceFixture.Mapper,
        _mockOptions.Object);

    _mockScoreResult.Setup(t => t.TriagedCompletionLevel)
      .Returns(TriageLevel.High);

    _mockScoreResult.Setup(t => t.TriagedWeightedLevel)
      .Returns(TriageLevel.Medium);

    _mockPatientTriageService.Setup(t =>
        t.GetScores(It.IsAny<CourseCompletionParameters>()))
      .Returns(_mockScoreResult.Object);

    _mockGpDocumentProxyOptions.Setup(x => x.Value)
      .Returns(_gpDocumentProxyOptions);

    _service =
      new ReferralService(
        _context,
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

    _validGeneralReferralCreate = RandomModelCreator
      .CreateRandomGeneralReferralCreate(
        address1: "Address1",
        address2: "Address2",
        address3: "Address3",
        dateOfBirth: DateTimeOffset.Now.AddYears(-40),
        dateOfBmiAtRegistration: DateTimeOffset.Now.AddMonths(-6),
        ethnicity: Enums.Ethnicity.White.ToString(),
        hasDiabetesType1: null,
        hasDiabetesType2: null,
        hasHypertension: null,
        hasALearningDisability: false,
        hasAPhysicalDisability: false,
        heightCm: 150m,
        postcode: "TF1 4NF",
        serviceUserEthnicity: "Irish",
        serviceUserEthnicityGroup: "White",
        sex: "Male",
        telephone: "+441743123456",
        weightKg: 120m,
        consentForFutureContactForEvaluation: true,
        consentForGpAndNhsNumberLookup: true,
        consentForReferrerUpdatedWithOutcome: true);

    _validSelfReferralCreate = RandomModelCreator
      .CreateRandomSelfReferralCreate(
        address1: "Address1",
        address2: "Address2",
        address3: "Address3",
        dateOfBirth: DateTimeOffset.Now.AddYears(-40),
        dateOfBmiAtRegistration: DateTimeOffset.Now.AddMonths(-6),
        ethnicity: Enums.Ethnicity.White.ToString(),
        hasDiabetesType1: null,
        hasDiabetesType2: null,
        hasHypertension: null,
        hasALearningDisability: false,
        hasAPhysicalDisability: false,
        heightCm: 150m,
        postcode: "TF1 4NF",
        serviceUserEthnicity: "Irish",
        serviceUserEthnicityGroup: "White",
        sex: "Male",
        staffRole: ServiceFixture.STAFFROLE_AMBULANCEWORKER,
        telephone: "+441743123456",
        weightKg: 120m,
        consentForFutureContactForEvaluation: true);
  }

  public virtual void Dispose()
  {
    _context.Calls.RemoveRange(_context.Calls);
    _context.Providers.RemoveRange(_context.Providers);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.TextMessages.RemoveRange(_context.TextMessages);
    _context.SaveChanges();
    GC.SuppressFinalize(this);
  }

  private SelfReferralCreate UniqueValidReferralCreate()
  {
    while (true)
    {
      SelfReferralCreate r = _validSelfReferralCreate;

      if (!_context.Referrals.Any(t => t.Email == r.Email))
      {
        return r;
      }
    }
  }

  private GeneralReferralCreate UniqueValidGeneralReferralCreate()
  {
    while (true)
    {
      GeneralReferralCreate r = _validGeneralReferralCreate;

      if (!_context.Referrals.Any(t => t.Email == r.Email))
      {
        return r;
      }
    }
  }

  public static ReferralCreate ValidReferralCreate()
  {
    ReferralCreate referral = RandomModelCreator.CreateRandomReferralCreate(
      calculatedBmiAtRegistration: 30m,
      dateOfBirth: DateTimeOffset.Now.AddYears(-40),
      dateOfBmiAtRegistration: DateTimeOffset.Now.AddMonths(-6),
      dateOfReferral: DateTimeOffset.Now,
      documentVersion: 1.2m,
      email: "beaulah.casper37@ethereal.email",
      ethnicity: Enums.Ethnicity.White.ToString(),
      familyName: "FamilyName",
      givenName: "GivenName",
      hasALearningDisability: false,
      hasAPhysicalDisability: false,
      hasDiabetesType1: true,
      hasDiabetesType2: false,
      hasHypertension: true,
      hasRegisteredSeriousMentalIllness: false,
      heightCm: 150m,
      isVulnerable: false,
      mobile: "+447886123456",
      nhsNumber: null,
      postcode: "TF1 4NF",
      referralAttachmentId: Guid.NewGuid().ToString(),
      referringGpPracticeName: "Marden Medical Practice",
      referringGpPracticeNumber: "M82047",
      serviceId: "12345678",
      sourceSystem: SourceSystem.Emis,
      sex: "Male",
      telephone: "+441743123456",
      ubrn: null,
      vulnerableDescription: "Not Vulnerable",
      weightKg: 120m);

    referral.Address1 = null;
    referral.Address2 = null;
    referral.Address3 = null;

    return referral;
  }

  public static ReferralUpdate ValidReferralUpdate()
  {
    ReferralUpdate referral = RandomModelCreator
      .CreateRandomReferralUpdate(
      address1: "Address1",
      address2: "Address2",
      address3: "Address3",
      calculatedBmiAtRegistration: 30m,
      dateOfBirth: DateTimeOffset.Now.AddYears(-40),
      dateOfBmiAtRegistration: DateTimeOffset.Now.AddMonths(-6),
      dateOfReferral: DateTimeOffset.Now,
      email: "beaulah.casper37@ethereal.email",
      ethnicity: Enums.Ethnicity.White.ToString(),
      familyName: "FamilyName",
      givenName: "GivenName",
      hasALearningDisability: false,
      hasAPhysicalDisability: false,
      hasDiabetesType1: true,
      hasDiabetesType2: false,
      hasHypertension: true,
      hasRegisteredSeriousMentalIllness: false,
      heightCm: 150m,
      isVulnerable: false,
      mobile: "+447886123456",
      nhsNumber: null,
      postcode: "TF1 4NF",
      referralAttachmentId: Guid.NewGuid().ToString(),
      referringGpPracticeName: "Marden Medical Practice",
      referringGpPracticeNumber: "M82047",
      sex: "Male",
      telephone: "+441743123456",
      ubrn: null,
      vulnerableDescription: "Not Vulnerable",
      weightKg: 120m);

    return referral;
  }

  public class ReferralServiceConstructor : ReferralServiceTests
  {
    public ReferralServiceConstructor(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public void ReferralServiceInstantiate()
    {
      // Arrange.
      ProviderService _providerService = new(_context, _serviceFixture.Mapper, _mockOptions.Object);

      // Act.
      ReferralService service = new (
        _context,
        null,
        _serviceFixture.Mapper,
        _providerService,
        _mockCsvExport.Object,
        _mockPatientTriageService.Object);

      // Assert.
      service.Should().NotBeNull();
    }
  }

  public class ConfirmProviderAsync : ReferralServiceTests
  {
    public ConfirmProviderAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid referralId = INVALID_ID;
      Guid providerId = Guid.NewGuid();

      // Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(
        async () => await _service
          .ConfirmProviderAsync(referralId, providerId));
    }

    [Fact]
    public async Task ProviderUpdatedAsExpected()
    {
      Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Referrals.Add(referral);
      _context.Providers.Add(provider);
      _context.SaveChanges();

      string expectedStatus = ReferralStatus.ProviderAwaitingStart.ToString();

      // Act.
      IReferral referralReturned =
        await _service.ConfirmProviderAsync(referral.Id, provider.Id);

      // Assert.
      referralReturned.Status.Should().Be(expectedStatus);
      referralReturned.ProviderId = provider.Id;
      referralReturned.DateOfProviderSelection = DateTimeOffset.Now;
    }
  }

  public class ConfirmProviderAsyncWithReferral : ReferralServiceTests
  {
    public ConfirmProviderAsyncWithReferral(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Entities.Referral referral =
        CreateUniqueReferral(
          status: ReferralStatus.New);

      Referral referralModel = base._serviceFixture.Mapper
        .Map<Referral>(referral);

      // Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(
        async () => await _service
          .ConfirmProviderAsync(referralModel));
    }

    [Fact]
    public async Task ValidNhsNumber_ProviderAwaitingStart()
    {
      // Arrange.
      Entities.Referral referralExisting = CreateUniqueReferral(
        status: ReferralStatus.New);
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Referrals.Add(referralExisting);
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Referral referralModel = base._serviceFixture.Mapper
        .Map<Referral>(referralExisting);

      referralModel.ProviderId = provider.Id;

      string expectedStatus =
        ReferralStatus.ProviderAwaitingStart.ToString();
      string expectedEmail = referralExisting.Email;
      bool expectedConsentForFutureContactForEvaluation = true;

      // Act.
      IReferral referralReturned =
        await _service.ConfirmProviderAsync(referralModel);

      // Assert.
      referralReturned.Status.Should().Be(expectedStatus);
      referralReturned.ProviderId.Should().Be(provider.Id);
      referralReturned.DateOfProviderSelection.Should()
        .BeSameDateAs(DateTimeOffset.Now);
      referralReturned.ConsentForFutureContactForEvaluation.Should()
        .Be(expectedConsentForFutureContactForEvaluation);
      referralReturned.Email.Should().Be(expectedEmail);
      referralReturned.Status.Should()
        .Be(ReferralStatus.ProviderAwaitingStart.ToString());
      referralExisting.MethodOfContact.Should()
        .Be((int)MethodOfContact.RmcCall);
      referralExisting.NumberOfContacts.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task NullOrWhiteSpaceNhsNumber_ProviderAwaitingTrace(
      string nhsNumber)
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Entities.Referral referralExisting = CreateUniqueReferral(
        status: ReferralStatus.New,
        referralSource: ReferralSource.SelfReferral.ToString());
      referralExisting.NhsNumber = nhsNumber;
      _context.Referrals.Add(referralExisting);

      _context.SaveChanges();

      Referral referralModel = _serviceFixture.Mapper
        .Map<Referral>(referralExisting);
      referralModel.ProviderId = provider.Id;

      string expectedStatus =
        ReferralStatus.ProviderAwaitingTrace.ToString();

      // Act.
      IReferral referralReturned =
        await _service.ConfirmProviderAsync(referralModel);

      // Assert.
      referralReturned.Status.Should().Be(expectedStatus);
      referralReturned.ProviderId.Should().Be(provider.Id);
      referralReturned.DateOfProviderSelection.Should()
        .BeSameDateAs(DateTimeOffset.Now);
      referralExisting.MethodOfContact.Should()
        .Be((int)MethodOfContact.RmcCall);
      referralExisting.NumberOfContacts.Should().Be(1);
    }
  }

  public class CreateException : ReferralServiceTests
  {
    const string UBRN_VALID = "123456789012";
    const string UBRN_SHORT = "12345678901";
    const string UBRN_LONG = "1234567890123";
    const string NHSNO_VALID = "9999999992";
    const string NHSNO_VALID2 = "8888888888";
    const string NHSNO_INVALID = "1234567890";
    const string NHSNO_SHORT = "123456789";
    const string NHSNO_LONG = "12345678901";

    public CreateException(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Assert.
      await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await _service.CreateException(null));
    }

    [Fact]
    public async Task ReferralCreateException_Undefined()
    {
      // Arrange.
      IReferralExceptionCreate model = RandomModelCreator
        .CreateRandomReferralExceptionCreate(
          CreateReferralException.Undefined);

      // Assert.
      await Assert.ThrowsAsync<ReferralCreateException>(
        async () => await _service.CreateException(model));
    }

    [Theory]
    // invalid ubrn
    [InlineData(null, "Ubrn")]
    [InlineData("", "Ubrn")]
    [InlineData(UBRN_SHORT, "Ubrn")]
    [InlineData(UBRN_LONG, "Ubrn")]
    public async Task ReferralCreateException_MissingAttachment(
      string ubrn, string expectedMessageContent)
    {
      // Arrange.
      IReferralExceptionCreate model = RandomModelCreator
        .CreateRandomReferralExceptionCreate(
          CreateReferralException.MissingAttachment);
      model.Ubrn = ubrn;

      // Assert.
      ReferralCreateException exception = 
        await Assert.ThrowsAsync<ReferralCreateException>(
          async () => await _service.CreateException(model));

      exception.Message.Should().Contain(expectedMessageContent);
    }

    [Theory]
    // invalid nhsNumberAttachment
    [InlineData(null, NHSNO_VALID, UBRN_VALID, "NhsNumber")]
    [InlineData("", NHSNO_VALID, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_SHORT, NHSNO_VALID, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_LONG, NHSNO_VALID, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_INVALID, NHSNO_VALID, UBRN_VALID, "NhsNumber")]
    // invalid nhsNumberWorkList
    [InlineData(NHSNO_VALID, null, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_VALID, "", UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_VALID, NHSNO_SHORT, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_VALID, NHSNO_LONG, UBRN_VALID, "NhsNumber")]
    [InlineData(NHSNO_VALID, NHSNO_INVALID, UBRN_VALID, "NhsNumber")]
    // invalid ubrn
    [InlineData(NHSNO_VALID, NHSNO_VALID2, null, "Ubrn")]
    [InlineData(NHSNO_VALID, NHSNO_VALID2, "", "Ubrn")]
    [InlineData(NHSNO_VALID, NHSNO_VALID2, UBRN_SHORT, "Ubrn")]
    [InlineData(NHSNO_VALID, NHSNO_VALID2, UBRN_LONG, "Ubrn")]
    // matching nhsNumberAttachment and nhsNumberWorkList
    [InlineData(NHSNO_VALID, NHSNO_VALID, UBRN_VALID, "match")]
    public async Task ReferralCreateException_NhsNumberMismatch(
      string nhsNumberAttachment,
      string nhsNumberWorkList,
      string ubrn,
      string expectedMessageContent)
    {

      // Arrange.
      IReferralExceptionCreate model = RandomModelCreator
        .CreateRandomReferralExceptionCreate(
          CreateReferralException.NhsNumberMismatch);
      model.NhsNumberAttachment = nhsNumberAttachment;
      model.NhsNumberWorkList = nhsNumberWorkList;
      model.Ubrn = ubrn;

      // Assert.
      ReferralCreateException exception = await Assert.ThrowsAsync<ReferralCreateException>(
        async () => await _service.CreateException(model));

      exception.Message.Should().Contain(expectedMessageContent);
    }

    [Fact]
    public async Task ExistingReferralWithCancelledByEreferralsStatus_UpdateReferral()
    {
      // Arrange.
      Entities.Referral existingReferral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.CancelledByEreferrals,
        ubrn: UBRN_VALID);
      _context.Referrals.Add(existingReferral);
      _context.SaveChanges();

      IReferralExceptionCreate referralExceptionCreate = RandomModelCreator
        .CreateRandomReferralExceptionCreate(
          exceptionType: CreateReferralException.MissingAttachment,
          mostRecentAttachmentDate: DateTimeOffset.Now.AddDays(-1),
          referralAttachmentId: "123456",
          ubrn: UBRN_VALID);

      Mock<ReferralService> referralServiceMock = new(
        _context,
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
        CallBase = true
      };

      IReferralExceptionUpdate referralExceptionUpdate = null;
      referralServiceMock
        .Setup(x => x.UpdateReferralToStatusExceptionAsync(It.IsAny<IReferralExceptionUpdate>()))        
        .Callback<IReferralExceptionUpdate>(x => referralExceptionUpdate = x)
        .ReturnsAsync((IReferral)null)
        .Verifiable(Times.Once);

      // Act.
      _ = await referralServiceMock.Object.CreateException(referralExceptionCreate);

      // Assert.
      referralExceptionUpdate.Should().BeEquivalentTo(referralExceptionCreate, options => options
        .Excluding(x => x.DocumentVersion)
        .Excluding(x => x.ServiceId)
        .Excluding(x => x.SourceSystem));

      referralServiceMock.Verify();
    }


    [Fact]
    public async Task ReferralNotUniqueException()
    {
      // Arrange.
      Entities.Referral existingReferral = RandomEntityCreator.CreateRandomReferral(
        ubrn: UBRN_VALID);

      _context.Referrals.Add(existingReferral);
      _context.SaveChanges();

      List<IReferralExceptionCreate> referrals = new()
      {
        RandomModelCreator.CreateRandomReferralExceptionCreate(
          exceptionType: CreateReferralException.MissingAttachment,
          ubrn: UBRN_VALID),
        RandomModelCreator.CreateRandomReferralExceptionCreate(
          exceptionType: CreateReferralException.NhsNumberMismatch,
          mostRecentAttachmentDate: DateTimeOffset.Now,
          referralAttachmentId: Guid.NewGuid().ToString(),
          ubrn: UBRN_VALID),
      };

      foreach (IReferralExceptionCreate referral in referrals)
      {
        // Assert.
        ReferralNotUniqueException exception = await Assert.ThrowsAsync<ReferralNotUniqueException>(
          async () => await _service.CreateException(referral));
      }
    }
  }

  public class DelayReferralForSevenDaysAsync : ReferralServiceTests
  {
    public DelayReferralForSevenDaysAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid referralId = INVALID_ID;
      string reason = "this is a delay test";

      // Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(
        async () => await _service
          .DelayReferralUntilAsync(referralId, reason, DateTimeOffset.Now));
    }

    [Fact]
    public async Task DelayReferralForSevenDaysAsync_AsExpected()
    {
      string reason = "this is a delay test";
      Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      string expectedStatus = ReferralStatus.RmcDelayed.ToString();
      DateTimeOffset newDelayDate = DateTimeOffset.Now.AddDays(7);

      // Act.
      IReferral referralReturned = await _service
        .DelayReferralUntilAsync(referral.Id, reason, newDelayDate);

      Entities.Referral updatedReferral = await _context
      .Referrals
      .Where(r => r.Id == referral.Id)
      .FirstOrDefaultAsync();

      // Assert.
      referralReturned.Status.Should().Be(expectedStatus);
      updatedReferral.DateToDelayUntil?.Date.Should().Be(newDelayDate.Date);
      referralReturned.MethodOfContact.Should()
        .Be((int)MethodOfContact.RmcCall);
      referralReturned.NumberOfContacts.Should().Be(1);
    }
  }

  public class GetOpenErsGpReferralsThatAreNotCancelledByEreferalsTests 
    : ReferralServiceTests, IDisposable
  {
    private const string SERVICE_ID_0 = "9988770";
    private const string SERVICE_ID_1 = "9988771";

    public GetOpenErsGpReferralsThatAreNotCancelledByEreferalsTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      CleanUp();
    }

    private void CleanUp()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    public new void Dispose()
    {
      GC.SuppressFinalize(this);
      CleanUp();
    }

    public static TheoryData<string> ServiceIdTheoryData()
    {
      return new() { { null }, { SERVICE_ID_1 } };
    }

    [Theory]
    [MemberData(nameof(ServiceIdTheoryData))]
    public async Task NoActiveReferrals_EmptyList(string serviceId)
    {
      // Arrange.
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: false,
        isErsClosed: false,
        referralSource: ReferralSource.GpReferral,
        serviceId: serviceId ?? SERVICE_ID_0,
        status: ReferralStatus.Complete));
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      // Assert.
      result.Should().HaveCount(0);
    }

    [Theory]
    [MemberData(nameof(ServiceIdTheoryData))]
    public async Task NoGpReferrals_EmptyList(string serviceId)
    {
      // Arrange.
      foreach (ReferralSource referralSource in Enum
        .GetValues(typeof(ReferralSource)))
      {
        if (referralSource != ReferralSource.GpReferral)
        {
          _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
            isActive: true,
            isErsClosed: false,
            referralSource: referralSource,
            serviceId: serviceId ?? SERVICE_ID_0,
            status: ReferralStatus.Complete));
        }
      }

      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      // Assert.
      result.Should().HaveCount(0);
    }

    [Fact]
    public async Task NoReferralsForServiceId_EmptyList()
    {
      // Arrange.
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        isErsClosed: false,
        referralSource: ReferralSource.GpReferral,
        serviceId: SERVICE_ID_0,
        status: ReferralStatus.Complete));
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(SERVICE_ID_1);

      // Assert.
      result.Should().HaveCount(0);
    }

    [Fact]
    public async Task OnlyOpenErsGpReferralsThatAreNotCancelled_List()
    {
      // Arrange.
      List<Entities.Referral> referrals = new();
      foreach (ReferralStatus status in Enum
        .GetValues(typeof(ReferralStatus)))
      {
        referrals.Add(RandomEntityCreator.CreateRandomReferral(
          cri: RandomEntityCreator.ReferralCri(),
          isActive: true,
          isErsClosed: false,
          referralAttachmentId: Guid.NewGuid().ToString(),
          referralSource: ReferralSource.GpReferral,
          status: status,
          serviceId: SERVICE_ID_1));
      }

      _context.AddRange(referrals);

      Entities.Referral[] referralsAwaitingUpdate = referrals
        .Where(r => r.Status == ReferralStatus
          .RejectedToEreferrals.ToString())
        .ToArray();

      Entities.Referral[] referralsInProgress = referrals
        .Where(r => r.Status != ReferralStatus
          .CancelledByEreferrals.ToString())
        .Where(r => r.Status != ReferralStatus
          .Exception.ToString())
        .Where(r => r.Status != ReferralStatus
          .RejectedToEreferrals.ToString())
        .ToArray();

      Entities.Referral[] referralsOnHold = referrals
        .Where(r => r.Status == ReferralStatus.Exception.ToString())
        .ToArray();

      int expectedActiveReferralsCount =
        referralsAwaitingUpdate.Length
        + referralsInProgress.Length
        + referralsOnHold.Length;

      _context.SaveChanges();

      // Act. 
      List<ActiveReferralAndExceptionUbrn> activeReferrals = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals();

      // Assert.
      activeReferrals.Should().HaveCount(expectedActiveReferralsCount);

      foreach (ActiveReferralAndExceptionUbrn activeReferral
        in activeReferrals)
      {
        Entities.Referral referral = referrals
          .Single(x => x.Ubrn == activeReferral.Ubrn);

        activeReferral.CriLastUpdated.Should()
          .Be(referral.Cri.ClinicalInfoLastUpdated);

        activeReferral.MostRecentAttachmentDate.Should()
          .Be(referral.MostRecentAttachmentDate);

        activeReferral.ServiceId.Should().Be(referral.ServiceId);

        activeReferral.Ubrn.Should().Be(referral.Ubrn);

        if (referralsAwaitingUpdate.Contains(referral))
        {
          activeReferral.ReferralAttachmentId.Should()
            .Be(referral.ReferralAttachmentId);
          activeReferral.Status.Should().Be(
            ErsReferralStatus.AwaitingUpdate.ToString());
        }
        else if (referralsInProgress.Contains(referral))
        {
          activeReferral.ReferralAttachmentId.Should().BeNull();
          activeReferral.Status.Should().Be(
            ErsReferralStatus.InProgress.ToString());
        }
        else if (referralsOnHold.Contains(referral))
        {
          activeReferral.ReferralAttachmentId.Should().BeNull();
          activeReferral.Status.Should().Be(
            ErsReferralStatus.OnHold.ToString());
        }
        else
        {
          throw new XunitException($"Status {referral.Status} is missing");
        }
      }
    }

    [Fact]
    public async Task NullMostRecentAttachmentId_UsesReferralAttachmentId()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        cri: RandomEntityCreator.ReferralCri(),
        isActive: true,
        isErsClosed: false,
        mostRecentAttachmentDate: null,
        referralSource: ReferralSource.GpReferral,
        referralAttachmentId: Guid.NewGuid().ToString(),
        serviceId: SERVICE_ID_1,
        status: ReferralStatus.New);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> results = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals();

      // Assert.
      results.Should().HaveCount(1);

      ActiveReferralAndExceptionUbrn activeReferral = results.Single();

      activeReferral.CriLastUpdated.Should()
        .Be(referral.Cri.ClinicalInfoLastUpdated);

      activeReferral.MostRecentAttachmentDate.Should()
        .Be(referral.MostRecentAttachmentDate);

      activeReferral.ReferralAttachmentId.Should().BeNull();

      activeReferral.ServiceId.Should().Be(referral.ServiceId);

      activeReferral.Status.Should()
        .Be(ErsReferralStatus.InProgress.ToString());

      activeReferral.Ubrn.Should().Be(referral.Ubrn);
    }

    [Theory]
    [MemberData(nameof(ServiceIdTheoryData))]
    public async Task OnlyReferralsWithErsClosed_EmptyList(string serviceId)
    {
      // Arrange.
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        isErsClosed: true,
        referralSource: ReferralSource.GpReferral,
        serviceId: serviceId ?? SERVICE_ID_0,
        status: ReferralStatus.Exception));
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      // Assert.
      result.Should().HaveCount(0);
    }

    [Theory]
    [MemberData(nameof(ServiceIdTheoryData))]
    public async Task OnlyReferralsWithStatusCancelledByEreferrals_EmptyList(
      string serviceId)
    {
      // Arrange.
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        isErsClosed: false,
        referralSource: ReferralSource.GpReferral,
        serviceId: serviceId ?? SERVICE_ID_0,
        status: ReferralStatus.CancelledByEreferrals));
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      // Assert.
      result.Should().HaveCount(0);
    }

    [Theory]
    [InlineData(SERVICE_ID_1, 1)]
    [InlineData(null, 2)]
    public async Task ServiceIdFilter_List(
      string serviceId,
      int expectedCount)
    {
      // Arrange. 
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        isErsClosed: false,
        referralSource: ReferralSource.GpReferral,
        serviceId: SERVICE_ID_0,
        status: ReferralStatus.Complete));

      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        isErsClosed: false,
        referralSource: ReferralSource.GpReferral,
        serviceId: SERVICE_ID_1,
        status: ReferralStatus.Complete));

      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> result = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      // Assert.
      result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task StatusRejectedToEreferrals_ReferralAttachmentIdNotNull()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        cri: RandomEntityCreator.ReferralCri(),
        isActive: true,
        isErsClosed: false,
        mostRecentAttachmentDate: DateTimeOffset.Now,
        referralSource: ReferralSource.GpReferral,
        referralAttachmentId: Guid.NewGuid().ToString(),
        serviceId: SERVICE_ID_1,
        status: ReferralStatus.RejectedToEreferrals);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      List<ActiveReferralAndExceptionUbrn> results = await _service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals();

      // Assert.
      results.Should().HaveCount(1);

      ActiveReferralAndExceptionUbrn activeReferral = results.Single();

      activeReferral.CriLastUpdated.Should()
        .Be(referral.Cri.ClinicalInfoLastUpdated);

      activeReferral.MostRecentAttachmentDate.Should()
        .Be(referral.MostRecentAttachmentDate);

      activeReferral.ReferralAttachmentId.Should()
        .Be(referral.ReferralAttachmentId);

      activeReferral.ServiceId.Should().Be(referral.ServiceId);

      activeReferral.Status.Should()
        .Be(ErsReferralStatus.AwaitingUpdate.ToString());

      activeReferral.Ubrn.Should().Be(referral.Ubrn);
    }
  }

  public class GetNhsNumbers : ReferralServiceTests
  {
    public GetNhsNumbers(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [InlineData(10, 10)]
    [InlineData(null, 1)]
    [InlineData(100, 100)]
    [InlineData(200, 200)]
    [Theory]
    public void Valid(int? arrayCount, int countCheck)
    {
      // Act.
      string[] nhsNumberList = _service.GetNhsNumbers(arrayCount);

      // Assert.
      nhsNumberList.Should().NotBeNull();
      nhsNumberList.Should().BeOfType<string[]>();
      nhsNumberList.Length.Should().Be(countCheck);
    }
  }

  public class GetReferralById : ReferralServiceTests
  {
    public GetReferralById(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Entities.Referral createdReferral = CreateUniqueReferral();
      _context.Referrals.Add(createdReferral);
      _context.SaveChanges();
      Guid expected = createdReferral.Id;

      // Act.
      IReferral referral = await _service.GetReferralWithTriagedProvidersById(expected);

      // Assert.
      referral.Should().NotBeNull();
      referral.Should().BeOfType<Referral>();
      referral.Id.Should().Be(expected);
    }

    [Fact]
    public async Task Valid_TriageSetProviderList()
    {
      // Arrange.
      Entities.Referral createdReferral = RandomEntityCreator
        .CreateRandomReferral(
        triagedCompletionLevel: ((int)Enums.TriageLevel.Medium).ToString());
      _context.Referrals.Add(createdReferral);

      int expectedProviderCount = _context.Providers
      .Where(p => p.IsActive)
      .Where(p => p.Level2 == true)
      .Count();

      if (expectedProviderCount == 0)
      {
        Provider createdProvider = RandomEntityCreator.CreateRandomProvider(
        isLevel2: true);
        _context.Providers.Add(createdProvider);
        expectedProviderCount = 1;
      }

      _context.SaveChanges();
      Guid expected = createdReferral.Id;

      // Act.
      IReferral referral = await _service.GetReferralWithTriagedProvidersById(createdReferral.Id);

      // Assert.
      referral.Should().NotBeNull();
      referral.Should().BeOfType<Referral>();
      referral.Id.Should().Be(expected);
      referral.Providers.Count.Should().Be(expectedProviderCount);
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid referralId = INVALID_ID;

      // Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(
        async () => await _service.GetReferralWithTriagedProvidersById(referralId));

    }
  }

  public class GetReferralByTextMessageId : ReferralServiceTests
  {
    public GetReferralByTextMessageId(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator
        .CreateRandomReferral(status: ReferralStatus.TextMessage1);
      Entities.TextMessage textMessage =
        RandomEntityCreator.CreateRandomTextMessage();
      textMessage.IsActive = true;
      referral.TextMessages = new List<Entities.TextMessage>()
      {
        textMessage
      };
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      Guid expectedReferralId = referral.Id;

      // Act.
      IReferral referralReturned =
        await _service.GetReferralByTextMessageId(textMessage.Id);

      // Assert.
      referralReturned.Should().NotBeNull();
      referralReturned.Should().BeOfType<Referral>();
      referralReturned.Id.Should().Be(expectedReferralId);
      referralReturned.Status.Should()
        .Be(Enums.ReferralStatus.TextMessage1.ToString());
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid textMessageId = INVALID_ID;

      // Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(async () =>
          await _service.GetReferralByTextMessageId(textMessageId));
    }

    [Fact]
    public async Task TextMessageExpiredException()
    {
      // Arrange.
      Entities.Referral referral =
        CreateUniqueReferral();
      Entities.TextMessage textMessage = _serviceFixture.Mapper
        .Map<Entities.TextMessage>(ServiceFixture.ValidTextMessageEntity);
      textMessage.IsActive = false;
      referral.TextMessages = new List<Entities.TextMessage>()
      {
        textMessage
      };
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Assert.
      await Assert.ThrowsAsync<TextMessageExpiredException>(async () =>
        await _service.GetReferralByTextMessageId(textMessage.Id));
    }

    [Fact]
    public async Task ReferralInvalidStatusException()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.Status = Enums.ReferralStatus.Exception.ToString();

      Entities.TextMessage textMessage = _serviceFixture.Mapper
        .Map<Entities.TextMessage>(ServiceFixture.ValidTextMessageEntity);
      referral.TextMessages = new List<Entities.TextMessage>()
      {
        textMessage
      };
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Assert.
      await Assert.ThrowsAsync<ReferralInvalidStatusException>(async () =>
        await _service.GetReferralByTextMessageId(textMessage.Id));
    }
  }

  public class GetServiceUserReferralAsync : ReferralServiceTests
  {
    public GetServiceUserReferralAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("�$desa")]
    public async Task ServiceUserLinkIdInvalidException(string serviceUserLinkId)
    {
      // Act.
      Func<Task> act = async () =>
        await _service.GetServiceUserReferralAsync(serviceUserLinkId);

      // Assert.
       await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ServiceUserLinkIdDoesNotExistException()
    {
      // Arrange.
      string serviceUserLinkId = LinkIdService.GenerateDummyId();

      // Act.
      Func<Task> act = async () =>
        await _service.GetServiceUserReferralAsync(serviceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Theory]
    [InlineData(ReferralStatus.TextMessage1)]
    [InlineData(ReferralStatus.TextMessage2)]
    [InlineData(ReferralStatus.ChatBotCall1)]
    [InlineData(ReferralStatus.ChatBotTransfer)]
    [InlineData(ReferralStatus.RmcCall)]
    [InlineData(ReferralStatus.RmcDelayed)]
    public async Task TextMessage1_Referral(ReferralStatus status)
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(status: status);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));

      textMsg1.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      IReferral serviceUserReferral = await _service
        .GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.
      serviceUserReferral.Id.Should().Be(referral.Id);
    }

    [Theory]
    [InlineData(ReferralStatus.TextMessage1)]
    [InlineData(ReferralStatus.TextMessage2)]
    [InlineData(ReferralStatus.ChatBotCall1)]
    [InlineData(ReferralStatus.ChatBotTransfer)]
    [InlineData(ReferralStatus.RmcCall)]
    [InlineData(ReferralStatus.RmcDelayed)]
    public async Task TextMessage2_Referral(ReferralStatus status)
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(status: status);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
      Entities.TextMessage textMsg2 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-98));

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      IReferral serviceUserReferral = await _service
        .GetServiceUserReferralAsync(textMsg2.ServiceUserLinkId);

      // Assert.
      serviceUserReferral.Id.Should().Be(referral.Id);
    }

    [Fact]
    public async Task TextMessage1WithTextMessage2_Referral()
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
      Entities.TextMessage textMsg2 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-98));

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      IReferral serviceUserReferral = await _service
.GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.
      serviceUserReferral.Id.Should().Be(referral.Id);
    }

    [Fact]
    public async Task TextMessage1OutcomeDoNotContactEmail_Exception()
    {
      // Arrange.
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now,
          outcome: Constants.DO_NOT_CONTACT_EMAIL);

      textMsg1.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.      
      await act.Should().ThrowAsync<TextMessageExpiredByEmailException>();
    }

    [Fact]
    public async Task TextMessage2OutcomeDoNotContactEmail_Exception()
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddHours(-Constants.HOURS_BEFORE_NEXT_STAGE));
      Entities.TextMessage textMsg2 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent,
          outcome: Constants.DO_NOT_CONTACT_EMAIL);

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg2.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<TextMessageExpiredByEmailException>();
    }

    [Fact]
    public async Task TextMessage1ProviderSelection_Exception()
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: dateSent,
          providerId: Guid.NewGuid(),
          status: ReferralStatus.ProviderAwaitingStart);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage();

      textMsg1.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<TextMessageExpiredByProviderSelectionException>();
    }

    [Fact]
    public async Task TextMessage2ProviderSelection_Exception()
    {
      // Arrange.
      DateTimeOffset dateSent = DateTimeOffset.Now;
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: dateSent,
          providerId: Guid.NewGuid(),
          status: ReferralStatus.ProviderAwaitingStart);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
      Entities.TextMessage textMsg2 = 
        RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent);

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg2.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<TextMessageExpiredByProviderSelectionException>();
    }

    [Fact]
    public async Task TextMessage1OutcomeDateOfBirthExpiry_Exception()
    {
      // Arrange.
      Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);
      Entities.TextMessage textMsg1 =
        RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now,
          outcome: Constants.DATE_OF_BIRTH_EXPIRY,
          received: DateTimeOffset.Now);

      textMsg1.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<TextMessageExpiredByDoBCheckException>();
    }

    [Fact]
    public async Task TextMessage2OutcomeDateOfBirthExpiry_Exception()
    {
      // Arrange.
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);
      Entities.TextMessage textMsg1 =
        RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now.AddDays(-100));
      Entities.TextMessage textMsg2 =
        RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now.AddDays(-98),
          outcome: Constants.DATE_OF_BIRTH_EXPIRY,
          received: DateTimeOffset.Now);

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg2.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<TextMessageExpiredByDoBCheckException>();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      { ReferralStatus.TextMessage1,
        ReferralStatus.TextMessage2,
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage3
      })]
    public async Task TextMessage1ReferralInvalidStatus_Exception(
      ReferralStatus status)
    {
      // Arrange.
      Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status: status);
      Entities.TextMessage textMsg1 =
        RandomEntityCreator.CreateRandomTextMessage();
      
      textMsg1.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg1.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidStatusException>();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      { ReferralStatus.TextMessage1,
        ReferralStatus.TextMessage2,
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage3
      })]
    public async Task TextMessage2ReferralInvalidStatus_Exception(
      ReferralStatus status)
    {
      // Arrange.
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          status: status);
      Entities.TextMessage textMsg1 = 
        RandomEntityCreator.CreateRandomTextMessage();
      Entities.TextMessage textMsg2 = 
        RandomEntityCreator.CreateRandomTextMessage();

      textMsg1.Referral = referral;
      textMsg2.Referral = referral;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg1);
      _context.TextMessages.Add(textMsg2);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => await _service
        .GetServiceUserReferralAsync(textMsg2.ServiceUserLinkId);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidStatusException>();
    }
  }

  public class PrepareRmcCallsAsync : ReferralServiceTests
  {
    public PrepareRmcCallsAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ChatBotTransfer_PreparesRmcCall()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.Status = ReferralStatus.ChatBotTransfer.ToString();

      Entities.Call call = RandomEntityCreator.CreateRandomChatBotCall();
      call.ReferralId = referral.Id;
      call.Referral = referral;
      call.Sent = DateTimeOffset.Now.Date.AddHours(-(Constants.HOURS_BEFORE_NEXT_STAGE+1));

      _context.Referrals.Add(referral);
      _context.Calls.Add(call);
      await _context.SaveChangesAsync();

      string expectedMessage = "Prepared 1 referral(s) for an RMC call.";

      // Act.
      string returnedMessage = await _service.PrepareRmcCallsAsync();

      // Assert.
      returnedMessage.Should().NotBeNull().And.Be(expectedMessage);
      referral.Status.Should().Be(ReferralStatus.RmcCall.ToString());
    }
  }

  public class Search : ReferralServiceTests
  {
    public Search(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]  // TODO: SEE #1668
    public async Task ReturnsChatBotTransferAtTop()
    {
      // Arrange. -- add one RmcCall and ChatBotTransfer
      Entities.Referral rmcCallReferral = 
        CreateUniqueReferral(status: ReferralStatus.RmcCall);
      Entities.Referral chatBotTransferReferral = 
        CreateUniqueReferral(status: ReferralStatus.ChatBotTransfer);

      _context.Referrals.Add(rmcCallReferral);
      _context.Referrals.Add(chatBotTransferReferral);
      _context.SaveChanges();

      int expected = _context.Referrals
        .Count(r => r.Status == ReferralStatus.ChatBotTransfer.ToString());

      // Act.
      IReferralSearchResponse response =
          await _service.Search(new ReferralSearch());
      int referralCount = response.Count;
      IReferral referral = response.Referrals.FirstOrDefault();

      // Assert.
      expected.Should().BeGreaterThan(0);
      referral.Status.Should().Be(ReferralStatus.ChatBotTransfer.ToString());
    }

    [Fact]  // TODO: SEE #1668
    public async Task ReturnsChatBotTransferAtTop_WithSearchFields()
    {
      // Arrange. -- add one RmcCall and ChatBotTransfer;
      Entities.Referral rmcCallReferral = 
        CreateUniqueReferral(
          status: ReferralStatus.RmcCall);
      Entities.Referral chatBotTransferReferral = 
        CreateUniqueReferral(
          status: ReferralStatus.ChatBotTransfer);

      _context.Referrals.Add(rmcCallReferral);
      _context.Referrals.Add(chatBotTransferReferral);
      _context.SaveChanges();

      // Act.
      IReferralSearchResponse response = await _service.Search(
        new ReferralSearch());

      // Assert.
      IReferral referral = response.Referrals.FirstOrDefault();
      referral.Status.Should().Be(ReferralStatus.ChatBotTransfer.ToString());
    }

    [Fact]  // TODO: SEE #1668
    public async Task ReturnsChatBotTransferAtTop_WithStatusSearchField()
    {
      // Arrange.
      Entities.Referral rmcCallReferral = 
        CreateUniqueReferral(
          status: ReferralStatus.RmcCall);
      Entities.Referral chatBotTransferReferral = 
        CreateUniqueReferral(
          status: ReferralStatus.ChatBotTransfer);

      _context.Referrals.Add(rmcCallReferral);
      _context.Referrals.Add(chatBotTransferReferral);
      _context.SaveChanges();

      int expected = _context.Referrals
        .Count(r => r.Status == ReferralStatus.ChatBotTransfer.ToString());
      ReferralSearch search = new ()
      {
        Statuses = new string[] { ReferralStatus.ChatBotTransfer.ToString() }
      };

      // Act.
      IReferralSearchResponse response = await _service.Search(search);

      // Assert.
      response.Should().NotBeNull();
      response.Referrals.Should().NotBeNullOrEmpty();
      response.Referrals.Count().Should().Be(expected);
      response.Referrals.First().Status.Should().Be(ReferralStatus.ChatBotTransfer.ToString());
    }
  }

  public class TestCreateWithChatBotStatus : ReferralServiceTests
  {
    public TestCreateWithChatBotStatus(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Arrange.
      ReferralCreate model = null;

      // Act.
      Func<Task> act = async () => 
        await _service.TestCreateWithChatBotStatus(model);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReferralInvalidCreationException()
    {
      // Arrange.
      ReferralCreate model = ValidReferralCreate();
      model.NhsNumber = null;

      // Act.
      Func<Task> act = async () =>
        await _service.TestCreateWithChatBotStatus(model);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidCreationException>();
    }

    [Fact]
    public async Task ReferralUpdated()
    {
      // Arrange.
      ReferralCreate model = ValidReferralCreate();

      // Act.
      IReferral returned =
        await _service.TestCreateWithChatBotStatus(model);

      // Assert.
      returned.Should().NotBeNull();
      returned.IsActive.Should().Be(true);
      returned.Status.Should().Be(ReferralStatus.TextMessage2.ToString());
      returned.StatusReason.Should().Be("TestCreateWithChatBotStatus");
      returned.TextMessages.Count.Should().Be(2);
      returned.TextMessages[0].Sent.Date.Should()
        .Be(DateTimeOffset.Now.AddHours(-48).Date);
      returned.TextMessages[1].Sent.Date.Should()
        .Be(DateTimeOffset.Now.AddHours(-96).Date);
    }
  }

  public class Update : ReferralServiceTests
  {
    public Update(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Act.
      Func<Task> act = async () => await _service.UpdateGpReferral(null);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);
      ReferralUpdate referralUpdate = 
        _serviceFixture.Mapper.Map<ReferralUpdate>(referral);

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ReferralInvalidStatusException()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral(
      status: ReferralStatus.New);
      ReferralUpdate referralUpdate = base._serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => 
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      await act.Should().ThrowAsync<ReferralInvalidStatusException>();
    }

    /// <summary>
    /// A unit test the covers the following scenario.
    /// A referral was submitted but cancelled by ereferrals and returned
    /// to GP. The referral row ws set IsActive = false;
    /// A subsiquent referral was submitted with a different UBRN but same
    /// NHSNumber, whih was cancelled ToEreferrals as duplicate NHS recored
    /// found.
    /// However, this recored did not get set to isActive = false prior
    /// to the next referral as rejected to eReferrals as duplicate UBRN
    /// because the second referral was still active.
    /// a fourth referral was then submitted which should return
    /// ReferralNotUnique exception
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ValidReferralUpdate_ExceptionSingleORDefault()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());
      Entities.Referral referral1 = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: Generators.GenerateUbrn(new Random()),
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals);
      referral1.IsActive = false;
      _context.Referrals.Add(referral1);

      Entities.Referral referral2 = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals);
      _context.Referrals.Add(referral2);
      referral2.IsActive = true;

      Entities.Referral referral3 = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      referral3.IsActive = true;
      _context.Referrals.Add(referral3);

      _context.SaveChanges();

      ReferralCreate model = ValidReferralCreate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      string expected = "There are 2 active referrals with the same " +
        $"UBRN of {ubrn}.";

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(referral3);

      // Act.
      Func<Task> act = async () => 
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotUniqueException>().WithMessage(expected);
    }

    [Fact]
    public async Task DeprivationUpdated()
    {
      // Arrange.
      string oldDeprivation = "IMD1";
      string expectedDeprivation = "IMD3";
      string oldPostCode = "NN2 3ER";
      string expectedPostCode = "MK18 4LQ";

      Entities.Referral referral = RandomEntityCreator.
        CreateRandomReferral(
        status: ReferralStatus.RejectedToEreferrals);
      referral.Postcode = oldPostCode;
      referral.Deprivation = oldDeprivation;

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = base._serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.Postcode = expectedPostCode;

      // Act.
      IReferral referralReturned =
        await _service.UpdateGpReferral(referralUpdate);

      Entities.Referral updatedReferral = 
        await _context.Referrals
          .Where(r => r.Id == referral.Id)
          .FirstOrDefaultAsync();

      // Assert.
      referralReturned.Postcode.Should().Be(expectedPostCode).And.NotBe(oldPostCode);
      updatedReferral.Deprivation.Should().Be(expectedDeprivation).And.NotBe(oldDeprivation);
    }

    [Fact]
    public async Task Update_AsExpected()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.Status = ReferralStatus.RejectedToEreferrals.ToString();

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.HasHypertension = false;

      // Act.
      IReferral referralReturned =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      referralReturned.HasHypertension.Should().BeFalse();
    }

    [Fact]
    public async Task Update_NullEthnicity_TriageLevel_2_ShouldBeNull()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.Status = ReferralStatus.RejectedToEreferrals.ToString();

      referral.Ethnicity = null;
      referral.TriagedCompletionLevel = "2";
      referral.TriagedWeightedLevel = "1";

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      // Act.
      IReferral referralReturned =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      referralReturned.Ethnicity.Should().BeNull();
      referralReturned.TriagedCompletionLevel.Should().BeNull();
      referralReturned.TriagedWeightedLevel.Should().BeNull();
    }

    [Fact]
    public async Task Create_MultiReason_Invalid()
    {
      // Arrange.
      _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .Throws(new DeprivationNotFoundException());

      ProviderService _providerService =
        new (
          _context,
          _serviceFixture.Mapper,
          _mockOptions.Object);

      ReferralService _serviceToTest =
        new (
          _context,
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

      string expectedStatusReason =
        "The field CalculatedBmiAtRegistration must be between 27.5 " +
        "and 90. A diagnosis of Diabetes Type 1 or Diabetes " +
        "Type 2 or Hypertension is required.";
      string ubrn = $"{DateTimeOffset.Now:MMddHHmmssff}";

      Entities.Referral referral = RandomEntityCreator
        .CreateRandomReferral(
        calculatedBmiAtRegistration: 20,
        hasDiabetesType1: true,
        hasDiabetesType2: false,
        hasRegisteredSeriousMentalIllness: false,
        isVulnerable: false,
        status: ReferralStatus.RejectedToEreferrals,
        ubrn: ubrn);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.HasHypertension = false;
      referralUpdate.Ubrn = ubrn;
      referralUpdate.CalculatedBmiAtRegistration = 120;
      referralUpdate.Postcode = "WC4 4FN";
      referralUpdate.HasRegisteredSeriousMentalIllness = false;
      referralUpdate.HasALearningDisability = false;
      referralUpdate.HasAPhysicalDisability = false;
      referralUpdate.HasDiabetesType1 = false;
      referralUpdate.HasDiabetesType2 = false;
      referralUpdate.HasHypertension = false;
      referralUpdate.IsVulnerable = false;

      // Act.
      IReferral referralReturned = 
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      referralReturned.Status.Should().Be(ReferralStatus.Exception.ToString());
      referralReturned.StatusReason.Should().Be(expectedStatusReason);
    }
  }

  public class UpdateContactNumbers : ReferralServiceTests
  {
    public UpdateContactNumbers(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Theory]
    [MemberData(nameof(MobileInvalidTelephoneInvalidData))]
    public async Task UpdateGpReferral_MobileInvalidTelephoneInvalid(
      string mobile, 
      string telephone)
    {
      // Arrange.
      string expectedMobile = mobile == "" ? null : mobile;
      string expectedTelephone = telephone == "" ? null : telephone;

      Entities.Referral model = CreateUniqueReferral(
        hasHypertension: true, 
        hasDiabetesType1: true, 
        hasDiabetesType2: false);
      model.Mobile = null;
      model.Telephone = null;
      model.Status = ReferralStatus.RejectedToEreferrals.ToString();
      
      _context.Referrals.Add(model);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(model);

      referralUpdate.HasDiabetesType1 = true;
      referralUpdate.HasDiabetesType2 = false;
      referralUpdate.HasHypertension = true;
      referralUpdate.Mobile = mobile;
      referralUpdate.Telephone = telephone;

      string expectedStatusReason =
        $"One of the fields: {nameof(model.Telephone)} or " +
        $"{nameof(model.Mobile)} is required.";

      // Act.
      IReferral result =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Mobile.Should().Be(expectedMobile);
      result.IsMobileValid.Should().BeFalse();
      result.Telephone.Should().Be(expectedTelephone);
      result.IsTelephoneValid.Should().BeFalse();
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expectedStatusReason);
    }

    [Theory]
    [MemberData(nameof(MobileInvalidTelephoneIsMobileData))]
    public async Task UpdateGpReferral_MobileInvalidTelephoneIsMobileData(
      string mobile,
      string telephone)
    {
      // Arrange.
      Entities.Referral model = CreateUniqueReferral(
        hasHypertension: true,
        hasDiabetesType1: true,
        hasDiabetesType2: false);
      model.Mobile = null;
      model.Telephone = null;
      model.Status = ReferralStatus.RejectedToEreferrals.ToString();

      _context.Referrals.Add(model);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(model);

      referralUpdate.HasDiabetesType1 = true;
      referralUpdate.HasDiabetesType2 = false;
      referralUpdate.HasHypertension = true;
      referralUpdate.Mobile = mobile;
      referralUpdate.Telephone = telephone;

      // Act.
      IReferral result =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Mobile.Should().Be(MOBILE_E164, $"Mobile tested: {mobile}.");
      result.IsMobileValid.Should().BeTrue();
      result.Telephone.Should().BeNull($"Telephone tested: {telephone}.");
      result.IsTelephoneValid.Should().BeFalse();
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(MobileIsTelephoneTelephoneInvalidData))]
    public async Task UpdateGpReferral_MobileIsTelephoneTelephoneInvalidData(
      string mobile,
      string telephone)
    {
      // Arrange.
      Entities.Referral model = CreateUniqueReferral(
        hasHypertension: true,
        hasDiabetesType1: true,
        hasDiabetesType2: false);
      model.Mobile = null;
      model.Telephone = null;
      model.Status = ReferralStatus.RejectedToEreferrals.ToString();

      _context.Referrals.Add(model);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(model);

      referralUpdate.HasDiabetesType1 = true;
      referralUpdate.HasDiabetesType2 = false;
      referralUpdate.HasHypertension = true;
      referralUpdate.Mobile = mobile;
      referralUpdate.Telephone = telephone;

      // Act.
      IReferral result =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Mobile.Should().BeNull();
      result.IsMobileValid.Should().BeFalse();
      result.Telephone.Should().Be(TELEPHONE_E164);
      result.IsTelephoneValid.Should().BeTrue();
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }

    [Theory]
    [InlineData(TELEPHONE_E164, MOBILE_E164)]
    [InlineData(TELEPHONE, MOBILE)]
    public async Task 
      UpdateGpReferral_MobileIsTelephone_TelephoneIsMobile_SwitchValues(
        string mobile,
        string telephone)
    {
      // Arrange.
      Entities.Referral model = CreateUniqueReferral(
        hasHypertension: true,
        hasDiabetesType1: true,
        hasDiabetesType2: false);
      model.Mobile = null;
      model.Telephone = null;
      model.Status = ReferralStatus.RejectedToEreferrals.ToString();

      _context.Referrals.Add(model);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(model);

      referralUpdate.HasDiabetesType1 = true;
      referralUpdate.HasDiabetesType2 = false;
      referralUpdate.HasHypertension = true;

      referralUpdate.Mobile = mobile;
      referralUpdate.Telephone = telephone;

      // Act.
      IReferral result =
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Mobile.Should().Be(MOBILE_E164);
      result.IsMobileValid.Should().BeTrue();
      result.Telephone.Should().Be(TELEPHONE_E164);
      result.IsTelephoneValid.Should().BeTrue();
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }

    [Fact]
    public async Task Invalid_NoMobileAnd_NoTelephone()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.Mobile = "+447752267496";
      referral.Telephone = "+441366387303";
      referral.Status = ReferralStatus.RejectedToEreferrals.ToString();

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      string expected =
        "One of the fields: Telephone or Mobile is required.";

      ReferralUpdate referralUpdate = _serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.Mobile = null;
      referralUpdate.Telephone = null;
      referralUpdate.HasHypertension = false;
      referralUpdate.HasDiabetesType1 = true;
      referralUpdate.HasDiabetesType2 = false;

      // Act.
      IReferral result = await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expected);
    }
  }

  public class UpdateConsentForFutureContactForEvaluation
    : ReferralServiceTests
  {
    public UpdateConsentForFutureContactForEvaluation(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Valid()
    {
      // Update consent from false to true
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.ConsentForFutureContactForEvaluation = false;

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      bool consentGiven = false;

      // Act.
      IReferral updatedReferral = 
        await _service.UpdateConsentForFutureContactForEvaluation(
          referral.Id, 
          consentGiven,
          consentGiven);

      // Assert.
      updatedReferral.Should().NotBeNull();
      updatedReferral.Should().BeOfType<Referral>();
      updatedReferral.ConsentForFutureContactForEvaluation
        .Should()
        .Be(consentGiven);
    }

    [Fact]
    public async Task WithEmail()
    {
      // Update consent from false to true
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();
      referral.ConsentForFutureContactForEvaluation = false;

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      bool consentGiven = true;
      string emailAddress = "test@test.co.uk";

      // Act.
      IReferral updatedReferral = 
        await _service.UpdateConsentForFutureContactForEvaluation(
          referral.Id, 
          consentGiven, 
          consentGiven, 
          emailAddress);

      // Assert.
      updatedReferral.Should().NotBeNull();
      updatedReferral.Should().BeOfType<Referral>();
      updatedReferral.ConsentForFutureContactForEvaluation
        .Should()
        .Be(consentGiven);
      updatedReferral.Email.Should().Be(emailAddress);
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid referralId = INVALID_ID;
      bool consent = true;

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateConsentForFutureContactForEvaluation(
          referralId,
          consent,
          consent);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ReferralContactEmailException()
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      bool consent = true;
      bool emailNotSupplied = true;
      string emailAddress = "";

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateConsentForFutureContactForEvaluation(
          referral.Id,
          emailNotSupplied,
          consent,
          emailAddress);

      // Assert.
      await act.Should().ThrowAsync<ReferralContactEmailException>();
    }
  }

  public class UpdateEthnicity : ReferralServiceTests
  {
    public UpdateEthnicity(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Valid()
    {
      // Update ethnicity from Asian to Black.
      // Arrange.
      Entities.Referral referral =  CreateUniqueReferral();
      referral.Ethnicity = Enums.Ethnicity.Asian.ToString();

      Mock<CourseCompletionResult> courseCompletionResult =new ();
      courseCompletionResult.Setup(t => t.TriagedCompletionLevel).Returns(TriageLevel.Low);
      courseCompletionResult.Setup(t => t.TriagedWeightedLevel).Returns(TriageLevel.Low);

      _mockPatientTriageService
        .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()))
        .Returns(courseCompletionResult.Object);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      string displayName = "Caribbean";
      string groupName =
        EthnicityGroup.BlackAfricanCaribbeanOrBlackBritish.GetDescriptionAttributeValue();
      string triageName = Enums.Ethnicity.Black.ToString();

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        TriageName = triageName,
        MinimumBmi = 27.50m
      };

      string[] expectedTriageLevels = ["1", "2", "3"];

      // Act.
      IReferral updatedReferral = await _service.UpdateEthnicity(referral.Id, ethnicity);

      // Assert.
      updatedReferral.Should().NotBeNull().And.BeOfType<Referral>();
      updatedReferral.Ethnicity.Should().Be(triageName);
      updatedReferral.ServiceUserEthnicity.Should().Be(displayName);
      updatedReferral.ServiceUserEthnicityGroup.Should().Be(groupName);
      updatedReferral.TriagedCompletionLevel.Should()
        .BeOneOf(expectedTriageLevels);
      updatedReferral.TriagedWeightedLevel.Should()
        .BeOneOf(expectedTriageLevels);
    }

    [Fact]
    public async Task BmiIsTooLowSetsProvidersToEmptyArrayAndTriageLevelsToNull()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        calculatedBmiAtRegistration: 20m);

      Provider provider = RandomEntityCreator.CreateRandomProvider(
        isLevel1: true,
        isLevel2: true,
        isLevel3: true);

      _context.Providers.Add(provider);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      string displayName = "Caribbean";
      string groupName =
        EthnicityGroup.BlackAfricanCaribbeanOrBlackBritish.GetDescriptionAttributeValue();
      string triageName = Enums.Ethnicity.Black.ToString();

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        TriageName = triageName,
        MinimumBmi = 27.50m
      };

      // Act.
      IReferral updatedReferral = await _service.UpdateEthnicity(
        referral.Id, ethnicity);

      // Assert.
      updatedReferral.Should().NotBeNull().And.BeOfType<Referral>();
      updatedReferral.Providers.Should().BeEmpty();
      updatedReferral.Ethnicity.Should().Be(triageName);
      updatedReferral.ServiceUserEthnicity.Should().Be(displayName);
      updatedReferral.ServiceUserEthnicityGroup.Should().Be(groupName);
      updatedReferral.TriagedCompletionLevel.Should().BeNull();
      updatedReferral.TriagedWeightedLevel.Should().BeNull();
      updatedReferral.OfferedCompletionLevel.Should().BeNull();
    }

    [Fact]
    public async Task MinimumBmiNotSetThrowsException()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      Business.Models.Ethnicity ethnicity = new()
      {
        MinimumBmi = null,
        TriageName = "TriageName"
      };

      string expectedMessage = $"MinimumBmi*{ethnicity.TriageName}*";

      // Act.
      Func<Task> act = async () =>
      await _service.UpdateEthnicity(referral.Id, ethnicity);

      // Assert.
      await act.Should().ThrowAsync<EthnicityNotFoundException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task ProviderAlreadySelectedThrowsException()
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        providerId: provider.Id);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task> act = async () => 
      await _service.UpdateEthnicity(referral.Id, It.IsAny<Business.Models.Ethnicity>());

      // Assert.
      await act.Should().ThrowAsync<ReferralProviderSelectedException>();
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid referralId = INVALID_ID;

      // Act.
      Func<Task> act = async () => 
        await _service.UpdateEthnicity(referralId, It.IsAny<Business.Models.Ethnicity>());

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }
  }

  public class UpdateServiceUserEthnicityAsync : ReferralServiceTests
  {
    public UpdateServiceUserEthnicityAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task IdEmpty_Exception()
    {
      // Arrange.
      Guid referralId = Guid.Empty;
      string ethnicity = null;

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateServiceUserEthnicityAsync(
          referralId,
          ethnicity);

      // Assert
      await act.Should().ThrowAsync<ArgumentException>()
        .WithMessage("id cannot be empty. (Parameter 'id')");
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task EthnicityDisplayNameNullOrWhiteSpace_Exception(
      string ethnicity)
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string expectedErrMessage =
        "ethnicityDisplayName cannot be null or white space. " +
        "(Parameter 'ethnicityDisplayName')";
      // Act.
      Func<Task> act = async() => 
        await _service.UpdateServiceUserEthnicityAsync(
          referralId,
          ethnicity);

      // Assert.
      await act.Should().ThrowAsync<ArgumentException>()
        .WithMessage(expectedErrMessage);
    }

    [Fact]
    public async Task ReferralNotFound_Exception()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string ethnicity = "Chinese";
      string expectedErrMessage = 
        $"Unable to find a referral with an id of {referralId}.";

      // Act.
      Func<Task> act = async () => 
        await _service.UpdateServiceUserEthnicityAsync(
          referralId,
          ethnicity);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>()
        .WithMessage(expectedErrMessage);
    }

    [Fact]
    public async Task ReferralInactive_Exception()
    {
      // Arrange.
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          isActive: false);

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Func<Task> act = async () => 
        await _service.UpdateServiceUserEthnicityAsync(
          referral.Id,
          referral.Ethnicity);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>()
        .WithMessage(
          $"Unable to find a referral with an id of {referral.Id}.");

      _context.Referrals.Single(r => r.Id == referral.Id)
        .Should()
        .BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
    }

    [Fact]
    public async Task ReferralHasSelectedProvider_Exception()
    {
      // Arrange.
      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          providerId: Guid.NewGuid());

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateServiceUserEthnicityAsync(
          referral.Id,
          referral.Ethnicity);

      // Assert.
      await act.Should().ThrowAsync<ReferralProviderSelectedException>()
        .WithMessage(
          $"The referral {referral.Id} has previously had its provider " +
          $"selected {referral.ProviderId}.");

      _context.Referrals.Single(r => r.Id == referral.Id)
        .Should()
        .BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
    }

    [Fact]
    public async Task EthnicityDisplayNameNotFound_Exception()
    {
      // Arrange.
      string ethnicityDisplayName = "UnknownDisplayName";
      Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral();

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicityDisplayName));

      // Assert.
      ex.Should().BeOfType<EthnicityNotFoundException>();
      ex.Message.Should().Be(
        $"The ethnicity with a display " +
        $"name of {ethnicityDisplayName} cannot be found.");

      Entities.Referral dbReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);
      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits));
    }

    [Fact]
    public async Task CalculatedAtRegistrationBmiTooLow_Exception()
    {
      // Arrange.
      Entities.Ethnicity ethnicity = _context
        .Ethnicities
        .First(x => x.MinimumBmi == 30);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        calculatedBmiAtRegistration: 28.8m,
        ethnicity: null,
        heightCm: 150,
        serviceUserEthnicity: null,
        serviceUserEthnicityGroup: null,
        weightKg: 65);

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMsg = $"The calculated BMI of " +
        $"{referral.CalculatedBmiAtRegistration} is too low, the minimum " +
        $"for an ethnicity of {ethnicity.TriageName} " +
        $"is {ethnicity.MinimumBmi}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName));

      // Assert.
      ex.Should().BeOfType<BmiTooLowException>();
      ex.Message.Should().Be(expectedMsg);

      Entities.Referral dbReferral = _context
        .Referrals
        .Include(x => x.Audits)
        .Single(r => r.Id == referral.Id);

      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      dbReferral.Audits.Should().HaveCount(2);
      dbReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      dbReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      dbReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      dbReferral.OfferedCompletionLevel.Should().BeNull();
      dbReferral.ServiceUserEthnicity.Should().Be(ethnicity.DisplayName);
      dbReferral.ServiceUserEthnicityGroup.Should().Be(ethnicity.GroupName);
      dbReferral.Status.Should().Be(ReferralStatus.New.ToString());
      dbReferral.StatusReason.Should().Be(null);
      dbReferral.TriagedCompletionLevel.Should().BeNull();
      dbReferral.TriagedWeightedLevel.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData),
      new ReferralSource[] { ReferralSource.GpReferral })]
    public async Task 
      CalculatedBmiAtRegistration_Null_NotGpReferral_Calculated_Exception(
        ReferralSource referralSource)
    {
      // Arrange.
      Entities.Ethnicity ethnicity = _context
        .Ethnicities
        .First(x => x.MinimumBmi == 30);
      int height = 150;
      int weight = 35;
      decimal calculatedBmi = BmiHelper.CalculateBmi(weight, height);
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        ethnicity: null,
        heightCm: height,
        referralSource: referralSource,
        serviceUserEthnicity: null,
        serviceUserEthnicityGroup: null,
        weightKg: weight);

      referral.CalculatedBmiAtRegistration = null;

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMsg = $"The calculated BMI of {calculatedBmi} is too" +
        $" low, the minimum for an ethnicity of {ethnicity.TriageName} " +
        $"is {ethnicity.MinimumBmi}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName));

      // Assert.
      ex.Should().BeOfType<BmiTooLowException>();
      ex.Message.Should().Be(expectedMsg);

      Entities.Referral dbReferral = _context
        .Referrals
        .Include(x => x.Audits)
        .Single(r => r.Id == referral.Id);

      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.CalculatedBmiAtRegistration)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      dbReferral.Audits.Should().HaveCount(2);
      dbReferral.CalculatedBmiAtRegistration.Should()
        .Be(calculatedBmi);
      dbReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      dbReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      dbReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      dbReferral.OfferedCompletionLevel.Should().BeNull();
      dbReferral.ServiceUserEthnicity.Should().Be(ethnicity.DisplayName);
      dbReferral.ServiceUserEthnicityGroup.Should().Be(ethnicity.GroupName);
      dbReferral.Status.Should().Be(ReferralStatus.New.ToString());
      dbReferral.StatusReason.Should().Be(null);
      dbReferral.TriagedCompletionLevel.Should().BeNull();
      dbReferral.TriagedWeightedLevel.Should().BeNull();
    }

    [Fact]
    public async Task
      CalculatedBmiAtRegistration_Null_GpReferral_Calculated_Exception()
    {
      // Arrange.
      Entities.Ethnicity ethnicity = _context
        .Ethnicities
        .First(x => x.MinimumBmi == 30);
      int height = 150;
      int weight = 35;
      decimal calculatedBmi = BmiHelper.CalculateBmi(weight, height);
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        ethnicity: null,
        heightCm: height,
        referralSource: ReferralSource.GpReferral,
        serviceUserEthnicity: null,
        serviceUserEthnicityGroup: null,
        weightKg: weight);

      referral.CalculatedBmiAtRegistration = null;

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedMsg = "Validation of BMI cannot proceed without a " +
        "valid BMI value.";

      // Act.
      Exception ex = await Record.ExceptionAsync(() => _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName));

      // Assert.
      ex.Should().BeOfType<BmiTooLowException>();
      ex.Message.Should().Be(expectedMsg);

      Entities.Referral dbReferral = _context
        .Referrals
        .Include(x => x.Audits)
        .Single(r => r.Id == referral.Id);

      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.CalculatedBmiAtRegistration)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      dbReferral.Audits.Should().HaveCount(2);
      dbReferral.CalculatedBmiAtRegistration.Should().BeNull();
      dbReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      dbReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      dbReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      dbReferral.OfferedCompletionLevel.Should().BeNull();
      dbReferral.ServiceUserEthnicity.Should().Be(ethnicity.DisplayName);
      dbReferral.ServiceUserEthnicityGroup.Should().Be(ethnicity.GroupName);
      dbReferral.Status.Should().Be(ReferralStatus.New.ToString());
      dbReferral.StatusReason.Should().Be(null);
      dbReferral.TriagedCompletionLevel.Should().BeNull();
      dbReferral.TriagedWeightedLevel.Should().BeNull();
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Entities.Ethnicity ethnicity = _context.Ethnicities.First();

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        calculatedBmiAtRegistration: 44.4m,
        ethnicity: null,
        heightCm: 150,
        serviceUserEthnicity: null,
        serviceUserEthnicityGroup: null,
        weightKg: 100);

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      IReferral updatedReferral = await _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName);

      // Assert.
      updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Cri)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.DateToDelayUntil)
        .Excluding(r => r.IsErsClosed)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ReferralQuestionnaire)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.TextMessages)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      updatedReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      updatedReferral.OfferedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      updatedReferral.ServiceUserEthnicity.Should()
        .Be(ethnicity.DisplayName);
      updatedReferral.ServiceUserEthnicityGroup.Should()
        .Be(ethnicity.GroupName);
      updatedReferral.TriagedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      updatedReferral.TriagedWeightedLevel.Should()
        .BeOneOf(_expectedTriageLevels);

      Entities.Referral dbReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);
      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      dbReferral.Audits.Should().HaveCount(1);
      dbReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      dbReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      dbReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      dbReferral.OfferedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      dbReferral.ServiceUserEthnicity.Should().Be(ethnicity.DisplayName);
      dbReferral.ServiceUserEthnicityGroup.Should().Be(ethnicity.GroupName);
      dbReferral.TriagedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      dbReferral.TriagedWeightedLevel.Should()
        .BeOneOf(_expectedTriageLevels);
    }

    [Fact]
    public async Task 
      GpReferral_Provided_CalculatedBmiAtRegistration_Not_Overwritten()
    {
      // Arrange.
      Entities.Ethnicity ethnicity = _context.Ethnicities.First();
      int height = 180;
      int weight = 95;
      decimal calculatedBmiAtRegistration = 31.30m;
      decimal calculatedBmi = BmiHelper.CalculateBmi(weight, height);
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        calculatedBmiAtRegistration: calculatedBmiAtRegistration,
        ethnicity: null,
        heightCm: height,
        referralSource: ReferralSource.GpReferral,
        serviceUserEthnicity: null,
        serviceUserEthnicityGroup: null,
        weightKg: weight);

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      IReferral updatedReferral = await _service
        .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName);

      // Assert.
      updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Cri)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.DateToDelayUntil)
        .Excluding(r => r.IsErsClosed)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ReferralQuestionnaire)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.TextMessages)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      updatedReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      updatedReferral.OfferedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      updatedReferral.ServiceUserEthnicity.Should()
        .Be(ethnicity.DisplayName);
      updatedReferral.ServiceUserEthnicityGroup.Should()
        .Be(ethnicity.GroupName);
      updatedReferral.TriagedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      updatedReferral.TriagedWeightedLevel.Should()
        .BeOneOf(_expectedTriageLevels);

      Entities.Referral dbReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);
      dbReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Ethnicity)
        .Excluding(r => r.IsErsClosed)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.OfferedCompletionLevel)
        .Excluding(r => r.ServiceUserEthnicity)
        .Excluding(r => r.ServiceUserEthnicityGroup)
        .Excluding(r => r.TriagedCompletionLevel)
        .Excluding(r => r.TriagedWeightedLevel));

      dbReferral.Audits.Should().HaveCount(1);
      dbReferral.CalculatedBmiAtRegistration.Should()
        .Be(calculatedBmiAtRegistration);
      dbReferral.CalculatedBmiAtRegistration.Should()
        .NotBe(calculatedBmi);
      dbReferral.Ethnicity.Should().Be(ethnicity.TriageName);
      dbReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      dbReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      dbReferral.OfferedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      dbReferral.ServiceUserEthnicity.Should().Be(ethnicity.DisplayName);
      dbReferral.ServiceUserEthnicityGroup.Should().Be(ethnicity.GroupName);
      dbReferral.TriagedCompletionLevel.Should()
        .BeOneOf(_expectedTriageLevels);
      dbReferral.TriagedWeightedLevel.Should()
        .BeOneOf(_expectedTriageLevels);
    }
  }

  public class PrepareDelayedCalls : ReferralServiceTests
  {
    public PrepareDelayedCalls(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task PrepareDelayedCalls_SingleUpdate()
    {
      // Arrange.
      string expectedResonse = "Prepared DelayedCalls - " +
      "1 referral(s) set to 'RmcCall'.";

      DateTimeOffset dateInFurther1 = DateTimeOffset.Now.AddDays(+2);
      DateTimeOffset dateInFurther2 = DateTimeOffset.Now.AddDays(+6);
      DateTimeOffset dateNow = DateTimeOffset.Now.AddDays(-1);

      Entities.Referral referral1 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateInFurther1
        );
      Entities.Referral referral2 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateInFurther2
        );
      Entities.Referral referral3 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateNow
        );

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      _context.Referrals.Add(referral3);
      await _context.SaveChangesAsync();

      // Act.
      string response = await _service.PrepareDelayedCallsAsync();

      // Assert.
      response.Should().BeOfType<string>();
      response.Should().Be(expectedResonse);
      referral1.Status.Should().NotBe(ReferralStatus.RmcCall.ToString());
      referral2.Status.Should().NotBe(ReferralStatus.RmcCall.ToString());
      referral3.Status.Should().Be(ReferralStatus.RmcCall.ToString());

      referral1.DateToDelayUntil.Should().NotBeNull();
      referral2.DateToDelayUntil.Should().NotBeNull();
      referral3.DateToDelayUntil.Should().BeNull();
    }

    [Fact]
    public async Task PrepareDelayedCalls_UpdateSuccess()
    {
      // Arrange.
      string expectedResonse = "Prepared DelayedCalls - " +
      "2 referral(s) set to 'RmcCall'.";

      DateTimeOffset dateInFurther = DateTimeOffset.Now.AddDays(+2);
      DateTimeOffset dateInPast1 = DateTimeOffset.Now.AddDays(-2);
      DateTimeOffset dateInPast2 = DateTimeOffset.Now.AddDays(-6);
      DateTimeOffset dateNow = DateTimeOffset.Now;

      Entities.Referral referral1 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateInPast1
        );
      Entities.Referral referral2 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateInFurther
        );
      Entities.Referral referral3 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateInPast2
        );
      Entities.Referral referral4 = RandomEntityCreator
        .CreateRandomReferral(
        status: ReferralStatus.RmcDelayed,
        dateToDelayUntil: dateNow
        );

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      _context.Referrals.Add(referral3);
      _context.Referrals.Add(referral4);
      await _context.SaveChangesAsync();

      // Act.
      string response = await _service.PrepareDelayedCallsAsync();

      // Assert.
      response.Should().BeOfType<string>();
      response.Should().Be(expectedResonse);
      referral1.Status.Should().Be(ReferralStatus.RmcCall.ToString());
      referral3.Status.Should().Be(ReferralStatus.RmcCall.ToString());

      referral1.DateToDelayUntil.Should().BeNull();
      referral2.DateToDelayUntil.Should().NotBeNull();
      referral3.DateToDelayUntil.Should().BeNull();
    }
  }

  public class UpdateStatusFromRmcCallToFailedToContactAsync
    : ReferralServiceTests
  {
    public UpdateStatusFromRmcCallToFailedToContactAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralIdDoesNotExist_Exception()
    {
      // Arrange.
      Guid referralId = INVALID_ID;
      string expectedMessage = new ReferralNotFoundException(referralId)
        .Message;
      string statusReason = "It is a test for ReferralIdDoesNotExist.";

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.UpdateStatusFromRmcCallToFailedToContactAsync(
          referralId,
          statusReason));

      // Assert.
      ex.Should().BeOfType<ReferralNotFoundException>();
      ex.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData),
      new ReferralStatus[] { ReferralStatus.RmcCall })]
    public async Task ReferralInvalidStatus_Exception(ReferralStatus status)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: status);
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string statusReason = "It is a test for ReferralIdDoesNotExist.";
      string expectedMessage = "Unable to set status to " +
        $"{ReferralStatus.FailedToContact} because status is {status} " +
        $"when it must be {ReferralStatus.RmcCall}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _service.UpdateStatusFromRmcCallToFailedToContactAsync(
          referral.Id,
          statusReason)
        );

      // Assert.
      ex.Should().BeOfType<ReferralInvalidStatusException>();
      ex.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(
      ReferralSource.GeneralReferral,
      ReferralStatus.FailedToContact)]
    [InlineData(
      ReferralSource.GpReferral,
      ReferralStatus.FailedToContact)]
    [InlineData(
      ReferralSource.Pharmacy,
      ReferralStatus.FailedToContact)]
    [InlineData(
      ReferralSource.SelfReferral,
      ReferralStatus.FailedToContact)]
    public async Task ReferralSource_UpdateStatus(
      ReferralSource referralSource,
      ReferralStatus expectedStatus)
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: ReferralStatus.RmcCall);
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      int expectedMethodOfContact = (int)MethodOfContact.RmcCall;
      int expectedNumberOfContacts = 1;

      string expectedStatusReason = 
        "It is a test for ReferralSource_UpdateStatus.";
      // Act.
      IReferral result = await _service
        .UpdateStatusFromRmcCallToFailedToContactAsync(
          referral.Id,
          expectedStatusReason);

      // Assert.
      result.MethodOfContact.Should().Be(expectedMethodOfContact);
      result.NumberOfContacts.Should().Be(expectedNumberOfContacts);
      result.Status.Should().Be(expectedStatus.ToString());
      result.StatusReason.Should().Be(expectedStatusReason);

      Entities.Referral updatedReferral = _context.Referrals
        .Find(referral.Id);

      updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.MethodOfContact)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.NumberOfContacts)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason));

      updatedReferral.MethodOfContact.Should().Be(expectedMethodOfContact);
      updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      updatedReferral.NumberOfContacts.Should().Be(expectedNumberOfContacts);
      updatedReferral.Status.Should().Be(expectedStatus.ToString());
      updatedReferral.StatusReason.Should().Be(expectedStatusReason);
    }
  }

  public class UpdateStatusToRejectedToEreferralsAsync
    : ReferralServiceTests
  {
    public UpdateStatusToRejectedToEreferralsAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      Guid refId = INVALID_ID;

      // Act.
      Func<Task> act = async () =>
        await _service
          .UpdateStatusToRejectedToEreferralsAsync(refId, null);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Theory]
    [InlineData("Test", null, "Test")]
    [InlineData("Test", "", "Test")]
    [InlineData("Test", "UpdateTest", "UpdateTest")]
    public async Task ValidUpdate(
      string initialReason, string updateReason, string expectedReason)
    {
      // Arrange.
      DateTimeOffset startOfTest = DateTimeOffset.Now;
      Entities.Referral referral = RandomEntityCreator
      .CreateRandomReferral(
        statusReason: initialReason,
        status: ReferralStatus.Exception);

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      ReferralStatus expectedStatus =
        ReferralStatus.RejectedToEreferrals;

      // Act.
      IReferral returned = await _service
        .UpdateStatusToRejectedToEreferralsAsync(referral.Id, updateReason);

      // Assert.
      returned.Should().BeEquivalentTo(referral, options => options
        .ExcludingMissingMembers()
        .Excluding(o => o.TextMessages)
        .Excluding(o => o.Status)
        .Excluding(o => o.StatusReason)
        .Excluding(o => o.ModifiedAt)
        .Excluding(o => o.ModifiedByUserId));

      returned.Status.Should().Be(expectedStatus.ToString());
      returned.StatusReason.Should().Be(expectedReason);
      returned.ModifiedAt.Should().BeAfter(startOfTest);
      returned.ModifiedByUserId.Should().Be(_service.User.GetUserId());
    }
  }

  public class GetStaffRolesAsync : ReferralServiceTests
  {
    public GetStaffRolesAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      ServiceFixture.PopulateStaffRoles(_context);
      _context.SaveChanges();
    }

    public override void Dispose()
    {
      _context.StaffRoles.RemoveRange(_context.StaffRoles);
      _context.SaveChanges();
      base.Dispose();
    }

    [Fact]
    public async Task GetStaffRolesAsync_ReturnsListOfStaffRoles()
    {
      // Arrange.
      int expectedCount = _context.StaffRoles.Count();

      // Act.
      IEnumerable<IStaffRole> staffList = await _service.GetStaffRolesAsync();

      // Assert.
      Assert.Equal(expectedCount, staffList.Count());
    }
  }

  public class CreateGeneralReferralTests : ReferralServiceTests
  {
    public CreateGeneralReferralTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      if (!_context.Ethnicities.Any())
      {
        ServiceFixture.PopulateEthnicities(_context);
      }
    }

    [Fact]
    public async Task CreateGeneralReferral_ArgumentNullException()
    {
      // Arrange.
      GeneralReferralCreate model = null;

      // Act.
      Func<Task> act = async () => await _service
        .CreateGeneralReferral(model);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReferralContactEmailException_NotRealEmail()
    {
      // Arrange.
      DateTimeOffset methodCallTime = DateTimeOffset.Now;
      GeneralReferralCreate model = _validGeneralReferralCreate;
      model.Email = "Incorrect_EmailAddress";

      string expectedErrorMessage = 
        $"The Email field is not a valid e-mail address.";

      // Act.
      Func<Task> act = async () => 
        await _service.CreateGeneralReferral(model);

      // Assert.
      ExceptionAssertions<GeneralReferralValidationException> 
        exception = await act.Should()
        .ThrowAsync<GeneralReferralValidationException>();

      Dictionary<string, string[]> results =
          exception.Subject.First().ValidationResults;
      results.Count().Should().Be(1);
      results.First().Value.First().Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task NoProvidersAvailable_Exception()
    {
      // Arrange.
      List<Business.Models.Provider> providers = new ();
      Mock<IProviderService> mockProviderService = new ();
      mockProviderService
        .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
        .ReturnsAsync(providers);

      ReferralService service = new(
        _context,
        _serviceFixture.Mapper,
        mockProviderService.Object,
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

      // Act.
      Func<Task> act = async () => await service
        .CreateGeneralReferral(_validGeneralReferralCreate);

      // Assert.
      await act.Should().ThrowAsync<NoProviderChoicesFoundException>();
    }

    [Theory]
    [InlineData(TriageLevel.High)]
    [InlineData(TriageLevel.Medium)]
    public async Task NoLevel3or2Providers_OfferedCompletionLevel1(
      TriageLevel triageLevel)
    {
      // Arrange.
      // ...the referral to be triaged to the tested level
      _mockScoreResult.Setup(t => t.TriagedCompletionLevel)
        .Returns(triageLevel);

      // ...there to be no providers available at tested level
      Mock<IProviderService> mockProviderService = new ();
      List<Business.Models.Provider> noProviders = new ();
      mockProviderService
        .Setup(x => x.GetProvidersAsync(
          It.Is<TriageLevel>(t => t == triageLevel)))
        .ReturnsAsync(noProviders);

      // ...there to be one level 1 (Low) provider
      Business.Models.Provider level1Provider = 
        RandomModelCreator.CreateRandomProvider(
          level1: true, 
          level2: false, 
          level3: false);
      List<Business.Models.Provider> level1Providers = new ()
        { level1Provider };
      mockProviderService
        .Setup(x => x.GetProvidersAsync(
          It.Is<TriageLevel>(t => t == TriageLevel.Low)))
        .ReturnsAsync(level1Providers);

      // ...the referral service to use the movked provider service.
      ReferralService service = new (
        _context,
        _serviceFixture.Mapper,
        mockProviderService.Object,
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

      // Act.
      IReferral result = await service
        .CreateGeneralReferral(_validGeneralReferralCreate);

      // Assert.
      // ...the referral returned is the level 1 provider
      result.Should().BeOfType<Referral>();
      result.Providers.Count().Should().Be(1);
      result.Providers.Single().Should().BeEquivalentTo(level1Provider);

      // ...the tested triaged level providers are requested
      mockProviderService.Verify(
        x => x.GetProvidersAsync(It.Is<TriageLevel>(
          t => t == triageLevel)),
        Times.Once);

      // ...the level 1 providers are requested
      mockProviderService.Verify(
        x => x.GetProvidersAsync(It.Is<TriageLevel>(
          t => t == TriageLevel.Low)),
        Times.Once);

      // ... the created referral has the expected triaged and offered level
      Entities.Referral referral = 
        _context.Referrals.Single(r => r.Id == result.Id);
      referral.TriagedCompletionLevel
        .Should().Be($"{(int)triageLevel}");
      referral.OfferedCompletionLevel
        .Should().Be($"{(int)TriageLevel.Low}");
    }

    [Fact]
    public async Task CreateGeneralReferral_Valid()
    {
      // Arrange.
      DateTimeOffset methodCallTime = DateTimeOffset.Now;
      GeneralReferralCreate model = _validGeneralReferralCreate;
      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();

      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      RemoveReferralBeforeTest(model.Email);

      // Act.
      IReferral result = await _service.CreateGeneralReferral(model);

      // Assert.
      result.Should().BeOfType<Referral>();
      result.IsActive.Should().BeTrue();
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
      result.ModifiedAt.Should().BeAfter(methodCallTime);
      result.ModifiedByUserId.Should().Be(_service.User.GetUserId());
      result.DateCompletedProgramme.Should().BeNull();
      result.DateOfProviderSelection.Should().BeNull();
      result.DateStartedProgramme.Should().BeNull();
      result.ProgrammeOutcome.Should().BeNull();
      result.TriagedCompletionLevel.Should()
        .Be(TriageLevel.High.ToString("d"));
      result.TriagedWeightedLevel.Should()
        .Be(TriageLevel.Medium.ToString("d"));
      result.ReferringGpPracticeNumber.Should()
        .Be(model.ReferringGpPracticeNumber);
      result.ConsentForFutureContactForEvaluation.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGeneralReferral_Invalid_NoConsent()
    {
      // Arrange.
      string expected =
        "The ConsentForFutureContactForEvaluation field is required.";
      GeneralReferralCreate model = UniqueValidGeneralReferralCreate();
      model.ConsentForFutureContactForEvaluation = null;

      _context.SaveChanges();

      // Act.
      Func<Task> act = async () => 
        await _service.CreateGeneralReferral(model);

      // Assert.
      ExceptionAssertions<GeneralReferralValidationException>
        exception = await act.Should()
        .ThrowAsync<GeneralReferralValidationException>();

      Dictionary<string, string[]> results =
        exception.Subject.First().ValidationResults;
      results.Count().Should().Be(1);
      results.First().Value.First().Should().Be(expected);
    }

      [Fact]
      public async Task UpdateGeneralReferralUbrnAsyncError_DeactivatesReferral()
      {
        // Arrange.
        DatabaseContextOriginReferralsException testContext = new(_serviceFixture.Options);
        testContext.Referrals.RemoveRange(testContext.Referrals);

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

        Entities.Ethnicity ethnicity = RandomEntityCreator.CreateRandomEthnicity(
          minimumBmi: 0);
        testContext.Ethnicities.Add(ethnicity);

        Entities.Provider provider =
          RandomEntityCreator.CreateRandomProvider();
        testContext.Providers.Add(provider);

        await testContext.SaveChangesAsync();

        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Ethnicity = ethnicity.TriageName;
        model.ServiceUserEthnicityGroup = ethnicity.GroupName;
        model.ServiceUserEthnicity = ethnicity.DisplayName;

        try
        {
          // Act.
          await testService.CreateGeneralReferral(model);
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

      public class CheckNewGeneralReferralIsUniqueAsyncTests :
        CreateGeneralReferralTests
      {
        public CheckNewGeneralReferralIsUniqueAsyncTests(
          ServiceFixture serviceFixture,
          ITestOutputHelper testOutputHelper) 
          : base(serviceFixture, testOutputHelper)
        { }

      [Fact]
      public async Task Invalid_ReferralExists()
      {
        // Arrange.
        Random rnd = new();
        Entities.Referral referral = CreateUniqueReferral();
        referral.ReferralSource = ReferralSource.GeneralReferral.ToString();
        referral.Ubrn = Generators.GenerateUbrnSelf(rnd);
        referral.Email = Generators.GenerateNhsEmail();
        referral.Status = ReferralStatus.New.ToString();
        referral.NhsNumber = Generators.GenerateNhsNumber(rnd);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        string expectedErrMessage = 
          $"Referral cannot be created because there are in progress " +
          $"referrals with the same NHS number: (UBRN {referral.Ubrn}).";

        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.NhsNumber = referral.NhsNumber;

        await _context.SaveChangesAsync();

        // Act.
        Func<Task> act = async () => await _service.CreateGeneralReferral(model);

        // Assert.
        await act.Should().ThrowAsync<ReferralNotUniqueException>()
          .WithMessage(expectedErrMessage);
      }
    }

    public class ValidateReferralTests : CreateGeneralReferralTests
    {
      public ValidateReferralTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task GeneralReferralValidationException_EthnicityGroup()
      {
        // Arrange.
        string expected = "The ServiceUserEthnicityGroup field is invalid.";
        Random rnd = new ();
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Email = Generators.GenerateEmail();
        model.ServiceUserEthnicityGroup = "Klingon";

        await _context.SaveChangesAsync();

        RemoveReferralBeforeTest(model.Email);

        // Act.
        Func<Task> act = async () => 
          await _service.CreateGeneralReferral(model);

        // Assert.
        ExceptionAssertions<GeneralReferralValidationException>
          exception = await act.Should()
          .ThrowAsync<GeneralReferralValidationException>();

        Dictionary<string, string[]> results =
          exception.Subject.First().ValidationResults;
        results.Count().Should().Be(1);
        results.First().Value.First().Should().Be(expected);
      }

      [Fact]
      public async Task GeneralReferralValidationException_Ethnicity()
      {
        // Arrange.
        string expected = "The ServiceUserEthnicity field is invalid.";
        Random rnd = new ();
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Email = Generators.GenerateNhsEmail();
        model.ServiceUserEthnicity = "Klingon";

        await _context.SaveChangesAsync();

        RemoveReferralBeforeTest(model.Email);

        // Act.
        Func<Task> act = async () => 
          await _service.CreateGeneralReferral(model);

        // Assert.
        ExceptionAssertions< GeneralReferralValidationException> 
          exception = await act.Should()
            .ThrowAsync<GeneralReferralValidationException>();

        Dictionary<string, string[]> results =
          exception.Subject.First().ValidationResults;
        results.Count().Should().Be(1);
        results.First().Value.First().Should().Be(expected);
      }

      [Fact]
      public async Task GeneralReferralValidationException_BMI()
      {
        // Arrange.
        Random rnd = new Random();
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Email = Generators.GenerateNhsEmail();
        model.HeightCm = 181;
        model.WeightKg = 78;
        string expected = "The calculated BMI of 23.8 is too low, the " +
          $"minimum for an ethnicity of {model.Ethnicity} is 30.00.";

        await _context.SaveChangesAsync();
        RemoveReferralBeforeTest(model.Email);

        // Act.
        Func<Task> act = async () =>
          await _service.CreateGeneralReferral(model);

        // Assert.
        ExceptionAssertions<GeneralReferralValidationException>
          exception = await act.Should()
            .ThrowAsync<GeneralReferralValidationException>();

        Dictionary<string, string[]> results =
          exception.Subject.First().ValidationResults;
        results.Count().Should().Be(1);
        results.First().Value.First().Should().Be(expected);
      }

      [Fact]
      public async Task GeneralReferralValidationException_Postcode()
      {
        // Arrange.
        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Email = Generators.GenerateNhsEmail();
        model.HeightCm = 180;
        model.WeightKg = 120;
        model.Postcode = "DG14 0RU";

        Mock<IPostcodesIoService> mockPostcodesIoService = new();
        mockPostcodesIoService
          .Setup(x => x.IsEnglishPostcodeAsync(It.IsAny<string>()))
          .ReturnsAsync(false);

        ReferralService service = new (
          _context,
          _serviceFixture.Mapper,
          _providerService,
          _mockDeprivationService.Object,
          _mockLinkIdService.Object,
          mockPostcodesIoService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object,
          _mockGpDocumentProxyOptions.Object,
          _mockReferralTimelineOptions.Object,
          null,
          _log)
        {
          User = GetClaimsPrincipal()
        };

        string expected = 
          $"The Postcode field does not contain a valid English postcode.";

        RemoveReferralBeforeTest(model.Email);

        // Act.
        Func<Task> act = async () => 
          await service.CreateGeneralReferral(model);

        // Assert.
        ExceptionAssertions<GeneralReferralValidationException>
          exception = await act.Should()
            .ThrowAsync<GeneralReferralValidationException>();

        Dictionary<string, string[]> results =
          exception.Subject.First().ValidationResults;
        results.Count().Should().Be(1);
        results.First().Value.First().Should().Be(expected);
      }
    }

    public class TraiageReferralUpdateAsyncTests : CreateGeneralReferralTests
    {
      public TraiageReferralUpdateAsyncTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      {
      }

      [Fact]

      public async Task Failed_Id_Empty()
      {
        // Arrange.
        Guid id = Guid.Empty;

        // Act.
        Func<Task> act = async () => 
          await _service.TriageReferralUpdateAsync(id);

        // Assert.
        await act.Should().ThrowAsync<ArgumentException>();
      }

      [Fact]
      public async Task Valid_Referral_Triage()
      {
        // Arrange.
        Random rnd = new ();
        Entities.Referral referral = CreateUniqueReferral(
          ubrn: Generators.GenerateUbrnSelf(rnd),
          heightCm: 181,
          weightKg: 108,
          calculatedBmiAtRegistration: 31);

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // Act.
        Func<Task> act = async () =>
          await _service.TriageReferralUpdateAsync(referral.Id);

        // Assert.
        referral.TriagedCompletionLevel.Should().BeNull();
        referral.TriagedWeightedLevel.Should().BeNull();

        await act.Invoke();

        referral.TriagedCompletionLevel.Should().NotBeNullOrWhiteSpace();
        referral.TriagedWeightedLevel.Should().NotBeNullOrWhiteSpace();
      }
    }
  }

  public class ExpireTextMessageDueToDobCheckAsync : ReferralServiceTests
  {
    public ExpireTextMessageDueToDobCheckAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullOrWhiteSpaceException()
    {
      // Arrange.
      string model = "";

      // Act.
      Func<Task> act = async () => 
        await _service.ExpireTextMessageDueToDobCheckAsync(model);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullOrWhiteSpaceException>();
    }

    [Fact]
    public async Task ReferralNotFoundException()
    {
      // Arrange.
      string model = "InValid";

      // Act.
      Func<Task> act = async () =>
        await _service.ExpireTextMessageDueToDobCheckAsync(model);

      // Assert.
      await act.Should().ThrowAsync<ReferralNotFoundException>();
    }

    [Fact]
    public async Task ValidUpdate()
    {
      // Arrange.
      string serviceUserLinkId = LinkIdService.GenerateDummyId();

      Entities.Referral referral =
        CreateUniqueReferral();
      Entities.TextMessage textMsg =
        RandomEntityCreator.CreateRandomTextMessage();
      textMsg.ReferralId = referral.Id;
      textMsg.ServiceUserLinkId = serviceUserLinkId;

      _context.Referrals.Add(referral);
      _context.TextMessages.Add(textMsg);
      await _context.SaveChangesAsync();

      // Act.
      await _service.ExpireTextMessageDueToDobCheckAsync(serviceUserLinkId);

      // Assert.
      textMsg.Outcome.Should().Be(Constants.DATE_OF_BIRTH_EXPIRY);
    }
  }

  public class DeprivationUpdateException : ReferralServiceTests
  {
    public DeprivationUpdateException(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .Throws(new DeprivationNotFoundException());

      ProviderService _providerService = new(_context, _serviceFixture.Mapper, _mockOptions.Object);

      ReferralService _serviceToTest =
        new(
          _context,
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
    }

    [Fact]
    public async Task DeprivationSetToImd1WithInvalidPostcode()
    {
      // Arrange.
      string oldDeprivation = "IMD1";
      string oldPostCode = "NN2 3ER";
      string expectedPostCode = "MK18 4LQ";

      Business.Entities.Referral referral = RandomEntityCreator.
        CreateRandomReferral(
        deprivation: oldDeprivation,
        isVulnerable: false,
        postcode: oldPostCode,
        status: ReferralStatus.RejectedToEreferrals,
        hasALearningDisability: false,
        hasAPhysicalDisability: false,
        hasDiabetesType1: false,
        hasDiabetesType2: false,
        hasHypertension: true,
        hasRegisteredSeriousMentalIllness: false);
      referral.IsVulnerable = false;

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = base._serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.Postcode = expectedPostCode;

      // Act.
      IReferral result = await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.Deprivation.Should().Be(Enums.Deprivation.IMD1.ToString());
    }

    [Fact]
    public async Task ExceptionThrown()
    {
      // Arrange.
      _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .Throws(new Exception());

      ReferralService _serviceToTest =
        new(
          _context,
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

      string oldDeprivation = "IMD1";
      string oldPostCode = "NN2 3ER";
      string expectedPostCode = "MK18 4LQ";

      Business.Entities.Referral referral = RandomEntityCreator.
        CreateRandomReferral(
        status: ReferralStatus.RejectedToEreferrals);
      referral.Postcode = oldPostCode;
      referral.Deprivation = oldDeprivation;

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      ReferralUpdate referralUpdate = base._serviceFixture.Mapper
        .Map<ReferralUpdate>(referral);

      referralUpdate.Postcode = expectedPostCode;

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateGpReferral(referralUpdate);

      // Assert.
      await act.Should().ThrowAsync<Exception>();
    }
  }

  public class PrepareFailedToContactAsync : ReferralServiceTests
  {
    public PrepareFailedToContactAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [MemberData(nameof(IneligibleStatusTheoryData))]
    public async Task IneligibleStatusDoesNotUpdateReferral(Entities.Referral referral)
    {
      // Arrange.
      string expectedStatus = referral.Status;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      string expectedOutput = "Processed 0 FailedToContact referrals.";

      // Act.
      string output = await _service.PrepareFailedToContactAsync();

      // Assert.
      output.Should().Be(expectedOutput);
      Entities.Referral storedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
      storedReferral.Status.Should().Be(expectedStatus);
      storedReferral.ProgrammeOutcome.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(InitialContactTooRecentTheoryData))]
    public async Task InitialContactTooRecentDoesNotUpdateReferral(Entities.Referral referral)
    {
      // Arrange.
      string expectedStatus = referral.Status;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      string expectedOutput = "Processed 0 FailedToContact referrals.";

      // Act.
      string output = await _service.PrepareFailedToContactAsync();

      // Assert.
      output.Should().Be(expectedOutput);
      Entities.Referral storedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
      storedReferral.Status.Should().Be(expectedStatus);
      storedReferral.ProgrammeOutcome.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ReferralToBePreparedTheoryData))]
    public async Task EligibleReferralUpdatesStatusAndProgrammeOutcome(
      ReferralStatus finalReferralStatus,
      Entities.Referral referral)
    {
      // Arrange.
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      string expectedOutput = "Processed 1 FailedToContact referrals.";

      // Act.
      string output = await _service.PrepareFailedToContactAsync();

      // Assert.
      output.Should().Be(expectedOutput);
      referral.Status.Should().Be(finalReferralStatus.ToString());
      referral.ProgrammeOutcome.Should().Be(ProgrammeOutcome.FailedToContact.ToString());
      _context.ReferralsAudit
        .Where(r => r.Id == referral.Id)
        .Where(r => r.Status == ReferralStatus.FailedToContact.ToString())
        .Any()
        .Should().BeTrue();
    }

    public static TheoryData<Entities.Referral> IneligibleStatusTheoryData()
    {
      ReferralStatus[] referralStatusesToBePrepared =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.FailedToContact,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage3
      ];

      IEnumerable<ReferralStatus> allReferralStatuses =
        (IEnumerable<ReferralStatus>)Enum.GetValues(typeof(ReferralStatus));

      TheoryData<Entities.Referral> theoryData = [];

      foreach (ReferralStatus status in allReferralStatuses.Except(referralStatusesToBePrepared))
      {
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(status: status);

        referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
          referralStatus: ReferralStatus.TextMessage1.ToString(),
          sent: DateTimeOffset.UtcNow.AddDays(-Constants.MAX_DAYS_UNTIL_FAILEDTOCONTACT-1))];

        theoryData.Add(referral);
      }

      return theoryData;
    }

    public static TheoryData<Entities.Referral> InitialContactTooRecentTheoryData()
    {
      ReferralStatus[] initialReferralStatuses =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage3
      ];

    TheoryData<Entities.Referral> theoryData = [];

      foreach (ReferralStatus status in initialReferralStatuses)
      {
        for (int i = 0; i < 2; i++)
        {
          Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(status: status);

          if (i == 1)
          {
            referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
              referralStatus: ReferralStatus.TextMessage1.ToString(),
              sent: DateTimeOffset.UtcNow.AddDays(-Constants.MAX_DAYS_UNTIL_FAILEDTOCONTACT+1))];
          }

          referral.Calls = [RandomEntityCreator.CreateRandomChatBotCall(
            sent:DateTimeOffset.UtcNow.AddDays(-Constants.MAX_DAYS_UNTIL_FAILEDTOCONTACT+1))];

          theoryData.Add(referral);
        }
      }

      return theoryData;
    }

    public static TheoryData<ReferralStatus, Entities.Referral> ReferralToBePreparedTheoryData()
    {
      ReferralStatus[] referralStatusesToBePrepared =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.FailedToContact,
        ReferralStatus.RmcCall,
        ReferralStatus.RmcDelayed,
        ReferralStatus.TextMessage3
      ];

      IEnumerable<ReferralSource> allReferralSources =
        (IEnumerable<ReferralSource>)Enum.GetValues(typeof(ReferralSource));

      TheoryData<ReferralStatus, Entities.Referral> theoryData = [];

      foreach (ReferralStatus status in referralStatusesToBePrepared)
      {
        foreach (ReferralSource referralSource in allReferralSources)
        {
          for (int i = 0; i < 2; i++)
          {
            Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
              referralSource: referralSource,
              status: status);

            if (i == 1)
            {
              referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
                referralStatus: ReferralStatus.TextMessage1.ToString(),
                sent: DateTimeOffset.UtcNow.AddDays(-Constants.MAX_DAYS_UNTIL_FAILEDTOCONTACT-1))];
            }

            referral.Calls = [RandomEntityCreator.CreateRandomChatBotCall(
              sent:DateTimeOffset.UtcNow.AddDays(-Constants.MAX_DAYS_UNTIL_FAILEDTOCONTACT-1))];

            if (referralSource == ReferralSource.GpReferral)
            {
              theoryData.Add(ReferralStatus.AwaitingDischarge, referral);
            }
            else
            {
              theoryData.Add(ReferralStatus.Complete, referral);
            }
          }
        }
      }

      return theoryData;
    }
  }

  public class SendReferralLettersAsyncTests : ReferralServiceTests
  {
    public SendReferralLettersAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Entities.Referral createdReferral1 =
        CreateUniqueReferral();
      _context.Referrals.Add(createdReferral1);
      Entities.Referral createdReferral2 =
        CreateUniqueReferral();

      _context.Referrals.Add(createdReferral2);
      _context.SaveChanges();

      string expected = "test";
      byte[] expectedBytes = Convert.FromBase64String(expected);
      DateTimeOffset exported = DateTimeOffset.Now;
      List<Guid> request = new() { createdReferral1.Id, createdReferral2.Id};
      _mockCsvExport.Setup(t =>
          t.Export<CsvExportAttribute>(It.IsAny<IEnumerable<Referral>>()))
        .Returns(expectedBytes);

      ProviderService _providerService =
        new (
          _context,
          _serviceFixture.Mapper,
          _mockOptions.Object)
        {
          User = GetClaimsPrincipal()
        };

      ReferralService service = new(
        _context,
        null,
        _serviceFixture.Mapper,
        _providerService,
        _mockCsvExport.Object,
        _mockPatientTriageService.Object)
      {
        User = GetClaimsPrincipal()
      };

      // Act.
      byte[] response =
        await service.SendReferralLettersAsync(request, exported);

      // Assert.
      response.Should().BeOfType<byte[]>();
      createdReferral1.MethodOfContact.Should()
        .Be((int)MethodOfContact.Letter);
      createdReferral1.NumberOfContacts.Should().Be(1);
      createdReferral2.MethodOfContact.Should()
        .Be((int)MethodOfContact.Letter);
      createdReferral2.NumberOfContacts.Should().Be(1);
    }
  }

  public class CreateDischargeLettersAsyncTests : ReferralServiceTests
  {
    public CreateDischargeLettersAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
    }

    //TODO: Unit test to cover this method
  }

  public class UpdateEmail : ReferralServiceTests
  {
    public UpdateEmail(
      ServiceFixture serviceFixture, 
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Theory]
    [InlineData("real_person@gmail.com")]
    [InlineData("jefffiddler@nhs.net")]
    [InlineData("anaardvark@nhs.net")]
    [InlineData("eleEester@nhs.net")]
    public async Task Valid(string expectedEmail)
    {
      // Arrange.
      Entities.Referral referral = CreateUniqueReferral();

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      IReferral result = await _service.UpdateEmail(referral.Id, expectedEmail);

      // Assert.
      result.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("test.com")]
    [InlineData("test@test@.com")]
    public async Task Invalid(string email)
    {
      // Act.
      Func<Task> act = async () =>
        await _service.UpdateEmail(Guid.NewGuid(), email);

      // Assert.
      await act.Should().ThrowAsync<ArgumentException>();
    }
  }

  public class UpdateGpReferral : ReferralServiceTests
  {
    public UpdateGpReferral(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData(true, true, true, "New", null)]
    [InlineData(true, true, false, "New", null)]
    [InlineData(true, false, true, "New", null)]
    [InlineData(false, true, true, "New", null)]
    [InlineData(true, false, false, "New", null)]
    [InlineData(false, true, false, "New", null)]
    [InlineData(false, false, true, "New", null)]
    [InlineData(false, false, false, "Exception", "A diagnosis of Diabetes " +
      "Type 1 or Diabetes Type 2 or Hypertension is required.")]
    public async Task Hypertension_And_Diabetes(
      bool hasDiabetesType1,
      bool hasDiabetesType2,
      bool hasHypertension,
      string expectedStatus,
      string expectedStatusReason)
    {
      // Arrange.
      string ubrn = $"{DateTimeOffset.Now:MMddHHmmssff}";
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        ubrn: ubrn,
        status: ReferralStatus.RejectedToEreferrals));
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.HasDiabetesType1 = hasDiabetesType1;
      model.HasDiabetesType2 = hasDiabetesType2;
      model.HasHypertension = hasHypertension;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(expectedStatus);
      result.StatusReason.Should().Be(expectedStatusReason);
    }

    [Fact]
    public async Task DuplicateUbrnsNotAllowedException()
    {
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        ubrn: Generators.GenerateUbrn(new Random()),
        nhsNumber: nhsNumber,
        status: ReferralStatus.CancelledByEreferrals));
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        ubrn: ubrn,
        nhsNumber: nhsNumber,
        status: ReferralStatus.CancelledByEreferrals));
      _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        ubrn: ubrn,
        nhsNumber: nhsNumber,
        status: ReferralStatus.RejectedToEreferrals));
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateGpReferral(model);

      // Assert
      await act.Should().ThrowAsync<ReferralNotUniqueException>();
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[] {
      ReferralStatus.CancelledByEreferrals,
      ReferralStatus.Complete,
      ReferralStatus.Cancelled
    })]
    public async Task DuplicateNhsNumber_InProgress_Exception(
      ReferralStatus status)
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral =
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
        status: ReferralStatus.CancelledByEreferrals);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete);
      Entities.Referral inProgressReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: status);
      Entities.Referral cancelledDuplicate =
       RandomEntityCreator.CreateRandomReferral(
         nhsNumber: nhsNumber,
         status: ReferralStatus.CancelledDuplicate);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.Referrals.Add(cancelledDuplicate);
      _context.SaveChanges();

      string expected = "Referral cannot be created " +
        "because there are in progress referrals with the same NHS " +
        $"number: (UBRN {cancelledDuplicate.Ubrn}).";

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expected);
    }

    [Fact]
    public async Task DuplicateNhsNumber_NoProviderId_New()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }

    [Fact]
    public async Task 
      DuplicateNhsNumber_DateOfProviderSelectionNull_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());
      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals,
          providerId: provider.Id);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.SaveChanges();

      string expected = "The previous referral " +
        $"(UBRN {cancelledReferral.Ubrn}) has a selected " +
        "provider without a matching date of provider selection.";

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expected);
    }

    [Fact]
    public async Task DuplicateNhsNumber_DateOfProviderSelection_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random()); 

      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(
            -Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION),
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals,
          providerId: provider.Id);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete,
          providerId: provider.Id);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.SaveChanges();

        string latestReferralDateOfProviderSelection = cancelledReferral
          .DateOfProviderSelection
          .Value
          .AddDays(Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 1)
          .Date
          .ToString("yyyy-MM-dd");

        string expected = "Referral can be created from " +
        latestReferralDateOfProviderSelection +
        " as an existing referral for this NHS number " +
        $"(UBRN {cancelledReferral.Ubrn}) selected a provider but did not start " +
        "the programme.";

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expected);
    }

    [Fact]
    public async Task DuplicateNhsNumber_DateOfProviderSelection_New()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: DateTimeOffset.Now.AddDays(
            -Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION - 1),
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals,
          providerId: provider.Id);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete,
          providerId: provider.Id);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }

    [Fact]
    public async Task DuplicateNhsNumber_DateStartedProgramme_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          dateStartedProgramme: DateTimeOffset.Now.AddDays(
            -Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME),
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals,
          providerId: provider.Id);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete,
          providerId: provider.Id);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

        string latestReferralDateStartedProgramme = cancelledReferral
          .DateStartedProgramme
          .Value
          .AddDays(Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME + 1)
          .Date
          .ToString("yyyy-MM-dd");

        string expected = "Referral can be created from " +
          latestReferralDateStartedProgramme +
          $" as an existing referral for this NHS number (UBRN " +
          $"{cancelledReferral.Ubrn}) started the programme.";

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.Exception.ToString());
      result.StatusReason.Should().Be(expected);
    }

    [Fact]
    public async Task DuplicateNhsNumber_DateStartedProgramme_New()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(new Random());
      string nhsNumber = Generators.GenerateNhsNumber(new Random());

      Entities.Provider provider = 
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      Entities.Referral referral = 
        RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.RejectedToEreferrals);
      Entities.Referral cancelledReferral = 
        RandomEntityCreator.CreateRandomReferral(
          dateStartedProgramme: DateTimeOffset.Now.AddDays(
            -Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME - 1),
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals,
          providerId: provider.Id);
      Entities.Referral completeReferral = 
        RandomEntityCreator.CreateRandomReferral(
          nhsNumber: nhsNumber,
          status: ReferralStatus.Complete,
          providerId: provider.Id);

      _context.Referrals.Add(referral);
      _context.Referrals.Add(cancelledReferral);
      _context.Referrals.Add(completeReferral);
      _context.SaveChanges();

      ReferralUpdate model = ValidReferralUpdate();
      model.Ubrn = ubrn;
      model.NhsNumber = nhsNumber;

      // Act.
      IReferral result = await _service.UpdateGpReferral(model);

      // Assert.
      result.Status.Should().Be(ReferralStatus.New.ToString());
      result.StatusReason.Should().BeNull();
    }
  }

  public class UpdateSelfReferralWithProviderAsyncTests
   : ReferralServiceTests
  {
    public UpdateSelfReferralWithProviderAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task InValid_ReferralNotFound()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid providerId = Guid.NewGuid();

      // Act.    
      try
      {
        IReferral result = await _service
          .UpdateReferralWithProviderAsync(referralId, providerId);

        // Assert.
        Assert.Fail("Expected ReferralNotFoundException");
      }
      catch (ReferralNotFoundException ex)
      {
        Assert.True(true, ex.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail($"Expected ReferralNotFoundException, but got {ex.Message}");
      }
    }

    [Fact]
    public async Task InValid_ReferralStatusNotNew()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      Entities.Referral referral = CreateUniqueReferral();
      referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      try
      {
        IReferral result =
          await _service.UpdateReferralWithProviderAsync(referral.Id,
            providerId);
        // Assert.
        Assert.Fail("Expected ReferralInvalidStatusException");
      }
      catch (Exception ex)
      {
        if (ex is ReferralInvalidStatusException)
        {
          Assert.True(true, ex.Message);
        }
        else
        {
          Assert.Fail($"Expected ReferralInvalidStatusException, but got {ex.Message}");
        }
      }
    }

    [Fact]
    public async Task InValid_ReferralIsNewButHasProviderId()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      Entities.Referral referral =
        CreateUniqueReferral(
          status: ReferralStatus.New);
      _context.Referrals.Add(referral);
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      referral.ProviderId = providerId;
      _context.SaveChanges();

      // Act.
      try
      {
        IReferral result =
          await _service.UpdateReferralWithProviderAsync(referral.Id,
            providerId);
        // Assert.
        Assert.Fail("Expected ReferralProviderSelectedException");
      }
      catch (ReferralProviderSelectedException ex)
      {
        Assert.True(true, ex.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail($"Expected ReferralProviderSelectedException, but got {ex.Message}");
      }
    }

    [Fact]
    public async Task Invalid_ProviderIdIsNotInProvidersForReferral()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      Entities.Provider provider =
        RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();
      Entities.Referral referral =
        CreateUniqueReferral(
          status: ReferralStatus.New,
          providerId: providerId);
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      try
      {
        IReferral result =
          await _service.UpdateReferralWithProviderAsync(referral.Id,
            providerId);
        // Assert.
        Assert.Fail("Expected ProviderSelectionMismatch");
      }
      catch (ProviderSelectionMismatch ex)
      {
        Assert.True(true, ex.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail($"Expected ProviderSelectionMismatch, but got {ex.Message}");
      }
    }

    [Fact]
    public async Task Invalid_TriagelCompletionLevelNotSet()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      Entities.Referral referral =
        CreateUniqueReferral(
          status: ReferralStatus.New,
          providerId: providerId);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      Func<Task> act = async () =>
        await _service.UpdateReferralWithProviderAsync(
          referral.Id,
          providerId);

      // Assert.
      await act.Should().ThrowAsync<TriageNotFoundException>();
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      DateTimeOffset executionTime = DateTimeOffset.Now;
      IReferral result = await _service
        .UpdateReferralWithProviderAsync(referral.Id, provider.Id);

      // Assert.
      result.Status.Should().Be(
        ReferralStatus.ProviderAwaitingStart.ToString());
      result.ProviderId.Should().Be(provider.Id);
      result.DateOfProviderSelection.Should().NotBeNull();
      result.DateOfProviderSelection.Should().BeAfter(executionTime);
    }

    [Fact]
    public async Task Valid_NoNhsNumber()
    {
      // Arrange.
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      referral.NhsNumber = null;

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      DateTimeOffset executionTime = DateTimeOffset.Now;
      IReferral result = await _service
        .UpdateReferralWithProviderAsync(referral.Id, provider.Id);

      // Assert.
      result.Status.Should().Be(
        ReferralStatus.ProviderAwaitingTrace.ToString());
      result.ProviderId.Should().Be(provider.Id);
      result.DateOfProviderSelection.Should().NotBeNull();
      result.DateOfProviderSelection.Should().BeAfter(executionTime);
    }
  }

  public class RejectionListSearch : ReferralServiceTests
  {
    public RejectionListSearch(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task FiltersAndReturnsOnlyGpReferrals()
    {
      // Arrange.
      int expectedCount = 3;
      AllStatusListTestData();
      ReferralSearch filter = new();
      filter.Statuses = RejectionListStatusList;
      filter.ReferralSource = ReferralSource.GpReferral.ToString();

      // Act.
      IReferralSearchResponse response = await _service.Search(filter);

      // Assert.
      int nullCount = response.Referrals
        .Count(t => t.ReferralSource == null);
      nullCount.Should().Be(0);
      response.Count.Should().Be(expectedCount);
      foreach (ReferralSource source in
        Enum.GetValues(typeof(ReferralSource))
          .Cast<ReferralSource>())
      {
        int count = response.Referrals
          .Count(t => t.ReferralSource == source.ToString());

        if (source == ReferralSource.GpReferral)
        {
          count.Should().Be(expectedCount);
        }
        else
        {
          count.Should().Be(0, $"{source} found and should be 0");
        }
      }
    }

    private string[] RejectionListStatusList =>
      RejectionListAttributeExtension
        .RejectionStatusItems<ReferralStatus>()
        .Select(t => (ReferralStatus)Enum.Parse(typeof(ReferralStatus),
          t.Name)).Select(t=>t.ToString()).ToArray();

    /// <summary>
    /// Provide an inMemory source of referrals
    /// using the ReferralSource enum and the RejectionStatuses
    /// </summary>
    private void AllStatusListTestData()
    {

      foreach (ReferralStatus status in 
               Enum.GetValues(typeof(ReferralStatus))
                 .Cast<ReferralStatus>())
      {
        foreach (ReferralSource source in 
                 Enum.GetValues(typeof(ReferralSource))
                   .Cast<ReferralSource>())
        {
          Entities.Referral referral = CreateUniqueReferral(
            status: status,
            source: source);

          _context.Referrals.Add(referral);
          _context.SaveChanges();
        }
      }
    }
  }

  private void RemoveReferralBeforeTest(string email)
  {
    Entities.Referral referral =
      _context.Referrals.FirstOrDefault(t => t.Email == email);

    if (referral == null)
    {
      return;
    }

    _context.Referrals.Remove(referral);
    _context.SaveChanges();
  }

  public Entities.Referral CreateUniqueReferral(string address1 = null,
      string address2 = null,
      string address3 = null,
      decimal calculatedBmiAtRegistration = -1m,
      bool? consentForFutureContactForEvaluation = true,
      DateTimeOffset? dateCompletedProgramme = null,
      DateTimeOffset dateOfBirth = default,
      DateTimeOffset dateOfBmiAtRegistration = default,
      DateTimeOffset? dateOfProviderSelection = null,
      DateTimeOffset dateOfReferral = default,
      DateTimeOffset? dateStartedProgramme = null,
      DateTimeOffset? dateToDelayUntil = null,
      DateTimeOffset? dateOfProviderContactedServiceUser = null,
      string deprivation = null,
      string email = null,
      string ethnicity = null,
      string familyName = null,
      string givenName = null,
      bool? hasALearningDisability = null,
      bool? hasAPhysicalDisability = null,
      bool? hasDiabetesType1 = null,
      bool? hasDiabetesType2 = null,
      bool? hasHypertension = null,
      bool? hasRegisteredSeriousMentalIllness = null,
      decimal heightCm = -1m,
      Guid id = default,
      bool isActive = true,
      bool? isMobileValid = null,
      bool? isTelephoneValid = null,
      bool? isVulnerable = null,
      string mobile = null,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string nhsNumber = null,
      string postcode = null,
      string programmeOutcome = null,
      Guid providerId = default,
      long? referralAttachmentId = 123456,
      string referringGpPracticeName = "Test Practice",
      string referringGpPracticeNumber = null,
      ReferralSource source = ReferralSource.GpReferral,
      string sex = null,
      ReferralStatus status = ReferralStatus.New,
      string statusReason = null,
      string telephone = null,
      string triagedCompletionLevel = null,
      string triagedWeightedLevel = null,
      string ubrn = null,
      string vulnerableDescription = "Not Vulnerable",
      decimal weightKg = 120m,
      string referralSource = "")
  {
    while (true)
    {
      Entities.Referral entity = RandomEntityCreator
        .CreateRandomReferral(
        address1: null,
        address2: null,
        address3: null,
        calculatedBmiAtRegistration: -1m,
        consentForFutureContactForEvaluation: true,
        dateCompletedProgramme: null,
        dateOfBirth: default,
        dateOfBmiAtRegistration: default,
        dateOfProviderSelection: null,
        dateOfReferral: default,
        dateStartedProgramme: null,
        dateToDelayUntil: null,
        dateOfProviderContactedServiceUser: null,
        deprivation: null,
        email: null,
        ethnicity: null,
        familyName: null,
        givenName: null,
        hasALearningDisability: null,
        hasAPhysicalDisability: null,
        hasDiabetesType1: null,
        hasDiabetesType2: null,
        hasHypertension: null,
        hasRegisteredSeriousMentalIllness: null,
        heightCm: -1m,
        id: default,
        isActive: true,
        isMobileValid: null,
        isTelephoneValid: null,
        isVulnerable: null,
        mobile: null,
        modifiedAt: default,
        modifiedByUserId: default,
        nhsNumber: null,
        postcode: null,
        programmeOutcome: null,
        providerId: default,
        referralAttachmentId: Guid.NewGuid().ToString(),
        referringGpPracticeName: "Test Practice",
        referringGpPracticeNumber: null,
        referralSource: source,
        sex: null,
        status: status,
        statusReason: null,
        telephone: null,
        triagedCompletionLevel: null,
        triagedWeightedLevel: null,
        ubrn: null,
        vulnerableDescription: "Not Vulnerable",
        weightKg: 120m);

      Entities.Referral found = _context.Referrals
        .FirstOrDefault(t => t.Id == entity.Id);
      if (found != null)
      {
        continue;
      }

      found = _context.Referrals
        .FirstOrDefault(t => t.Ubrn == entity.Ubrn);
      if (found != null)
      {
        continue;
      }

      found = _context.Referrals
        .FirstOrDefault(t => t.NhsNumber == entity.NhsNumber);
      if (found != null)
      {
        continue;
      }

        return entity;
      }
    }
  }

public class DatabaseContextOriginReferralsException : DatabaseContext
{
  public DatabaseContextOriginReferralsException(
    DbContextOptions<DatabaseContext> options) : base(options)
  { }

  public override DbSet<Entities.GeneralReferral> GeneralReferrals
  {
    get => throw new Exception();
  }
  public override DbSet<Entities.GpReferral> GpReferrals
  {
    get => throw new Exception();
  }
  public override DbSet<Entities.MskReferral> MskReferrals
  {
    get => throw new Exception();
  }
  public override DbSet<Entities.PharmacyReferral> PharmacyReferrals
  {
    get => throw new Exception();
  }
  public override DbSet<Entities.SelfReferral> SelfReferrals
  {
    get => throw new Exception();
  }
}
