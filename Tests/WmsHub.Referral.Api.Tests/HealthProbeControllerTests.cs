using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  public class HealthProbeControllerTestsFixture : IDisposable
  {
    internal readonly string MSK_APIKEY = "MskApiKey";
    internal readonly string MSK_APIKEYVALUE =
      "5ezlaUFEL6t!J4fr1x*nt#knG8*n1G!xN%dzVaT4pvb&!I6S#M";
    internal readonly string MSK_CLAIMTYPE = "MskClaimType";
    internal readonly string MSK_CLAIMTYPEVALUE =
      "*mgBTZG%ffPVF1&XWHmVcj20$e4eY*SGAy5#G7kYvFhZheO8X%";

    public IHost Host { get; private set; }

    public HealthProbeControllerTestsFixture()
    {
      Host = new HostBuilder()
        .ConfigureWebHost(webBuilder =>
        {
          webBuilder
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
              config.AddInMemoryCollection(new Dictionary<string, string>()
              {
                { MSK_APIKEY, MSK_APIKEYVALUE },
                { MSK_CLAIMTYPE, MSK_CLAIMTYPEVALUE }
              });
            })
          .UseTestServer()
          .UseStartup<Startup>()
          .ConfigureTestServices(services =>
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
          });
        })
        .Start();
    }

    public void Dispose()
    {
      Host.Dispose();
    }
  }

  public class HealthProbeControllerTests
    : IClassFixture<HealthProbeControllerTestsFixture>
  {
    private readonly TestServer _testServer;

    public HealthProbeControllerTests(
      HealthProbeControllerTestsFixture fixture)
    {
      _testServer = fixture.Host.GetTestServer();
      _testServer.BaseAddress = new Uri("https://localhost");
    }

    [Fact]
    public async Task EmptyRoute_Head_ReturnsSuccess()
    {
      // Arrange
      Action<HttpContext> action = new(c =>
      {
        c.Request.Method = HttpMethods.Head;
        c.Request.Path = "/";
      });

      // Act
      var context = await _testServer.SendAsync(action);

      // Assert
      context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

  }
}
