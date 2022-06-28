using Xunit;
using WmsHub.Referral.Api.Controllers.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WmsHub.Referral.Api.Tests;

namespace WmsHub.Referral.Api.Controllers.Admin.Tests
{
  public class PrepareRmcCallsControllerTests:TestSetup
  {
    private PrepareRmcCallsController _classToTest;

    public class GetTests : PrepareRmcCallsControllerTests
    {
      [Fact()]
      public async Task Valid()
      {
        //Arrange
        int expected = 200;
        string returnString = $"Prepared 1 referral(s) for an RMC call.";
        _mockReferralService.Setup(t => t.PrepareRmcCallsAsync())
         .Returns(Task.FromResult(returnString));
        _classToTest = new PrepareRmcCallsController(_mockReferralService.Object);
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact()]
      public async Task Invalid_DbUpdateExceptionThrown_Exception()
      {
        //Arrange
        int expected = 500;
        _mockReferralService.Setup(t => t.PrepareRmcCallsAsync())
         .Throws(new DbUpdateException("Test"));
        _classToTest = new PrepareRmcCallsController(_mockReferralService.Object);
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }
    
  }
}