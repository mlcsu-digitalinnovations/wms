using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Moq;
using WmsHub.Business;
using WmsHub.Business.Services;
using WmsHub.Tests.Helper;
using ValidationContext = 
  System.ComponentModel.DataAnnotations.ValidationContext;

namespace WmsHub.Referral.Api.Tests
{
  public class TestSetup: AModelsBaseTests
  {
    public const string TEST_USER_ID = 
      "571342f1-c67d-49bf-a9c6-40a41e6dc702";
    public const string CLAIM_NAME_REFERRAL_SERVICE = "Referral.Service";
    public const string TEST_UBRN = "123456789012";

    protected readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();
    
    protected readonly Mock<IProviderService> _mockProviderService =
      new Mock<IProviderService>();

    protected readonly Mock<IPostcodeService> _mockPostcodeService =
      new Mock<IPostcodeService>();

    protected readonly Mock<ICsvExportService> _mockCsvExport =
      new Mock<ICsvExportService>();

    protected readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    protected Mock<ReferralService> _mockReferralService;

    protected readonly Mock<IPatientTriageService> _mockPatientTriageService =
      new Mock<IPatientTriageService>();

    protected IMapper Mapper { get; set; }

    public TestSetup()
    {
      _mockReferralService = new Mock<ReferralService>(
        _mockContext.Object,
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

    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      List<Claim> claims = new List<Claim>()
        { new Claim(ClaimTypes.Sid, TEST_USER_ID) };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
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
