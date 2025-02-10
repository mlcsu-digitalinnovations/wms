using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
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
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models.GeneralReferral;
using Xunit;
using Xunit.Abstractions;
using Deprivation = WmsHub.Business.Models.Deprivation;
using Ethnicity = WmsHub.Business.Entities.Ethnicity;

namespace WmsHub.Referral.Api.Tests;

[Collection("Service collection")]
public partial class GeneralReferralControllerTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly GeneralReferralController _controller;
  private readonly GpDocumentProxyOptions _gpDocumentProxyOptions = new();
  private readonly Mock<IOptions<GpDocumentProxyOptions>> _mockGpDocumentProxyOptions = new();
  private readonly Mock<IDeprivationService> _mockDeprivationService = new();
  private readonly Deprivation _mockDeprivationValue = new() { ImdDecile = 6, Lsoa = "E00000001" };
  private readonly Mock<ILinkIdService> _mockLinkIdService = new();
  private readonly Mock<IOptions<ProviderOptions>> _mockOptions = new();
  private readonly Mock<IPatientTriageService> _mockPatientTriageService = new();
  private readonly Mock<ProviderService> _mockProviderService;
  private readonly Business.Models.Provider _mockProviderServiceProvider;
  private readonly Mock<ReferralService> _mockReferralService;
  private Mock<CourseCompletionResult> _mockScoreResult = new();
  private readonly Mock<IPostcodesIoService> _mockPostcodeIoService = new();
  private readonly Mock<IOptions<ReferralTimelineOptions>> _mockReferralTimelineOptions = new();

  public GeneralReferralControllerTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    _mockDeprivationService
      .Setup(x => x.GetByLsoa(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue);

    _mockPostcodeIoService
      .Setup(x => x.GetLsoaAsync(It.IsAny<string>()))
      .ReturnsAsync(_mockDeprivationValue.Lsoa);

    ProviderOptions _options = new()
    {
      CompletionDays = 84,
      NumDaysPastCompletedDate = 10
    };
    _mockOptions.Setup(x => x.Value).Returns(_options);

    _mockProviderService = new Mock<ProviderService>(
      _context,
      _serviceFixture.Mapper,
      _mockOptions.Object)
    {
      CallBase = true
    };
    _mockProviderService.Object.User = GetClaimsPrincipal();

    _mockProviderServiceProvider = Business.Helpers.RandomModelCreator.CreateRandomProvider();

    List<Business.Models.Provider> providers = [_mockProviderServiceProvider];

    _mockProviderService
      .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
      .ReturnsAsync(providers);

    _mockScoreResult
      .Setup(t => t.TriagedCompletionLevel)
      .Returns(TriageLevel.High);

    _mockScoreResult
      .Setup(t => t.TriagedWeightedLevel)
      .Returns(TriageLevel.Medium);

    _mockPatientTriageService
      .Setup(t => t.GetScores(It.IsAny<CourseCompletionParameters>()))
      .Returns(_mockScoreResult.Object);

    _mockGpDocumentProxyOptions
      .Setup(x => x.Value)
      .Returns(_gpDocumentProxyOptions);

    _mockPostcodeIoService
      .Setup(t => t.IsEnglishPostcodeAsync(It.IsAny<string>()))
      .ReturnsAsync(true);

    _mockReferralService = new Mock<ReferralService>(
      _context,
      _serviceFixture.Mapper,
      _mockProviderService.Object,
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
      CallBase = true
    };
    _mockReferralService.Object.User = GetClaimsPrincipal();

    _controller = new GeneralReferralController(
      _mockReferralService.Object,
      _serviceFixture.Mapper);

    ClaimsPrincipal user = new ClaimsPrincipal(
      new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.Name, "GeneralReferral.Service")},
        "TestAuthentication"));

    _controller.ControllerContext = new ControllerContext
    {
      HttpContext =
      new DefaultHttpContext { User = user }
    };

    Log.Logger = new LoggerConfiguration().CreateLogger();
  }

  public class Post : GeneralReferralControllerTests, IDisposable
  {
    public Post(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      IReferralPostResponse response = result.Should().BeOfType<OkObjectResult>()
        .Which.Value.Should().BeOfType<SelfReferralPostResponse>().Subject;

      response.ProviderChoices.Count().Should().Be(1);

      Business.Entities.Referral entity = _context.Referrals.Single(t => t.Id == response.Id);
      entity.Should().BeEquivalentTo(request);

      entity.Status.Should().Be(ReferralStatus.New.ToString());
      entity.ReferralSource.Should().Be(ReferralSource.GeneralReferral.ToString());
    }

    [Fact]
    public async Task IsPregnant_True_400()
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.IsPregnant = true;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      BadRequestObjectResult badRequestObjectResult = result
        .Should().BeOfType<BadRequestObjectResult>().Subject;
      badRequestObjectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      badRequestObjectResult.Value
        .Should().BeOfType<ValidationProblemDetails>()
        .Which.Errors.ContainsKey(nameof(request.IsPregnant));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task IsPregnant_FalseOrNull_200(bool? isPregnant)
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.IsPregnant = isPregnant;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().BeOfType<OkObjectResult>()
        .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task HasHadBariatricSurgery_True_400()
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.HasHadBariatricSurgery = true;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      BadRequestObjectResult badRequestObjectResult = result
        .Should().BeOfType<BadRequestObjectResult>().Subject;
      badRequestObjectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      badRequestObjectResult.Value
        .Should().BeOfType<ValidationProblemDetails>()
        .Which.Errors.ContainsKey(nameof(request.HasHadBariatricSurgery));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task HasHadBariatricSurgery_FalseOrNull_200(bool? hasHadBariatricSurgery)
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.HasHadBariatricSurgery = hasHadBariatricSurgery;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().BeOfType<OkObjectResult>()
        .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task HasActiveEatingDisorder_True_400()
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.HasActiveEatingDisorder = true;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      BadRequestObjectResult badRequestObjectResult = result
        .Should().BeOfType<BadRequestObjectResult>().Subject;
      badRequestObjectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      badRequestObjectResult.Value
        .Should().BeOfType<ValidationProblemDetails>()
        .Which.Errors.ContainsKey(nameof(request.HasActiveEatingDisorder));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task HasActiveEatingDisorder_False_200(bool? hasActiveEatingDisorder)
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.HasActiveEatingDisorder = hasActiveEatingDisorder;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      result.Should().BeOfType<OkObjectResult>()
        .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Valid_ButNoContent_204()
    {
      // Arrange.
      string expected = "Unable to find provider choices for the referral ";
      PostRequest request = CreateReferralPostRequest();

      // Override the existing setup to return 0 providers.
      _mockProviderService
        .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
        .ReturnsAsync([]);

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      Assert.NotNull(result);
      Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);

      // TODO - complete asserts for all properties
      ProblemDetails problem = (ProblemDetails)((ObjectResult)result).Value;
      problem.Status.Should().Be((int)HttpStatusCode.NoContent);

      var id = problem.Detail.Replace(expected, "").Replace(".", "");
      Guid.TryParse(id, out Guid referralId);

      expected += id + ".";
      problem.Detail.Should().Be(expected);
      Business.Entities.Referral referral = _context.Referrals
        .FirstOrDefault(t => t.Id == referralId);

      referral.Status.Should().Be(ReferralStatus.New.ToString());
    }

    [Theory]
    [InlineData("", "Mobile number is an empty string")]
    [InlineData(null, "Mobile number is null")]
    [InlineData("07715427599", "Mobile number does not being with +44")]
    public async Task InvalidMobile(string mobile, string because)
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.Mobile = mobile;
      int expectedStatus = StatusCodes.Status400BadRequest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      Assert.IsType<BadRequestObjectResult>(result);
      ((BadRequestObjectResult)result).StatusCode.Should()
        .Be(expectedStatus, because);
    }

    [Fact]
    public async Task DateOfBmiAtRegistrationMoreThan24MonthsAgo()
    {
      // Arrange.
      PostRequest request = CreateReferralPostRequest();
      request.DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-740);
      int expectedStatus = StatusCodes.Status400BadRequest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      Assert.IsType<BadRequestObjectResult>(result);
      ((BadRequestObjectResult)result).StatusCode.Should().Be(expectedStatus);
    }
  }

  /// <summary>
  /// Put is used to update the referral with a selected provider
  /// </summary>
  public class Put : GeneralReferralControllerTests, IDisposable
  {

    public Put(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task Valid_Status_New()
    {
      // Arrange.
      Business.Models.Provider provider = Business.Helpers.RandomModelCreator
        .CreateRandomProvider(level3: true);

      // Override the existing setup to return 1 level 3 provider.
      _mockProviderService
        .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
        .ReturnsAsync([provider]);

      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomGeneralReferral();

      PutRequest request = await SetupPutRequest(setProviderNull: true, referral: referral);

      // Act.
      IActionResult result = await _controller.Put(request, referral.Id);

      // Assert.
      IReferralPostResponse response = result.Should().BeOfType<OkObjectResult>()
        .Which.Value.Should().BeOfType<SelfReferralPostResponse>().Subject;

      response.ProviderChoices.Count().Should().Be(1);

      ProviderForSelection selectedProvider = response.ProviderChoices
        .Single(x => x.Id == provider.Id);
      selectedProvider.Id.Should().Be(provider.Id);
      selectedProvider.Logo.Should().Be(provider.Logo);
      selectedProvider.Name.Should().Be(provider.Name);
      selectedProvider.Summary.Should().Be(
        provider.Summary3,
        because: "mocked TriagedCompletionLevel is High which returns a level 3 summary.");
      selectedProvider.Website.Should().Be(provider.Website);

      Business.Entities.Referral entity = _context.Referrals.Single(t => t.Id == response.Id);
      entity.Should().BeEquivalentTo(request);
      entity.Status.Should().Be(ReferralStatus.New.ToString());
      entity.ReferralSource.Should().Be(ReferralSource.GeneralReferral.ToString());
    }

    private static PutRequest GetPutRequest(Business.Entities.Referral referral)
    {
      PutRequest request = new()
      {
        Address1 = referral.Address1,
        Address2 = referral.Address2,
        Address3 = referral.Address3,
        ConsentForFutureContactForEvaluation = referral.ConsentForFutureContactForEvaluation,
        ConsentForGpAndNhsNumberLookup = true,
        ConsentForReferrerUpdatedWithOutcome = referral.ConsentForReferrerUpdatedWithOutcome,
        DateOfBirth = referral.DateOfBirth,
        DateOfBmiAtRegistration = referral.DateOfBmiAtRegistration,
        Email = referral.Email,
        Ethnicity = referral.Ethnicity,
        FamilyName = referral.FamilyName,
        GivenName = referral.GivenName,
        HasActiveEatingDisorder = referral.HasActiveEatingDisorder,
        HasALearningDisability = referral.HasALearningDisability,
        HasAPhysicalDisability = referral.HasAPhysicalDisability,
        HasArthritisOfHip = referral.HasArthritisOfHip,
        HasArthritisOfKnee = referral.HasArthritisOfKnee,
        HasDiabetesType1 = referral.HasDiabetesType1,
        HasDiabetesType2 = referral.HasDiabetesType2,
        HasHadBariatricSurgery = referral.HasHadBariatricSurgery,
        HasHypertension = referral.HasHypertension,
        HeightCm = referral.HeightCm ?? 181m,
        Id = referral.Id,
        IsPregnant = referral.IsPregnant,
        Mobile = referral.Mobile,
        NhsLoginClaimEmail = $"Put{referral.NhsLoginClaimEmail}",
        NhsLoginClaimFamilyName = $"Put{referral.NhsLoginClaimFamilyName}",
        NhsLoginClaimGivenName = $"Put{referral.NhsLoginClaimGivenName}",
        NhsLoginClaimMobile = Generators.GenerateMobile(new Random()),
        NhsNumber = referral.NhsNumber,
        Postcode = referral.Postcode,
        ReferringGpPracticeNumber = referral.ReferringGpPracticeNumber,
        ServiceUserEthnicity = referral.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = referral.ServiceUserEthnicityGroup,
        Sex = referral.Sex,
        Telephone = referral.Telephone,
        WeightKg = referral.WeightKg ?? 120m,
      };

      return request;
    }

    [Fact]
    public async Task Invalid_ProviderPreviouslySelected()
    {
      // Arrange.
      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomGeneralReferral();

      PutRequest request = await SetupPutRequest(
        providerId: _mockProviderServiceProvider.Id,
        referral: referral);

      string expected =
        $"The referral {referral.Id} has previously had its provider " +
        $"selected {_mockProviderServiceProvider.Id}.";

      // Act.
      IActionResult response = await _controller.Put(request, referral.Id);

      // Assert.
      ObjectResult objectResult = Assert.IsType<ObjectResult>(response);
      ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
      problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
      problemDetails.Detail.Should().Contain(expected);
    }

    [Fact]
    public async Task Valid_ProviderNotSelected()
    {
      // Arrange.
      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomGeneralReferral();

      PutRequest request = await SetupPutRequest(
        providerId: _mockProviderServiceProvider.Id,
        referral: referral,
        setProviderNull: true);

      string expected =
        $"The referral {referral.Id} has previously had its provider " +
        $"selected {_mockProviderServiceProvider.Id}.";

      // Act.
      IActionResult response = await _controller.Put(request, referral.Id);

      // Assert.
      OkObjectResult objectResult = Assert.IsType<OkObjectResult>(response);
      Assert.IsType<SelfReferralPostResponse>(objectResult.Value);
    }

    [Theory]
    [MemberData(nameof(TestStatuses))]
    public async Task Invalid_ReferrralStatusIsNotValid(ReferralStatus status, bool isAllowed)
    {
      // Arrange.
      Business.Entities.Referral referral = RandomEntityCreator.CreateRandomGeneralReferral();
      
      PutRequest request = await SetupPutRequest(
        providerId: _mockProviderServiceProvider.Id,
        referral: referral,
        setProviderNull: true,
        referralStatus: status.ToString());
      
      string expected = 
        $"Referral {referral.Id} has a status of {referral.Status} and cannot be updated";
      
      // Act.
      IActionResult response = await _controller.Put(request, referral.Id);

      // Assert.
      if (isAllowed)
      {
        OkObjectResult objectResult = Assert.IsType<OkObjectResult>(response);
        Assert.IsType<SelfReferralPostResponse>(objectResult.Value);
      }
      else
      {
        ObjectResult objectResult = Assert.IsType<ObjectResult>(response);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Detail.Should().Contain(expected);
      }
    }

    private async Task<PutRequest> SetupPutRequest(
      Guid? providerId = null,
      bool setProviderNull = false,
      Business.Entities.Referral referral = null,
      string referralStatus = null,
      string referralSource = null,
      DateTimeOffset? dateofBmiRegistration = null)
    {
      referral.DateOfBmiAtRegistration = dateofBmiRegistration ?? DateTimeOffset.Now.AddDays(-1);
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      referral.ProviderId = setProviderNull ? null : providerId;
      referral.Status = referralStatus ?? ReferralStatus.New.ToString();
      referral.ConsentForGpAndNhsNumberLookup = true;
      referral.ConsentForFutureContactForEvaluation = false;
      referral.ConsentForReferrerUpdatedWithOutcome = true;
      referral.ReferralSource = referralSource ?? ReferralSource.GeneralReferral.ToString();

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      PutRequest request = GetPutRequest(referral);
      await SetEthnicity(request);
      return request;
    }

    private async Task SetEthnicity(PutRequest request)
    {
      Ethnicity[] ethnicities = await _context.Ethnicities.ToArrayAsync();
      
      int num = new Random().Next(0, ethnicities.Length - 1);
      Ethnicity ethnicity = ethnicities[num];
      request.Ethnicity = ethnicity.TriageName;
      request.ServiceUserEthnicityGroup = ethnicity.GroupName;
      request.ServiceUserEthnicity = ethnicity.DisplayName;
    }

    public static IEnumerable<object[]> TestStatuses()
    {
      List<object[]> validData = new();

      ReferralStatus accpepted =
        ReferralStatus.New |
        ReferralStatus.RmcCall |
        ReferralStatus.RmcDelayed |
        ReferralStatus.TextMessage1 |
        ReferralStatus.TextMessage2 |
        ReferralStatus.ChatBotCall1 |
        ReferralStatus.ChatBotTransfer |
        ReferralStatus.TextMessage3;

      foreach (long value in Enum.GetValues(typeof(ReferralStatus)))
      {
        ReferralStatus toAdd = (ReferralStatus)value;
        if (toAdd == ReferralStatus.Exception)
        {
          validData.Add(new object[] { toAdd, false });
        }
        else
        {
          validData.Add(new object[] { toAdd, accpepted.HasFlag(toAdd) });
        }

      }


      return validData;
    }

  }

  public class GetEthnicities : GeneralReferralControllerTests
  {
    public GetEthnicities(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task NotFoundException()
    {
      // Arrange.
      _mockReferralService
        .Setup(x => x.GetEthnicitiesAsync(It.IsAny<ReferralSource>()))
        .ThrowsAsync(new EthnicityNotFoundException());

      // Act.
      IActionResult result = await _controller.GetEthnicities();

      // Assert.
      Assert.NotNull(result);
      Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.

      // Act.
      IActionResult result = await _controller.GetEthnicities();

      // Assert.
      Assert.NotNull(result);
      Assert.IsType<OkObjectResult>(result);
    }
  }

  public class ValidateLinkId
  {
    private readonly GeneralReferralController _generalReferralController;
    private readonly Mock<IReferralService> _mockReferralService = new();
    private readonly Mock<AutoMapper.IMapper> _mockMapper = new();

    public ValidateLinkId()
    {
      _generalReferralController = new GeneralReferralController(
        _mockReferralService.Object,
        _mockMapper.Object);
    }

    [Fact]
    public async Task ExceptionReturns500InternalServerError()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      _mockReferralService
        .Setup(x => x.ElectiveCareReferralHasTextMessageWithLinkId(It.IsAny<string>()))
        .ThrowsAsync(new ArgumentException("Test Exception"));

      // Act.
      IActionResult result = await _generalReferralController.ValidateLinkId(linkId);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Which.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task InvalidLinkIdReturns400BadRequest(string linkId)
    {
      // Arrange.

      // Act.
      IActionResult result = await _generalReferralController.ValidateLinkId(linkId);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Which.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ValidLinkIdReturns200Ok()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      _mockReferralService
        .Setup(x => x.ElectiveCareReferralHasTextMessageWithLinkId(It.IsAny<string>()))
        .ReturnsAsync(true);

      // Act.
      IActionResult result = await _generalReferralController.ValidateLinkId(linkId);

      // Assert.
      result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ValidLinkIdReturns204NoContent()
    {
      // Arrange.
      string linkId = LinkIdService.GenerateDummyId();
      _mockReferralService
        .Setup(x => x.ElectiveCareReferralHasTextMessageWithLinkId(It.IsAny<string>()))
        .ReturnsAsync(false);

      // Act.
      IActionResult result = await _generalReferralController.ValidateLinkId(linkId);

      // Assert.
      result.Should().BeOfType<NoContentResult>();
    }
  }

  private PostRequest CreateReferralPostRequest()
  {
    Random rnd = new();

    bool notUnique = true;
    string nhsNumber = "";
    while (notUnique)
    {
      nhsNumber = Generators.GenerateNhsNumber(rnd);
      notUnique = _context.Referrals.Any(t => t.NhsNumber == nhsNumber);
    }

    return new PostRequest
    {
      NhsNumber = nhsNumber,
      Address1 = "Address1",
      Address2 = "Address2",
      Address3 = "Address3",
      DateOfBirth = DateTimeOffset.Now.AddYears(-40),
      DateOfBmiAtRegistration = DateTimeOffset.Now,
      Email = Generators.GenerateEmail(),
      Ethnicity = Business.Enums.Ethnicity.White.ToString(),
      FamilyName = "FamilyName",
      GivenName = "GivenName",
      HasALearningDisability = null,
      HasAPhysicalDisability = null,
      HasDiabetesType1 = null,
      HasDiabetesType2 = null,
      HasHypertension = null,
      HeightCm = 150m,
      Mobile = Generators.GenerateMobile(rnd),
      NhsLoginClaimEmail = Generators.GenerateEmail(),
      NhsLoginClaimMobile = Generators.GenerateMobile(rnd),
      NhsLoginClaimFamilyName = "FamilyNameNhsLoginClaim",
      NhsLoginClaimGivenName = "GivenNameNhsLoginClaim",
      Postcode = "TF1 4NF",
      ServiceUserEthnicity = ETHNICITY__IRISH,
      ServiceUserEthnicityGroup = ETHNICITY_GROUP__WHITE,
      Sex = "Male",
      Telephone = Generators.GenerateTelephone(rnd),
      WeightKg = 120m,
      ConsentForFutureContactForEvaluation = false,
      ConsentForReferrerUpdatedWithOutcome = true,
      ReferringGpPracticeNumber = Generators.GenerateGpPracticeNumber(rnd)
    };
  }
}
