using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Referral.Api.Tests.Models.MskReferral;
using WmsHub.Tests.Helper;
using Xunit;
using static
  WmsHub.Tests.Helper.EnvironmentVariableConfigurator.EnvironmentVariables;

namespace WmsHub.Referral.Api.Tests.Controllers;

public class MskReferralsControllerBase
{
  protected DatabaseContext _context;
  protected internal Dictionary<string, string> FakeDbConfigValues =>
    GetFakeDbConfigValues();

  protected internal Dictionary<string, string> FakeEnvironmentVariables => new();

  protected WebApplicationFactoryBuilder Builder => new()
  {
    EnvironmentVariables = FakeEnvironmentVariables,
    InMemoryValues = FakeDbConfigValues,
    PoliciesToInclude = new[] { "Admin", "Msk" }
  };

  public MskReferralsControllerBase()
  {
    _context = new DatabaseContext(
    new DbContextOptionsBuilder<DatabaseContext>()
     .UseInMemoryDatabase(databaseName: "wmsHub_TestApi")
        .Options);
  }

  private static Dictionary<string, string> GetFakeDbConfigValues()
  {
    Dictionary<string, string> options = new()
    {
      { ADMIN_APIKEY, ADMIN_APIKEYVALUE },
      { ELECTIVECARE_APIKEY, ELECTIVECARE_APIKEYVALUE },
      { GENERALREFERRAL_APIKEY, GENERALREFERRAL_APIKEYVALUE },
      { MSK_APIKEY, MSK_APIKEYVALUE },
      { MSK_EMAIL_WHITELIST, "nhs.net" }
    };

    return options.Concat(EnvironmentVariableConfigurator.FakeGpDocumentProxyOptions())
      .ToDictionary();
  }
}

public class MskReferralsControllerTests :
  MskReferralsControllerBase,
  IDisposable,
  IClassFixture<TestWebApplicationFactory<Startup>>
{
  private readonly HttpClient _client;
  protected static readonly List<string> _apiVersions = new() { "", "/v1" };
  protected TestWebApplicationFactory<Startup> _factory;
  public MskReferralsControllerTests(
    TestWebApplicationFactory<Startup> factory)
  {
    _factory = TestWebApplicationFactory<Startup>.Create(Builder);
    WebApplicationFactoryClientOptions clientOptions = new()
    {
      AllowAutoRedirect = true,
      BaseAddress = new Uri("https://localhost:44388"),
      HandleCookies = true,
      MaxAutomaticRedirections = 7
    };
    _client = factory.CreateClient(clientOptions);
    _client.DefaultRequestHeaders.Add("X-version", "1.0");
    _client.DefaultRequestHeaders.Add("X-API-KEY", MSK_APIKEYVALUE);
    if (!_context.Ethnicities.Any())
    {
      _factory.PopulateEthnicities(_context);
    }

    if (!_context.PatientTriages.Any())
    {
      _factory.PopulatePatientTriageService(_context);
    }

    _context.MskOrganisations.Add(
      new Business.Entities.MskOrganisation
      {
        IsActive = true,
        OdsCode = "TEST1",
        SendDischargeLetters = true,
        SiteName = "UNIT Test 1"
      });
    _context.SaveChanges();
  }

  public static TheoryData<string> EndpointsTheoryData(string endpoint)
  {
    TheoryData<string> endpoints = new();
    _apiVersions.ForEach(root => endpoints.Add($"{root}{endpoint}"));
    return endpoints;
  }

  public void Dispose()
  {
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.Providers.RemoveRange(_context.Providers);
    _context.PatientTriages.RemoveRange(_context.PatientTriages);
    _context.MskOrganisations.RemoveRange(_context.MskOrganisations);
    _context.SaveChanges();
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/GenerateKey")]
  public async Task AccessKey_EmailInvalid_400_BadRequestResponse(
   string apiUrl)
  {
    // Arrange.
    string expectedError = "The Email field is not a valid email address.";
    string url = $"{apiUrl}?email=invalid@test.com&expireMinutes=20";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);
    string content = await response.Content.ReadAsStringAsync();
    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should()
        .Be(System.Net.HttpStatusCode.BadRequest);
      content.Should().Contain(expectedError);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/GenerateKey")]
  public async Task AccessKey_EmailDomainNotInWhiteList_403_Forbidden(
   string apiUrl)
  {
    // Arrange.
    string expectedError = "Email's domain is not in the domain white list.";
    string url = $"{apiUrl}?email=notallowed@gmail.com";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);
    string content = await response.Content.ReadAsStringAsync();
    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should()
        .Be(System.Net.HttpStatusCode.Forbidden);
      content.Should().Contain(expectedError);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/GenerateKey")]
  public async Task AccessKey_EmailValid_200(string apiUrl)
  {
    // Arrange.
    string expectedDate = DateTime.UtcNow.AddMinutes(10)
      .ToString("yyyy-MM-ddTHH:mm");
    string url = $"{apiUrl}?email=test@nhs.net";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);
    string content = await response.Content.ReadAsStringAsync();
    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should()
        .Be(System.Net.HttpStatusCode.OK);
      content.Should().Contain(expectedDate);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/GenerateKey")]
  public async Task AccessKey_EmailValid_200AndExpiry(string apiUrl)
  {

    // Arrange.
    int expireMinutes = 100;
    string expectedDate = DateTime.UtcNow.AddMinutes(expireMinutes)
      .ToString("yyyy-MM-ddTHH:mm");
    string url = $"{apiUrl}?email=test@nhs.net" +
      $"&expireMinutes={expireMinutes}";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);
    string content = await response.Content.ReadAsStringAsync();
    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should()
        .Be(System.Net.HttpStatusCode.OK);
      content.Should().Contain(expectedDate);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/Ethnicity")]
  public async Task GetListOfEthnicities_Returns_Rows(string apiUrl)
  {
    // Arrange.
    string[] expectedNames = new[]
    {
        "English, Welsh, Scottish, Northern Irish or British",
        "Irish",
        "Gypsy or Irish Traveller",
        "Any other White background",
        "White and Black Caribbean",
        "White and Black African",
        "White and Asian",
        "Any other Mixed or Multiple ethnic background",
        "Indian",
        "Pakistani",
        "Bangladeshi",
        "Chinese",
        "Any other Asian background",
        "African",
        "Caribbean",
        "Any other Black, African or Caribbean background",
        "Arab",
        "Any other ethnic group",
        "I do not wish to Disclose my Ethnicity"
      };
    _factory.PopulateEthnicities(_context);
    // Act.
    HttpResponseMessage response = await _client.GetAsync(apiUrl);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
      if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
      {
        content.Should().Be("FAILED TEST");
      }

      List<EthnicitiesTestResponse> result =
        JsonConvert.DeserializeObject<List<EthnicitiesTestResponse>>(
          content);
      foreach (EthnicitiesTestResponse item in result)
      {
        string displayName = item.DisplayName;
        expectedNames.Should().Contain(displayName);
      }
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/MskHub")]
  public async Task WhitelistHasValues_200(string apiUrl)
  {
    // Arrange.

    // Act.
    HttpResponseMessage response = await _client.GetAsync(apiUrl);
    string content = await response.Content.ReadAsStringAsync();

    List<MskHub> expectedNames = JsonConvert.DeserializeObject<List<MskHub>>(content);

    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
      expectedNames
        .SingleOrDefault(t => t.Name == MSK_HUB_TEST1_VALUE)
        .Should()
        .NotBeNull();
      expectedNames
        .SingleOrDefault(t => t.Name == MSK_HUB_TEST1_VALUE)
        .OdsCode
       .Should()
       .Be("TEST1");
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/NhsNumber")]
  public async Task NhsNumberIsNotInUse_204(string apiUrl)
  {
    // Arrange.
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.SaveChanges();
    string nhsNumber = Generators.GenerateNhsNumber(new Random());
    Business.Entities.Referral referral =
      RandomEntityCreator.CreateRandomReferral();

    string link = $"{apiUrl}/{nhsNumber}";

    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();

    // Act.
    HttpResponseMessage response = await _client.GetAsync(link);

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
      {
        await ThrowContentAsync(response);
      }

      response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/NhsNumber")]
  public async Task InvalidNhsNumber_400(string apiUrl)
  {
    //Arrange.
    string nhsNumber = "1234567890";
    Business.Entities.Referral referral =
      RandomEntityCreator.CreateRandomReferral(
        nhsNumber: nhsNumber
        );
    string link = $"{apiUrl}/{nhsNumber}";
    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();
    string expectedErrorMsg = "The field NhsNumber is invalid.";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(link);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
      {
        await ThrowContentAsync(response);
      }

      ProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ProblemDetails>(content);
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
      problemDetails.Detail.Should().Be(expectedErrorMsg);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/NhsNumber")]
  public async Task NhsNumberIsInUse_409(string apiUrl)
  {
    // Arrange.
    string nhsNumber = Generators.GenerateNhsNumber(new Random());
    Business.Entities.Referral referral =
      RandomEntityCreator.CreateRandomReferral(nhsNumber: nhsNumber);
    string link = $"{apiUrl}/{nhsNumber}";
    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();

    string expectedErrorMsg = "Referral cannot be created because there" +
      $" are in progress referrals with the same NHS number:" +
      $" (UBRN {referral.Ubrn}).";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(link);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.Conflict)
      {
        await ThrowContentAsync(response);
      }

      ProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ProblemDetails>(content);

      response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
      problemDetails.Detail.Should().Be(expectedErrorMsg);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral/NhsNumber")]
  public async Task NhsNumberInUseButCancelledWithPreviousProvider_409(
    string apiUrl)
  {
    // Arrange.
    Business.Entities.Provider provider =
      RandomEntityCreator.CreateRandomProvider();
    _context.Providers.Add(provider);
    await _context.SaveChangesAsync();
    string nhsNumber = Generators.GenerateNhsNumber(new Random());
    Business.Entities.Referral referral =
      RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.UtcNow.AddDays(-10),
        dateOfReferral: DateTimeOffset.UtcNow.AddDays(-20),
        nhsNumber: nhsNumber,
        providerId: provider.Id,
        status: ReferralStatus.Complete
        );
    string link = $"{apiUrl}/{nhsNumber}";
    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();

    string expectedErrorMessage = "Referral can be created from " +
      DateTimeOffset.UtcNow.AddDays(33).Date.ToString("yyyy-MM-dd") +
      " as an existing referral for this NHS number " +
      $"(UBRN {referral.Ubrn}) selected a provider but did not start " +
      "the programme.";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(link);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.Conflict)
      {
        await ThrowContentAsync(response);
      }

      ProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ProblemDetails>(content);
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
      problemDetails.Detail.Should().Be(expectedErrorMessage);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral")]
  public async Task PostRequest_Valid_204(string apiUrl)
  {
    // Arrange.
    PostRequest postRequest = RandomModelCreator
      .CreateRandomMskReferralPostRequest(heightCm: 180, weightKg: 120,
      referringMskHubOdsCode: "TEST1");
    string json = JsonConvert.SerializeObject(postRequest);
    HttpContent httpContent = new StringContent(json, Encoding.UTF8);

    httpContent.Headers.ContentType =
      new MediaTypeHeaderValue("application/json");

    // Act.
    HttpResponseMessage response = await _client.PostAsync(
      apiUrl,
      httpContent);

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
      {
        await ThrowContentAsync(response);
      }

      response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral")]
  public async Task PostRequestInvalid_400(string apiUrl)
  {
    // Arrange.
    PostRequest postRequest = new();
    string json = JsonConvert.SerializeObject(postRequest);
    HttpContent httpContent = new StringContent(json, Encoding.UTF8);

    httpContent.Headers.ContentType =
      new MediaTypeHeaderValue("application/json");

    string expectedErrorMsg = "One or more validation errors occurred.";

    // Act.
    HttpResponseMessage response = await _client.PostAsync(
      apiUrl,
      httpContent);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
      {
        await ThrowContentAsync(response);
      }

      ValidationProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ValidationProblemDetails>(content);
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
      problemDetails.Title.Should().Be(expectedErrorMsg);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral")]
  public async Task PostRequestValid_InvalidMskHub_400(string apiUrl)
  {
    // Arrange.
    PostRequest postRequest = RandomModelCreator
      .CreateRandomMskReferralPostRequest(referringMskHubOdsCode: "Invalid");
    string json = JsonConvert.SerializeObject(postRequest);
    HttpContent httpContent = new StringContent(json, Encoding.UTF8);

    httpContent.Headers.ContentType =
      new MediaTypeHeaderValue("application/json");
    string expectedErrorMsg =
      "The ReferringMskHubOdsCode is not in the whitelist.";

    // Act.
    HttpResponseMessage response = await _client.PostAsync(
      apiUrl,
      httpContent);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
      {
        await ThrowContentAsync(response);
      }

      ProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ProblemDetails>(content);
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
      problemDetails.Detail.Should().Be(expectedErrorMsg);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/MskReferral")]
  public async Task ReferralNotUniqueException_409(string apiUrl)
  {
    // Arrange.

    string nhsNumber = Generators.GenerateNhsNumber(new Random());
    Business.Entities.Referral referral =
      RandomEntityCreator.CreateRandomReferral(nhsNumber: nhsNumber);
    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();
    PostRequest postRequest = RandomModelCreator
      .CreateRandomMskReferralPostRequest(
      nhsNumber: nhsNumber,
      referringMskHubOdsCode: "TEST1");
    string json = JsonConvert.SerializeObject(postRequest);
    HttpContent httpContent = new StringContent(json, Encoding.UTF8);

    httpContent.Headers.ContentType =
      new MediaTypeHeaderValue("application/json");
    string expectedErrorMsg = "Referral cannot be created because there" +
      $" are in progress referrals with the same NHS number: " +
      $"(UBRN {referral.Ubrn}).";

    // Act.
    HttpResponseMessage response = await _client.PostAsync(
      apiUrl,
      httpContent);
    string content = await response.Content.ReadAsStringAsync();

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        content.Should().Be(expectedErrorMsg);
      }

      ProblemDetails problemDetails =
        JsonConvert.DeserializeObject<ProblemDetails>(content);
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
      problemDetails.Detail.Should().Be(expectedErrorMsg);
    }
  }

  private async Task ThrowContentAsync(HttpResponseMessage response)
  {
    string content = await response.Content.ReadAsStringAsync();
    content.Should().Be("NO EXPECTED FAILED");
  }
}

