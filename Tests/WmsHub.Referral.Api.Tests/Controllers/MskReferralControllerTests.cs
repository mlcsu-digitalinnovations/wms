using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests.Controllers
{
  public class MskReferralControllerTestsFixture : IDisposable
  {
    internal readonly string MSK_APIKEY = "MskApiKey";
    internal readonly string MSK_APIKEYVALUE =
      "5ezlaUFEL6t!J4fr1x*nt#knG8*n1G!xN%dzVaT4pvb&!I6S#M";
    internal readonly string MSK_CLAIMTYPE = "MskClaimType";
    internal readonly string MSK_CLAIMTYPEVALUE =
      "*mgBTZG%ffPVF1&XWHmVcj20$e4eY*SGAy5#G7kYvFhZheO8X%";
    internal readonly string REFERRALSERVICE_APIKEY = "ApiKey";
    internal readonly string REFERRALSERVICE_APIKEYVALUE =
      "3ZuWAu2PxUz@%1GI&7sTj9V2d9UnTVGImV58fh7R%Hv4u54d7$";

    public IHost Host { get; private set; }
    public Mock<IReferralService> MockReferralService { get; private set; }
      = new();
    public Mock<IOptions<MskReferralOptions>>  MockMskReferralOptions
    { get; private set; } = new() { CallBase = true };

    public MskReferralControllerTestsFixture()
    {
      Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .UseSerilog((ctx, loggerConfig) => loggerConfig.WriteTo.Debug())
        .ConfigureWebHostDefaults(builder =>
        {
          builder.UseTestServer().UseStartup<Startup>();
        })        
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          config.AddInMemoryCollection(new Dictionary<string, string>()
          {
            { MSK_APIKEY, MSK_APIKEYVALUE },
            { MSK_CLAIMTYPE, MSK_CLAIMTYPEVALUE },
            { REFERRALSERVICE_APIKEY, REFERRALSERVICE_APIKEYVALUE }
          });
        })
        .ConfigureServices(services =>
        {
          // replace db context with in memory db context
          var context = services.FirstOrDefault(
            s => s.ServiceType == typeof(Business.DatabaseContext));
          if (context != null)
          {
            services.Remove(context);
            var options = services
              .Where(r => (r.ServiceType == typeof(DbContextOptions))
                || (r.ServiceType.IsGenericType
                  && r.ServiceType.GetGenericTypeDefinition()
                    == typeof(DbContextOptions<>)))
              .ToArray();
            foreach (var option in options)
            {
              services.Remove(option);
            }
          }

          services.AddDbContext<Business.DatabaseContext>(
            opt => opt.UseInMemoryDatabase("InMemWmsHub"));

          // replace the referral service with a mock referral service
          context = services.FirstOrDefault(
            s => s.ServiceType == typeof(IReferralService));
          if (context != null)
          {
            services.Remove(context);
            services.AddScoped(sp => MockReferralService.Object);
          }

          // replace the msk referral options with a mock
          context = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IConfigureOptions<MskReferralOptions>));
          if (context != null)
          {
            services.Remove(context);
            services.AddScoped(sp => MockMskReferralOptions.Object);
          }
        })
        .Start();
    }

    public void Dispose()
    {
      Host.Dispose();
    }
  }

  [CollectionDefinition("MskReferralControllerTestsCollection")]
  public class MskReferralControllerTestsCollection
    : ICollectionFixture<MskReferralControllerTestsFixture>
  { }

  public class MskReferralControllerTests : ABaseTests
  {
    private const string HEADER_APIKEY = "x-api-key";
    private const string TEST_NHSNUMBER = "9996529991";
    private readonly Mock<IReferralService> _mockReferralService;
    private readonly Mock<IOptions<MskReferralOptions>> _mockMskReferralOptions;
    private readonly Dictionary<string, string> _mockMskHubs = new();
    private readonly MskReferralControllerTestsFixture _fixture;
    private readonly TestServer _testServer;
    protected static readonly List<string> _apiVersions = new() { "", "/v1" };

    public MskReferralControllerTests(
      MskReferralControllerTestsFixture fixture,
      ITestOutputHelper testOutput)
    {
      _testOutput = testOutput;
      _fixture = fixture;
      _mockMskHubs = Generators.GetMskHubs();
      _mockMskReferralOptions = fixture.MockMskReferralOptions;
      _mockReferralService = fixture.MockReferralService;
      _mockReferralService.Invocations.Clear();
      _testServer = fixture.Host.GetTestServer();
      _testServer.BaseAddress = new Uri("https://localhost");

      Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Verbose()
      .WriteTo.TestOutput(_testOutput)
      .CreateLogger();
    }

    [Collection("MskReferralControllerTestsCollection")]
    public class GenericTests : MskReferralControllerTests
    {
      public GenericTests(
        MskReferralControllerTestsFixture fixture,
        ITestOutputHelper testOutput)
        : base(fixture, testOutput)
      { }

      public static TheoryData<string, string> EndpointsTheoryData()
      {
        TheoryData<string, string> endpoints = new();

        _apiVersions.ForEach(endpoint => endpoints
          .Add(GetEthnicities.METHOD, $"{endpoint}{GetEthnicities.PATH}"));

        _apiVersions.ForEach(endpoint => endpoints
          .Add(IsNhsNumberInUseTests.METHOD,
            $"{endpoint}{IsNhsNumberInUseTests.PATH}/{TEST_NHSNUMBER}"));

        _apiVersions.ForEach(endpoint => endpoints
          .Add(PostTests.METHOD, $"{endpoint}{PostTests.PATH}"));

        return endpoints;
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NoApiKey_401(string method, string path)
      {
        // arrange
        var action = new Action<HttpContext>(c =>
        {
          c.Request.Method = method;
          c.Request.Path = path;
        });

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status401Unauthorized);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task UnauthorizedApiKey_403(string method, string path)
      {
        // arrange
        var action = new Action<HttpContext>(c =>
        {
          c.Request.Method = method;
          c.Request.Path = path;
          c.Request.Headers
            .Add(HEADER_APIKEY, _fixture.REFERRALSERVICE_APIKEYVALUE);
        });

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status403Forbidden);
      }
    }

    [Collection("MskReferralControllerTestsCollection")]
    public class GetEthnicities : MskReferralControllerTests
    {
      public GetEthnicities(
        MskReferralControllerTestsFixture fixture,
        ITestOutputHelper testOutput)
        : base(fixture, testOutput)
      { }

      public const string METHOD = "GET";
      public const string PATH = "/MskReferral/Ethnicity";

      public static TheoryData<string> EndpointsTheoryData()
      {
        TheoryData<string> endpoints = new();
        _apiVersions.ForEach(root => endpoints.Add($"{root}{PATH}"));
        return endpoints;
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task EthnicitiesAvailable_200(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        var ethnicity = RandomModelCreator.CreateRandomEthnicity();
        _mockReferralService
          .Setup(x => x.GetEthnicitiesAsync(It.IsAny<ReferralSource>()))
          .ReturnsAsync(new List<Business.Models.Ethnicity>() { ethnicity });

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        _mockReferralService.Verify(x => x.GetEthnicitiesAsync(
            It.IsAny<ReferralSource>()),
            Times.Once);

        var ethnicities = await JsonSerializer
          .DeserializeAsync<IEnumerable<Api.Models.Ethnicity>>(
            context.Response.Body,
            options: new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true });

        ethnicities.Should().HaveCount(1);
        ethnicities.Single().Should().BeEquivalentTo(ethnicity, opt => opt
          .Excluding(e => e.OldName)
          .Excluding(e => e.MinimumBmi)
          .Excluding(e => e.Id)
          .Excluding(e => e.IsActive)
          .Excluding(e => e.ModifiedAt)
          .Excluding(e => e.ModifiedByUserId));
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task EthnicitiesException_500(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        _mockReferralService
          .Setup(x => x.GetEthnicitiesAsync(It.IsAny<ReferralSource>()))
          .ThrowsAsync(new EthnicityNotFoundException());

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);
        _mockReferralService.Verify(x => x.GetEthnicitiesAsync(
            It.IsAny<ReferralSource>()),
            Times.Once);
      }

      private async Task<Action<HttpContext>> CreateAction(
        string path)
      {
        var action = new Action<HttpContext>(c =>
        {
          c.Request.Method = HttpMethods.Get;
          c.Request.Path = path;
          c.Request.Headers.Add(HEADER_APIKEY, _fixture.MSK_APIKEYVALUE);
        });
        return action;
      }
    }

    [Collection("MskReferralControllerTestsCollection")]
    public class GetPilotMskHubs : MskReferralControllerTests
    {
      public GetPilotMskHubs(
        MskReferralControllerTestsFixture fixture,
        ITestOutputHelper testOutput)
        : base(fixture, testOutput)
      {
        // this is required because some tests in GetPilotMskHubs add 
        // setups that will break these tests
        _mockMskReferralOptions.Reset();
        _mockMskReferralOptions
          .Setup(x => x.Value.MskHubs)
          .Returns(_mockMskHubs);
      }

      public const string METHOD = "GET";
      public const string PATH = "/MskReferral/MskHub";

      public static TheoryData<string> EndpointsTheoryData()
      {
        TheoryData<string> endpoints = new();
        _apiVersions.ForEach(root => endpoints.Add($"{root}{PATH}"));
        return endpoints;
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task WhitelistHasValues_200(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        var mskHubs = await JsonSerializer
          .DeserializeAsync<IEnumerable<MskHub>>(
            context.Response.Body,
            options: new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true });
        mskHubs.First().Name.Should().Be(_mockMskHubs.Values.First());
        mskHubs.First().OdsCode.Should().Be(_mockMskHubs.Keys.First());
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task WhitelistHasNoValues_500(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        _mockMskReferralOptions
          .Setup(x => x.Value.WhitelistHasValues)
          .Returns(false);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task WhitelistHasValuesButMskHubsEmpty_500(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        _mockMskReferralOptions
          .Setup(x => x.Value.MskHubs)
          .Returns<Dictionary<string, string>>(null);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }

      private async Task<Action<HttpContext>> CreateAction(
        string path)
      {
        var action = new Action<HttpContext>(c =>
        {
          c.Request.Method = HttpMethods.Get;
          c.Request.Path = path;
          c.Request.Headers.Add(HEADER_APIKEY, _fixture.MSK_APIKEYVALUE);
        });
        return action;
      }
    }

    [Collection("MskReferralControllerTestsCollection")]
    public class IsNhsNumberInUseTests : MskReferralControllerTests
    {
      public IsNhsNumberInUseTests(
        MskReferralControllerTestsFixture fixture,
        ITestOutputHelper testOutput)
        : base(fixture, testOutput)
      { }

      public const string METHOD = "GET";
      public const string PATH = "/MskReferral/NhsNumber";

      public static TheoryData<string> EndpointsTheoryData()
      {
        TheoryData<string> endpoints = new();
        _apiVersions.ForEach(root => endpoints.Add($"{root}{PATH}"));
        return endpoints;
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NhsNumberIsNotInUse_204(string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()))
          .ReturnsAsync(new InUseResponse());

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status204NoContent);
        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()), Times.Once);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task InvalidNhsNumber_400(string path)
      {
        // arrange
        string nhsNumber = "1234567890";
        Action<HttpContext> action = await CreateAction(path, nhsNumber);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()));

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        var problemDetails = await JsonSerializer
          .DeserializeAsync<ProblemDetails>(context.Response.Body);
        Log.Information(problemDetails.Detail);
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);

        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()),
            Times.Never);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NhsNumberIsInUse_409(string path)
      {
        // arrange
        string expectedErrorMsg =
          $"NHS number {TEST_NHSNUMBER} is already in use.";
        Action<HttpContext> action = await CreateAction(path);

        var mockInUseResponse = new Mock<InUseResponse>();
        mockInUseResponse.Setup(x => x.InUseResult)
          .Returns(InUseResult.Found);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()))
          .ReturnsAsync(mockInUseResponse.Object);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()), Times.Once);

        var problemDetails = await JsonSerializer
          .DeserializeAsync<ProblemDetails>(context.Response.Body);
        problemDetails.Detail.Should().Be(expectedErrorMsg);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NhsNumberIsInUseCancelledWithPreviousProvider_204(
        string path)
      {
        // arrange
        Action<HttpContext> action = await CreateAction(path);

        var mockInUseResponse = new Mock<InUseResponse>();
        mockInUseResponse.Setup(x => x.InUseResult)
          .Returns(InUseResult.Found
            | InUseResult.Cancelled
            | InUseResult.ProviderNotSelected);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()))
          .ReturnsAsync(mockInUseResponse.Object);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()), Times.Once);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NhsNumberInUseButCancelledWithPreviousProvider_409(
        string path)
      {
        // arrange
        string expectedErrorMsg = $"NHS number {TEST_NHSNUMBER} was " +
          $"previously used with a referral that had selected a provider.";
        Action<HttpContext> action = await CreateAction(path);

        var mockInUseResponse = new Mock<InUseResponse>();
        mockInUseResponse.Setup(x => x.InUseResult)
          .Returns(InUseResult.Found
            | InUseResult.Cancelled
            | InUseResult.ProviderSelected);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()))
          .ReturnsAsync(mockInUseResponse.Object);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()), Times.Once);

        var problemDetails = await JsonSerializer
          .DeserializeAsync<ProblemDetails>(context.Response.Body);
        problemDetails.Detail.Should().Be(expectedErrorMsg);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task NhsNumberIsInUseThrowsException_500(string path)
      {
        // arrange
        string expectedErrorMsg = "Test Exception";
        Exception ex = new(expectedErrorMsg);
        Action<HttpContext> action = await CreateAction(path);

        _mockReferralService
          .Setup(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()))
          .ThrowsAsync(ex);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);
        _mockReferralService
          .Verify(x => x.IsNhsNumberInUseAsync(It.IsAny<string>()), Times.Once);

        var problemDetails = await JsonSerializer
          .DeserializeAsync<ProblemDetails>(context.Response.Body);
        problemDetails.Detail.Should().Be(expectedErrorMsg);
      }

      private async Task<Action<HttpContext>> CreateAction(
        string path,
        string nhsNumber = TEST_NHSNUMBER)
      {
        var action = new Action<HttpContext>(c =>
        {
          c.Request.Method = METHOD;
          c.Request.Path = $"{path}/{nhsNumber}";
          c.Request.Headers.Add(HEADER_APIKEY, _fixture.MSK_APIKEYVALUE);
        });
        return action;
      }
    }

    [Collection("MskReferralControllerTestsCollection")]
    public class PostTests : MskReferralControllerTests
    {
      public PostTests(
        MskReferralControllerTestsFixture fixture,
        ITestOutputHelper testOutput)
        : base(fixture, testOutput)
      {
        // this is required because some tests in GetPilotMskHubs add 
        // setups that will break these tests
        _mockMskReferralOptions.Reset();
        _mockMskReferralOptions
          .Setup(x => x.Value.MskHubs)
          .Returns(_mockMskHubs);
      }

      public const string METHOD = "POST";
      public const string PATH = "/MskReferral";


      public static TheoryData<string> EndpointsTheoryData()
      {
        TheoryData<string> endpoints = new();
        _apiVersions.ForEach(root => endpoints.Add($"{root}{PATH}"));
        return endpoints;
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task PostRequest_Valid_204(string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest();

        IMskReferralCreate mskReferralCreate = null;

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()))
          .Callback((IMskReferralCreate value) =>
          { mskReferralCreate = value; });

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        _mockReferralService.Verify(x => x
          .CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()), Times.Once);
        mskReferralCreate.Should().BeEquivalentTo(postRequest, opt => opt
          .WithMapping<IMskReferralCreate>(
            p => p.ReferringMskClinicianEmailAddress,
            r => r.ReferringMskClinicianEmailAddress));
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task MskOdsCodeNotInWhitelistButDisabled_204(string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest(
            referringMskHubOdsCode: "X11111");

        IMskReferralCreate mskReferralCreate = null;

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()))
          .Callback((IMskReferralCreate value) =>
          { mskReferralCreate = value; });

        _mockMskReferralOptions
          .Setup(x => x.Value.IsWhitelistEnabled)
          .Returns(false);

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        _mockReferralService.Verify(x => x
          .CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()), Times.Once);
        mskReferralCreate.Should().BeEquivalentTo(postRequest, opt => opt
          .WithMapping<IMskReferralCreate>(
            p => p.ReferringMskClinicianEmailAddress,
            r => r.ReferringMskClinicianEmailAddress));
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task PostRequestInvalid_400(string path)
      {
        // arrange
        PostRequest postRequest = new();

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()));

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);
        _mockReferralService.Verify(x => x.CreateMskReferralAsync(
          It.IsAny<IMskReferralCreate>()),
          Times.Never);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task PostRequestValid_InvalidMskHub_400(string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest(
            referringMskHubOdsCode: "Invalid");

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()));

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);

        _mockReferralService.Verify(x => x.CreateMskReferralAsync(
          It.IsAny<IMskReferralCreate>()),
          Times.Never);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task MskReferralValidationException_400(
        string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest();

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()))
          .ThrowsAsync(new MskReferralValidationException(
            new() { new ValidationResult("Test Exception") }));

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status400BadRequest);
        _mockReferralService.Verify(x => x
          .CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()), Times.Once);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task ReferralNotUniqueException_409(
        string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest();

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()))
          .ThrowsAsync(new ReferralNotUniqueException());

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _mockReferralService.Verify(x => x
          .CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()), Times.Once);
      }

      [Theory]
      [MemberData(nameof(EndpointsTheoryData))]
      public async Task Exception_500(string path)
      {
        // arrange
        PostRequest postRequest = RandomModelCreator
          .CreateRandomMskReferralPostRequest();

        Action<HttpContext> action = await CreateAction(path, postRequest);

        _mockReferralService
          .Setup(x => x.CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()))
          .ThrowsAsync(new Exception());

        // act
        HttpContext context = await _testServer.SendAsync(action);

        // assert        
        await LogProblemDetailsAsync(context);
        context.Response.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);
        _mockReferralService.Verify(x => x
          .CreateMskReferralAsync(It.IsAny<IMskReferralCreate>()), Times.Once);
      }

      private async Task<Action<HttpContext>> CreateAction(
        string path,
        PostRequest request)
      {
        MemoryStream memoryStream = new();
        await JsonSerializer.SerializeAsync(memoryStream, request);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var action = new Action<HttpContext>(c =>
          {
            c.Request.Method = METHOD;
            c.Request.Path = $"{path}";
            c.Request.Headers.Add(HEADER_APIKEY, _fixture.MSK_APIKEYVALUE);
            c.Request.Body = memoryStream;
            c.Request.ContentType = "application/json";
          });

        return action;
      }
    }
  }
}
