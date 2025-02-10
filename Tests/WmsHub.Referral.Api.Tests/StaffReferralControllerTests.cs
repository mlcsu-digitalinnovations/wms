using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.AccessKeys;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;
using Xunit.Abstractions;
using AccessKeys = WmsHub.Business.Models.ReferralService.AccessKeys;
using Deprivation = WmsHub.Business.Models.Deprivation;
using Provider = WmsHub.Business.Entities.Provider;

namespace WmsHub.Referral.Api.Tests;

[Collection("Service collection")]
public class StaffReferralControllerTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly IProviderService _providerService;
  private readonly Mock<ReferralService> _mockReferralService;
  private readonly StaffReferralController _controller;
  private readonly Mock<IOptions<GpDocumentProxyOptions>> _mockGpDocumentProxyOptions = new();
  private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
    new Mock<IOptions<ProviderOptions>>();
  private readonly Deprivation _mockDeprivationValue = new Deprivation
  { ImdDecile = 6, Lsoa = "E00000001" };
  private readonly Mock<IDeprivationService> _mockDeprivationService =
    new Mock<IDeprivationService>();
  private readonly Mock<ILinkIdService> _mockLinkIdService = new();
  private readonly Mock<IPostcodesIoService> _mockPostcodeIoService =
    new Mock<IPostcodesIoService>();
  private readonly Mock<IOptions<ReferralTimelineOptions>> _mockReferralTimelineOptions = new();
  private Mock<CourseCompletionResult> _mockScoreResult = new();
  private readonly Mock<IPatientTriageService> _mockPatientTriageService =
    new Mock<IPatientTriageService>();
  private Mock<IOptions<StaffReferralOptions>>
    _mockStaffReferralOptions = new();
  private Mock<ILogger> _mockLogger;

  public StaffReferralControllerTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
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

    _mockReferralService = new Mock<ReferralService>(
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
      CallBase = true
    };
    _mockReferralService.Object.User = GetClaimsPrincipal();

    _mockStaffReferralOptions = new Mock<IOptions<StaffReferralOptions>>();
    _mockStaffReferralOptions.Setup(x => x.Value).Returns(
      new StaffReferralOptions
      {
        EmailDomainWhitelist = new List<string>()
        {
          { "nhs.net" },
          {  "nhs.uk"}
        }.ToArray(),
      });

    _mockLogger = new Mock<ILogger>();
    _mockLogger.Setup(x => x.ForContext<StaffReferralController>())
      .Returns(_mockLogger.Object);

    _controller = new StaffReferralController(
      _mockLogger.Object,
      _serviceFixture.Mapper,
      _mockStaffReferralOptions.Object,
      _mockReferralService.Object);

    ClaimsPrincipal user = new ClaimsPrincipal(
      new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.Name, "SelfReferral.Service")},
        "TestAuthentication"));

    _controller.ControllerContext = new ControllerContext
    {
      HttpContext =
      new DefaultHttpContext { User = user }
    };
  }

  public class Post : StaffReferralControllerTests    {
    public Post(
      ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {

      _context.StaffRoles.RemoveRange(_context.StaffRoles);
      _context.StaffRoles.Add(new Business.Entities.StaffRole()
      {
        DisplayName = STAFF_ROLE__AMBULANCE_STAFF,
        DisplayOrder = 1
      });

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.SaveChanges();
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult okObjectResult = result.As<OkObjectResult>();
        okObjectResult.Should().NotBeNull();

        IReferralPostResponse referral = 
          okObjectResult.Value.As<IReferralPostResponse>();
        referral.Should().NotBeNull();
        referral.ProviderChoices.Count().Should().BeGreaterThan(0);
        Business.Entities.Referral entity =
          _context.Referrals.FirstOrDefault(t => t.Id == referral.Id);
        entity.Status.Should().Be(ReferralStatus.New.ToString());
      }
    }

    [Fact]
    public async Task Valid_ButNoContent_204()
    {
      // Arrange.
      string expected =
        "Unable to find provider choices for the referral ";
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult noContentResult = result.As<ObjectResult>();
        noContentResult.Should().NotBeNull();
        noContentResult.StatusCode.Should()
          .Be(StatusCodes.Status204NoContent);

        ProblemDetails problem =
          noContentResult.Value.As<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Status.Should().Be(StatusCodes.Status204NoContent);

        string id = problem.Detail.Replace(expected, "").Replace(".", "");

        if (Guid.TryParse(id, out Guid referralId))
        {
          expected += id + ".";
          problem.Detail.Should().Be(expected);
        }
        
        Business.Entities.Referral referral =
          _context.Referrals.FirstOrDefault(t => t.Id == referralId);
        referral.Status.Should().Be(ReferralStatus.New.ToString());
      }
    }

    [Theory]
    [InlineData("no_domain.com")]
    [InlineData("two@domains@yahoo.com")]
    public async Task InvalidEmail(string email)
    {
      // Arrange.
      string expectedError = 
        $"Email address {email} contains a domain that is not in " +
          $"the white list.";
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      request.Email = email;
      int expectedStatus = StatusCodes.Status400BadRequest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badRequestResult = 
          result.As<ObjectResult>();
        badRequestResult.Should().BeOfType<ObjectResult>();
        badRequestResult.StatusCode.Should().Be(expectedStatus);
        ((ProblemDetails)badRequestResult.Value).Detail
          .Should().Be(expectedError);
      }
    }

    [Fact]
    public async Task Invalid_EmailDomainNotAllowed()
    {
      // Arrange.
      string emailToTest = "mock.test@gmail.com";
      string expectedMessage =
        $"Email address {emailToTest} contains a domain that is not " +
        $"in the white list.";
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      request.Email = emailToTest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain(expectedMessage);
      }

      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Theory]
    [InlineData("mock.test@nhs.net")]
    [InlineData("Mock.test@nhs.net")]
    [InlineData("Mock.Test@nhs.net")]
    [InlineData("Mock.Test@NHS.net")]
    [InlineData("MOCK.TEST@NHS.NET")]
    public async Task EmailInUse(string differringCaseEmail)
    {
      // Arrange.
      var email = "mock.test@nhs.net";
      var referral = RandomEntityCreator.CreateRandomReferral(
        email: email);
      _context.Add(referral);
      _context.SaveChanges();

      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      request.Email = differringCaseEmail;
      int expectedStatus = StatusCodes.Status409Conflict;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using(new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(expectedStatus);
      }
    }

    [Theory]
    [InlineData("", "Mobile number is an empty string")]
    [InlineData(null, "Mobile number is null")]
    [InlineData("07715427599", "Mobile number does not being with +44")]
    public async Task InvalidMobile(string mobile, string because)
    {
      // Arrange.
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      request.Mobile = mobile;
      int expectedStatus = StatusCodes.Status400BadRequest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        BadRequestObjectResult badRequestObjectResult = 
          result.As<BadRequestObjectResult>();
        badRequestObjectResult.Should().NotBeNull();
        badRequestObjectResult.StatusCode.Should()
          .Be(expectedStatus, because);
      }
    }

    [Fact]
    public async Task DateOfBmiAtRegistrationMoreThan24MonthsAgo()
    {
      // Arrange.
      SelfReferralPostRequest request = CreateSelfReferralPostRequest();
      request.DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-740);
      int expectedStatus = StatusCodes.Status400BadRequest;

      // Act.
      IActionResult result = await _controller.Post(request);

      // Assert.
      using (new AssertionScope())
      {
        BadRequestObjectResult badRequestObjectResult =
          result.As<BadRequestObjectResult>();
        badRequestObjectResult.Should().NotBeNull();
        badRequestObjectResult.StatusCode.Should()
          .Be(expectedStatus);
      }
    }
  }

  /// <summary>
  /// Put is used to update teh referral with a selected provider
  /// </summary>
  public class Put : StaffReferralControllerTests
  {
    public Put(ServiceFixture serviceFixture, 
      ITestOutputHelper testOutputHelper) :
      base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      Random rnd = new ();

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      
      Business.Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status:ReferralStatus.New,
          email: Generators.GenerateNhsEmail());
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      _context.Referrals.Add(referral);

      await _context.SaveChangesAsync();

      SelfReferralPutRequest request = new ()
      {
        Id = referral.Id,
        ProviderId = provider.Id
      };

      // Act.
      IActionResult result = await _controller.Put(request);

      // Assert.
      using(new AssertionScope())
      {
        OkResult okResult = result.As<OkResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      _context.Referrals.Remove(referral);
      _context.Providers.Remove(provider);
      _context.SaveChanges();
    }

    [Fact]
    public async Task InValid_TriageLevelNotSet()
    {
      // Arrange.
      string expected = "Triage completion level is null";
      Random rnd = new ();

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Business.Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          email: Generators.GenerateNhsEmail());
      _context.Referrals.Add(referral);

      await _context.SaveChangesAsync();

      SelfReferralPutRequest request = new ()
      {
        Id = referral.Id,
        ProviderId = provider.Id
      };

      // Act.
      IActionResult result = await _controller.Put(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
        problemDetails.Detail.Should().Contain(expected);
      }

      _context.Referrals.Remove(referral);
      _context.Providers.Remove(provider);
      _context.SaveChanges();
    }

    [Fact]
    public async Task InValid_ProviderNotFound()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      string expected = 
        $"Provider {providerId} was not found in the list of " +
        $"providers for the selected Triage Level.";
      Random rnd = new ();

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Business.Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          email: Generators.GenerateNhsEmail());
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      _context.Referrals.Add(referral);

      await _context.SaveChangesAsync();

      SelfReferralPutRequest request = new ()
      {
        Id = referral.Id,
        ProviderId = providerId
      };

      // Act.
      IActionResult result = await _controller.Put(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain(expected);
      }

      _context.Referrals.Remove(referral);
      _context.Providers.Remove(provider);
      _context.SaveChanges();
    }

    [Fact]
    public async Task Invalid_ReferralNotNew()
    {
      // Arrange.
      Random rnd = new ();

      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);

      Business.Entities.Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          email: Generators.GenerateNhsEmail());
      referral.TriagedCompletionLevel = "3";
      referral.TriagedWeightedLevel = "2";
      referral.ProviderId = provider.Id;
      referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
      _context.Referrals.Add(referral);

      await _context.SaveChangesAsync();

      string expected =
        $"Referral Id {referral.Id} already has a provider selected.";

      SelfReferralPutRequest request = new()
      {
        Id = referral.Id,
        ProviderId = provider.Id
      };

      // Act.
      IActionResult result = await _controller.Put(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status409Conflict);
        problemDetails.Detail.Should().Contain(expected);
      }

      _context.Referrals.Remove(referral);
      _context.Providers.Remove(provider);
      _context.SaveChanges();

    }
  }

  public class GetEthnicities : StaffReferralControllerTests
  {
    public GetEthnicities(
      ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
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
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
            objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
      }
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.

      // Act.
      IActionResult result = await _controller.GetEthnicities();

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult okObjectResult = result.As<OkObjectResult>();
        okObjectResult.Should().NotBeNull();
      }
    }
  }

  public class GetStaffRoles : StaffReferralControllerTests
  {
    public GetStaffRoles(
      ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task NotFoundException()
    {
      // Arrange.
      _context.StaffRoles.RemoveRange(_context.StaffRoles);
      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.GetStaffRoles();

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
            objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
      }
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange.
      _context.StaffRoles.RemoveRange(_context.StaffRoles);
      _context.StaffRoles.Add(new Business.Entities.StaffRole()
      {
        DisplayName = "StaffRole1",
        DisplayOrder = 1,
        IsActive = true
      });
      _context.StaffRoles.Add(new Business.Entities.StaffRole()
      {
        DisplayName = "StaffRole2",
        DisplayOrder = 2,
        IsActive = true
      });

      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.GetStaffRoles();

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult okObjectResult = result.As<OkObjectResult>();
        okObjectResult.Should().NotBeNull();
      }
    }

  }

  public class IsEmailInUse : StaffReferralControllerTests
  {
    public IsEmailInUse(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }


    [Fact]
    public async Task EmailDomainNotAllowed()
    {
      // Arrange.
      string emailToTest = "mock.test@gmail.com";
      string expectedMessage = 
        $"Email address {emailToTest} contains a domain that is not in " +
        $"the white list.";

      // Act.

      IActionResult result = await _controller.IsEmailInUse(
        new SelfReferralEmailInUse() { Email = emailToTest });

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain(expectedMessage);
      }

      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task NotInUse_Return_200()
    {
      // Arrange.
      var entity = RandomEntityCreator.CreateRandomReferral(
        email: "mock.test@nhs.net");
      _context.Referrals.Add(entity);
      _context.SaveChanges();

      // Act.
      var result = await _controller.IsEmailInUse(
        new SelfReferralEmailInUse() { Email = "mock2.test@nhs.net" });

      // Assert.
      using (new AssertionScope())
      {
        OkResult okResult = result.As<OkResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Theory]
    [InlineData("mock.test@nhs.net")]
    [InlineData("Mock.test@nhs.net")]
    [InlineData("Mock.Test@nhs.net")]
    [InlineData("Mock.Test@NHS.net")]
    [InlineData("MOCK.TEST@NHS.NET")]
    public async Task InUse_ProviderNotSelected_StatusComplete_Return_200(
      string differringCaseEmail)
    {
      // Arrange.
      var entity = RandomEntityCreator.CreateRandomReferral(
        email: "mock.test@nhs.net",
        status: ReferralStatus.Complete);
      _context.Referrals.Add(entity);
      _context.SaveChanges();

      // Act.
      var result = await _controller.IsEmailInUse(
        new SelfReferralEmailInUse() { Email = differringCaseEmail });

      // Assert.
      using (new AssertionScope())
      {
        OkResult okResult = result.As<OkResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Theory]
    [InlineData(null, ReferralStatus.New, "mock.test@nhs.net")]
    [InlineData(null, ReferralStatus.New, "Mock.test@nhs.net")]
    [InlineData(null, ReferralStatus.New, "Mock.Test@nhs.net")]
    [InlineData(null, ReferralStatus.New, "Mock.Test@NHS.net")]
    [InlineData(null, ReferralStatus.New, "MOCK.TEST@NHS.NET")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.New, "mock.test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.New, "Mock.test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.New, "Mock.Test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.New, "Mock.Test@NHS.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.New, "MOCK.TEST@NHS.NET")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.Complete, "mock.test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.Complete, "Mock.test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.Complete, "Mock.Test@nhs.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.Complete, "Mock.Test@NHS.net")]
    [InlineData("fb4e2106-4e16-4209-a8b6-01ce9ace0917", ReferralStatus.Complete, "MOCK.TEST@NHS.NET")]
    public async Task InUse_Return_409(
      string providerIdString,
      ReferralStatus referralStatus,
      string differringCaseEmail)
    {
      // Arrange.
      var referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now,
        email: "mock.test@nhs.net",
        status: referralStatus,
        providerId: providerIdString == null 
          ? default 
          : new Guid(providerIdString));
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      // Act.
      IActionResult result = await _controller.IsEmailInUse(
        new SelfReferralEmailInUse() { Email = differringCaseEmail });

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult objectResult = result.As<ObjectResult>();
        objectResult.Should().NotBeNull();

        ProblemDetails problemDetails =
          objectResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status409Conflict);
      }

      // clean up
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }
  }

  public class GenerateAccessKey : StaffReferralControllerTests
  {
    public GenerateAccessKey(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task InternalServerError()
    {
      // Arrange.
      string expectedDetail = 
        "Staff referral email domain whitelist is enabled but empty.";
      
      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>().ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new (
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result = 
        await controller.GenerateAccessKey("joe@yahoo.com");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task Forbidden()
    {
      // Arrange.
      string expectedDetail =
        "Email's domain is not in the domain white list.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@yahoo.com");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
      }
    }

    [Fact]
    public async Task BadRequest()
    {
      // Arrange.
      string expectedDetail =
        "The Email field is not a valid email address.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@test.com");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Validation_BadRequest()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "Validation error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.CreateAccessKeyAsync(It.IsAny<CreateAccessKey>()))
        .ReturnsAsync(new AccessKeys.CreateAccessKeyResponse(
          ResponseBase.ErrorTypes.Validation, "Validation error."));

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@nhs.net");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Validation_MaxActiveAccessKeys()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "Validation error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true,
          MaxActiveAccessKeys = 2
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.CreateAccessKeyAsync(It.IsAny<CreateAccessKey>()))
        .ReturnsAsync(new AccessKeys.CreateAccessKeyResponse(
          ResponseBase.ErrorTypes.MaxActiveAccessKeys, "Validation error."));

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@nhs.net");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should()
          .Be(StatusCodes.Status429TooManyRequests);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Be("MaxActiveAccessKeys");
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status429TooManyRequests);
      }
    }

    [Fact]
    public async Task ResponseError_InternalServerError()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "Unknown error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.CreateAccessKeyAsync(It.IsAny<CreateAccessKey>()))
        .ReturnsAsync(new AccessKeys.CreateAccessKeyResponse(
          ResponseBase.ErrorTypes.Unknown, "Unknown error."));

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@nhs.net");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      DateTimeOffset expiry = DateTimeOffset.UtcNow
        .AddMinutes(10);
      Api.Models.CreateAccessKeyResponse response = new() 
      {
        KeyCode = "abcdef",
        Expires = expiry
      };

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.CreateAccessKeyAsync(It.IsAny<CreateAccessKey>()))
        .ReturnsAsync(
          new AccessKeys.CreateAccessKeyResponse(expiry, "abcdef", "joe@nhs.net"));

      // Act.
      IActionResult result =
        await controller.GenerateAccessKey("joe@nhs.net");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        okResult.Value.Should()
          .BeOfType<Api.Models.CreateAccessKeyResponse>();
                  Api.Models.CreateAccessKeyResponse resultValue =
          okResult.Value.As<Api.Models.CreateAccessKeyResponse>();
        resultValue.Should().NotBeNull();
        resultValue.Should().BeEquivalentTo(response);
      }
    }
  }

  public class ValidateAccessKey : StaffReferralControllerTests
  {
    public ValidateAccessKey(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
    }

    [Fact]
    public async Task InternalServerError()
    {
      // Arrange.
      string expectedDetail =
        "Staff referral email domain whitelist is enabled but empty.";
      
      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>().ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@yahoo.com", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task Forbidden()
    {
      // Arrange.
      string expectedDetail =
        "Email's domain is not in the domain white list.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@yahoo.com", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
      }
    }

    [Fact]
    public async Task BadRequest()
    {
      // Arrange.
      string expectedDetail =
        "The Email field is not a valid email address.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        _mockReferralService.Object);

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@test.com", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Validation_BadRequest()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "Validation error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
          "Validation error.",
          ResponseBase.ErrorTypes.Validation));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);

        badResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Type.Should().Be("Validation");
        problemDetails.Status.Should()
          .Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task ResponseError_NotFound()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "NotFound error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
          "NotFound error.",
          ResponseBase.ErrorTypes.NotFound));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status404NotFound);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Type.Should().Be("NotFound");
        problemDetails.Status.Should()
          .Be(StatusCodes.Status404NotFound);
      }
    }

    [Fact]
    public async Task ResponseError_Expired()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "The Security Code you have entered has expired, " +
        "please request a new Security Code by clicking on the email " +
        "not received link.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);
      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
            "Expired error.",
            ResponseBase.ErrorTypes.Expired));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Type.Should().Be("Expired");
        problemDetails.Status.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);
      }
    }

    [Fact]
    public async Task ResponseError_TooManyAttempts()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "You have exhausted all your allowable attempts to " +
        "access the system, please request a new Security Code by " +
        "clicking on the email not received link.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
          "TooManyAttempts error.",
          ResponseBase.ErrorTypes.TooManyAttempts));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Type.Should().Be("TooManyAttempts");
        problemDetails.Status.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);
      }
    }

    [Fact]
    public async Task ResponseError_Incorrect()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "The Security Code you have entered is incorrect.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
          "Incorrect error.",
          ResponseBase.ErrorTypes.Incorrect));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Type.Should().Be("Incorrect");
        problemDetails.Status.Should()
          .Be(StatusCodes.Status422UnprocessableEntity);
      }
    }

    [Fact]
    public async Task ResponseError_InternalServerError()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      string expectedDetail =
        "Unknown error.";

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(new AccessKeys.ValidateAccessKeyResponse(
          "Unknown error.",
          ResponseBase.ErrorTypes.Unknown));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        outputResult.Value.Should().BeOfType<ProblemDetails>();
        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      Mock<IReferralService> mockReferralService = new();
      DateTimeOffset expiry = DateTimeOffset.UtcNow
        .AddMinutes(10);
      Api.Models.ValidateAccessKeyResponse response = new()
      {
        ValidCode = true,
        Expires = expiry
      };

      _mockStaffReferralOptions.Setup(x => x.Value)
        .Returns(new StaffReferralOptions
        {
          EmailDomainWhitelist = new List<string>()
          {
            { "nhs.net" },
            { "nhs.uk" }
          }.ToArray(),
          IsEmailDomainWhitelistEnabled = true
        });

      StaffReferralController controller = new(
        _mockLogger.Object,
        _serviceFixture.Mapper,
        _mockStaffReferralOptions.Object,
        mockReferralService.Object);

      mockReferralService.Setup(x =>
          x.ValidateAccessKeyAsync(It.IsAny<AccessKeys.ValidateAccessKey>()))
        .ReturnsAsync(
          new AccessKeys.ValidateAccessKeyResponse(true, expiry));

      // Act.
      IActionResult result =
        await controller.ValidateAccessKey("joe@nhs.net", "abcdef");

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        okResult.Value.Should()
          .BeOfType<Api.Models.ValidateAccessKeyResponse>();
        Api.Models.ValidateAccessKeyResponse resultValue =
          okResult.Value.As<Api.Models.ValidateAccessKeyResponse>();
        resultValue.Should().NotBeNull();
        resultValue.Should().BeEquivalentTo(response);
      }
    }
  }

  private SelfReferralPostRequest CreateSelfReferralPostRequest()
  {
    Random random = new Random();
    return new SelfReferralPostRequest()
    {
      Address1 = "Address1",
      Address2 = "Address2",
      Address3 = "Address3",
      ConsentForFutureContactForEvaluation = true,
      DateOfBirth = DateTimeOffset.Now.AddYears(-40),
      DateOfBmiAtRegistration = DateTimeOffset.Now,
      Email = Generators.GenerateNhsEmail(),
      Ethnicity = Business.Enums.Ethnicity.White.ToString(),
      FamilyName = "FamilyName",
      GivenName = "GivenName",
      HasALearningDisability = null,
      HasAPhysicalDisability = null,        
      HasRegisteredSeriousMentalIllness = null,
      HasDiabetesType1 = null,
      HasDiabetesType2 = null,
      HasHypertension = null,
      HeightCm = 150m,
      Mobile = "+447886123456",
      Postcode = "TF1 4NF",
      ServiceUserEthnicity = ETHNICITY__IRISH,
      ServiceUserEthnicityGroup = ETHNICITY_GROUP__WHITE,
      Sex = "Male",
      StaffRole = STAFF_ROLE__AMBULANCE_STAFF,
      Telephone = "+441743123456",
      WeightKg = 120m
    };
  }
}
