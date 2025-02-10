using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WmsHub.Ui.Middleware;
using Xunit;

namespace WmsHub.Ui.Tests.Middleware;

[ApiController]
[Route("[Controller]")]
public class TestController : ControllerBase
{
  [HttpGet("GetAction")]
  public IActionResult GetAction()
  {
    return Ok("GetAction");
  }
}

public abstract class SecurityHeadersExtensionTests : IDisposable
{
  protected IHost _host;
  protected readonly IConfiguration _configuration;

  public SecurityHeadersExtensionTests(bool isDevelopment)
  {
    Dictionary<string, string> inMemorySettings = new()
    {
      { "RmcUi:SecurityHeaderScriptSrc", "script-src 'self'" },
      { "RmcUi:SecurityHeaderStyleSrc", "style-src 'self'" },
      { "RmcUi:DevelopmentConnectSrc", "developmentConnectSrc" }
    };

    _configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(inMemorySettings)
      .Build();

    _host = new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder
          .UseTestServer()
          .ConfigureServices(services =>
          {
            services.AddControllers();
          })
          .Configure(app =>
          {
            app.UseRouting();
            app.UseSecurityHeaders(
              _configuration,
              isDevelopment,
              "link/signalRUrl");
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
          });
      })
      .Start();
  }

  protected async Task SecurityHeadersTest(string expectedCsp)
  {
    // Arrange.
    const string CONST_CONTENT_SECURITY_POLICY =
      "Content-Security-Policy";
    const string CONST_X_XSS_PROTECTION =
      "X-XSS-Protection";
    string expectedXss = "1";

    TestServer server = _host.GetTestServer();
    server.BaseAddress = new Uri("https://localhost");

    // Act.
    HttpContext context = await server.SendAsync(c => {
      c.Request.Method = HttpMethods.Get;
      c.Request.Path = "/test/GetAction";
    });

    // Assert.
    using (new AssertionScope())
    {
      context.Response.Headers
        .ContainsKey(CONST_CONTENT_SECURITY_POLICY);
      context.Response.Headers
        .ContainsKey(CONST_X_XSS_PROTECTION);
      context.Response.Headers[CONST_CONTENT_SECURITY_POLICY]
        .Single().Should().Be(expectedCsp);
      context.Response.Headers[CONST_X_XSS_PROTECTION]
        .Single().Should().Be(expectedXss);
    }
  }

  public void Dispose()
  {
    _host.Dispose();
  }
}

public class SecurityHeadersExtensionDevelopmentEnvironmentTest
  : SecurityHeadersExtensionTests
{
  public SecurityHeadersExtensionDevelopmentEnvironmentTest()
    : base(true) { }

  [Fact]
  public async Task SecurityHeaders()
  {
    // Arrange.
    string expectedCsp =
        "connect-src 'self' link/signalRUrl developmentConnectSrc; " +
        "script-src 'self'; style-src 'self'; " +
        "child-src 'unsafe-inline'; frame-src 'unsafe-inline';";

    // Act & Assert.
    await SecurityHeadersTest(expectedCsp);
  }
}

public class SecurityHeadersExtensionEnvironmentTest
  : SecurityHeadersExtensionTests
{
  public SecurityHeadersExtensionEnvironmentTest()
    : base(false) { }

  [Fact]
  public async Task SecurityHeaders()
  {
    // Arrange.
    string expectedCsp =
        "connect-src 'self' link/signalRUrl; " +
        "script-src 'self'; style-src 'self'; " +
        "child-src 'unsafe-inline'; frame-src 'unsafe-inline';";

    // Act & Assert.
    await SecurityHeadersTest(expectedCsp);
  }
}