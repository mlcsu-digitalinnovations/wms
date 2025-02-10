using System;
using System.Net.Http;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using WmsHub.Business;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Tests.Helper;

namespace WmsHub.ProviderApi.Tests
{
  public class TestSetup : ATheoryData
  {
    protected readonly Mock<DatabaseContext> _mockContext =
                     new Mock<DatabaseContext>();
    protected readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    protected Mock<ProviderService> _mockProviderService;
    protected Mock<WmsAuthService> _mockAuthService;
    protected Mock<ILogger> _mockLogger = new();

    protected Mock<NotificationService> _mockNotificationService = new();

    protected readonly ClaimsPrincipal _providerUser;
    protected readonly ClaimsPrincipal _providerUserNoSid;
    protected readonly ClaimsPrincipal _providerAdminUser;
    protected readonly Guid _providerId = Guid.NewGuid();
    protected readonly Guid _providerAdminId = Guid.NewGuid();

    public TestSetup()
    {
      _mockNotificationService = new(
        _mockContext.Object,
        _mockLogger.Object,
        new HttpClient(),
        Options.Create(new NotificationOptions()));

      _mockProviderService = new Mock<ProviderService>(
        _mockContext.Object,
        _mockMapper.Object,
        TestConfiguration.CreateProviderOptions());

      _mockAuthService = new Mock<WmsAuthService>(
        _mockContext.Object,
        TestConfiguration.CreateProviderAuthOptions(),
        _mockMapper.Object,
        _mockNotificationService.Object,
        _mockLogger.Object);

     _providerUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.NameIdentifier, "Provider"),
        new Claim(ClaimTypes.Name, "Provider"),
        new Claim(ClaimTypes.Sid, _providerId.ToString())
        // other required and custom claims
      }, "TestAuthentication"));

      _providerAdminUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
        new Claim(ClaimTypes.Name, "Provider_admin"),
        new Claim(ClaimTypes.Sid, _providerAdminId.ToString())
        // other required and custom claims
      }, "TestAuthentication"));

      _providerUserNoSid = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.NameIdentifier, "Provider"),
        new Claim(ClaimTypes.Name, "Provider")
        // other required and custom claims
      }, "TestAuthentication"));
    }
  }
}
