using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Referral.Api.Controllers.Admin;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public class PrepareUnableToContactControllerTests : TestSetup
  {
    private PrepareUnableToContactReferralsController _controller;
    private new readonly Mock<IReferralService> _mockReferralService = new();
    public PrepareUnableToContactControllerTests()
    {
    }

    public class Get : PrepareUnableToContactControllerTests
    {
      public Get()
      { }

      [Fact]
      public async Task Valid()
      {
        // ARRANGE
        string[] expectedMessage = Array.Empty<string>();

        _mockReferralService.Setup(r => r.PrepareUnableToContactAsync())
            .Returns(Task.FromResult(expectedMessage));

        _controller =
          new PrepareUnableToContactReferralsController(
            _mockReferralService.Object);

        // ACT
        var result = await _controller.Get();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        var outputResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedMessage, outputResult.Value);
      }
    }
  }
}