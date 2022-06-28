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
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models.GeneralReferral;
using Xunit;
using Xunit.Abstractions;
using Deprivation = WmsHub.Business.Models.Deprivation;
using Ethnicity = WmsHub.Business.Entities.Ethnicity;
using Provider = WmsHub.Business.Entities.Provider;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public partial class GeneralReferralControllerTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IProviderService _providerService;
    private readonly IReferralService _referralService;
    private readonly GeneralReferralController _controller;
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

    public GeneralReferralControllerTests(
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

      _controller = new GeneralReferralController(
        _referralService,
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

      Log.Logger = new LoggerConfiguration()
      .WriteTo.TestOutput(testOutputHelper)
      .CreateLogger();
    }

    public class Post : GeneralReferralControllerTests
    {
      public Post(
        ServiceFixture serviceFixture, 
        ITestOutputHelper testOutputHelper)
        : base(
            serviceFixture, 
            testOutputHelper)
      {

        _context.Ethnicities.RemoveRange(_context.Ethnicities);
        _context.Ethnicities.Add(new Ethnicity()
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

        _context.Referrals.RemoveRange(_context.Referrals);
        _context.Providers.RemoveRange(_context.Providers);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid()
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();
        // ACT

        IActionResult result = await _controller.Post(request);

        // ASSERT
        var okObjectResult = Assert.IsType<OkObjectResult>(result);

        IReferralPostResponse response =
          Assert.IsType<SelfReferralPostResponse>(okObjectResult.Value);

        response.ProviderChoices.Count().Should().Be(1);
        ProviderForSelection selectedProvider = response
          .ProviderChoices.Single();
        selectedProvider.Id.Should().Be(provider.Id);
        selectedProvider.Logo.Should().Be(provider.Logo);
        selectedProvider.Name.Should().Be(provider.Name);
        selectedProvider.Summary.Should().Be(provider.Summary3, 
          because: "the mocked TriagedCompletionLevel is High which should " +
          "return the level 3 summary.");
        selectedProvider.Website.Should().Be(provider.Website);

        Business.Entities.Referral entity =
          _context.Referrals.Single(t => t.Id == response.Id);

        entity.Should().BeEquivalentTo(request);

        entity.Status.Should().Be(ReferralStatus.New.ToString());
        entity.ReferralSource.Should()
          .Be(ReferralSource.GeneralReferral.ToString());
      }

      [Fact]
      public async Task IsPregnant_True_400()
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.IsPregnant = true;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = Assert
          .IsType<ValidationProblemDetails>(objectResult.Value);

        problemDetails.Errors
          .First().Key.Should().Be(nameof(request.IsPregnant));
      }

      [Theory]
      [InlineData(false)]
      [InlineData(null)]
      public async Task IsPregnant_FalseOrNull_200(bool? isPregnant)
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.IsPregnant = isPregnant;
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<OkObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      [Fact]
      public async Task HasHadBariatricSurgery_True_400()
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.HasHadBariatricSurgery = true;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = Assert
          .IsType<ValidationProblemDetails>(objectResult.Value);

        problemDetails.Errors
          .First().Key.Should().Be(nameof(request.HasHadBariatricSurgery));
      }

      [Theory]
      [InlineData(false)]
      [InlineData(null)]
      public async Task HasHadBariatricSurgery_FalseOrNull_200(
        bool? hasHadBariatricSurgery)
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.HasHadBariatricSurgery = hasHadBariatricSurgery;
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<OkObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      [Fact]
      public async Task HasActiveEatingDisorder_True_400()
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.HasActiveEatingDisorder = true;

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = Assert
          .IsType<ValidationProblemDetails>(objectResult.Value);

        problemDetails.Errors
          .First().Key.Should().Be(nameof(request.HasActiveEatingDisorder));
      }

      [Theory]
      [InlineData(false)]
      [InlineData(null)]
      public async Task HasActiveEatingDisorder_False_200(
        bool? hasActiveEatingDisorder)
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
        request.HasActiveEatingDisorder = hasActiveEatingDisorder;
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.Post(request);

        // ASSERT
        var objectResult = Assert.IsType<OkObjectResult>(result);
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      [Fact]
      public async Task Valid_ButNoContent_204()
      {
        // ARRANGE
        string expected =
          "Unable to find provider choices for the referral ";
        PostRequest request = CreateReferralPostRequest();
        // ACT

        IActionResult result = await _controller.Post(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<ProblemDetails>(((ObjectResult)result).Value);
        // TODO - complete asserts for all properties
        ProblemDetails problem =
          (ProblemDetails)((ObjectResult)result).Value;
        problem.Status.Should().Be((int)HttpStatusCode.NoContent);
        var id = problem.Detail.Replace(expected, "").Replace(".", "");
        Guid.TryParse(id, out Guid referralId);
        expected += id + ".";
        problem.Detail.Should().Be(expected);
        Business.Entities.Referral referral =
          _context.Referrals.FirstOrDefault(t => t.Id == referralId);
        referral.Status.Should().Be(ReferralStatus.New.ToString());

      }


      [Theory]
      [InlineData("", "Mobile number is an empty string")]
      [InlineData(null, "Mobile number is null")]
      [InlineData("07715427599", "Mobile number does not being with +44")]
      public async Task InvalidMobile(string mobile, string because)
      {
        // ARRANGE
        PostRequest request = CreateReferralPostRequest();
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
        PostRequest request = CreateReferralPostRequest();
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
    /// Put is used to update the referral with a selected provider
    /// </summary>
    public class Put : GeneralReferralControllerTests
    {
      public Put(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper) :
        base(serviceFixture, testOutputHelper)
      {
      }

      [Fact]
      public async Task Valid_Status_New()
      {
        //arrange
        Random rnd = new Random();

        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        
        Business.Entities.Referral referral = RandomEntityCreator
          .CreateRandomGeneralReferral();
        
        PutRequest request = await SetupPutRequest(
          provider: provider,
          setProviderNull: true,
          referral: referral);

        //act
        try
        {
          var result = await _controller.Put(request, referral.Id);

          //assert
          var okObjectResult = Assert.IsType<OkObjectResult>(result);

          IReferralPostResponse response =
            Assert.IsType<SelfReferralPostResponse>(okObjectResult.Value);

          response.ProviderChoices.Count().Should().Be(1);
          ProviderForSelection selectedProvider = response
            .ProviderChoices.Single();
          selectedProvider.Id.Should().Be(provider.Id);
          selectedProvider.Logo.Should().Be(provider.Logo);
          selectedProvider.Name.Should().Be(provider.Name);
          selectedProvider.Summary.Should().Be(provider.Summary3,
            because: "the mocked TriagedCompletionLevel is High which should " +
            "return the level 3 summary.");
          selectedProvider.Website.Should().Be(provider.Website);

          Business.Entities.Referral entity =
            _context.Referrals.Single(t => t.Id == response.Id);

          entity.Should().BeEquivalentTo(request);

          entity.Status.Should().Be(ReferralStatus.New.ToString());
          entity.ReferralSource.Should()
            .Be(ReferralSource.GeneralReferral.ToString());

        }
        finally
        {
          _context.Referrals.Remove(referral);
          _context.Providers.Remove(provider);
          _context.SaveChanges();
        }
      }

      private static PutRequest GetPutRequest(
        Business.Entities.Referral referral, Provider provider)
      {
        PutRequest request = new PutRequest
        {
          Id = referral.Id,
          Address1 = referral.Address1,
          Address2 = referral.Address2,
          Address3 = referral.Address3,
          Email = referral.Email,
          WeightKg = referral.WeightKg ?? 120m,
          HeightCm = referral.HeightCm ?? 181m,
          Ethnicity = referral.Ethnicity,
          ServiceUserEthnicity = referral.ServiceUserEthnicity,
          ServiceUserEthnicityGroup = referral.ServiceUserEthnicityGroup,
          DateOfBirth = referral.DateOfBirth,
          Sex = referral.Sex,
          FamilyName = referral.FamilyName,
          GivenName = referral.GivenName,
          Postcode = referral.Postcode,
          Telephone = referral.Telephone,
          Mobile = referral.Mobile,
          ReferringGpPracticeNumber = referral.ReferringGpPracticeNumber,
          ConsentForFutureContactForEvaluation =
            referral.ConsentForFutureContactForEvaluation,
          ConsentForReferrerUpdatedWithOutcome =
            referral.ConsentForReferrerUpdatedWithOutcome,
          DateOfBmiAtRegistration = referral.DateOfBmiAtRegistration,
          HasALearningDisability = referral.HasALearningDisability,
          HasAPhysicalDisability = referral.HasAPhysicalDisability,
          HasActiveEatingDisorder = referral.HasActiveEatingDisorder,
          HasArthritisOfHip = referral.HasArthritisOfHip,
          HasArthritisOfKnee = referral.HasArthritisOfKnee,
          HasDiabetesType1 = referral.HasDiabetesType1,
          HasDiabetesType2 = referral.HasDiabetesType2,
          HasHadBariatricSurgery = referral.HasHadBariatricSurgery,
          HasHypertension = referral.HasHypertension,
          IsPregnant = referral.IsPregnant,
          NhsNumber = referral.NhsNumber,
          ConsentForGpAndNhsNumberLookup = true,
          NhsLoginClaimEmail = $"Put{referral.NhsLoginClaimEmail}",
          NhsLoginClaimFamilyName= $"Put{referral.NhsLoginClaimFamilyName}",
          NhsLoginClaimGivenName = $"Put{referral.NhsLoginClaimGivenName}",
          NhsLoginClaimMobile = Generators.GenerateMobile(new Random())
        };
        return request;
      }

      [Fact]
      public async Task Invalid_ProviderPreviouslySelected()
      {
        //arrange
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);

        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomGeneralReferral();

        PutRequest request = await SetupPutRequest(
          provider: provider,
          referral: referral);

        string expected =
          $"The referral {referral.Id} has previously had its provider " +
          $"selected {provider.Id}.";

        //act
        try
        {
          var response = await _controller.Put(request, referral.Id);
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
      public async Task Valid_ProviderNotSelected()
      {
        //arrange
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);

        Business.Entities.Referral referral = RandomEntityCreator
          .CreateRandomGeneralReferral();

        PutRequest request = await SetupPutRequest(
          provider: provider,
          referral: referral,
          setProviderNull: true);

        string expected =
          $"The referral {referral.Id} has previously had its provider " +
          $"selected {provider.Id}.";
        //act
        try
        {
          var response = await _controller.Put(request, referral.Id);
          //assert
          OkObjectResult objectResult = Assert.IsType<OkObjectResult>(response);
          SelfReferralPostResponse result =
            Assert.IsType<SelfReferralPostResponse>(objectResult.Value);
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

      [Theory]
      [MemberData(nameof(TestStatuses))]
      public async Task Invalid_ReferrralStatusIsNotValid(
        ReferralStatus status, bool isAllowed)
      {
        //arrange
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Random rnd = new Random();
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomGeneralReferral();
        PutRequest request = await SetupPutRequest(
          provider: provider,
          referral: referral,
          setProviderNull: true,
          referralStatus: status.ToString());
        string expected =
          $"Referral {referral.Id} has a status of {referral.Status} and" +
          $" cannot be updated";
        //act
        try
        {
          var response = await _controller.Put(request, referral.Id);
          //assert

          if (isAllowed)
          {
            OkObjectResult objectResult = Assert.IsType<OkObjectResult>(response);
            SelfReferralPostResponse result =
              Assert.IsType<SelfReferralPostResponse>(objectResult.Value);
          }
          else
          {
            ObjectResult objectResult = Assert.IsType<ObjectResult>(response);
            ProblemDetails problemDetails =
              Assert.IsType<ProblemDetails>(objectResult.Value);
            problemDetails.Status.Should()
              .Be(StatusCodes.Status409Conflict);
            problemDetails.Detail.Should().Contain(expected);
          }
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

      private async Task<PutRequest> SetupPutRequest(
        Provider provider = null,
        bool setProviderNull = false,
        Business.Entities.Referral referral = null,
        string referralStatus = null,
        string referralSource = null,
        DateTimeOffset? dateofBmiRegistration = null)
      {
        referral.DateOfBmiAtRegistration =
          dateofBmiRegistration ?? DateTimeOffset.Now.AddDays(-1);
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        referral.ProviderId = setProviderNull ? null : provider.Id;
        referral.Provider = setProviderNull ? null : provider;
        referral.Status = referralStatus ?? ReferralStatus.New.ToString();
        referral.ConsentForGpAndNhsNumberLookup = true;
        referral.ConsentForFutureContactForEvaluation = false;
        referral.ConsentForReferrerUpdatedWithOutcome = true;
        referral.ReferralSource =
          referralSource ?? ReferralSource.GeneralReferral.ToString();
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        PutRequest request = GetPutRequest(referral, provider);
        await SetEthnicity(request);
        return request;
      }

      private async Task SetEthnicity(PutRequest request)
      {
        var ethnicities = await _context.Ethnicities.ToArrayAsync();
        Random random = new Random();
        try
        {
          if (ethnicities == null || ethnicities.Length < 4)
          {
            _context.Ethnicities.RemoveRange(_context.Ethnicities);
            base._serviceFixture.PopulateEthnicities(_context);
            await _context.SaveChangesAsync();
            ethnicities = await _context.Ethnicities.ToArrayAsync();
          }

          int num = random.Next(0, ethnicities.Length - 1);
          Ethnicity ethnicity = ethnicities[num];
          request.Ethnicity = ethnicity.TriageName;
          request.ServiceUserEthnicityGroup = ethnicity.GroupName;
          request.ServiceUserEthnicity = ethnicity.DisplayName;
        }
        catch (Exception)
        {
          base._serviceFixture.PopulateEthnicities(_context);
          await _context.SaveChangesAsync();
          ethnicities = await _context.Ethnicities.ToArrayAsync();
          int num = random.Next(0, ethnicities.Length - 1);
          Ethnicity ethnicity = ethnicities[num];
          request.Ethnicity = ethnicity.TriageName;
          request.ServiceUserEthnicityGroup = ethnicity.GroupName;
          request.ServiceUserEthnicity = ethnicity.DisplayName;
        }
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
          ReferralStatus.ChatBotCall2 |
          ReferralStatus.ChatBotTransfer;

        foreach (int value in Enum.GetValues(typeof(ReferralStatus)))
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


    public class NhsNumbersInUseTests : GeneralReferralControllerTests
    {
      public NhsNumbersInUseTests(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper) :
        base(serviceFixture, testOutputHelper)
      {
      }

      [Theory]
      [MemberData(nameof(NhsLookupStatuses))]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
      "xUnit1026:Theory methods should use all of their parameters",
      Justification = "<Pending>")]
      public async Task NhsNumberLookupDependantReturnTypes(
        string message,
        bool isValidNhsNumber,
        bool isProviderIncluded,
        ReferralStatus status,
        int date,
        Type type,
        bool expectEmptyArray,
        int expected)
      {
        //arrange
        Random rnd = new Random();
        Provider provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        Business.Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            status: status,
            email: Generators.GenerateNhsEmail(rnd));
        referral.TriagedCompletionLevel = "3";
        referral.TriagedWeightedLevel = "2";
        referral.ReferralSource = ReferralSource.GeneralReferral.ToString();
        referral.DateOfReferral = DateTimeOffset.Now.AddMonths(date);
        if (isProviderIncluded)
        {
          referral.Provider = provider;
        }
        _context.Referrals.Add(referral);
        _context.SaveChanges();
        //act
        try
        {
          var response =
            await _controller.GetNhsNumber(
              isValidNhsNumber ? referral.NhsNumber : "12345678901");
          //assert
          response.GetType().Should().Be(type);
          if (type == typeof(OkObjectResult))
          {
            Validate_OkObjectResult(expected, response, expectEmptyArray);
          }

          if (type == typeof(ObjectResult))
          {
            ObjectResult result = (ObjectResult)response;
            result.StatusCode.Should().Be(expected);
          }

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

      private static void Validate_OkObjectResult(
        int expected, IActionResult response, bool expectEmptyArray)
      {

        var result = Assert.IsType<OkObjectResult>(response);
        result.StatusCode.Should().Be(expected);
        var okResponse = Assert.IsType<GetNhsNumberOkResponse>(result.Value);
        okResponse.Should().NotBeNull();
      }

      public static IEnumerable<object[]> NhsLookupStatuses()
      {
        List<object[]> validData = new();

        bool nhsNumberTrue = true;
        bool nhsNumberFalse = false;
        bool providerTrue = true;
        bool providerFalse = false;

        validData.Add(new object[]
        {
          "Invalid NHS Number", nhsNumberFalse, providerTrue,
          ReferralStatus.New, -1,
          typeof(ObjectResult), false,(int)HttpStatusCode.BadRequest
        });
        validData.Add(new object[]
        {
          "NHS Match Provider Is Null", nhsNumberTrue, providerFalse,
          ReferralStatus.New, -1,
          typeof(OkObjectResult), false, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "NHS Match Provider Is Null", nhsNumberTrue, providerFalse,
          ReferralStatus.TextMessage1, -1,
          typeof(OkObjectResult), false, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "NHS Match Provider Is Null", nhsNumberTrue, providerFalse,
          ReferralStatus.TextMessage2, -1,
          typeof(OkObjectResult), false, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "NHS Match CancelledByEReferrals", nhsNumberTrue, providerFalse,
          ReferralStatus.CancelledByEreferrals, -1,
          typeof(NoContentResult),
          true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "NHS Match CancelledDuplicate", nhsNumberTrue, providerFalse,
          ReferralStatus.CancelledDuplicate, -1,
          typeof(NoContentResult),
          true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "NHS Match CancelledDuplicateTextMessage", nhsNumberTrue,
          providerFalse, ReferralStatus.CancelledDuplicateTextMessage, -1,
          typeof(NoContentResult),
          true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "RmcCall", nhsNumberTrue, false,
          ReferralStatus.RmcCall, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "ProviderCompleted", nhsNumberTrue, providerTrue,
          ReferralStatus.ProviderCompleted, -2,
          typeof(ConflictObjectResult), true, (int)HttpStatusCode.Conflict
        });
        validData.Add(new object[]
        {
          "ChatBotCall1", nhsNumberTrue, providerFalse,
          ReferralStatus.ChatBotCall1, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "ChatBotCall2", nhsNumberTrue, providerFalse,
          ReferralStatus.ChatBotCall2, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "ChatBotTransfer", nhsNumberTrue, providerFalse,
          ReferralStatus.ChatBotTransfer, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "FailedToContact", nhsNumberTrue, providerFalse,
          ReferralStatus.FailedToContact, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "Exception", nhsNumberTrue, providerFalse,
          ReferralStatus.Exception, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "FailedToContactTextMessage", nhsNumberTrue, providerFalse,
          ReferralStatus.FailedToContactTextMessage, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "Letter", nhsNumberTrue, providerFalse,
          ReferralStatus.Letter, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "LetterSent", nhsNumberTrue, providerFalse,
          ReferralStatus.LetterSent, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "ProviderAccepted", nhsNumberTrue, providerFalse,
          ReferralStatus.ProviderAccepted, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });
        validData.Add(new object[]
        {
          "ProviderAwaitingStart", nhsNumberTrue, providerFalse,
          ReferralStatus.ProviderAwaitingStart, -2,
          typeof(OkObjectResult), true, (int)HttpStatusCode.OK
        });


        return validData;
      }
    }

    public class GetEthnicities : GeneralReferralControllerTests
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

    private PostRequest CreateReferralPostRequest()
    {
      Random rnd = new Random();

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
        Email = Generators.GenerateEmail(rnd),
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
        NhsLoginClaimEmail = Generators.GenerateEmail(rnd),
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
}
