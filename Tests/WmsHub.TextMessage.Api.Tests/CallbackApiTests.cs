#nullable enable
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.Notify;
using Xunit;
using static WmsHub.Tests.Helper.EnvironmentVariableConfigurator;
using Provider = WmsHub.Business.Entities.Provider;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.TextMessage.Api.Tests;

public class BaseCommon
{
  public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
  public const string _ubrn = "120000000001";
  public const string toNumber = "+447512751212";
  public DatabaseContext _ctx;

  public BaseCommon()
  {
    ConfigureEnvironmentVariablesForTextMessaging();
    ConfigureEnvironmentVariablesForAlwaysEncrypted();
    _ctx = new DatabaseContext(
    new DbContextOptionsBuilder<DatabaseContext>()
     .UseInMemoryDatabase(databaseName: "wmsHub_CallbackApi")
        .Options);
  }
}

//public class CallbackApiTests : BaseCommon, IDisposable,
//  IClassFixture<CustomWebApplicationFactory<Startup>>
//{
//  private HttpClient? _client;
//  private const string _apiBase = "https://localhost:44388";
//  private readonly Mock<ICallbackRequest> _mockCallback =
//    new();
//  private readonly string _token;

//  private CallbackRequest? _request;
//  private string _json => JsonConvert.SerializeObject(_request);

//  private readonly Mock<IOptions<ITextOptions>> _mockOptions = new();
//  private readonly ITextOptions _options = new TextOptions();

//  public CallbackApiTests(CustomWebApplicationFactory<Startup> factory)
//  {
//    _options.Audience = EnvironmentVariables.AUDIENCE;
//    _options.Issuer = EnvironmentVariables.ISSUER;
//    _options.TokenSecret = EnvironmentVariables.TOKENSECRET;
//    _options.ValidUsers = new List<string>
//    {
//      EnvironmentVariables.VALIDUSER
//    };

//    _mockOptions.Setup(t => t.Value).Returns(_options);

//    NotifyTokenHandler.Configure(_mockOptions.Object, "", null);

//    if (string.IsNullOrWhiteSpace(_token))
//    {
//      _token = NotifyTokenHandler.GenerateToken(
//        "TEST",
//        EnvironmentVariables.VALIDUSER,
//        10);
//    }

//    _mockCallback.Setup(x => x.Id).Returns(TEST_USER_ID);
//    _mockCallback.Setup(x => x.CreatedAt).Returns(DateTime.Now);
//    _mockCallback.Setup(x => x.Status).Returns("delivered");
//    _mockCallback.Setup(x => x.Reference)
//     .Returns(Guid.NewGuid().ToString());
//    _mockCallback.Setup(x => x.NotificationType).Returns("sms");
//    _mockCallback.Setup(x => x.To).Returns("077000");

//  }

//  public class ValidBearerTokenRequests : CallbackApiTests
//  {

//    public ValidBearerTokenRequests(
//      CustomWebApplicationFactory<Startup> factory) : base(factory)
//    {
//      WebApplicationFactoryClientOptions clientOptions =
//        new()
//        {
//          AllowAutoRedirect = true,
//          BaseAddress = new Uri("https://localhost:44388"),
//          HandleCookies = true,
//          MaxAutomaticRedirections = 7
//        };
//      _client = factory.CreateClient(clientOptions);

//      _client.DefaultRequestHeaders.Add("X-version", "1.0");
//      _client.DefaultRequestHeaders.Authorization =
//        new AuthenticationHeaderValue("Bearer", _token);
//    }

//    public class PostRequestsTests : ValidBearerTokenRequests
//    {
//#pragma warning disable 414
//      private readonly Business.Entities.Referral _referral = null!;
//#pragma warning restore 414
//      public PostRequestsTests(
//        CustomWebApplicationFactory<Startup> factory) : base(factory)
//      {
//        //Seed DB
//        DbGenerator.Initialise(_ctx);

//        Guid id = _ctx.TextMessages.First().Id;
//        _request = new CallbackRequest()
//        {
//          Id = Guid.NewGuid().ToString(),
//          To = toNumber,
//          Reference = id.ToString(),
//          Status = "delivered",
//          CreatedAt = DateTimeOffset.Now,
//          CompletedAt = DateTimeOffset.Now.AddSeconds(5),
//          SentAt = DateTimeOffset.Now
//        };

//      }

//      [Fact]
//      public async Task InValidBearerTokenReturnsUnauthorised401()
//      {
//        // Arrange.
//        _client!.DefaultRequestHeaders.Add("X-version", "1.0");
//        _client.DefaultRequestHeaders.Authorization =
//          new AuthenticationHeaderValue("Bearer", "Incorrect Token");

//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);

//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        // Assert.
//        apiResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
//      }

//      [Fact]
//      public async Task CorrectVersionReturnsOk()
//      {
//        // Arrange.
//        int expected = (int)HttpStatusCode.OK;
//        _client!.DefaultRequestHeaders.Add("X-version", "1.0");

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);

//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        // Assert.
//        Assert.Equal(expected, (int)apiResponse.StatusCode);
//      }

//      [Fact] //Removed as endpoint fundamentally altered
//      public async Task IncorrectVersionReturnsBadRequest()
//      {
//        // Arrange.
//        _client!.DefaultRequestHeaders.Add("X-version", "0.0");
//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);
//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task MissingReference()
//      {
//        // Arrange.
//        _mockCallback.Setup(x => x.Reference).Returns((string)null!);

//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        object? jsonObject = JsonConvert.DeserializeObject(responseString);

//        BadRequestObjectScaffold? scaffold =
//          JsonConvert.DeserializeObject<BadRequestObjectScaffold>(
//            jsonObject?.ToString() ?? string.Empty);

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);
//        if (scaffold == null)
//        {
//          Assert.Fail("Invalid responseString");
//        }
//        else
//        {
//          Assert.True(scaffold.Errors.Count == 1);
//          Assert.True(scaffold.Errors[0] == "Reference must be supplied");
//        }
//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task MissingTo()
//      {
//        // Arrange.
//        _mockCallback.Setup(x => x.To).Returns("");

//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        object? jsonObject = JsonConvert.DeserializeObject(responseString);

//        BadRequestObjectScaffold? scaffold =
//          JsonConvert.DeserializeObject<BadRequestObjectScaffold>(
//            jsonObject?.ToString() ?? string.Empty);

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);

//        if (scaffold == null)
//        {
//          Assert.Fail("Invalid responseString");
//        }
//        else
//        {
//          Assert.True(scaffold.Errors.Count == 1);
//          Assert.True(scaffold.Errors[0] == "Message 'TO' must be supplied");
//        }
//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task IncorectFormattingTo()
//      {
//        // Arrange.
//        string number = "123456789";
//        string test = $"To: {number} must be a mobile number";
//        _mockCallback.Setup(x => x.Reference).Returns("Test_Ref");
//        _mockCallback.Setup(x => x.NotificationType).Returns("sms");
//        _mockCallback.Setup(x => x.To).Returns(number);

//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        object? jsonObject = JsonConvert.DeserializeObject(responseString);

//        BadRequestObjectScaffold? scaffold =
//          JsonConvert.DeserializeObject<BadRequestObjectScaffold>(
//            jsonObject?.ToString() ?? string.Empty);

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);
//        if (scaffold == null)
//        {
//          Assert.Fail("Invalid responseString");
//        }
//        else
//        {
//          Assert.True(scaffold.Errors.Count == 1);
//          Assert.True(scaffold.Errors[0] == test);
//        }
//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task InValidCallbackUsing07Number()
//      {
//        // Arrange.
//        string number = "07512751212";

//        _request!.To = number;

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);
//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task ValidCallbackUsing447Number()
//      {
//        // Arrange.
//        string number = "+447512751212";
//        string expected = "responseStatus\":0,\"errors\":[],";

//        _request!.To = number;

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        // Assert.
//        Assert.Contains(expected, responseString);
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.OK);

//      }

//      [Fact]//Removed as endpoint fundamentally altered
//      public async Task InValidCallbackUsing01752Number()
//      {
//        // Arrange.
//        string number = "01752";
//        _mockCallback.Setup(x => x.To).Returns(number);

//        string expected = $"To: {number} must be a mobile number";

//        _request = new CallbackRequest(_mockCallback.Object);

//        HttpContent httpContent = new StringContent(_json, Encoding.UTF8);
//        httpContent.Headers.ContentType =
//          new MediaTypeHeaderValue("application/json");
//        _client!.DefaultRequestHeaders.Accept.Add(
//          new MediaTypeWithQualityHeaderValue("application/json"));

//        // Act.
//        HttpResponseMessage apiResponse =
//          await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//        string responseString =
//          await apiResponse.Content.ReadAsStringAsync();

//        // Assert.
//        Assert.True(apiResponse.StatusCode == HttpStatusCode.BadRequest);
//        Assert.Contains(expected, responseString);
//      }

//    }
//  }

//  public class AnonymousTokenRequests : CallbackApiTests
//  {

//    public AnonymousTokenRequests(
//      CustomWebApplicationFactory<Startup> factory) : base(factory)
//    {
//      WebApplicationFactoryClientOptions clientOptions =
//        new()
//        {
//          AllowAutoRedirect = true,
//          BaseAddress = new Uri("https://localhost:44388"),
//          HandleCookies = true,
//          MaxAutomaticRedirections = 7
//        };
//      _client = factory.CreateClient(clientOptions);

//      _client.DefaultRequestHeaders.Add("X-version", "1.0");
//    }


//    //[Fact]  System.InvalidOperationException :
//    //Key store providers cannot be set more than once.
//    public async Task Post_CallbackGetRequestIsUnauthorised()
//    {
//      // Arrange.
//      _request = new CallbackRequest(_mockCallback.Object);

//      HttpContent httpContent = new StringContent(_json, Encoding.UTF8);

//      httpContent.Headers.ContentType =
//            new MediaTypeHeaderValue("application/json");
//      _client!.DefaultRequestHeaders.Accept.Add(
//            new MediaTypeWithQualityHeaderValue("application/json"));

//      // Act.
//      HttpResponseMessage apiResponse =
//        await _client.PostAsync($"{_apiBase}/Callback", httpContent);

//      // Assert.
//      apiResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
//    }
//  }

//  private static string GenerateToken(string nameIdentifier, string sid)
//  {
//    if (!Guid.TryParse(sid, out Guid userId) || userId == Guid.Empty)
//    {
//      throw new ArgumentException($"{sid} is not a valid user id");
//    }

//    SymmetricSecurityKey mySecurityKey = new(
//      Encoding.ASCII.GetBytes(EnvironmentVariables.TOKENSECRET));

//    JwtSecurityTokenHandler tokenHandler = new();
//    SecurityTokenDescriptor tokenDescriptor = new()
//    {
//      Subject = new ClaimsIdentity(new Claim[]
//      {
//        new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
//        new Claim(ClaimTypes.Sid, sid),
//      }),
//      Expires = DateTime.Now.AddDays(1),
//      Issuer = EnvironmentVariables.ISSUER,
//      Audience = EnvironmentVariables.AUDIENCE,
//      SigningCredentials =
//        new SigningCredentials(mySecurityKey,
//          SecurityAlgorithms.HmacSha256Signature)
//    };

//    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
//    return tokenHandler.WriteToken(token);
//  }

//  private TokenValidationParameters GetFakeTokenValidationParameters()
//  {
//    SymmetricSecurityKey mySecurityKey = new(
//      Encoding.ASCII.GetBytes(EnvironmentVariables.TOKENSECRET));
//    return new TokenValidationParameters
//    {
//      ValidateIssuerSigningKey = true,
//      ValidateIssuer = true,
//      ValidateAudience = true,
//      ValidIssuer = EnvironmentVariables.ISSUER,
//      ValidAudience = EnvironmentVariables.AUDIENCE,
//      IssuerSigningKey = mySecurityKey
//    };

//  }

//  private bool _disposed;
//  public void Dispose()
//  {
//    if (!_disposed)
//    {
//      _disposed = true;
//      _ctx.Dispose();
//    }
//  }
//}

public class DbGenerator : BaseCommon
{
  public static void Initialise(DatabaseContext ctx)
  {
    Guid providerId = Guid.Parse(TEST_USER_ID);
    Provider? provider = ctx.Providers
      .SingleOrDefault(t => t.Id == providerId);
    if (provider == null)
    {
      //Add a provider
      provider = RandomEntityCreator.CreateRandomProvider(
        id: providerId,
        isActive: true,
        isLevel1: true);

      ctx.Providers.Add(provider);

      ctx.SaveChanges();
    }

    //Add a service user
    Referral? referral = ctx.Referrals
      .Include(t => t.TextMessages)
      .FirstOrDefault();
    if (referral == null)
    {
      referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now,
        modifiedByUserId: provider.Id,
        providerId: provider.Id,
        referringGpPracticeName: provider.Id.ToString(),
        status: ReferralStatus.TextMessage1,
        triagedCompletionLevel: "1",
        ubrn: _ubrn);

      ctx.Referrals.Add(referral);

      ctx.SaveChanges();
    }

    Business.Entities.TextMessage tm = new()
    {
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Parse(TEST_USER_ID),
      Number = toNumber
    };

    referral.TextMessages ??= new List<Business.Entities.TextMessage>();

    referral.TextMessages.Add(tm);

    ctx.SaveChanges();

  }
}

