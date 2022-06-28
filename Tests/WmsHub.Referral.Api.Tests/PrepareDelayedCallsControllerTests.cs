using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;
using WmsHub.Referral.Api.Controllers.Admin;
using WmsHub.Business.Services;
using Moq;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public class PrepareDelayedCallsControllerTests : TestSetup
  {
    private PrepareDelayedCallsController _controller;
    private new Mock<IReferralService> _mockReferralService;

    public PrepareDelayedCallsControllerTests()
    {
      _mockReferralService = new Mock<IReferralService>();
      _controller = new PrepareDelayedCallsController(
        _mockReferralService.Object);
    }

    public class Get : PrepareDelayedCallsControllerTests
    {      
      [Fact]
      public async Task Valid()
      {
        // ARRANGE
        string responseString = "Prepared DelayedCalls - " +
          "2 referral(s) set to 'RmcCall'.";

        _mockReferralService.Setup(r => r.PrepareDelayedCallsAsync())
          .Returns(Task.FromResult(responseString));
        _controller = new PrepareDelayedCallsController(
          _mockReferralService.Object);

        // ACT
        var result = await _controller.Get();

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        var outputResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responseString, outputResult.Value);
      }
    }
  }
}
