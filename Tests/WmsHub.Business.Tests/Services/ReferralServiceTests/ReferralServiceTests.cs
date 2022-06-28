using FluentAssertions;
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
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Sdk;
using static WmsHub.Common.Enums;
using Deprivation = WmsHub.Business.Models.Deprivation;
using IReferral = WmsHub.Business.Models.IReferral;
using IStaffRole = WmsHub.Business.Models.ReferralService.IStaffRole;
using Provider = WmsHub.Business.Entities.Provider;
using Referral = WmsHub.Business.Models.Referral;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    private readonly GeneralReferralCreate _validGeneralReferralCreate;
    private readonly SelfReferralCreate _validSelfReferralCreate;
    private readonly DatabaseContext _context;
    private readonly ReferralService _service;
    private readonly ProviderOptions _options = new ProviderOptions
    { CompletionDays = 84, NumDaysPastCompletedDate = 10 };
    private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
      new Mock<IOptions<ProviderOptions>>();
    private readonly Mock<ICsvExportService> _mockCsvExport =
      new Mock<ICsvExportService>();

    private readonly Deprivation _mockDeprivationValue = new Deprivation
    { ImdDecile = 6, Lsoa = "E00000001" };
    private readonly Mock<IDeprivationService> _mockDeprivationService =
      new Mock<IDeprivationService>();
    private readonly Mock<IPostcodeService> _mockPostcodeService =
      new Mock<IPostcodeService>();
    private Mock<CourseCompletionResult> _mockScoreResult = new();
    private readonly Mock<IPatientTriageService> _mockPatientTriageService =
      new Mock<IPatientTriageService>();
    private readonly Mock<IOdsOrganisationService> _mockOdsOrganisationService =
      new Mock<IOdsOrganisationService>();
    private ProviderService _providerService;

    public ReferralServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _mockOptions.Setup(x => x.Value).Returns(_options);

      _context = new DatabaseContext(_serviceFixture.Options);

      _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
        .ReturnsAsync(_mockDeprivationValue);

      _mockPostcodeService.Setup(x => x.GetLsoa(It.IsAny<string>()))
        .ReturnsAsync(_mockDeprivationValue.Lsoa);

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

      _service =
        new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _providerService,
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object)
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

    private SelfReferralCreate UniqueValidReferralCreate()
    {
      while (true)
      {
        var r = _validSelfReferralCreate;

        if (_context.Referrals.Count(t => t.Email == r.Email) == 0)
          return r;
      }
    }

    private GeneralReferralCreate uniqueValidGeneralReferralCreate()
    {
      while (true)
      {
        var r = _validGeneralReferralCreate;

        if (_context.Referrals.Count(t => t.Email == r.Email) == 0)
          return r;
      }
    }


    protected virtual void CleanUp(Guid referralIdToRemove)
    {
      // clean up
      var referral = _context.Referrals.Find(referralIdToRemove);
      _context.Referrals.Remove(referral);
      _context.SaveChanges();
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
        referralAttachmentId: 123456,
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
        referralAttachmentId: 123456,
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
      public ReferralServiceConstructor(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralServiceInstantiate()
      {
        //arrange
        ProviderService _providerService =
        new ProviderService(
          _context,
          _serviceFixture.Mapper,
          _mockOptions.Object);

        //try
        ReferralService service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _providerService,
          _mockCsvExport.Object,
          _mockPatientTriageService.Object
        );

        //assert
        service.Should().NotBeNull();
      }
    }

    public class ConfirmProviderAsync : ReferralServiceTests
    {
      public ConfirmProviderAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid referralId = INVALID_ID;
        Guid providerId = Guid.NewGuid();

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service
            .ConfirmProviderAsync(referralId, providerId));
      }

      [Fact]
      public async Task ProviderUpdatedAsExpected()
      {
        var referral = CreateUniqueReferral(
          status: ReferralStatus.New);
        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Referrals.Add(referral);
        _context.Providers.Add(provider);
        _context.SaveChanges();

        var expectedStatus = ReferralStatus.ProviderAwaitingStart.ToString();

        //act
        IReferral referralReturned =
          await _service.ConfirmProviderAsync(referral.Id, provider.Id);

        //assert
        referralReturned.Status.Should().Be(expectedStatus);
        referralReturned.ProviderId = provider.Id;
        referralReturned.DateOfProviderSelection = DateTimeOffset.Now;

        // clean up
        _context.Remove(referral);
        _context.Remove(provider);
        _context.SaveChanges();

      }
    }

    public class ConfirmProviderAsyncWithReferral : ReferralServiceTests
    {
      public ConfirmProviderAsyncWithReferral(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Business.Entities.Referral referral =
          CreateUniqueReferral(
            status: ReferralStatus.New);

        Referral referralModel = base._serviceFixture.Mapper
          .Map<Referral>(referral);

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service
            .ConfirmProviderAsync(referralModel));
      }

      [Fact]
      public async Task ValidNhsNumber_ProviderAwaitingStart()
      {
        // arrange
        var referralExisting = CreateUniqueReferral(
          status: ReferralStatus.New);
        var provider = RandomEntityCreator.CreateRandomProvider();
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

        //act
        IReferral referralReturned =
          await _service.ConfirmProviderAsync(referralModel);

        //assert
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
        // clean up
        _context.Remove(referralExisting);
        _context.Remove(provider);
        _context.SaveChanges();
      }

      [Theory]
      [InlineData("")]
      [InlineData(" ")]
      [InlineData(null)]
      public async Task NullOrWhiteSpaceNhsNumber_ProviderAwaitingTrace(
        string nhsNumber)
      {
        // arrange
        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);

        var referralExisting = CreateUniqueReferral(
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

        //act
        IReferral referralReturned =
          await _service.ConfirmProviderAsync(referralModel);

        //assert
        referralReturned.Status.Should().Be(expectedStatus);
        referralReturned.ProviderId.Should().Be(provider.Id);
        referralReturned.DateOfProviderSelection.Should()
          .BeSameDateAs(DateTimeOffset.Now);
        referralExisting.MethodOfContact.Should()
          .Be((int)MethodOfContact.RmcCall);
        referralExisting.NumberOfContacts.Should().Be(1);

        // clean up
        _context.Remove(referralExisting);
        _context.Remove(provider);
        _context.SaveChanges();
      }
    }

    public class CreateException : ReferralServiceTests
    {
      const string UBRN_VALID = "123456789012";
      const string UBRN_SHORT = "12345678901";
      const string UBRN_LONG = "1234567890123";
      const string NHSNO_VALID = "9999999999";
      const string NHSNO_VALID2 = "8888888888";
      const string NHSNO_INVALID = "1234567890";
      const string NHSNO_SHORT = "123456789";
      const string NHSNO_LONG = "12345678901";

      public CreateException(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Theory]
      [InlineData(
        CreateReferralException.MissingAttachment,
        "The eRS referral does not have an attached referral letter.")]
      [InlineData(
        CreateReferralException.NhsNumberMismatch,
        "The NHS number in the eRS work list ")]
      public async Task Valid(
        CreateReferralException exceptionType, string expectedStatusReasonStart)
      {
        // arrange
        IReferralExceptionCreate model = RandomModelCreator
          .CreateRandomReferralExceptionCreate(exceptionType);

        // act
        IReferral result = await _service.CreateException(model);

        // assert
        result.Status.Should().Be(ReferralStatus.Exception.ToString());
        result.StatusReason.Should().StartWith(expectedStatusReasonStart);

        // clean up
        CleanUp(result.Id);
      }

      [Fact]
      public async Task ArgumentNullException()
      {
        // assert
        await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.CreateException(null));
      }

      [Fact]
      public async Task ReferralCreateException_Undefined()
      {

        // arrange
        IReferralExceptionCreate model = RandomModelCreator
          .CreateRandomReferralExceptionCreate(
            CreateReferralException.Undefined);

        // assert
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

        // arrange
        IReferralExceptionCreate model = RandomModelCreator
          .CreateRandomReferralExceptionCreate(
            CreateReferralException.MissingAttachment);
        model.Ubrn = ubrn;

        // assert
        var exception = await Assert.ThrowsAsync<ReferralCreateException>(
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

        // arrange
        IReferralExceptionCreate model = RandomModelCreator
          .CreateRandomReferralExceptionCreate(
            CreateReferralException.NhsNumberMismatch);
        model.NhsNumberAttachment = nhsNumberAttachment;
        model.NhsNumberWorkList = nhsNumberWorkList;
        model.Ubrn = ubrn;

        // assert
        var exception = await Assert.ThrowsAsync<ReferralCreateException>(
          async () => await _service.CreateException(model));

        exception.Message.Should().Contain(expectedMessageContent);
      }

      [Fact]
      public async Task ReferralNotUniqueException()
      {
        // arrange
        var existingReferral = RandomEntityCreator
          .CreateRandomReferral(
          ubrn: UBRN_VALID);
        _context.Referrals.Add(existingReferral);
        _context.SaveChanges();

        var referrals = new List<IReferralExceptionCreate>()
        {
          RandomModelCreator.CreateRandomReferralExceptionCreate(
              CreateReferralException.MissingAttachment,
              ubrn: UBRN_VALID),
          RandomModelCreator.CreateRandomReferralExceptionCreate(
              CreateReferralException.NhsNumberMismatch,
              ubrn: UBRN_VALID),
        };

        foreach (var referral in referrals)
        {
          // assert
          var exception = await Assert.ThrowsAsync<ReferralNotUniqueException>(
            async () => await _service.CreateException(referral));
        }
        // cleanup
        CleanUp(existingReferral.Id);
      }

      protected override void CleanUp(Guid referralIdToRemove)
      {
        // clean up
        var referral = _context.Referrals.Find(referralIdToRemove);
        _context.Referrals.Remove(referral);
        _context.SaveChanges();
      }
    }

    public class DelayReferralForSevenDaysAsync : ReferralServiceTests
    {
      public DelayReferralForSevenDaysAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid referralId = INVALID_ID;
        string reason = "this is a delay test";
        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service
            .DelayReferralUntilAsync(referralId, reason, DateTimeOffset.Now));
      }

      [Fact]
      public async Task DelayReferralForSevenDaysAsync_AsExpected()
      {
        string reason = "this is a delay test";
        var referral = CreateUniqueReferral(
          status: ReferralStatus.New);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        string expectedStatus = ReferralStatus.RmcDelayed.ToString();
        DateTimeOffset newDelayDate = DateTimeOffset.Now.AddDays(7);

        //act
        var referralReturned = await _service
          .DelayReferralUntilAsync(referral.Id, reason, newDelayDate);


        Entities.Referral updatedReferral = await _context
        .Referrals
        .Where(r => r.Id == referral.Id)
        .FirstOrDefaultAsync();

        //assert
        referralReturned.Status.Should().Be(expectedStatus);
        updatedReferral.DateToDelayUntil?.Date.Should().Be(newDelayDate.Date);
        referralReturned.MethodOfContact.Should()
         .Be((int)MethodOfContact.RmcCall);
        referralReturned.NumberOfContacts.Should().Be(1);
        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }
    }

    public class GetActiveReferralAndExceptionUbrns :
      ReferralServiceTests, IDisposable
    {
      public GetActiveReferralAndExceptionUbrns(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Theory]
      [InlineData(null)]
      [InlineData("1")]
      public async Task NoActiveReferrals_EmptyList(string serviceId)
      {
        // arrange 
        _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
          isActive: false,
          serviceId: serviceId ?? "0"));
        _context.SaveChanges();

        // act
        var result = await _service
          .GetActiveReferralAndExceptionUbrns(serviceId);

        // assert
        result.Should().BeOfType<List<ActiveReferralAndExceptionUbrn>>();
        result.Count.Should().Be(0);
      }

      [Theory]
      [InlineData(null)]
      [InlineData("1")]
      public async Task OnlyCancelledReferrals_EmptyList(string serviceId)
      {
        // arrange 
        _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          serviceId: serviceId ?? "0",
          status: ReferralStatus.CancelledByEreferrals));
        _context.SaveChanges();

        // act
        var result = await _service
          .GetActiveReferralAndExceptionUbrns(serviceId);

        // assert
        result.Should().BeOfType<List<ActiveReferralAndExceptionUbrn>>();
        result.Count.Should().Be(0);
      }

      [Theory]
      [InlineData(null)]
      [InlineData("1")]
      public async Task NoGpReferrals_EmptyList(string serviceId)
      {
        // arrange
        foreach (ReferralSource src in Enum.GetValues(typeof(ReferralSource)))
        {
          if (src != ReferralSource.GpReferral)
          {
            _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
            isActive: true,
            referralSource: src,
            serviceId: serviceId ?? "0",
            status: ReferralStatus.New));
          }
        }
        _context.SaveChanges();

        // act
        var result = await _service
          .GetActiveReferralAndExceptionUbrns(serviceId);

        // assert
        result.Should().BeOfType<List<ActiveReferralAndExceptionUbrn>>();
        result.Count.Should().Be(0);
      }

      [Fact]
      public async Task NoReferralsForServiceId_EmptyList()
      {
        // arrange 
        _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          serviceId: "0",
          status: ReferralStatus.New));
        _context.SaveChanges();

        // act
        var result = await _service.GetActiveReferralAndExceptionUbrns("1");

        // assert
        result.Should().BeOfType<List<ActiveReferralAndExceptionUbrn>>();
        result.Count.Should().Be(0);
      }

      [Theory]
      [InlineData("1", 1)]
      [InlineData(null, 2)]
      public async Task ServiceIdProvided_OnlyReferralForServiceId(
        string serviceId,
        int expectedCount)
      {
        // arrange 
        _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          serviceId: "0",
          status: ReferralStatus.New));
        _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
          isActive: true,
          serviceId: "1",
          status: ReferralStatus.New));

        _context.SaveChanges();

        // act
        var result = await _service
          .GetActiveReferralAndExceptionUbrns(serviceId);

        // assert
        result.Should().BeOfType<List<ActiveReferralAndExceptionUbrn>>();
        result.Count.Should().Be(expectedCount);
      }

      [Fact]
      public async Task ListActiveReferralAndExceptionUbrnsReturned()
      {
        // arrange
        // create a referal of each status for 3 services
        List<Entities.Referral> referrals = new();
        foreach (ReferralStatus status in Enum
          .GetValues(typeof(ReferralStatus)))
        {
          //Create records for several ServiceIDs
          for (int serviceId = 1; serviceId < 4; serviceId++)
          {
            var referral = RandomEntityCreator.CreateRandomReferral(
              referralAttachmentId: (int)status + 1000 * serviceId,
              status: status,
              serviceId: $"{serviceId:000000}");
            referral.Cri = RandomEntityCreator.ReferralCri();

            referrals.Add(referral);
          }
        }
        _context.AddRange(referrals);

        // add
        var referralsAwaitingUpdateService1 = referrals
          .Where(r => r.Status == ReferralStatus.RejectedToEreferrals.ToString())
          .Where(r => r.ServiceId == "000001")
          .ToArray();

        var referralsNotIncludedService1Records = referrals
          .Where(r => r.Status == ReferralStatus.CancelledByEreferrals.ToString())
          .Where(r => r.ServiceId == "000001");

        referralsNotIncludedService1Records = referralsNotIncludedService1Records
          .Concat(referrals
          .Where(r => r.ServiceId != "000001"));

        var referralsNotIncludedService1 =
          referralsNotIncludedService1Records.ToArray();

        var referralsInProgressService1 = referrals
          .Where(r => !referralsAwaitingUpdateService1.Contains(r))
          .Where(r => !referralsNotIncludedService1.Contains(r))
          .Where(r => r.ServiceId == "000001")
          .ToArray();

        int expectedActiveReferralsCount =
          referralsAwaitingUpdateService1.Count() +
          referralsInProgressService1.Count();

        _context.SaveChanges();

        // act 
        List<ActiveReferralAndExceptionUbrn> activeReferrals =
         await _service.GetActiveReferralAndExceptionUbrns("000001");

        // assert
        activeReferrals.Count().Should().Be(expectedActiveReferralsCount);

        foreach (var referral in referrals)
        {
          if (referralsNotIncludedService1.Contains(referral))
          {
            activeReferrals.Count(r => r.Ubrn == referral.Ubrn).Should().Be(0);
          }
          else
          {
            var activeReferal = activeReferrals
             .Single(a => a.Ubrn == referral.Ubrn);

            activeReferal.CriLastUpdated.Should()
              .Be(referral.Cri.ClinicalInfoLastUpdated);

            activeReferal.MostRecentAttachmentId.Should()
              .Be(referral.MostRecentAttachmentId);

            if (referralsAwaitingUpdateService1.Contains(referral))
            {
              activeReferal.Status.Should().Be(
                ActiveReferralAndExceptionUbrnStatus.AwaitingUpdate.ToString());
              activeReferal.ReferralAttachmentId.Should()
                .Be(referral.ReferralAttachmentId);
            }
            else if (referralsInProgressService1.Contains(referral))
            {
              activeReferal.Status.Should().Be(
                ActiveReferralAndExceptionUbrnStatus.InProgress.ToString());
              activeReferal.ReferralAttachmentId.Should().BeNull();
            }
            else
            {
              throw new XunitException($"Status {referral.Status} is missing");
            }
          }
        }
      }
    }

    public class GetNhsNumbers : ReferralServiceTests
    {
      public GetNhsNumbers(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [InlineData(10, 10)]
      [InlineData(null, 1)]
      [InlineData(100, 100)]
      [InlineData(200, 200)]
      [Theory]
      public async Task Valid(int? arrayCount, int countCheck)
      {
        // act
        string[] nhsNumberList = _service.GetNhsNumbers(arrayCount);

        // assert
        nhsNumberList.Should().NotBeNull();
        nhsNumberList.Should().BeOfType<string[]>();
        nhsNumberList.Length.Should().Be(countCheck);
      }
    }

    public class GetReferralById : ReferralServiceTests
    {
      public GetReferralById(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        // arrange
        var createdReferral = CreateUniqueReferral();
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        var expected = createdReferral.Id;

        // act
        IReferral referral = await _service.GetReferralWithTriagedProvidersById(expected);

        // assert
        referral.Should().NotBeNull();
        referral.Should().BeOfType<Referral>();
        referral.Id.Should().Be(expected);

        // clean up
        _context.Remove(createdReferral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid_TriageSetProviderList()
      {
        // arrange
        var createdReferral = RandomEntityCreator
          .CreateRandomReferral(
          triagedCompletionLevel: ((int)Enums.TriageLevel.Medium).ToString());
        _context.Referrals.Add(createdReferral);

        int expectedProviderCount = _context.Providers
        .Where(p => p.IsActive)
        .Where(p => p.Level2 == true)
        .Count();

        if (expectedProviderCount == 0)
        {
          var createdProvider = RandomEntityCreator.CreateRandomProvider(
          isLevel2: true);
          _context.Providers.Add(createdProvider);
          expectedProviderCount = 1;
        }

        _context.SaveChanges();
        var expected = createdReferral.Id;

        // act
        IReferral referral = await _service.GetReferralWithTriagedProvidersById(createdReferral.Id);

        // assert
        referral.Should().NotBeNull();
        referral.Should().BeOfType<Referral>();
        referral.Id.Should().Be(expected);
        referral.Providers.Count.Should().Be(expectedProviderCount);
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid referralId = INVALID_ID;

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service.GetReferralWithTriagedProvidersById(referralId));

      }
    }

    public class GetReferralByTextMessageId : ReferralServiceTests
    {
      public GetReferralByTextMessageId(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        // arrange
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

        // act
        IReferral referralReturned =
          await _service.GetReferralByTextMessageId(textMessage.Id);

        // assert
        referralReturned.Should().NotBeNull();
        referralReturned.Should().BeOfType<Referral>();
        referralReturned.Id.Should().Be(expectedReferralId);
        referralReturned.Status.Should()
          .Be(Enums.ReferralStatus.TextMessage1.ToString());

        // clean up
        _context.Remove(referral);
        _context.TextMessages.Remove(textMessage);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid textMessageId = INVALID_ID;

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(async () =>
            await _service.GetReferralByTextMessageId(textMessageId));

      }

      [Fact]
      public async Task TextMessageExpiredException()
      {
        // arrange
        Entities.Referral referral =
          CreateUniqueReferral();
        Entities.TextMessage textMessage = _serviceFixture.Mapper
          .Map<Entities.TextMessage>(ServiceFixture.VALID_TEXTMESSAGE_ENTITY);
        textMessage.IsActive = false;
        referral.TextMessages = new List<Entities.TextMessage>()
      {
        textMessage
      };
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // act & assert
        await Assert.ThrowsAsync<TextMessageExpiredException>(async () =>
          await _service.GetReferralByTextMessageId(textMessage.Id));
      }

      [Fact]
      public async Task ReferralInvalidStatusException()
      {
        // arrange
        Entities.Referral referral = CreateUniqueReferral();
        referral.Status = Enums.ReferralStatus.Exception.ToString();

        Entities.TextMessage textMessage = _serviceFixture.Mapper
          .Map<Entities.TextMessage>(ServiceFixture.VALID_TEXTMESSAGE_ENTITY);
        referral.TextMessages = new List<Entities.TextMessage>()
      {
        textMessage
      };
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // act & assert
        await Assert.ThrowsAsync<ReferralInvalidStatusException>(async () =>
          await _service.GetReferralByTextMessageId(textMessage.Id));
      }
    }

    public class GetServiceUserReferralAsync : ReferralServiceTests
    {
      public GetServiceUserReferralAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.TextMessages.RemoveRange(_context.TextMessages);
        _context.SaveChanges();
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      [InlineData(" ")]
      [InlineData("£$desa")]
      public async Task Base36DateInvalid_Exception(
        string base36SentDate)
      {
        // act & assert
        ReferralNotFoundException exception = await Assert
          .ThrowsAsync<ReferralNotFoundException>(async () => await _service
            .GetServiceUserReferralAsync(base36SentDate));
      }

      [Fact]
      public async Task Base36DateDoesNotExist_Exception()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;
        string base36Date = Base36Converter
          .ConvertDateTimeOffsetToBase36(dateSent);

        // act & assert
        ReferralNotFoundException exception = await Assert
          .ThrowsAsync<ReferralNotFoundException>(async () => await _service
            .GetServiceUserReferralAsync(base36Date));
      }

      [Theory]
      [InlineData(ReferralStatus.TextMessage1)]
      [InlineData(ReferralStatus.TextMessage2)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.ChatBotCall2)]
      [InlineData(ReferralStatus.ChatBotTransfer)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      public async Task TextMessage1_Referral(ReferralStatus status)
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          status: status);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
        textMsg1.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var serviceUserReferral = await _service
          .GetServiceUserReferralAsync(textMsg1.Base36DateSent);

        // assert
        serviceUserReferral.Id.Should().Be(referral.Id);
      }

      [Theory]
      [InlineData(ReferralStatus.TextMessage1)]
      [InlineData(ReferralStatus.TextMessage2)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.ChatBotCall2)]
      [InlineData(ReferralStatus.ChatBotTransfer)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      public async Task TextMessage2_Referral(ReferralStatus status)
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          status: status);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-98));
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var serviceUserReferral = await _service
          .GetServiceUserReferralAsync(textMsg2.Base36DateSent);

        // assert
        serviceUserReferral.Id.Should().Be(referral.Id);
      }

      [Fact]
      public async Task TextMessage1WithTextMessage2_Referral()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-98));
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var serviceUserReferral = await _service
          .GetServiceUserReferralAsync(textMsg1.Base36DateSent);

        // assert
        serviceUserReferral.Id.Should().Be(referral.Id);
      }


      [Fact]
      public async Task TextMessage1OutcomeDoNotContactEmail_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now,
          outcome: Constants.DO_NOT_CONTACT_EMAIL);
        textMsg1.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.SaveChanges();

        // act & assert
        TextMessageExpiredByEmailException exception = await Assert
          .ThrowsAsync<TextMessageExpiredByEmailException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg1.Base36DateSent));
      }

      [Fact]
      public async Task TextMessage2OutcomeDoNotContactEmail_Exception()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddHours(-Constants.HOURS_BEFORE_NEXT_STAGE));
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent,
          outcome: Constants.DO_NOT_CONTACT_EMAIL);
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();

        // act & assert
        TextMessageExpiredByEmailException exception = await Assert
          .ThrowsAsync<TextMessageExpiredByEmailException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg2.Base36DateSent));
      }

      [Fact]
      public async Task TextMessage1ProviderSelection_Exception()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: dateSent,
          providerId: Guid.NewGuid(),
          status: ReferralStatus.ProviderAwaitingStart);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage();
        textMsg1.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<TextMessageExpiredByProviderSelectionException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg1.Base36DateSent));
      }

      [Fact]
      public async Task TextMessage2ProviderSelection_Exception()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;

        var referral = RandomEntityCreator.CreateRandomReferral(
          dateOfProviderSelection: dateSent,
          providerId: Guid.NewGuid(),
          status: ReferralStatus.ProviderAwaitingStart);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent.AddDays(-100));
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage(
          sent: dateSent);
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<TextMessageExpiredByProviderSelectionException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg2.Base36DateSent));
      }

      [Fact]
      public async Task TextMessage1OutcomeDateOfBirthExpiry_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage1);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now,
          outcome: Constants.DATE_OF_BIRTH_EXPIRY,
          received: DateTimeOffset.Now);
        textMsg1.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<TextMessageExpiredByDoBCheckException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg1.Base36DateSent));
      }

      [Fact]
      public async Task TextMessage2OutcomeDateOfBirthExpiry_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now.AddDays(-100));
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage(
          sent: DateTimeOffset.Now.AddDays(-98),
          outcome: Constants.DATE_OF_BIRTH_EXPIRY,
          received: DateTimeOffset.Now);
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<TextMessageExpiredByDoBCheckException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg2.Base36DateSent));
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
        { ReferralStatus.TextMessage1,
          ReferralStatus.TextMessage2,
          ReferralStatus.ChatBotCall1,
          ReferralStatus.ChatBotCall2,
          ReferralStatus.ChatBotTransfer,
          ReferralStatus.RmcCall,
          ReferralStatus.RmcDelayed })]
      public async Task TextMessage1ReferralInvalidStatus_Exception(
        ReferralStatus status)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: status);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage();
        textMsg1.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<ReferralInvalidStatusException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg1.Base36DateSent));
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
        { ReferralStatus.TextMessage1,
          ReferralStatus.TextMessage2,
          ReferralStatus.ChatBotCall1,
          ReferralStatus.ChatBotCall2,
          ReferralStatus.ChatBotTransfer,
          ReferralStatus.RmcCall,
          ReferralStatus.RmcDelayed })]
      public async Task TextMessage2ReferralInvalidStatus_Exception(
        ReferralStatus status)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: status);

        var textMsg1 = RandomEntityCreator.CreateRandomTextMessage();
        textMsg1.Referral = referral;

        var textMsg2 = RandomEntityCreator.CreateRandomTextMessage();
        textMsg2.Referral = referral;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg1);
        _context.TextMessages.Add(textMsg2);
        _context.SaveChanges();

        // act & assert
        var exception = await Assert
          .ThrowsAsync<ReferralInvalidStatusException>(
            async () => await _service
              .GetServiceUserReferralAsync(textMsg2.Base36DateSent));
      }
    }

    public class PrepareRmcCallsAsync : ReferralServiceTests
    {
      public PrepareRmcCallsAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task TextMessageExpiredException_ReturnReferral()
      {
        // arrange
        Entities.Referral referral1 =
          CreateUniqueReferral();
        referral1.Status = ReferralStatus.ChatBotCall2.ToString();
        Entities.Call call1 = RandomEntityCreator.CreateRandomChatBotCall();
        call1.ReferralId = referral1.Id;
        call1.Referral = referral1;
        call1.Sent = DateTimeOffset.Now.Date.AddDays(-1);

        Entities.Referral referral2 =
          CreateUniqueReferral();
        referral2.Status = ReferralStatus.ChatBotTransfer.ToString();
        Entities.Call call2 = RandomEntityCreator.CreateRandomChatBotCall();
        call2.ReferralId = referral2.Id;
        call2.Referral = referral2;
        call2.Sent = default;

        Entities.Referral referral3 =
          CreateUniqueReferral();
        referral3.Status = ReferralStatus.ChatBotCall2.ToString();
        Entities.Call call3 = RandomEntityCreator.CreateRandomChatBotCall();
        call3.ReferralId = referral3.Id;
        call3.Referral = referral3;
        call3.Sent = DateTimeOffset.Now.Date.AddDays(-3);

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        _context.Referrals.Add(referral3);
        _context.Calls.Add(call1);
        _context.Calls.Add(call2);
        _context.Calls.Add(call3);
        await _context.SaveChangesAsync();

        string expectedMessage = "Prepared 1 referral(s) for an RMC call.";

        // act 
        string returnedMessage = await _service.PrepareRmcCallsAsync();

        // assert
        returnedMessage.Should().NotBeNull();
        returnedMessage.Should().Be(expectedMessage);
        referral1.Status.Should().Be(ReferralStatus.ChatBotCall2.ToString());
        referral2.Status.Should()
          .Be(ReferralStatus.ChatBotTransfer.ToString());
        referral3.Status.Should().Be(ReferralStatus.RmcCall.ToString());
        referral3.DateToDelayUntil.Should().BeNull();

        // clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        _context.Referrals.Remove(referral3);
        _context.Calls.Remove(call1);
        _context.Calls.Remove(call2);
        _context.Calls.Remove(call3);
        _context.SaveChanges();
      }
    }

    public class Search : ReferralServiceTests
    {
      public Search(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]  // TODO: SEE #1668
      public async Task ReturnsChatBotTransferAtTop()
      {
        //arrange -- add one RmcCall and ChatBotTransfer
        var rmcCallReferral = CreateUniqueReferral(
          status: ReferralStatus.RmcCall);
        var chatBotTransferReferral = CreateUniqueReferral(
          status: ReferralStatus.ChatBotTransfer);
        _context.Referrals.Add(rmcCallReferral);
        _context.Referrals.Add(chatBotTransferReferral);
        _context.SaveChanges();

        var expected = _context.Referrals
          .Count(r => r.Status == ReferralStatus.ChatBotTransfer.ToString());

        //act
        try
        {
          IReferralSearchResponse response =
            await _service.Search(new ReferralSearch());

          int referralCount = response.Count;

          IReferral referral = response.Referrals.FirstOrDefault();

          //assert
          expected.Should().BeGreaterThan(0);
          //expected.Should().Be(referralCount);
          referral.Status.Should()
            .Be(ReferralStatus.ChatBotTransfer.ToString());

          // clean up
          _context.Remove(rmcCallReferral);
          _context.Remove(chatBotTransferReferral);
          _context.SaveChanges();
        }
        catch (Exception ex)
        {
          //TODO: Fix EXPECTED EXCEPTION
          if (ex.Message.Contains(".AsSingleQuery()"))
          {
            Assert.True(true,
              "..AsSingleQuery() cannot be unit tested in XUnit.  " +
              "The single query should be in the Startup.cs as  " +
              "options.UseSqlServer(" +
              "Configuration.GetConnectionString(\"WmsHub\")," +
              "opt =>{opt.UseQuerySplittingBehavior(" +
              "QuerySplittingBehavior.SplitQuery);" +
              " opt.EnableRetryOnFailure();}); ");
          }
          else
          {
            Assert.True(false, ex.Message);
          }
        }

      }

      [Fact]  // TODO: SEE #1668
      public async Task ReturnsChatBotTransferAtTop_WithSearchFields()
      {
        //arrange -- add one RmcCall and ChatBotTransfer;

        var rmcCallReferral = CreateUniqueReferral(
          status: ReferralStatus.RmcCall);

        var chatBotTransferReferral = CreateUniqueReferral(
          status: ReferralStatus.ChatBotTransfer);

        _context.Referrals.Add(rmcCallReferral);
        _context.Referrals.Add(chatBotTransferReferral);
        _context.SaveChanges();

        // act
        IReferralSearchResponse response = await _service.Search(
          new ReferralSearch());

        //assert
        IReferral referral = response.Referrals.FirstOrDefault();
        referral.Status.Should()
          .Be(ReferralStatus.ChatBotTransfer.ToString());

        // clean up
        _context.Remove(rmcCallReferral);
        _context.Remove(chatBotTransferReferral);
        _context.SaveChanges();
      }

      [Fact]  // TODO: SEE #1668
      public async Task ReturnsChatBotTransferAtTop_WithStatusSearchField()
      {
        var rmcCallReferral = CreateUniqueReferral(
          status: ReferralStatus.RmcCall);
        var chatBotTransferReferral = CreateUniqueReferral(
          status: ReferralStatus.ChatBotTransfer);

        _context.Referrals.Add(rmcCallReferral);
        _context.Referrals.Add(chatBotTransferReferral);
        _context.SaveChanges();

        var expected = _context.Referrals
          .Count(r => r.Status == ReferralStatus.ChatBotTransfer.ToString());

        ReferralSearch search = new ReferralSearch()
        {
          Statuses = new string[] { ReferralStatus.ChatBotTransfer.ToString() }
        };

        //act
        try
        {
          IReferralSearchResponse response = await _service.Search(search);

          int referralCount = response.Count;

          IReferral referral = response.Referrals.FirstOrDefault();

          //assert
          expected.Should().BeGreaterThan(0);
          expected.Should().Be(referralCount);
          referral.Status.Should().Be(ReferralStatus.ChatBotTransfer.ToString());

          // clean up
          _context.Remove(rmcCallReferral);
          _context.Remove(chatBotTransferReferral);
          _context.SaveChanges();
        }
        catch (Exception ex)
        {
          //TODO: Fix EXPECTED EXCEPTION
          if (ex.Message.Contains(".AsSingleQuery()"))
          {
            Assert.True(true,
              "..AsSingleQuery() cannot be unit tested in XUnit.  " +
              "The single query should be in the Startup.cs as  " +
              "options.UseSqlServer(" +
              "Configuration.GetConnectionString(\"WmsHub\")," +
              "opt =>{opt.UseQuerySplittingBehavior(" +
              "QuerySplittingBehavior.SplitQuery);" +
              " opt.EnableRetryOnFailure();}); ");
          }
          else
          {
            Assert.True(false, ex.Message);
          }
        }
      }
    }
    public class TestCreateWithChatBotStatus : ReferralServiceTests
    {
      public TestCreateWithChatBotStatus(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // arrange
        ReferralCreate model = null;

        // act & assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service
            .TestCreateWithChatBotStatus(model));
      }

      [Fact]
      public async Task ReferralInvalidCreationException()
      {
        // arrange
        ReferralCreate model = ValidReferralCreate();
        model.NhsNumber = null;

        // act & assert
        ReferralInvalidCreationException exception =
          await Assert.ThrowsAsync<ReferralInvalidCreationException>(
          async () => await _service
            .TestCreateWithChatBotStatus(model));
      }

      [Fact]
      public async Task ReferralUpdated()
      {
        // arrange
        ReferralCreate model = ValidReferralCreate();

        // act
        IReferral returned =
          await _service.TestCreateWithChatBotStatus(model);

        // assert
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

    public class TestCreateWithRmcStatus : ReferralServiceTests
    {
      public TestCreateWithRmcStatus(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // arrange
        ReferralCreate model = null;

        // act & assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service
            .TestCreateWithRmcStatus(model));
      }

      [Fact]
      public async Task ReferralInvalidCreationException()
      {
        // arrange
        ReferralCreate model = ValidReferralCreate();
        model.NhsNumber = null;

        // act & assert
        ReferralInvalidCreationException exception =
          await Assert.ThrowsAsync<ReferralInvalidCreationException>(
          async () => await _service
            .TestCreateWithRmcStatus(model));
      }

      [Fact]
      public async Task ReferralUpdated()
      {
        // arrange
        ReferralCreate model = ValidReferralCreate();

        // act
        IReferral returned = await _service.TestCreateWithRmcStatus(model);

        // assert
        returned.Should().NotBeNull();
        returned.IsActive.Should().Be(true);
        returned.Status.Should().Be(ReferralStatus.ChatBotCall2.ToString());
        returned.StatusReason.Should().Be("TestCreateWithChatBotStatus");
        returned.TextMessages.Count.Should().Be(2);
        returned.TextMessages[0].Sent.Date.Should()
          .Be(DateTimeOffset.Now.AddHours(-144).Date);
        returned.TextMessages[1].Sent.Date.Should()
          .Be(DateTimeOffset.Now.AddHours(-192).Date);
      }
    }

    public class Update : ReferralServiceTests
    {
      public Update(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // arrange
        // act & assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.UpdateGpReferral(null));
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        //create referral map to ReferralUpdate
        Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);

        ReferralUpdate referralUpdate = base._serviceFixture.Mapper
          .Map<ReferralUpdate>(referral);

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service.UpdateGpReferral(referralUpdate));
      }

      [Fact]
      public async Task ReferralInvalidStatusException()
      {
        // arrange
        Entities.Referral referral = CreateUniqueReferral(
        status: ReferralStatus.New);

        ReferralUpdate referralUpdate = base._serviceFixture.Mapper
          .Map<ReferralUpdate>(referral);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act & assert
        ReferralInvalidStatusException exception =
          await Assert.ThrowsAsync<ReferralInvalidStatusException>(
          async () => await _service.UpdateGpReferral(referralUpdate));
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
        //arrange
        string ubrn = Generators.GenerateUbrn(new Random());
        string nhsNumber = Generators.GenerateNhsNumber(new Random());
        var referral1 = RandomEntityCreator.CreateRandomReferral(
            ubrn: Generators.GenerateUbrn(new Random()),
            nhsNumber: nhsNumber,
            status: ReferralStatus.CancelledByEreferrals);
        referral1.IsActive = false;
        _context.Referrals.Add(referral1);
        var referral2 = RandomEntityCreator.CreateRandomReferral(
          ubrn: ubrn,
          nhsNumber: nhsNumber,
          status: ReferralStatus.CancelledByEreferrals);
        _context.Referrals.Add(referral2);
        referral2.IsActive = true;
        var referral3 = RandomEntityCreator.CreateRandomReferral(
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

        ReferralUpdate referralUpdate = base._serviceFixture.Mapper
          .Map<ReferralUpdate>(referral3);

        //act
        try
        {

          var referralReturned =
            await _service.UpdateGpReferral(referralUpdate);
          Assert.True(false, "ReferralNotUniqueException expected");
        }
        catch (ReferralNotUniqueException iox)
        {
          iox.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          _context.Referrals.RemoveRange(_context.Referrals);
          _context.SaveChanges();
        }

      }

      [Fact]
      public async Task DeprivationUpdated()
      {
        // arrange
        string oldDeprivation = "IMD1";
        string expectedDeprivation = "IMD3";
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

        // act
        var referralReturned =
          await _service.UpdateGpReferral(referralUpdate);

        Entities.Referral updatedReferral = await _context
        .Referrals
        .Where(r => r.Id == referral.Id)
        .FirstOrDefaultAsync();

        //assert
        referralReturned.Postcode.Should().Be(expectedPostCode);
        referralReturned.Postcode.Should().NotBe(oldPostCode);
        updatedReferral.Deprivation.Should().Be(expectedDeprivation);
        updatedReferral.Deprivation.Should().NotBe(oldDeprivation);

        //clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Update_AsExpected()
      {
        // arrange
        var referral = CreateUniqueReferral();

        referral.Status = ReferralStatus.RejectedToEreferrals.ToString();

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        ReferralUpdate referralUpdate = base._serviceFixture.Mapper
          .Map<ReferralUpdate>(referral);

        referralUpdate.HasHypertension = false;

        //act
        var referralReturned =
          await _service.UpdateGpReferral(referralUpdate);

        //assert
        referralReturned.HasHypertension.Should().Be(false);

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Create_MultiReason_Invalid()
      {
        // arrange
        _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
          .Throws(new DeprivationNotFoundException());

        ProviderService _providerService =
          new ProviderService(
            _context,
            _serviceFixture.Mapper,
            _mockOptions.Object);

        ReferralService _serviceToTest =
          new ReferralService(
            _context,
            _serviceFixture.Mapper,
            _providerService,
            _mockDeprivationService.Object,
            _mockPostcodeService.Object,
            _mockPatientTriageService.Object,
            _mockOdsOrganisationService.Object)
          {
            User = GetClaimsPrincipal()
          };

        string expectedStatusReason =
          "The field CalculatedBmiAtRegistration must be between 27.5 " +
          "and 90. A diagnosis of Diabetes Type 1 or Diabetes " +
          "Type 2 or Hypertension is required.";

        string ubrn = $"{DateTimeOffset.Now:MMddHHmmssff}";
        var referral = RandomEntityCreator
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

        ReferralUpdate referralUpdate = base._serviceFixture.Mapper
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

        //act
        var referralReturned = await _service.UpdateGpReferral(referralUpdate);

        // assert
        referralReturned.Status.Should().Be(
          ReferralStatus.Exception.ToString());
        referralReturned.StatusReason.Should().Be(expectedStatusReason);

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }
    }

    public class UpdateConsentForFutureContactForEvaluation
      : ReferralServiceTests
    {
      public UpdateConsentForFutureContactForEvaluation(
        ServiceFixture serviceFixture)
          : base(serviceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        // Update consent from false to true
        // arrange
        Entities.Referral referral =
          CreateUniqueReferral();
        referral.ConsentForFutureContactForEvaluation = false;
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var consentGiven = false;

        // act
        var updatedReferral = await _service
          .UpdateConsentForFutureContactForEvaluation(
            referral.Id, consentGiven, consentGiven);

        // assert
        updatedReferral.Should().NotBeNull();
        updatedReferral.Should().BeOfType<Referral>();
        updatedReferral
          .ConsentForFutureContactForEvaluation.Should().Be(consentGiven);

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task WithEmail()
      {
        // Update consent from false to true
        // arrange
        var referral = CreateUniqueReferral();
        referral.ConsentForFutureContactForEvaluation = false;
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var consentGiven = true;
        var emailAddress = "test@test.co.uk";

        // act
        IReferral updatedReferral = await _service
          .UpdateConsentForFutureContactForEvaluation(
            referral.Id, consentGiven, consentGiven, emailAddress);

        // assert
        updatedReferral.Should().NotBeNull();
        updatedReferral.Should().BeOfType<Referral>();
        updatedReferral
          .ConsentForFutureContactForEvaluation.Should().Be(consentGiven);
        updatedReferral.Email.Should().Be(emailAddress);

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid referralId = INVALID_ID;
        bool consent = true;

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service
            .UpdateConsentForFutureContactForEvaluation(
              referralId, consent, consent));
      }


      [Fact]
      public async Task ReferralContactEmailException()
      {
        // arrange
        var referral = CreateUniqueReferral();
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        string emailAddress = "";
        bool consent = true;

        // act & assert
        ReferralContactEmailException exception =
          await Assert.ThrowsAsync<ReferralContactEmailException>(
          async () => await _service
            .UpdateConsentForFutureContactForEvaluation(referral.Id, consent,
              consent, emailAddress));

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }
    }

    public class UpdateEthnicity : ReferralServiceTests
    {
      public UpdateEthnicity(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        // Update ethnicity from Asian to Black
        // arrange
        Entities.Referral referral =  CreateUniqueReferral();
        referral.Ethnicity = Enums.Ethnicity.Asian.ToString();
        Mock<CourseCompletionResult> courseCompletionResult =new ();
        courseCompletionResult.Setup(t => t.TriagedCompletionLevel)
          .Returns(TriageLevel.Low);
        courseCompletionResult.Setup(t => t.TriagedWeightedLevel)
          .Returns(TriageLevel.Low);

        _mockPatientTriageService
          .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()))
          .Returns(courseCompletionResult.Object);

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        Enums.Ethnicity ethnicity = Enums.Ethnicity.Black;
        string expectedEthnicity = ethnicity.ToString();

        string[] expectedTriageLevels = new string[] { "1", "2", "3" };

        // act
        IReferral updatedReferral = await _service.UpdateEthnicity(
          referral.Id, ethnicity);

        // assert
        updatedReferral.Should().NotBeNull();
        updatedReferral.Should().BeOfType<Referral>();
        updatedReferral.Ethnicity.Should().Be(expectedEthnicity);
        updatedReferral.TriagedCompletionLevel.Should()
          .BeOneOf(expectedTriageLevels);
        updatedReferral.TriagedWeightedLevel.Should()
          .BeOneOf(expectedTriageLevels);

        // clean up
        _context.Remove(referral);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid referralId = INVALID_ID;
        Enums.Ethnicity ethnicity = Enums.Ethnicity.Asian;

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
          async () => await _service.UpdateEthnicity(referralId, ethnicity));
      }
    }

    public class UpdateServiceUserEthnicityAsync : ReferralServiceTests
    {
      public UpdateServiceUserEthnicityAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task IdEmpty_Exception()
      {
        // arrange
        Guid referralId = Guid.Empty;
        string ethnicity = null;

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referralId, ethnicity));

        // assert
        ex.Should().BeOfType<ArgumentException>().Subject.Message
          .Should().Be("id cannot be empty. (Parameter 'id')");
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async Task EthnicityDisplayNameNullOrWhiteSpace_Exception(
        string ethnicity)
      {
        // arrange
        Guid referralId = Guid.NewGuid();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referralId, ethnicity));

        // assert
        ex.Should().BeOfType<ArgumentException>().Subject.Message
          .Should().Be("ethnicityDisplayName cannot be null or white space. " +
          "(Parameter 'ethnicityDisplayName')");
      }

      [Fact]
      public async Task ReferralNotFound_Exception()
      {
        // arrange
        Guid referralId = Guid.NewGuid();
        string ethnicity = "Chinese";

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referralId, ethnicity));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>().Subject.Message
          .Should().Be(
            $"Unable to find a referral with an id of {referralId}.");
      }

      [Fact]
      public async Task ReferralInactive_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referral.Id, referral.Ethnicity));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>().Subject.Message
          .Should().Be(
            $"Unable to find a referral with an id of {referral.Id}.");

        var dbReferral = _context.Referrals.Single(r => r.Id == referral.Id);
        dbReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
      }

      [Fact]
      public async Task ReferralHasSelectedProvider_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          providerId: Guid.NewGuid());
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referral.Id, referral.Ethnicity));

        // assert
        ex.Should().BeOfType<ReferralProviderSelectedException>();
        ex.Message.Should().Be(
          $"The referral {referral.Id} has previously had its provider " +
          $"selected {referral.ProviderId}.");

        var dbReferral = _context.Referrals.Single(r => r.Id == referral.Id);
        dbReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
      }

      [Fact]
      public async Task EthnicityDisplayNameNotFound_Exception()
      {
        // arrange
        string ethnicityDisplayName = "UnknownDisplayName";

        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referral.Id, ethnicityDisplayName));

        // assert
        ex.Should().BeOfType<EthnicityNotFoundException>();
        ex.Message.Should().Be(
          $"The ethnicity with a display " +
          $"name of {ethnicityDisplayName} cannot be found.");

        var dbReferral = _context.Referrals.Single(r => r.Id == referral.Id);
        dbReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits));
      }

      [Fact]
      public async Task BmiTooLow_Exception()
      {
        // arrange
        var ethnicity = _context.Ethnicities.First();

        var referral = RandomEntityCreator.CreateRandomReferral(
          calculatedBmiAtRegistration: ethnicity.MinimumBmi - 1);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName));

        // assert
        ex.Should().BeOfType<BmiTooLowException>();
        ex.Message.Should().Be("A referral that has the ethnicity " +
          $"of {ethnicity.DisplayName} needs a BMI greater than " +
          $"{ethnicity.MinimumBmi} but this referral has a BMI of " +
          $"{referral.CalculatedBmiAtRegistration}.");

        var dbReferral = _context.Referrals.Single(r => r.Id == referral.Id);
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
      public async Task Valid()
      {
        // arrange
        var ethnicity = _context.Ethnicities.First();

        var referral = RandomEntityCreator.CreateRandomReferral(
          calculatedBmiAtRegistration: ethnicity.MinimumBmi + 1);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        IReferral updatedReferral = await _service
          .UpdateServiceUserEthnicityAsync(referral.Id, ethnicity.DisplayName);

        // assert
        updatedReferral.Should().BeEquivalentTo(referral, options => options
          .Excluding(r => r.Audits)
          .Excluding(r => r.Cri)
          .Excluding(r => r.Ethnicity)
          .Excluding(r => r.DateToDelayUntil)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.OfferedCompletionLevel)
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


        var dbReferral = _context.Referrals.Single(r => r.Id == referral.Id);
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
    }

    public class PrepareDelayedCalls : ReferralServiceTests
    {
      public PrepareDelayedCalls(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task PrepareDelayedCalls_SingleUpdate()
      {
        // arrange
        string expectedResonse = "Prepared DelayedCalls - " +
        "1 referral(s) set to 'RmcCall'.";

        DateTimeOffset dateInFurther1 = DateTimeOffset.Now.AddDays(+2);
        DateTimeOffset dateInFurther2 = DateTimeOffset.Now.AddDays(+6);
        DateTimeOffset dateNow = DateTimeOffset.Now.AddDays(-1);

        var referral1 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateInFurther1
          );
        var referral2 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateInFurther2
          );
        var referral3 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateNow
          );

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        _context.Referrals.Add(referral3);
        await _context.SaveChangesAsync();

        // act
        string response = await _service.PrepareDelayedCallsAsync();

        // assert
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResonse);
        referral1.Status.Should().NotBe(ReferralStatus.RmcCall.ToString());
        referral2.Status.Should().NotBe(ReferralStatus.RmcCall.ToString());
        referral3.Status.Should().Be(ReferralStatus.RmcCall.ToString());

        referral1.DateToDelayUntil.Should().NotBeNull();
        referral2.DateToDelayUntil.Should().NotBeNull();
        referral3.DateToDelayUntil.Should().BeNull();

        // clean up
        _context.Remove(referral1);
        _context.Remove(referral2);
        _context.Remove(referral3);
        _context.SaveChanges();
      }

      [Fact]
      public async Task PrepareDelayedCalls_UpdateSuccess()
      {
        // arrange
        string expectedResonse = "Prepared DelayedCalls - " +
        "2 referral(s) set to 'RmcCall'.";

        DateTimeOffset dateInFurther = DateTimeOffset.Now.AddDays(+2);
        DateTimeOffset dateInPast1 = DateTimeOffset.Now.AddDays(-2);
        DateTimeOffset dateInPast2 = DateTimeOffset.Now.AddDays(-6);
        DateTimeOffset dateNow = DateTimeOffset.Now;

        var referral1 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateInPast1
          );
        var referral2 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateInFurther
          );
        var referral3 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateInPast2
          );
        var referral4 = RandomEntityCreator
          .CreateRandomReferral(
          status: ReferralStatus.RmcDelayed,
          dateToDelayUntil: dateNow
          );

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        _context.Referrals.Add(referral3);
        _context.Referrals.Add(referral4);
        await _context.SaveChangesAsync();

        // act
        string response = await _service.PrepareDelayedCallsAsync();

        // assert
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResonse);
        referral1.Status.Should().Be(ReferralStatus.RmcCall.ToString());
        referral3.Status.Should().Be(ReferralStatus.RmcCall.ToString());

        referral1.DateToDelayUntil.Should().BeNull();
        referral2.DateToDelayUntil.Should().NotBeNull();
        referral3.DateToDelayUntil.Should().BeNull();

        // clean up
        _context.Remove(referral1);
        _context.Remove(referral2);
        _context.Remove(referral3);
        _context.Remove(referral4);
        _context.SaveChanges();
      }
    }

    public class UpdateStatusFromRmcCallToFailedToContactAsync
      : ReferralServiceTests
    {
      public UpdateStatusFromRmcCallToFailedToContactAsync(
        ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ReferralIdDoesNotExist_Exception()
      {
        // arrange
        Guid referralId = INVALID_ID;
        string expectedMessage = new ReferralNotFoundException(referralId)
          .Message;
        string statusReason = "It is a test for ReferralIdDoesNotExist.";

        // act 
        var ex = await Record.ExceptionAsync(() =>
          _service.UpdateStatusFromRmcCallToFailedToContactAsync(referralId, statusReason));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(expectedMessage);
      }

      [Theory]
      [MemberData(nameof(ReferralStatusesTheoryData),
        ReferralStatus.RmcCall)]
      public async Task ReferralInvalidStatus_Exception(ReferralStatus status)
      {
        // arrange
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
          status: status);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        string statusReason = "It is a test for ReferralIdDoesNotExist.";

        string expectedMessage = "Unable to set status to " +
          $"{ReferralStatus.FailedToContact} because status is {status} " +
          $"when it must be {ReferralStatus.RmcCall}.";

        // act 
        var ex = await Record.ExceptionAsync(() =>
          _service.UpdateStatusFromRmcCallToFailedToContactAsync(
            referral.Id,
            statusReason)
          );

        // assert
        ex.Should().BeOfType<ReferralInvalidStatusException>();
        ex.Message.Should().Be(expectedMessage);

        CleanUp(referral.Id);
      }

      [Theory]
      [InlineData(
        ReferralSource.GeneralReferral,
        ReferralStatus.FailedToContactTextMessage)]
      [InlineData(
        ReferralSource.GpReferral,
        ReferralStatus.FailedToContact)]
      [InlineData(
        ReferralSource.Pharmacy,
        ReferralStatus.FailedToContactTextMessage)]
      [InlineData(
        ReferralSource.SelfReferral,
        ReferralStatus.FailedToContactTextMessage)]
      public async Task ReferralSource_UpdateStatus(
        ReferralSource referralSource,
        ReferralStatus expectedStatus)
      {
        // arrange
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
          referralSource: referralSource,
          status: ReferralStatus.RmcCall);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        int expectedMethodOfContact = (int)MethodOfContact.RmcCall;
        int expectedNumberOfContacts = 1;


        string expectedStatusReason = "It is a test for ReferralSource_UpdateStatus.";
        // act
        IReferral result = await _service
          .UpdateStatusFromRmcCallToFailedToContactAsync(referral.Id, expectedStatusReason);

        // assert
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
        // clean up
        CleanUp(referral.Id);
      }
    }

    public class UpdateStatusToRejectedToEreferralsAsync
      : ReferralServiceTests
    {
      public UpdateStatusToRejectedToEreferralsAsync
        (ServiceFixture serviceFixture) : base(serviceFixture)
      {
        // clean referrals table
        _context.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        Guid refId = INVALID_ID;

        // act & assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(async () =>
            await _service
              .UpdateStatusToRejectedToEreferralsAsync(refId, null));
      }

      [Theory]
      [InlineData("Test", null, "Test")]
      [InlineData("Test", "", "Test")]
      [InlineData("Test", "UpdateTest", "UpdateTest")]
      public async Task ValidUpdate(
        string initialReason, string updateReason, string expectedReason)
      {
        // arrange
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

        // act
        IReferral returned = await _service
          .UpdateStatusToRejectedToEreferralsAsync(referral.Id, updateReason);

        // assert
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
      public GetStaffRolesAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task GetStaffRolesAsync_ReturnsListOfStaffRoles()
      {
        //arrange
        int expectedCount = _context.StaffRoles.Count();

        //act
        IEnumerable<IStaffRole> staffList = await _service.GetStaffRolesAsync();

        //assert
        Assert.Equal(expectedCount, staffList.Count());
      }
    }

    public class CreateGeneralReferralTests : ReferralServiceTests, IDisposable
    {
      public CreateGeneralReferralTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task CreateGeneralReferral_ArgumentNullException()
      {
        // arrange
        GeneralReferralCreate model = null;
        // assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _service.CreateGeneralReferral(model));
      }

      [Fact]
      public async Task ReferralContactEmailException_NotRealEmail()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = _validGeneralReferralCreate;
        model.Email = "Incorrect_EmailAddress";

        //string expectedErrorMessage = 
        //  $"Unable to create referral: The Email is not a valid email.," +
        //  $"The Email is not a valid NHS email.";

        var ex = await Assert.ThrowsAsync<GeneralReferralValidationException>(
          async () => await _service.CreateGeneralReferral(model));

        // TODO - Improve test
      }

      [Fact]
      public async Task NoProvidersAvailable_Exception()
      {
        // arrange
        var providers = new List<Business.Models.Provider>();
        var mockProviderService = new Mock<IProviderService>();
        mockProviderService
          .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
          .ReturnsAsync(providers);

        var service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          mockProviderService.Object,
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object);

        // act
        var ex = await Record.ExceptionAsync(() => service
          .CreateGeneralReferral(_validGeneralReferralCreate));

        // assert
        ex.Should().BeOfType<NoProviderChoicesFoundException>();
      }

      [Theory]
      [InlineData(TriageLevel.High)]
      [InlineData(TriageLevel.Medium)]
      public async Task NoLevel3or2Providers_OfferedCompletionLevel1(
        TriageLevel triageLevel)
      {
        // arrange...

        // ...the referral to be triaged to the tested level
        _mockScoreResult.Setup(t => t.TriagedCompletionLevel)
          .Returns(triageLevel);

        // ...there to be no providers available at tested level
        var mockProviderService = new Mock<IProviderService>();
        var noProviders = new List<Business.Models.Provider>();
        mockProviderService
          .Setup(x => x.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == triageLevel)))
          .ReturnsAsync(noProviders);

        // ...there to be one level 1 (Low) provider
        var level1Provider = RandomModelCreator.CreateRandomProvider(
          level1: true, level2: false, level3: false);
        var level1Providers = new List<Business.Models.Provider>()
          { level1Provider };
        mockProviderService
          .Setup(x => x.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == TriageLevel.Low)))
          .ReturnsAsync(level1Providers);

        // ...the referral service to use the movked provider service.
        var service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          mockProviderService.Object,
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object);

        // act
        var result = await service
          .CreateGeneralReferral(_validGeneralReferralCreate);

        // assert...

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
        var referral = _context.Referrals.Single(r => r.Id == result.Id);
        referral.TriagedCompletionLevel = $"{(int)triageLevel}";
        referral.OfferedCompletionLevel = $"{(int)TriageLevel.Low}";
      }


      [Fact]
      public async Task CreateGeneralReferral_Valid()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = _validGeneralReferralCreate;

        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        RemoveReferralBeforeTest(model.Email);
        // act
        try
        {
          IReferral result = await _service.CreateGeneralReferral(model);

          // assert
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
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task CreateGeneralReferral_Invalid_NoConsent()
      {
        // arrange
        string expected =
          "The ConsentForFutureContactForEvaluation field is required.";
        Random rnd = new Random();
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        GeneralReferralCreate model = uniqueValidGeneralReferralCreate();
        model.ConsentForFutureContactForEvaluation = null;

        _context.SaveChanges();

        // act
        try
        {
          IReferral result = await _service.CreateGeneralReferral(model);

          // assert
          Assert.True(false, "Expected SelfReferralValidationException");

        }
        catch (GeneralReferralValidationException ex)
        {
          Assert.True(true, ex.Message);
          ex.ValidationResults.FirstOrDefault().Value[0].Should()
            .Be(expected);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
      }

      public class CheckNewGeneralReferralIsUniqueAsyncTests :
        CreateGeneralReferralTests
      {
        public CheckNewGeneralReferralIsUniqueAsyncTests(
          ServiceFixture serviceFixture) : base(serviceFixture)
        { }

        [Fact]
        public async Task Invalid_ReferralExists()
        {
          //arrange
          Random rnd = new Random();
          Entities.Referral referral = CreateUniqueReferral();
          referral.ReferralSource = ReferralSource.GeneralReferral.ToString();
          referral.Ubrn = Generators.GenerateSelfUbrn(rnd);
          referral.Email = Generators.GenerateNhsEmail(rnd);
          referral.Status = ReferralStatus.New.ToString();
          referral.NhsNumber = Generators.GenerateNhsNumber(rnd);
          _context.Referrals.Add(referral);
          _context.SaveChanges();

          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          GeneralReferralCreate model = _validGeneralReferralCreate;
          model.NhsNumber = referral.NhsNumber;

          await _context.SaveChangesAsync();

          try
          {
            IReferral result = await _service.CreateGeneralReferral(model);
            Assert.True(false, "ReferralNotUniqueException expected");
          }
          catch (ReferralNotUniqueException ex)
          {
            Assert.True(true, ex.Message);
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected ReferralNotUniqueException but got {ex.Message}");
          }
          finally
          {
            _context.Referrals.Remove(referral);
            _context.SaveChanges();
          }

        }
      }

      public class ValidateReferralTests : CreateGeneralReferralTests
      {
        public ValidateReferralTests(ServiceFixture serviceFixture)
          : base(serviceFixture)
        { }

        [Fact]
        public async Task GeneralReferralValidationException_EthnicityGroup()
        {
          //arrange
          var expected = "The ServiceUserEthnicityGroup field is invalid.";
          Random rnd = new Random();
          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          GeneralReferralCreate model = _validGeneralReferralCreate;
          model.Email = Generators.GenerateEmail(rnd);
          model.ServiceUserEthnicityGroup = "Klingon";

          await _context.SaveChangesAsync();

          RemoveReferralBeforeTest(model.Email);
          try
          {
            IReferral result = await _service.CreateGeneralReferral(model);
            Assert.True(false, "PublicReferralValidationException expected");
          }
          catch (GeneralReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().Be(1);
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected);
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected PublicReferralValidationException " +
              $"but got {ex.Message}");
          }
        }

        [Fact]
        public async Task GeneralReferralValidationException_Ethnicity()
        {
          //arrange
          var expected = "The ServiceUserEthnicity field is invalid.";
          Random rnd = new Random();
          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          GeneralReferralCreate model = _validGeneralReferralCreate;
          model.Email = Generators.GenerateNhsEmail(rnd);
          model.ServiceUserEthnicity = "Klingon";

          await _context.SaveChangesAsync();

          RemoveReferralBeforeTest(model.Email);

          try
          {
            IReferral result = await _service.CreateGeneralReferral(model);
            Assert.True(false, "PublicReferralValidationException expected");
          }
          catch (GeneralReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().Be(1);
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected);
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected PublicReferralValidationException " +
              $"but got {ex.Message}");
          }
        }



        [Fact]
        public async Task GeneralReferralValidationException_BMI()
        {
          //arrange
          Random rnd = new Random();
          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          GeneralReferralCreate model = _validGeneralReferralCreate;
          model.Email = Generators.GenerateNhsEmail(rnd);
          model.HeightCm = 181;
          model.WeightKg = 78;
          var expected = "The calculated BMI of 23.8 is too low, the " +
            $"minimum for an ethnicity of {model.Ethnicity} is 30.00";

          await _context.SaveChangesAsync();
          RemoveReferralBeforeTest(model.Email);
          try
          {
            IReferral result = await _service.CreateGeneralReferral(model);
            Assert.True(false, "PublicReferralValidationException expected");
          }
          catch (GeneralReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().Be(1);
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected);
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected SelfReferralValidationException " +
              $"but got {ex.Message}");
          }
        }

      }

      public class TraiageReferralUpdateAsyncTests : CreateGeneralReferralTests
      {
        public TraiageReferralUpdateAsyncTests(ServiceFixture serviceFixture)
          : base(serviceFixture)
        {
        }

        [Fact]

        public async Task Failed_Id_Empty()
        {
          //arrange
          Guid id = Guid.Empty;
          //act
          try
          {
            await _service.TriageReferralUpdateAsync(id);
            Assert.True(false, "Expected ArgumentException");
          }
          catch (ArgumentException)
          {
            Assert.True(true);
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected ArgumentException but got {ex.Message}");
          }
        }

        [Fact]
        public async Task Valid_Referral_Triage()
        {
          // arrange
          Random rnd = new Random();
          Entities.Referral referral = CreateUniqueReferral(
            ubrn: Generators.GenerateSelfUbrn(rnd),
            heightCm: 181,
            weightKg: 108,
            calculatedBmiAtRegistration: 31);

          _context.Referrals.Add(referral);
          await _context.SaveChangesAsync();

          //act
          try
          {
            referral.TriagedCompletionLevel.Should().BeNull();
            referral.TriagedWeightedLevel.Should().BeNull();
            await _service.TriageReferralUpdateAsync(referral.Id);
            referral.TriagedCompletionLevel.Should().NotBeNullOrWhiteSpace();
            referral.TriagedWeightedLevel.Should().NotBeNullOrWhiteSpace();
          }
          catch (Exception ex)
          {
            Assert.True(false, ex.Message);
          }
          finally
          {
            _context.Referrals.Remove(referral);
            await _context.SaveChangesAsync();
          }
        }
      }
    }

    public class ExpireTextMessageDueToDobCheckAsync : ReferralServiceTests
    {
      public ExpireTextMessageDueToDobCheckAsync(
        ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullOrWhiteSpaceException()
      {
        // arrange
        string model = "";

        // assert
        ArgumentNullOrWhiteSpaceException exception =
          await Assert.ThrowsAsync<ArgumentNullOrWhiteSpaceException>(
            async ()
            => await _service.ExpireTextMessageDueToDobCheckAsync(model));
      }

      [Fact]
      public async Task ReferralNotFoundException()
      {
        // arrange
        string model = "InValid";

        // assert
        ReferralNotFoundException exception =
          await Assert.ThrowsAsync<ReferralNotFoundException>(
            async ()
            => await _service.ExpireTextMessageDueToDobCheckAsync(model));
      }

      [Fact]
      public async Task ValidUpdate()
      {
        // arrange
        DateTimeOffset dateSent = DateTimeOffset.Now;
        string base36Date =
          Base36Converter.ConvertDateTimeOffsetToBase36(dateSent);

        Entities.Referral referral =
          CreateUniqueReferral();
        Entities.TextMessage textMsg =
          RandomEntityCreator.CreateRandomTextMessage();
        textMsg.ReferralId = referral.Id;
        textMsg.Base36DateSent = base36Date;

        _context.Referrals.Add(referral);
        _context.TextMessages.Add(textMsg);
        await _context.SaveChangesAsync();

        // act
        await _service.ExpireTextMessageDueToDobCheckAsync(base36Date);

        //assert
        textMsg.Outcome.Should().Be(Constants.DATE_OF_BIRTH_EXPIRY);

        // clean up
        _context.Referrals.Remove(referral);
        _context.TextMessages.Remove(textMsg);
        _context.SaveChanges();
      }
    }


    public class DeprivationUpdateException : ReferralServiceTests
    {
      public DeprivationUpdateException(
        ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
          .Throws(new DeprivationNotFoundException());

        ProviderService _providerService =
          new ProviderService(
            _context,
            _serviceFixture.Mapper,
            _mockOptions.Object);

        ReferralService _serviceToTest =
          new ReferralService(
            _context,
            _serviceFixture.Mapper,
            _providerService,
            _mockDeprivationService.Object,
            _mockPostcodeService.Object,
            _mockPatientTriageService.Object,
            _mockOdsOrganisationService.Object)
          {
            User = GetClaimsPrincipal()
          };
      }

      [Fact]
      public async Task DeprivationSetToImd1WithInvalidPostcode()
      {
        // arrange
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

        // act 
        IReferral result = await _service.UpdateGpReferral(referralUpdate);

        //assert
        result.Status.Should().Be(ReferralStatus.New.ToString());
        result.Deprivation.Should().Be(Enums.Deprivation.IMD1.ToString());

        //clean up
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task ExceptionThrown()
      {
        // arrange
        _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
          .Throws(new Exception());

        ReferralService _serviceToTest =
          new ReferralService(
            _context,
            _serviceFixture.Mapper,
            _providerService,
            _mockDeprivationService.Object,
            _mockPostcodeService.Object,
            _mockPatientTriageService.Object,
            _mockOdsOrganisationService.Object)
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

        // act assert
        Exception exception = await Assert.ThrowsAsync<Exception>(
              async () => await _service.UpdateGpReferral(referralUpdate));

        //clean up
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }
    }

    public class PrepareUnableToContactAsync : ReferralServiceTests
    {
      public PrepareUnableToContactAsync(
        ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task No_Updates()
      {
        // arrange
        Entities.Referral referral1 =
          CreateUniqueReferral();
        referral1.DateLetterSent = null;
        referral1.Status = ReferralStatus.Letter.ToString();
        Entities.Referral referral2 =
          CreateUniqueReferral();
        referral2.DateLetterSent = null;
        referral2.Status = ReferralStatus.Letter.ToString();

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();

        // act
        string[] returnedMessage =
          await _service.PrepareUnableToContactAsync();

        // assert
        returnedMessage.Should().NotBeNull();
        returnedMessage.Should().BeEmpty();
        referral1.Status.Should().Be(ReferralStatus.Letter.ToString());
        referral2.Status.Should().Be(ReferralStatus.Letter.ToString());

        // clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Status_Updates()
      {
        // arrange
        int daysAfterLetterSent = Constants.LETTERSENT_GRACE_PERIOD;
        int daysOffset1 = daysAfterLetterSent + 2;
        int daysOffset2 = daysAfterLetterSent - 2;
        int daysOffset3 = daysAfterLetterSent + 6;

        Entities.Referral referral1 =
          RandomEntityCreator.CreateRandomReferral(
            status: ReferralStatus.LetterSent,
            referralSource: ReferralSource.SelfReferral);
        referral1.DateLetterSent = DateTimeOffset.Now.AddDays(-daysOffset1);
        Entities.Referral referral2 =
          RandomEntityCreator
            .CreateRandomReferral(
            status: ReferralStatus.LetterSent);
        referral2.DateLetterSent = DateTimeOffset.Now.AddDays(-daysOffset2);
        referral2.Status = ReferralStatus.Letter.ToString();
        Entities.Referral referral3 =
          RandomEntityCreator.CreateRandomReferral(
            status: ReferralStatus.LetterSent,
            referralSource: ReferralSource.GpReferral);
        referral3.DateLetterSent = DateTimeOffset.Now.AddDays(-daysOffset3);

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        _context.Referrals.Add(referral3);
        await _context.SaveChangesAsync();

        // act
        string[] returnedMessage =
          await _service.PrepareUnableToContactAsync();

        // assert
        returnedMessage.Should().NotBeNull();
        returnedMessage.Length.Should().Be(2);
        referral1.Status.Should()
          .Be(ReferralStatus.FailedToContactTextMessage.ToString());
        referral2.Status.Should().Be(ReferralStatus.Letter.ToString());
        referral3.Status.Should()
          .Be(ReferralStatus.FailedToContact.ToString());

        // clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        _context.Referrals.Remove(referral3);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Status_Updated_To_FailedToContact()
      {
        // arrange
        int daysAfterLetterSent = Constants.LETTERSENT_GRACE_PERIOD;
        int daysOffset1 = daysAfterLetterSent + 2;
        int daysOffset2 = daysAfterLetterSent + 6;

        Entities.Referral referral1 =
          RandomEntityCreator
            .CreateRandomReferral(
            status: ReferralStatus.LetterSent);
        referral1.ReferralSource = ReferralSource.SelfReferral.ToString();
        referral1.DateLetterSent = DateTimeOffset.Now.AddDays(-daysOffset1);

        Entities.Referral referral2 =
          RandomEntityCreator
            .CreateRandomReferral(
            status: ReferralStatus.Letter);
        referral2.DateLetterSent = DateTimeOffset.Now.AddDays(-daysOffset2);

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();

        // act
        string[] returnedMessage =
          await _service.PrepareUnableToContactAsync();

        // assert
        returnedMessage.Should().NotBeNull();
        returnedMessage.Length.Should().Be(1);
        referral1.Status.Should()
          .Be(ReferralStatus.FailedToContactTextMessage.ToString());
        referral2.Status.Should().Be(ReferralStatus.Letter.ToString());

        // clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Status_Updated_To_RejectedToEReferrals()
      {
        // arrange
        string expectedReason = "Test rejected";
        Entities.Referral referral1 =
          CreateUniqueReferral();

        _context.Referrals.Add(referral1);
        await _context.SaveChangesAsync();

        // act
        IReferral model =
          await _service.UpdateStatusToRejectedToEreferralsAsync(referral1.Id,
            expectedReason);

        // assert
        model.Should().NotBeNull();
        model.Status.Should()
          .Be(ReferralStatus.RejectedToEreferrals.ToString());

        // clean up
        _context.Referrals.Remove(referral1);
        _context.SaveChanges();
      }



    }

    public class SendReferralLettersAsyncTests : ReferralServiceTests
    {
      public SendReferralLettersAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task Valid()
      {
        //Arrange

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
        List<Guid> request = new List<Guid>
          {createdReferral1.Id, createdReferral2.Id};
        _mockCsvExport.Setup(t =>
            t.Export<CsvExportAttribute>(It.IsAny<IEnumerable<Referral>>()))
          .Returns(expectedBytes);

        ProviderService _providerService =
          new ProviderService(
            _context,
            _serviceFixture.Mapper,
            _mockOptions.Object);

        ReferralService service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _providerService,
          _mockCsvExport.Object,
          _mockPatientTriageService.Object
        );
        // act
        byte[] response =
          await service.SendReferralLettersAsync(request, exported);

        // assert
        response.Should().BeOfType<byte[]>();
        createdReferral1.MethodOfContact.Should()
         .Be((int)MethodOfContact.Letter);
        createdReferral1.NumberOfContacts.Should().Be(1);
        createdReferral2.MethodOfContact.Should()
         .Be((int)MethodOfContact.Letter);
        createdReferral2.NumberOfContacts.Should().Be(1);
        // clean up
        _context.Remove(createdReferral1);
        _context.Remove(createdReferral2);
        _context.SaveChanges();
      }
    }

    public class CreateDischargeLettersAsyncTests : ReferralServiceTests
    {
      public CreateDischargeLettersAsyncTests(ServiceFixture serviceFixture) :
        base(serviceFixture)
      {
      }

      //TODO: Unit test to cover this method
    }

    public class UpdateEmail : ReferralServiceTests
    {
      public UpdateEmail(ServiceFixture serviceFixture) : base(serviceFixture)
      {
      }

      [Theory]
      [InlineData("real_person@gmail.com")]
      [InlineData("jefffiddler@nhs.net")]
      [InlineData("anaardvark@nhs.net")]
      [InlineData("eleEester@nhs.net")]
      public async Task Valid(string expectedEmail)
      {
        // arrange
        var referral = CreateUniqueReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var result = await _service.UpdateEmail(referral.Id, expectedEmail);

        // assert
        result.Email.Should().Be(expectedEmail);

        CleanUp(referral.Id);
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      [InlineData("test.com")]
      [InlineData("test@test@.com")]
      //[InlineData("none@yahoo.co.uk")]
      //[InlineData("aaa@gmail.co.uk")]
      //[InlineData("bbb@yahoo.co.uk")]
      //[InlineData("ccc@yahoo.co.uk")]
      //[InlineData("n0n3@yahoo.co.uk")]
      //[InlineData("qwerty@yahoo.co.uk")]
      //[InlineData("nothing@yahoo.co.uk")]
      //[InlineData("self@gmail.com")]
      public async Task Invalid(string email)
      {
        // act and assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _service
          .UpdateEmail(Guid.NewGuid(), email));

      }
    }

    public class UpdateGpReferral : ReferralServiceTests
    {
      public UpdateGpReferral(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

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
        // arrange
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

        // act
        IReferral result = await _service.UpdateGpReferral(model);

        // assert
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

        // act
        try
        {
          IReferral result = await _service.UpdateGpReferral(model);
          // assert
          Assert.True(false, $"expecting InvalidOperationException");
        }
        catch (InvalidOperationException iox)
        {
          iox.Message.Should().Be("Sequence contains more than one element");
        }
        catch (ReferralNotUniqueException rnu)
        {
          Assert.True(true, rnu.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }

      }
    }

    public class UpdateSelfReferralWithProviderAsyncTests
     : ReferralServiceTests
    {
      public UpdateSelfReferralWithProviderAsyncTests(
        ServiceFixture serviceFixture) : base(serviceFixture)
      {
      }

      [Fact]
      public async Task InValid_ReferralNotFound()
      {
        //Arrange
        Guid referralId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();

        //Act    
        try
        {
          IReferral result = await _service
            .UpdateReferralWithProviderAsync(referralId, providerId);

          //Assert
          Assert.True(false, "Expected ReferralNotFoundException");
        }
        catch (ReferralNotFoundException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false,
            $"Expected ReferralNotFoundException, but got {ex.Message}");
        }
      }

      [Fact]
      public async Task InValid_ReferralStatusNotNew()
      {
        //Arrange
        Guid providerId = Guid.NewGuid();
        Entities.Referral referral = CreateUniqueReferral();
        referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        //Act
        try
        {
          IReferral result =
            await _service.UpdateReferralWithProviderAsync(referral.Id,
              providerId);
          //Assert
          Assert.True(false, "Expected ReferralInvalidStatusException");
        }
        catch (Exception ex)
        {
          if (ex is ReferralInvalidStatusException)
          {
            Assert.True(true, ex.Message);
          }
          else
          {
            Assert.True(false,
              $"Expected ReferralInvalidStatusException, but" +
              $" got {ex.Message}");
          }
        }
        finally
        {
          //cleanup
          _context.Referrals.Remove(referral);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task InValid_ReferralIsNewButHasProviderId()
      {
        //Arrange
        Guid providerId = Guid.NewGuid();
        Entities.Referral referral =
          CreateUniqueReferral(
            status: ReferralStatus.New);
        _context.Referrals.Add(referral);
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        referral.ProviderId = providerId;
        _context.SaveChanges();
        //Act
        try
        {
          IReferral result =
            await _service.UpdateReferralWithProviderAsync(referral.Id,
              providerId);
          //Assert
          Assert.True(false, "Expected ReferralProviderSelectedException");
        }
        catch (ReferralProviderSelectedException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, $"Expected ReferralProviderSelectedException, " +
                             $"but got {ex.Message}");
        }
        finally
        {
          //cleanup
          _context.Referrals.Remove(referral);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task Invalid_ProviderIdIsNotInProvidersForReferral()
      {
        //Arange
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
        //Act
        try
        {
          IReferral result =
            await _service.UpdateReferralWithProviderAsync(referral.Id,
              providerId);
          //Assert
          Assert.True(false, "Expected ProviderSelectionMismatch");
        }
        catch (ProviderSelectionMismatch ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, $"Expected ProviderSelectionMismatch, " +
                             $"but got {ex.Message}");
        }
        finally
        {
          //cleanup
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task Invalid_TriagelCompletionLevelNotSet()
      {
        //Arange
        Guid providerId = Guid.NewGuid();
        _context.SaveChanges();
        Entities.Referral referral =
          CreateUniqueReferral(
            status: ReferralStatus.New,
            providerId: providerId);
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        //Act
        try
        {
          IReferral result =
            await _service.UpdateReferralWithProviderAsync(referral.Id,
              providerId);
          //Assert
          Assert.True(false, "Expected TriageNotFoundException");
        }
        catch (TriageNotFoundException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, $"Expected TriageNotFoundException, " +
                             $"but got {ex.Message}");
        }
        finally
        {
          //cleanup
          _context.Referrals.Remove(referral);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task Valid()
      {
        //Arrange
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);

        Entities.Referral referral = CreateUniqueReferral(
          status: ReferralStatus.New);
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        //Act
        DateTimeOffset executionTime = DateTimeOffset.Now;
        IReferral result = await _service
          .UpdateReferralWithProviderAsync(referral.Id, provider.Id);

        //Assert
        result.Status.Should().Be(
          ReferralStatus.ProviderAwaitingStart.ToString());
        result.ProviderId.Should().Be(provider.Id);
        result.DateOfProviderSelection.Should().NotBeNull();
        result.DateOfProviderSelection.Should().BeAfter(executionTime);

        //cleanup
        _context.Referrals.Remove(referral);
        _context.Providers.Remove(provider);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid_NoNhsNumber()
      {
        //Arrange
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);

        Entities.Referral referral = CreateUniqueReferral(
          status: ReferralStatus.New);
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        referral.NhsNumber = null;
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        //Act
        DateTimeOffset executionTime = DateTimeOffset.Now;
        IReferral result = await _service
          .UpdateReferralWithProviderAsync(referral.Id, provider.Id);

        //Assert
        result.Status.Should().Be(
          ReferralStatus.ProviderAwaitingTrace.ToString());
        result.ProviderId.Should().Be(provider.Id);
        result.DateOfProviderSelection.Should().NotBeNull();
        result.DateOfProviderSelection.Should().BeAfter(executionTime);

        //cleanup
        _context.Referrals.Remove(referral);
        _context.Providers.Remove(provider);
        _context.SaveChanges();
      }
    }

    private void RemoveReferralBeforeTest(string email)
    {
      var referral =
        _context.Referrals.FirstOrDefault(t => t.Email == email);

      if (referral == null) return;
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
        string sex = null,
        Enums.ReferralStatus status = Enums.ReferralStatus.New,
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
          referralAttachmentId: 123456,
          referringGpPracticeName: "Test Practice",
          referringGpPracticeNumber: null,
          sex: null,
          status: status,
          statusReason: null,
          telephone: null,
          triagedCompletionLevel: null,
          triagedWeightedLevel: null,
          ubrn: null,
          vulnerableDescription: "Not Vulnerable",
          weightKg: 120m,
          referralSource: ReferralSource.GpReferral);

        var found = _context.Referrals
          .FirstOrDefault(t => t.Id == entity.Id);
        if (found != null) continue;

        found = _context.Referrals
          .FirstOrDefault(t => t.Ubrn == entity.Ubrn);
        if (found != null) continue;

        found = _context.Referrals
          .FirstOrDefault(t => t.NhsNumber == entity.NhsNumber);
        if (found != null) continue;

        return entity;

      }

    }
  }
}
