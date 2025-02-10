using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ChatBot.Api.Tests;

public class HealthCheckTestsFixture : IDisposable
{
  public IHost Host { get; private set; }

  public HealthCheckTestsFixture()
  {
    Host = new HostBuilder()
      .ConfigureWebHost(webBuilder => webBuilder
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
