using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests;

public class HealthCheckTestsFixture : IDisposable
{
  private const string AdminApiKey = "AdminApiKey";
  private const string AdminApiKeyValue =
    "hWAFRf9qG1H1CKyHf8lQHatD3a6hnbyqJY1kx0zNInoODH4ed2";
  private const string ElectiveCareApiKey = "ElectiveCareApiKey";
  private const string ElectiveCareApiKeyValue =
    "hOJsUsxZVtey1fOmLS3AEKjEeJqkayyxgmQYH1e0uBoqMgmBSd";
  private const string GeneralReferralApiKey = "GeneralReferralApiKey";
  private const string GeneralReferralApiKeyValue =
    "u4VjGGIQ6rnmKdf8ozMinqG2wOrEYBfQ4WfWe0y0vRJ2oC0BGw";
  private const string MskApiKey = "MskApiKey";
  private const string MskApiKeyValue =
    "5ezlaUFEL6t!J4fr1x*nt#knG8*n1G!xN%dzVaT4pvb&!I6S#M";
  private const string QuestionnaireApiKey = "QuestionnaireApiKey";
  private const string QuestionnaireApiKeyValue =
    "FqXK0J*n1pX3$lL8XUQpCk!8ej$8iUxb86L7vgS6n&JfJ^5n8#";
  private const string ProcessStatusAppName = "ProcessStatusServiceOptions:appName";
  private const string ProcessStatusAppNameValue = "Health";
  private const string ProcessStatusBaseAddress = "ProcessStatusServiceOptions:baseAddress";
  private const string ProcessStatusBaseAddressValue = "https://";

  public IHost Host { get; private set; }

  public HealthCheckTestsFixture()
  {
    Host = new HostBuilder()
      .ConfigureWebHost(webBuilder => webBuilder
          .ConfigureAppConfiguration((hostingContext, config) => 
          config.AddInMemoryCollection(GetFakeConfigValues()))
        .UseTestServer()
        .UseStartup<Startup>()
        .ConfigureTestServices(services =>
        {
          // replace db context with in memory db context
          ServiceDescriptor context = services.FirstOrDefault(
            s => s.ServiceType == typeof(Business.DatabaseContext));
          if (context != null)
          {
            services.Remove(context);
            EnvironmentVariableConfigurator
              .ConfigureEnvironmentVariablesForAlwaysEncrypted();
            ServiceDescriptor[] options = services
              .Where(r => (r.ServiceType == typeof(DbContextOptions))
                || (r.ServiceType.IsGenericType
                  && r.ServiceType.GetGenericTypeDefinition()
                    == typeof(DbContextOptions<>)))
              .ToArray();
            foreach (ServiceDescriptor option in options)
            {
              services.Remove(option);
            }
          }

          services.AddDbContext<Business.DatabaseContext>(
            opt => opt.UseInMemoryDatabase("InMemWmsHub"));

        }))
      .Start();
  }

  public void Dispose()
  {
    Host.Dispose();
    GC.SuppressFinalize(this);
  }

  private static Dictionary<string, string> GetFakeConfigValues()
  {
    Dictionary<string, string> options = new()
    {
      { AdminApiKey, AdminApiKeyValue },
      { ElectiveCareApiKey, ElectiveCareApiKeyValue},
      { GeneralReferralApiKey, GeneralReferralApiKeyValue},
      { MskApiKey, MskApiKeyValue },
      { QuestionnaireApiKey, QuestionnaireApiKeyValue },
      { ProcessStatusAppName, ProcessStatusAppNameValue },
      { ProcessStatusBaseAddress, ProcessStatusBaseAddressValue }
    };

    return options.Concat(EnvironmentVariableConfigurator.FakeGpDocumentProxyOptions())
      .ToDictionary();
  }
}

public class HealthCheckTests : IClassFixture<HealthCheckTestsFixture>
{
  private IHost _host;
  public HealthCheckTests(HealthCheckTestsFixture fixture)
  {
    try
    {
      _host = fixture.Host;
    }
    catch (Exception ex)
    {
      throw new Exception(nameof(fixture), ex);
    }
  }

  [Fact]
  public async Task HealthRouteGetReturnsSuccess()
  {
    // Arrange.
    _host ??= new HealthCheckTestsFixture().Host;
    TestServer testServer = _host.GetTestServer();
    testServer.BaseAddress = new Uri("https://localhost");
    Action<HttpContext> action = new(c =>
    {
      c.Request.Method = HttpMethods.Get;
      c.Request.Path = "/health";
    });

    // Act.
    HttpContext context = await testServer.SendAsync(action);

    // Assert.
    context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
  }
}
