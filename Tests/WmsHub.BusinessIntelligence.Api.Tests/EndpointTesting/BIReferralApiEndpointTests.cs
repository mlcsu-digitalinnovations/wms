using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Tests.Helper;
using Xunit;
using static WmsHub.Tests.Helper.EnvironmentVariableConfigurator.EnvironmentVariables;

namespace WmsHub.BusinessIntelligence.Api.Tests.EndpointTesting;

public class BiReferralApiEndpointTestsBase
{
  protected DatabaseContext _context;

  // Used to set the inMemory values as if they are in ConfigurationValues
  protected internal static Dictionary<string, string> FakeDbConfigValues => new()
  {
    { BI_APIKEY,  BI_APIKEYVALUE},
    { BI_PROVIDER_STATUSES,BI_PROVIDER_STATUSES_VALUE },
    { BI_WHITELIST_ENABLED, "false" },
    { REFERRAL_COUNTS_API_KEY, REFERRAL_COUNTS_API_KEY_VALUE },
    { RMC_API_KEY, RMC_API_KEY_VALUE }
  };

  protected internal static Dictionary<string, string> FakeEnvironmentVariables => [];

  protected static WebApplicationFactoryBuilder Builder => new()
  {
    EnvironmentVariables = FakeEnvironmentVariables,
    InMemoryValues = FakeDbConfigValues,
    PoliciesToInclude = ["Default"]
  };

  public BiReferralApiEndpointTestsBase()
  {
    _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(databaseName: "wmsHub_TestApi")
      .Options);
  }
}

public class BIReferralApiEndpointTests
  : BiReferralApiEndpointTestsBase, IClassFixture<TestWebApplicationFactory<Startup>>
{
  private readonly HttpClient _client;
  protected static readonly List<string> _apiVersions = ["", "/v1"];
  protected TestWebApplicationFactory<Startup> _factory;

  public BIReferralApiEndpointTests(TestWebApplicationFactory<Startup> factory)
  {
    _factory = TestWebApplicationFactory<Startup>.Create(Builder);
    WebApplicationFactoryClientOptions clientOptions = new()
    {
      AllowAutoRedirect = true,
      BaseAddress = new Uri("https://localhost:44332"),
      HandleCookies = true,
      MaxAutomaticRedirections = 7
    };

    _client = factory.CreateClient(clientOptions);
    _client.DefaultRequestHeaders.Add("X-version", "1.0");
    _client.DefaultRequestHeaders.Add("X-API-KEY", BI_APIKEYVALUE);
  }

  public static TheoryData<string> EndpointsTheoryData(string endpoint)
  {
    TheoryData<string> endpoints = [];
    _apiVersions.ForEach(root => endpoints.Add($"{root}{endpoint}"));
    return endpoints;
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/referral")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/changes")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/counts")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/ElectiveCarePostErrors")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerrejected")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerdeclined")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerterminated")]
  public async Task AccessKey_Unauthorized_401(string apiUrl)
  {
    // Arrange.
    _client.DefaultRequestHeaders.Remove("X-API-KEY");
    _client.DefaultRequestHeaders.Add("X-API-KEY", Guid.NewGuid().ToString());
    string expectedError = "Unauthorized";
    string url = $"{apiUrl}";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);

    // Assert.
    using (new AssertionScope())
    {
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
      response.ReasonPhrase.Should().Be(expectedError);
    }
  }

  [Theory]
  [MemberData(nameof(EndpointsTheoryData), "/referral")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerrejected")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerdeclined")]
  [MemberData(nameof(EndpointsTheoryData), "/referral/providerterminated")]
  public async Task AccessKey_NoContent_204(string apiUrl)
  {
    // Arrange.  
    string expected = "No Content";
    string url = $"{apiUrl}";

    // Act.
    HttpResponseMessage response = await _client.GetAsync(url);

    // Assert.
    using (new AssertionScope())
    {
      if (response.StatusCode != HttpStatusCode.NoContent)
      {
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Be(expected);
      }
      else
      {
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.ReasonPhrase.Should().Be(expected);
      }
    }
  }
}
