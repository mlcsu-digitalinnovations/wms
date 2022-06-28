using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;
using Xunit.Abstractions;
using Deprivation = WmsHub.Business.Models.Deprivation;
using Provider = WmsHub.Business.Entities.Provider;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public class SelfReferralControllerTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IProviderService _providerService;
    private readonly IReferralService _referralService;
    private readonly SelfReferralController _controller;
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

    public SelfReferralControllerTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
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

      _controller = new SelfReferralController(
        _referralService,
        _serviceFixture.Mapper);

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

    public class Post : SelfReferralControllerTests
    {
      public Post(
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

        _context.Referrals.RemoveRange(_context.Referrals);
        _context.Providers.RemoveRange(_context.Providers);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid()
      {
        // ARRANGE
        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();
        // ACT

        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        // TODO - complete asserts for all properties
        IReferralPostResponse referral =
          (IReferralPostResponse) ((ObjectResult) result).Value;
        referral.ProviderChoices.Count().Should().BeGreaterThan(0);
        Business.Entities.Referral entity =
          _context.Referrals.FirstOrDefault(t => t.Id == referral.Id);
        entity.Status.Should().Be(ReferralStatus.New.ToString());

      }

      [Fact]
      public async Task Valid_ButNoContent_204()
      {
        // ARRANGE
        string expected =
          "Unable to find provider choices for the referral ";
        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        // ACT

        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<ProblemDetails>(((ObjectResult) result).Value);
        // TODO - complete asserts for all properties
        ProblemDetails problem =
          (ProblemDetails) ((ObjectResult) result).Value;
        problem.Status.Should().Be((int) HttpStatusCode.NoContent);
        var id = problem.Detail.Replace(expected, "").Replace(".", "");
        Guid.TryParse(id, out Guid referralId);
        expected += id + ".";
        problem.Detail.Should().Be(expected);
        Business.Entities.Referral referral =
          _context.Referrals.FirstOrDefault(t => t.Id == referralId);
        referral.Status.Should().Be(ReferralStatus.New.ToString());

      }

      [Theory]
      [InlineData("no_domain.com")]
      [InlineData("two@domains@yahoo.com")]
      [InlineData("invalid_domain@com")]
      [InlineData("not_an_nhs_domain@yahoo.com")]
      [InlineData("")]
      [InlineData(null)]
      public async Task InvalidEmail(string email)
      {
        // ARRANGE
        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        request.Email = email;
        int expectedStatus = StatusCodes.Status400BadRequest;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.IsType<BadRequestObjectResult>(result);
        ((BadRequestObjectResult)result).StatusCode.Should().Be(expectedStatus);
      }

      [Theory]
      [InlineData("mock.test@nhs.net")]
      [InlineData("Mock.test@nhs.net")]
      [InlineData("Mock.Test@nhs.net")]
      [InlineData("Mock.Test@NHS.net")]
      [InlineData("MOCK.TEST@NHS.NET")]
      public async Task EmailInUse(string differringCaseEmail)
      {
        // ARRANGE
        var email = "mock.test@nhs.net";
        var referral = RandomEntityCreator.CreateRandomReferral(
          email: email);
        _context.Add(referral);
        _context.SaveChanges();

        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        request.Email = differringCaseEmail;
        int expectedStatus = StatusCodes.Status409Conflict;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.IsType<ObjectResult>(result);
        ((ObjectResult)result).StatusCode.Should().Be(expectedStatus);
      }



      [Theory]
      [InlineData("", "Mobile number is an empty string")]
      [InlineData(null, "Mobile number is null")]
      [InlineData("07715427599", "Mobile number does not being with +44")]
      public async Task InvalidMobile(string mobile, string because)
      {
        // ARRANGE
        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        request.Mobile = mobile;
        int expectedStatus = StatusCodes.Status400BadRequest;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.IsType<BadRequestObjectResult>(result);
        ((BadRequestObjectResult)result).StatusCode.Should()
          .Be(expectedStatus, because);
      }

      [Fact]
      public async Task DateOfBmiAtRegistrationMoreThan24MonthsAgo()
      {
        // ARRANGE
        SelfReferralPostRequest request = CreateSelfReferralPostRequest();
        request.DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-740);
        int expectedStatus = StatusCodes.Status400BadRequest;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.IsType<BadRequestObjectResult>(result);
        ((BadRequestObjectResult)result).StatusCode.Should().Be(expectedStatus);
      }
    }

    /// <summary>
    /// Put is used to update teh referral with a selected provider
    /// </summary>
    public class Put : SelfReferralControllerTests
    {
      public Put(ServiceFixture serviceFixture, 
        ITestOutputHelper testOutputHelper) :
        base(serviceFixture, testOutputHelper)
      {
      }

      [Fact]
      public async Task Valid()
      {
        //arrange
        Random rnd = new Random();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            status:ReferralStatus.New,
            email: Generators.GenerateNhsEmail(rnd));
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        SelfReferralPutRequest request = new SelfReferralPutRequest
        {
          Id = referral.Id,
          ProviderId = provider.Id
        };
        //act
        try
        {
          var response = await _controller.Put(request);
          //assert
          response.Should().BeOfType<OkResult>();
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task InValid_TriageLevelNotSet()
      {
        //arrange
        string expected = "Triage completion level is null";
        Random rnd = new Random();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            status: ReferralStatus.New,
            email: Generators.GenerateNhsEmail(rnd));
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        SelfReferralPutRequest request = new SelfReferralPutRequest
        {
          Id = referral.Id,
          ProviderId = provider.Id
        };
        //act
        try
        {
          var response = await _controller.Put(request);
          //assert
          ObjectResult objectResult = Assert.IsType<ObjectResult>(response);
          ProblemDetails problemDetails =
            Assert.IsType<ProblemDetails>(objectResult.Value);
          problemDetails.Status.Should()
            .Be(StatusCodes.Status500InternalServerError);
          problemDetails.Detail.Should().Contain(expected);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task InValid_ProviderNotFound()
      {
        //arrange
        Guid providerId = Guid.NewGuid();
        string expected = 
          $"Provider {providerId} was not found in the list of " +
          $"providers for the selected Triage Level.";
        Random rnd = new Random();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            status: ReferralStatus.New,
            email: Generators.GenerateNhsEmail(rnd));
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        SelfReferralPutRequest request = new SelfReferralPutRequest
        {
          Id = referral.Id,
          ProviderId = providerId
        };
        //act
        try
        {
          var response = await _controller.Put(request);
          //assert
          ObjectResult objectResult = Assert.IsType<ObjectResult>(response);
          ProblemDetails problemDetails =
            Assert.IsType<ProblemDetails>(objectResult.Value);
          problemDetails.Status.Should()
            .Be(StatusCodes.Status400BadRequest);
          problemDetails.Detail.Should().Contain(expected);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      [Fact]
      public async Task Invalid_ReferralNotNew()
      {
        //arrange
        Random rnd = new Random();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            status: ReferralStatus.New,
            email: Generators.GenerateNhsEmail(rnd));
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        referral.ProviderId = provider.Id;
        referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        string expected =
          $"Referral Id {referral.Id} already has a provider selected.";
        SelfReferralPutRequest request = new()
        {
          Id = referral.Id,
          ProviderId = provider.Id
        };
        
        try
        {
          //act
          var response = await _controller.Put(request);
          
          // assert
          var objectResult = Assert.IsType<ObjectResult>(response);
          var problemDetails = Assert
            .IsType<ProblemDetails>(objectResult.Value);
          problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
          problemDetails.Detail.Should().Be(expected);
        }
        finally
        {
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }


    }

    public class GetEthnicities : SelfReferralControllerTests
    {
      public GetEthnicities(
        ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task NotFoundException()
      {
        // ARRANGE
        _context.Ethnicities.RemoveRange(_context.Ethnicities);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.GetEthnicities();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);
      }

      [Fact]
      public async Task Valid()
      {
        //ARRANGE
        _context.Ethnicities.RemoveRange(_context.Ethnicities);
        _context.Ethnicities.Add(new Business.Entities.Ethnicity()
        {
          DisplayName = "Ethnicity1",
          DisplayOrder = 1,
          GroupOrder = 1,
          IsActive = true
        });
        _context.Ethnicities.Add(new Business.Entities.Ethnicity()
        {
          DisplayName = "Ethnicity2",
          DisplayOrder = 2,
          GroupOrder = 1,
          IsActive = true
        });

        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.GetEthnicities();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
      }
    }

    public class GetStaffRoles : SelfReferralControllerTests
    {
      public GetStaffRoles(
        ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task NotFoundException()
      {
        // ARRANGE
        _context.StaffRoles.RemoveRange(_context.StaffRoles);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.GetStaffRoles();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);
      }

      [Fact]
      public async Task Valid()
      {
        // ARRANGE
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

        // ACT
        IActionResult result = await _controller.GetStaffRoles();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
      }

    }

    public class IsEmailInUse : SelfReferralControllerTests
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
      public async Task NotInUse_Return_200()
      {
        // arrange
        var entity = RandomEntityCreator.CreateRandomReferral(
          email: "mock.test@nhs.net");
        _context.Referrals.Add(entity);
        _context.SaveChanges();

        // act
        var result = await _controller.IsEmailInUse(
          new SelfReferralEmailInUse() { Email = "mock2.test@nhs.net" });

        // assert
        var okResult = Assert.IsType<OkResult>(result);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

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
        // arrange
        var entity = RandomEntityCreator.CreateRandomReferral(
          email: "mock.test@nhs.net",
          status: ReferralStatus.Complete);
        _context.Referrals.Add(entity);
        _context.SaveChanges();

        // act
        var result = await _controller.IsEmailInUse(
          new SelfReferralEmailInUse() { Email = differringCaseEmail });

        // assert
        var okResult = Assert.IsType<OkResult>(result);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

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
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          email: "mock.test@nhs.net",
          status: referralStatus,
          providerId: providerIdString == null 
            ? default 
            : new Guid(providerIdString));
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        IActionResult result = await _controller.IsEmailInUse(
          new SelfReferralEmailInUse() { Email = differringCaseEmail });

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);

        // clean up
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
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
        Email = Generators.GenerateNhsEmail(random),
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
}
