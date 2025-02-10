using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using Xunit;

namespace WmsHub.Ui.Tests.Middleware
{
  [ApiController]
  [Route("[Controller]")]
  public class RmcController : ControllerBase
  {
    [HttpGet("GetAction")]
    public IActionResult GetAction()
    {
      return Ok("GetAction");
    }

    [HttpPost("PostAction")]
    public IActionResult PostAction()
    {
      return Ok("PostAction");
    }
  }

  public class UserActionLogServiceMock : IUserActionLogService
  {
    public static IUserActionLog UserActionLog { get; private set; }

    public UserActionLogServiceMock()
    {
      UserActionLog = null;
    }

    public Task CreateAsync(IUserActionLog entity)
    {
      UserActionLog = entity;
      return Task.CompletedTask;
    }
  }


  public class UserActionLoggingTests : IDisposable
  {
    protected IHost _host;  

    public UserActionLoggingTests()
    {
      _host = new HostBuilder()
        .ConfigureWebHost(webBuilder =>
        {
          webBuilder
            .UseTestServer()
            .ConfigureServices(services =>
            {
              services.AddControllers();
              services.AddDbContext<DatabaseContext>(options =>
              {
                options.UseInMemoryDatabase("WmsHub-" + Guid.NewGuid());
              });
              services
                .AddScoped<IUserActionLogService, UserActionLogServiceMock>();
            })
            .Configure(app =>
            {
              app.UseRouting();
              app.UseMiddleware<UserActionLogging>();              
              app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });
        })
        .Start();
    }

    public void Dispose()
    {
      _host.Dispose();
    }

    [Fact]
    public async Task HealthProbeRequest_NoDatabaseUpdate()
    {
      // arrange
      TestServer server = _host.GetTestServer();
      server.BaseAddress = new Uri("https://localhost");

      // act
      HttpContext context = await server.SendAsync(c => {
        c.Request.Method = HttpMethods.Head;
        c.Request.Path = "/HealthProbe/HealthProbe";
      });

      // assert
      IUserActionLog userActionLog = UserActionLogServiceMock.UserActionLog;
      userActionLog.Should().BeNull();
    }

      [Fact]
    public async Task GetRequest_UpdatesDatabase()
    {
      // arrange
      TestServer server = _host.GetTestServer();
      server.BaseAddress = new Uri("https://localhost");
      Guid userOid = Guid.NewGuid();
      ClaimsPrincipal user = new(new ClaimsIdentity(
        new List<Claim>()
        {
          new Claim(
            ClaimsPrincipalExtensions.CLAIM_OID,
            userOid.ToString())
        }));

      // act
      HttpContext context = await server.SendAsync(c => {
        c.Request.Method = HttpMethods.Get;
        c.Request.Path = "/Rmc/GetAction";
        c.User = user;
      });

      // assert
      context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

      IUserActionLog userActionLog = UserActionLogServiceMock.UserActionLog;
      userActionLog.Action.Should().Be("GetAction");
      userActionLog.Controller.Should().Be("Rmc");
      userActionLog.IpAddress.Should().BeNull();
      userActionLog.Method.Should().Be("GET");
      userActionLog.Request.Should()
        .Be("https://localhost/Rmc/GetAction");
      userActionLog.RequestAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0,0,1));
      userActionLog.UserId.Should().Be(userOid);
    }

    [Fact]
    public async Task PostRequest_UpdatesDatabase()
    {
      // arrange
      TestServer server = _host.GetTestServer();
      server.BaseAddress = new Uri("https://localhost");
      Guid userOid = Guid.NewGuid();
      ClaimsPrincipal user = new(new ClaimsIdentity(
        new List<Claim>()
        {
          new Claim(
            ClaimsPrincipalExtensions.CLAIM_OID,
            userOid.ToString())
        }));

      var bodyJson = new
      {
        Content = "Test Body"
      };
      var ms = new MemoryStream();
      await JsonSerializer.SerializeAsync(ms, bodyJson);
      ms.Seek(0, SeekOrigin.Begin);

      // act
      HttpContext context = await server.SendAsync(c => {
        c.Request.Body = ms;
        c.Request.Method = HttpMethods.Post;
        c.Request.Path = "/Rmc/PostAction";
        c.User = user;
      });

      // assert
      context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

      IUserActionLog userActionLog = UserActionLogServiceMock.UserActionLog;
      userActionLog.Action.Should().Be("PostAction");
      userActionLog.Controller.Should().Be("Rmc");
      userActionLog.IpAddress.Should().BeNull();
      userActionLog.Method.Should().Be("POST");
      userActionLog.Request.Should()
        .Be("https://localhost/Rmc/PostAction|{\"Content\":\"Test Body\"}");
      userActionLog.RequestAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      userActionLog.UserId.Should().Be(userOid);
    }
  }
}
