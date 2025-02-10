using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;
using Xunit.Abstractions;
using Deprivation = WmsHub.Business.Models.Deprivation;
using IReferral = WmsHub.Business.Models.IReferral;
using Provider = WmsHub.Business.Entities.Provider;

namespace WmsHub.Referral.Api.Tests;

[Collection("Service collection")]
public class PharmacyReferralControllerTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly IReferralService _referralService;
  private readonly IProviderService _providerService;
  private readonly PharmacyReferralController _controller;

  private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
    new Mock<IOptions<ProviderOptions>>();
  private readonly GpDocumentProxyOptions _gpDocumentProxyOptions = new();

  private readonly Deprivation _mockDeprivationValue = new Deprivation
  { ImdDecile = 6, Lsoa = "E00000001" };

  private readonly Mock<IDeprivationService> _mockDeprivationService =
    new Mock<IDeprivationService>();

  private readonly Mock<ILinkIdService> _mockLinkIdService = new();

  private readonly Mock<IPostcodesIoService> _mockPostcodeIoService =
    new Mock<IPostcodesIoService>();

  private Mock<CourseCompletionResult> _mockScoreResult = new();

  private readonly Mock<IPatientTriageService> _mockPatientTriageService =
    new Mock<IPatientTriageService>();
  private readonly Mock<IOptions<GpDocumentProxyOptions>>
    _mockGpDocumentProxyOptions = new();

  private readonly Mock<PharmacyReferralOptions> _mockPHSettings = new();
  private readonly Mock<IOptions<PharmacyReferralOptions>> _mockPHOptions =
    new();
  private readonly Mock<IOptions<ReferralTimelineOptions>> _mockReferralTimelineOptions = new();

  private const string PHARMACIST_EMAIL = "pharma01@nhs.net";

  public PharmacyReferralControllerTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _mockPHOptions.Setup(t => t.Value).Returns(_mockPHSettings.Object);
    _context = new DatabaseContext(_serviceFixture.Options);

    _mockDeprivationService.Setup(x => x.GetByLsoa(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue);

    _mockPostcodeIoService.Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue.Lsoa);

    ProviderOptions _options = new ProviderOptions
    {
      CompletionDays = 84,
      NumDaysPastCompletedDate = 10
    };
    _mockOptions.Setup(x => x.Value).Returns(_options);

    _providerService = new ProviderService(_context,
      _serviceFixture.Mapper,
      _mockOptions.Object)
    {
      User = GetClaimsPrincipal()
    };

    _mockScoreResult.Setup(t => t.TriagedCompletionLevel)
      .Returns(TriageLevel.High);

    _mockScoreResult.Setup(t => t.TriagedWeightedLevel)
      .Returns(TriageLevel.Medium);

    _mockPatientTriageService.Setup(t =>
        t.GetScores(It.IsAny<CourseCompletionParameters>()))
      .Returns(_mockScoreResult.Object);

    _mockGpDocumentProxyOptions.Setup(x => x.Value)
      .Returns(_gpDocumentProxyOptions);

    _referralService = new ReferralService(
      _context,
      _serviceFixture.Mapper,
      _providerService,
      _mockDeprivationService.Object,
      _mockLinkIdService.Object,
      _mockPostcodeIoService.Object,
      _mockPatientTriageService.Object,
      null,
      _mockGpDocumentProxyOptions.Object,
      _mockReferralTimelineOptions.Object,
      null,
      _log)
    {
      User = GetClaimsPrincipal()
    };

    _mockPHSettings.Object.Emails = new[] { PHARMACIST_EMAIL };
    _mockPHSettings.Object.IsWhitelistEnabled = false;

    _controller = new PharmacyReferralController(
      _referralService,
      _mockPHOptions.Object,
      _serviceFixture.Mapper);

    ClaimsPrincipal user = new ClaimsPrincipal(
      new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.Name, "PharmacyReferral.Service")
        },
        "TestAuthentication"));

    _controller.ControllerContext = new ControllerContext
    {
      HttpContext =
        new DefaultHttpContext { User = user }
    };
  }
  public class PostTests : PharmacyReferralControllerTests
  {
    private Pharmacist _pharmacist;

    public PostTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.StaffRoles.RemoveRange(_context.StaffRoles);
      _context.StaffRoles.Add(new Business.Entities.StaffRole()
      {
        DisplayName = STAFF_ROLE__AMBULANCE_STAFF,
        DisplayOrder = 1
      });

      _context.Pharmacists.RemoveRange(_context.Pharmacists);
      _pharmacist = new Pharmacist
      {
        Expires = DateTimeOffset.Now.AddMinutes(10),
        Id = Guid.NewGuid(),
        IsActive = true,
        ReferringPharmacyEmail = PHARMACIST_EMAIL,
        KeyCode = "Ab123!",
        ModifiedAt = DateTimeOffset.UtcNow,
        ModifiedByUserId = Guid.Empty
      };
      _context.Pharmacists.Add(_pharmacist);

      Pharmacist pharmacist = new()
      {
        Expires = DateTimeOffset.Now.AddMinutes(10),
        Id = Guid.NewGuid(),
        IsActive = true,
        ReferringPharmacyEmail = "notinwhitelist@nhs.net",
        KeyCode = "cd456!",
        ModifiedAt = DateTimeOffset.UtcNow,
        ModifiedByUserId = Guid.Empty
      };
      _context.Pharmacists.Add(pharmacist);        

      _context.Referrals.RemoveRange(_context.Referrals);

      _context.SaveChanges();
    }

    [Fact]
    public async Task Valid_WhitelistEnabled()
    {
      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator.CreatePharmacyReferralPostRequest(
        serviceUserEthnicity: ETHNICITY__IRISH,
        serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE);

      request.ReferringPharmacyEmail = _pharmacist.ReferringPharmacyEmail;
      request.HasDiabetesType2 = false;
      request.HasDiabetesType1 = true;
      request.ReferringGpPracticeNumber = Generators.GenerateGpPracticeNumber(new());

      _mockPHSettings.Object.IsWhitelistEnabled = true;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      Assert.NotNull(result);
      Assert.IsType<OkObjectResult>(result);
      
      // TODO - complete asserts for all properties
      IReferral referral = (IReferral)((ObjectResult)result).Value;
      Business.Entities.Referral entity = _context.Referrals
        .FirstOrDefault(t => t.Id == referral.Id);

      entity.Status.Should().Be(ReferralStatus.New.ToString());
    }

    [Fact]
    public async Task Valid_WhitelistDisabled()
    {
      // Arrange.
      Random rnd = new Random();
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );
      request.ReferringPharmacyEmail = "notinwhitelist@nhs.net";
      request.HasDiabetesType2 = false;
      request.HasDiabetesType1 = true;
      request.ReferringGpPracticeNumber =
        Generators.GenerateGpPracticeNumber(rnd);
      

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      // Act.
      _mockPHSettings.Object.IsWhitelistEnabled = false;
      IActionResult result = await _controller.Post(request);

      // Assert.
      Assert.NotNull(result);
      Assert.IsType<OkObjectResult>(result);
      // TODO - complete asserts for all properties
      IReferral referral = (IReferral)((ObjectResult)result).Value;
      Business.Entities.Referral entity =
        _context.Referrals.FirstOrDefault(t => t.Id == referral.Id);
      entity.Status.Should().Be(ReferralStatus.New.ToString());        
    }

    [Fact]
    public async Task FailAll()
    {

      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );
      request.Email = null;
      request.HasHypertension = false;
      request.HasDiabetesType1 = false;
      request.HasDiabetesType2 = false;
      request.HeightCm = 49m;
      request.WeightKg = 34;
      request.CalculatedBmiAtRegistration = 20;
      request.DateOfBmiAtRegistration = DateTimeOffset.Now.AddMonths(-25);
      request.DateOfBirth = DateTimeOffset.Now.AddYears(-17);
      request.Ethnicity = null;
      request.NhsNumber = null;
      request.ReferringGpPracticeName = null;
      request.ReferringGpPracticeNumber = null;
      request.ConsentForGpAndNhsNumberLookup = false;
      request.ConsentForReferrerUpdatedWithOutcome = null;
      request.ReferringPharmacyEmail = "biscoff@nhs.net";

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();
      // Act.

      IActionResult result = await _controller.Post(request);
      // Assert.
      result.Should().NotBeNull();
      Assert.IsType<ValidationProblemDetails>(((ObjectResult)result).Value);
      // TODO - complete asserts for all properties
      ValidationProblemDetails problem =
        (ValidationProblemDetails)((ObjectResult)result).Value;
      problem.Errors["Email"][0].Should().Be(
        "The Email field is required.");
      problem.Errors["ReferringGpPracticeName"][0].Should().Be(
        "The ReferringGpPracticeName field is required.");
      problem.Errors["ReferringGpPracticeNumber"][0].Should().Be(
        "The ReferringGpPracticeNumber field is required.");
      problem.Errors["NhsNumber"][0].Should().Be(
        "The NhsNumber field is required.");
      problem.Errors["Ethnicity"][0].Should().Be(
        "The Ethnicity field is required.");
      problem.Errors["HeightCm"][0].Should().Be(
        "The field HeightCm must be between 50 and 250.");
      problem.Errors["WeightKg"][0].Should().Be(
        "The field WeightKg must be between 35 and 500.");
      problem.Errors["CalculatedBmiAtRegistration"][0].Should().Be(
        "The field CalculatedBmiAtRegistration must be between 27.5 and 90.");
      problem.Errors["HasDiabetesType1, HasDiabetesType2, HasHypertension"]
        [0].Should().Be(
          "The HasDiabetesType1, HasDiabetesType2, HasHypertension " +
          "field 'A diagnosis of Diabetes Type 1 or Diabetes Type 2 or " +
          "Hypertension is required.' is invalid.");
      problem.Errors["ConsentForReferrerUpdatedWithOutcome"][0].Should()
        .Be("The ConsentForReferrerUpdatedWithOutcome field is required.");
      problem.Errors["ConsentForGpAndNhsNumberLookup"][0].Should().Be(
        "The ConsentForGpAndNhsNumberLookup field 'Consent is required to" +
        " continue with this referral.' is invalid.");
      problem.Errors["ReferringPharmacyEmailIsValid"][0].Should().Be(
        $"Email {request.ReferringPharmacyEmail} is not in pharmacy list");
      ((ObjectResult)result).StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task FailWeightHeightHigh()
    {

      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );
      request.HeightCm = 251m;
      request.WeightKg = 501m;
      request.CalculatedBmiAtRegistration = 30;
      request.ReferringPharmacyEmail = "test@nhs.net";

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();
      // Act.

      IActionResult result = await _controller.Post(request);
      // Assert.
      result.Should().NotBeNull();
      Assert.IsType<ValidationProblemDetails>(((ObjectResult)result).Value);
      // TODO - complete asserts for all properties
      ValidationProblemDetails problem =
        (ValidationProblemDetails)((ObjectResult)result).Value;
      problem.Errors["HeightCm"][0].Should().Be(
        "The field HeightCm must be between 50 and 250.");
      problem.Errors["WeightKg"][0].Should().Be(
        "The field WeightKg must be between 35 and 500.");
      ((ObjectResult)result).StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task FailEmail_NotInWhitelist()
    {

      // Arrange.
      string email = "wrongmail@nhs.net";

      // Act.
      _mockPHSettings.Object.IsWhitelistEnabled = true;
      IActionResult result = await _controller.GetKey(email);
      _mockPHSettings.Object.IsWhitelistEnabled = false;

      // Assert.
      result.Should().NotBeNull();
      Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);

      // TODO - complete asserts for all properties
      ProblemDetails problem =
        (ProblemDetails)((ObjectResult)result).Value;
      ((ObjectResult)result).StatusCode.Should().Be(403);
      problem.Detail.Should().Be(
        $"Email {email} is not in the pharmacy whitelist.");
    }

    [Fact]
    public async Task FailEmail_WrongDomain()
    {

      // Arrange.
      string email = "wrongmail@nhs.uk";
      // Act.

      IActionResult result = await _controller.GetKey(email);
      // Assert.
      result.Should().NotBeNull();
      Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);
      // TODO - complete asserts for all properties
      ProblemDetails problem =
        (ProblemDetails)((ObjectResult)result).Value;
      ((ObjectResult)result).StatusCode.Should().Be(422);
      problem.Detail.Should().Be(
        "Only emails from the domain @nhs.net are allowed.");
    }

    [Fact]
    public async Task Fail_Not_NhsNumber()
    {
      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE);

      request.NhsNumber = "abc123465";

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();
      
      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().NotBeNull();
      Assert.IsType<ValidationProblemDetails>(((ObjectResult)result).Value);
      // TODO - complete asserts for all properties
      ValidationProblemDetails problem =
        (ValidationProblemDetails)((ObjectResult)result).Value;

      problem.Errors["NhsNumber"][0].Should().Be(
        "The field NhsNumber must be 10 numbers only.");
    }

    [Fact]
    public async Task NhsNumber_ExistingNotCancelled_409()
    {
      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );

      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        nhsNumber: request.NhsNumber,
        status: ReferralStatus.New);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status409Conflict);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be("Referral cannot be created " +
          "because there are in progress referrals with the same NHS number: " +
          $"(UBRN {referral.Ubrn}).");
    }

    [Fact]
    public async Task NhsNumber_ExistingCancelledProviderSelected_409()
    {
      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );

      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now,
        nhsNumber: request.NhsNumber,
        providerId: Guid.NewGuid(),
        status: ReferralStatus.CancelledByEreferrals);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      string minDateSinceDateOfProviderSelection = referral
        .DateOfProviderSelection
        .Value
        .AddDays(Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION + 1)
        .Date
        .ToString("yyyy-MM-dd");

      string expectedErrorMessage = "Referral can be created from " +
        minDateSinceDateOfProviderSelection +
        " as an existing referral for this NHS number " +
        $"(UBRN {referral.Ubrn}) selected a provider but did not start " +
        "the programme.";

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status409Conflict);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task NhsNumber_ExistingCancelledProviderNotSelected_200()
    {
      // Arrange.
      PharmacyReferralPostRequest request = RandomModelCreator
        .CreatePharmacyReferralPostRequest(
          referringPharmacyEmail: PHARMACIST_EMAIL,
          serviceUserEthnicity: ETHNICITY__IRISH,
          serviceUserEthnicityGroup: ETHNICITY_GROUP__WHITE
        );

      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        nhsNumber: request.NhsNumber,
        status: ReferralStatus.CancelledByEreferrals);

      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      OkObjectResult okObjectResult = result.Should().BeOfType<OkObjectResult>().Subject;
      okObjectResult.StatusCode.Should().Be(StatusCodes.Status200OK);

      Business.Models.Referral createdReferral = 
        okObjectResult.Value.Should().BeOfType<Business.Models.Referral>().Subject;
      createdReferral.Status.Should().Be(ReferralStatus.New.ToString());
      createdReferral.Should().BeEquivalentTo(request, options => options
        .Excluding(x => x.ReferringPharmacyEmail)
        .Excluding(x => x.ReferringPharmacyOdsCode));
      createdReferral.ReferringOrganisationEmail.Should()
        .Be(request.ReferringPharmacyEmail);
      createdReferral.ReferringOrganisationOdsCode.Should()
        .Be(request.ReferringPharmacyOdsCode);
    }
  }
}