using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Referral.Api.Controllers.Admin;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Controllers.Admin;

public class ReferralDataControllerTests
{
  private readonly string[] _defaultIds =
  [
    "00000000-0000-0000-0000-000000000000",
    "00000000-0000-0000-0000-000000000001",
    "00000000-0000-0000-0000-000000000002"
  ];

  private readonly string[] _defaultUbrns = ["000000000000", "000000000001", "000000000002"];

  private readonly Mock<IReferralService> _referralService = new();

  [Fact]
  public async Task Error_Return500Response()
  {
    // Arrange.
    string exceptionBody = "Argument is null";
    _referralService
      .Setup(x => x.GetIdsFromUbrns(_defaultUbrns))
      .ThrowsAsync(new ArgumentException(exceptionBody));

    ReferralDataController controller = new(_referralService.Object);

    // Act.
    IActionResult res = await controller.UbrnToId(_defaultUbrns);

    // Assert.
    res.Should().BeOfType<BadRequestObjectResult>();
    BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult)res;
    badRequestObjectResult.Value.Should().Be(exceptionBody);
  }

  [Fact]
  public async Task ValidUbrns_Return200WithIds()
  {
    // Arrange.
    _referralService.Setup(x => x.GetIdsFromUbrns(_defaultUbrns)).ReturnsAsync(_defaultIds);
    ReferralDataController controller = new(_referralService.Object);

    // Act.
    IActionResult res = await controller.UbrnToId(_defaultUbrns);

    // Assert.
    res.Should().BeOfType<OkObjectResult>();
    OkObjectResult okResult = (OkObjectResult)res;
    okResult.Value.Should().Be(_defaultIds);
  }
}
