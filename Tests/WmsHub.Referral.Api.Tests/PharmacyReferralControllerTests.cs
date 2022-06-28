using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;
using Xunit.Abstractions;
using Deprivation = WmsHub.Business.Models.Deprivation;
using IReferral = WmsHub.Business.Models.IReferral;
using Provider = WmsHub.Business.Entities.Provider;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public class PharmacyReferralControllerTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IReferralService _referralService;
    private readonly IProviderService _providerService;
    private readonly PharmacyReferralController _controller;

    private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
      new Mock<IOptions<ProviderOptions>>();

    private readonly Deprivation _mockDeprivationValue = new Deprivation
    { ImdDecile = 6, Lsoa = "E00000001" };

    private readonly Mock<IDeprivationService> _mockDeprivationService =
      new Mock<IDeprivationService>();

    private readonly Mock<IPostcodeService> _mockPostcodeService =
      new Mock<IPostcodeService>();

    private Mock<CourseCompletionResult> _mockScoreResult = new();

    private readonly Mock<IPatientTriageService> _mockPatientTriageService =
      new Mock<IPatientTriageService>();

    private readonly Mock<PharmacyReferralOptions> _mockPHSettings = new();
    private readonly Mock<IOptions<PharmacyReferralOptions>> _mockPHOptions =
      new();

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

      _mockPostcodeService.Setup(x => x.GetLsoa(It.IsAny<string>()))
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

      _referralService = new ReferralService(
        _context,
        _serviceFixture.Mapper,
        _providerService,
        _mockDeprivationService.Object,
        _mockPostcodeService.Object,
        _mockPatientTriageService.Object,
        null)
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
      private Provider _provider;

      public PostTests(
        ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      {
        _context.Ethnicities.RemoveRange(_context.Ethnicities);
        _context.Ethnicities.Add(new Business.Entities.Ethnicity()
        {
          DisplayName = ETHNICITY__IRISH,
          DisplayOrder = 1,
          GroupOrder = 1,
          GroupName = ETHNICITY_GROUP__WHITE,
          IsActive = true,
          TriageName = Business.Enums.Ethnicity.White.ToString(),
          MinimumBmi = 30
        });
        _context.Ethnicities.Add(new Business.Entities.Ethnicity()
        {
          DisplayName = "Ethnicity2",
          DisplayOrder = 2,
          GroupOrder = 1,
          IsActive = true
        });

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

        var pharmacist = new Pharmacist
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

        _context.Providers.RemoveRange(_context.Providers);
        _provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(_provider);

        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid_WhitelistEnabled()
      {
        // ARRANGE
        Random rnd = new Random();
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();
        request.ReferringPharmacyEmail = _pharmacist.ReferringPharmacyEmail;
        request.HasDiabetesType2 = false;
        request.HasDiabetesType1 = true;
        request.ReferringGpPracticeNumber =
          Generators.GenerateGpPracticeNumber(rnd);

        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();

        // ACT
        _mockPHSettings.Object.IsWhitelistEnabled = true;
        IActionResult result = await _controller.Post(request);
        _mockPHSettings.Object.IsWhitelistEnabled = false;

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        // TODO - complete asserts for all properties
        IReferral referral = (IReferral)((ObjectResult)result).Value;
        Business.Entities.Referral entity =
          _context.Referrals.FirstOrDefault(t => t.Id == referral.Id);
        entity.Status.Should().Be(ReferralStatus.New.ToString());
      }

      [Fact]
      public async Task Valid_WhitelistDisabled()
      {
        // ARRANGE
        Random rnd = new Random();
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();
        request.ReferringPharmacyEmail = "notinwhitelist@nhs.net";
        request.HasDiabetesType2 = false;
        request.HasDiabetesType1 = true;
        request.ReferringGpPracticeNumber =
          Generators.GenerateGpPracticeNumber(rnd);
        

        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();

        // ACT
        _mockPHSettings.Object.IsWhitelistEnabled = false;
        IActionResult result = await _controller.Post(request);

        // ASSERT
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

        // ARRANGE
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();
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
        // ACT

        IActionResult result = await _controller.Post(request);
        // Assert
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

        // ARRANGE
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();
        request.HeightCm = 251m;
        request.WeightKg = 501m;
        request.CalculatedBmiAtRegistration = 30;
        request.ReferringPharmacyEmail = "test@nhs.net";

        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();
        // ACT

        IActionResult result = await _controller.Post(request);
        // Assert
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

        // ARRANGE
        string email = "wrongmail@nhs.net";

        // ACT
        _mockPHSettings.Object.IsWhitelistEnabled = true;
        IActionResult result = await _controller.GetKey(email);
        _mockPHSettings.Object.IsWhitelistEnabled = false;

        // Assert
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

        // ARRANGE
        string email = "wrongmail@nhs.uk";
        // ACT

        IActionResult result = await _controller.GetKey(email);
        // Assert
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

        // ARRANGE
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();
        request.NhsNumber = "abc123465";

        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();
        // ACT

        IActionResult result = await _controller.Post(request);
        // Assert
        result.Should().NotBeNull();
        Assert.IsType<ValidationProblemDetails>(((ObjectResult)result).Value);
        // TODO - complete asserts for all properties
        ValidationProblemDetails problem =
          (ValidationProblemDetails)((ObjectResult)result).Value;
        problem.Errors["NhsNumber"][0].Should().Be(
          "The NhsNumber must be 10 numbers only, " +
          "remove any spaces or dashes.");
      }

      [Fact]
      public async Task NhsNumber_ExistingNotCancelled_409()
      {
        // ARRANGE
        PharmacyReferralPostRequest request =
          CreatePharmacyReferralPostRequest();

        var referral = RandomEntityCreator.CreateRandomReferral(
          nhsNumber: request.NhsNumber,
          status: ReferralStatus.New);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Detail.Should().Be("Referral cannot be created " +
          "because there is an existing referral with the same NHS number: " +
          $"'{referral.Id}'.");
      }

      [Fact]
      public async Task NhsNumber_ExistingCancelledProviderSelected_409()
      {
        // ARRANGE
        var request = CreatePharmacyReferralPostRequest();        

        var referral = RandomEntityCreator.CreateRandomReferral(
          nhsNumber: request.NhsNumber,
          providerId: _provider.Id,
          status: ReferralStatus.CancelledByEreferrals);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Detail.Should().Be("Referral cannot be created " +
          "because there is an existing referral with the same NHS number: " +
          $"'{referral.Id}'.");
      }

      [Fact]
      public async Task NhsNumber_ExistingCancelledProviderNotSelected_200()
      {
        // ARRANGE
        var request = CreatePharmacyReferralPostRequest();

        var referral = RandomEntityCreator.CreateRandomReferral(
          nhsNumber: request.NhsNumber,
          status: ReferralStatus.CancelledByEreferrals);

        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        okObjectResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var createdReferral = Assert
          .IsType<Business.Models.Referral>(okObjectResult.Value);        
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

    private PharmacyReferralPostRequest CreatePharmacyReferralPostRequest()
    {
      Random rnd = new Random();
      return new PharmacyReferralPostRequest()
      {
        ReferringGpPracticeNumber = Generators.GenerateGpPracticeNumber(rnd),
        ReferringGpPracticeName = "Test",
        ReferringPharmacyEmail = PHARMACIST_EMAIL,
        ReferringPharmacyOdsCode = "abcd1234",
        NhsNumber = Generators.GenerateNhsNumber(rnd),
        ConsentForGpAndNhsNumberLookup = true,
        ConsentForReferrerUpdatedWithOutcome = true,
        Address1 = "Address1",
        Address2 = "Address2",
        Address3 = "Address3",
        DateOfBirth = DateTimeOffset.Now.AddYears(-40),
        DateOfBmiAtRegistration = DateTimeOffset.Now,
        Email = Generators.GenerateNhsEmail(rnd),
        Ethnicity = Business.Enums.Ethnicity.White.ToString(),
        FamilyName = "FamilyName",
        GivenName = "GivenName",
        HasALearningDisability = null,
        HasAPhysicalDisability = null,
        HasDiabetesType1 = false,
        HasDiabetesType2 = false,
        HasHypertension = true,
        HeightCm = 181m,
        Mobile = Generators.GenerateMobile(rnd),
        Postcode = "TF1 4NF",
        ServiceUserEthnicity = ETHNICITY__IRISH,
        ServiceUserEthnicityGroup = ETHNICITY_GROUP__WHITE,
        Sex = "Male",
        Telephone = Generators.GenerateTelephone(rnd),
        WeightKg = 110m,
        CalculatedBmiAtRegistration = BmiHelper.CalculateBmi(110m, 181m),
        IsVulnerable = false
      };
    }
  }
}