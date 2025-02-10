using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using Serilog.Events;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.UiTests
{
  public class TestSetup
  {
    protected static Guid TEST_USER_ID = 
      Guid.Parse("571342f1-c67d-49bf-a9c6-40a41e6dc702");
    protected readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();

    protected readonly Mock<IProviderService> _mockProviderService =
      new Mock<IProviderService>();

    protected readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

    protected Mock<IReferralService>
      _mockService = new Mock<IReferralService>();

    public TestSetup()
    {
      _mockService.Object.User = GetClaimsPrincipal();
    }

    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      List<Claim> claims = new List<Claim>()
        { new Claim(ClaimTypes.Sid, TEST_USER_ID.ToString()) };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }
  }
}
