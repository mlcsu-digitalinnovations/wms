using AutoMapper;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using WmsHub.Business;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Referral.Api.Models;
using WmsHub.Tests.Helper;

namespace WmsHub.Referral.Api.Tests
{
  public class TestSetup: AModelsBaseTests
  {
    public const string CLAIM_NAME_REFERRAL_SERVICE = "Referral.Service";
    public const string TEST_UBRN = "123456789012";

    protected readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();
    
    protected readonly Mock<IProviderService> _mockProviderService =
      new Mock<IProviderService>();

    protected readonly Mock<IPostcodesIoService> _mockPostcodeIoService =
      new Mock<IPostcodesIoService>();

    protected readonly Mock<ICsvExportService> _mockCsvExport =
      new Mock<ICsvExportService>();

    protected Mock<ILinkIdService> _mockLinkIdService = new();
    protected readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    protected Mock<ReferralService> _mockReferralService;
    protected Mock<IProcessStatusService> _mockProcessStatusService;
    protected Mock<IOptions<ProcessStatusOptions>> _mockProcessStatusOptions;

    protected readonly Mock<IPatientTriageService> _mockPatientTriageService =
      new Mock<IPatientTriageService>();

    protected IMapper Mapper { get; set; }

    public TestSetup()
    {
      _mockProcessStatusService = new Mock<IProcessStatusService>();
      _mockProcessStatusOptions = new Mock<IOptions<ProcessStatusOptions>>();

      _mockReferralService = new Mock<ReferralService>(
        _mockContext.Object,
        _mockLinkIdService.Object,
        _mockMapper.Object,
        _mockProviderService.Object,
        _mockCsvExport.Object,
        _mockPatientTriageService.Object
      );

      _mockReferralService.Object.User = GetClaimsPrincipal();

      MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Referral.Api"
        })
      );

      Mapper = mapperConfiguration.CreateMapper();
    }

    protected static ClaimsPrincipal GetUnknownClaimsPrincipal(string name)
    {
      List<Claim> claims = new List<Claim>()
      {
        new Claim(ClaimTypes.Sid, TEST_USER_ID),
        new Claim(ClaimTypes.Name, name)
      };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }
  }
}
