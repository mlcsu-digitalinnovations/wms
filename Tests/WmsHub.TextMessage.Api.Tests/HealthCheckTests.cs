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

namespace WmsHub.TextMessage.Api.Tests;

public class HealthCheckTestsFixture : IDisposable
{
  private const string GeneralReferralNotifyLink = "TextSettings:GeneralReferralNotifyLink";
  private const string GeneralReferralNotifyLinkValue = "https://";
  private const string IsNumberWhiteListEnabled = "TextSettings:IsNumberWhiteListEnabled";
  private const string IsNumberWhiteListEnabledValue = "false";
  private const string NotifyLink = "TextSettings:NotifyLink";
  private const string NotifyLinkValue = "https://";
  private const string SmsApiKey = "TextSettings:SmsApiKey";
  private const string SmsApiKeyValue = "1f9adb95-7e44-4180-9668-088fedf46002";
  private const string SmsSenderId = "TextSettings:SmsSenderId";
  private const string SmsSenderIdValue = "3b533c63-26e8-4923-b4ba-0649be92ce93";
  private const string TokenSecret = "TextSettings:TokenSecret";
  private const string TokenSecretValue = "ddf8b3d7-ed33-4d7e-b924-b59edbc13788";

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
              .Where(r => r.ServiceType == typeof(DbContextOptions)
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
    return new Dictionary<string, string>()
    {
      { GeneralReferralNotifyLink, GeneralReferralNotifyLinkValue },
      { IsNumberWhiteListEnabled, IsNumberWhiteListEnabledValue },
      { NotifyLink, NotifyLinkValue },
      { SmsApiKey, SmsApiKeyValue },
      { SmsSenderId, SmsSenderIdValue },
      { TokenSecret, TokenSecretValue }
    };
  }
}

public class HealthCheckTests(HealthCheckTestsFixture fixture)
  : IClassFixture<HealthCheckTestsFixture>
{
  private readonly IHost _host = fixture.Host;

  [Fact]
  public async Task HealthRouteGetReturnsSuccess()
  {
    // Arrange.
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