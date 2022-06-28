using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using WmsHub.Referral.Api.Tests;
using Xunit;

namespace WmsHub.Referral.Api.Controllers.Admin.Tests
{
  public class GeneratorControllerTests : TestSetup
  {
    private GeneratorController _classToTest;
    public class GetTests : GeneratorControllerTests
    {
      [Fact()]
      public void Valid()
      {
        Random random = new Random();
        int expected = 200;
        int expectedCount = 1;
        string[] nhsNumbers = new[] { "12345678901" };
        _mockReferralService.Setup(t => t.GetNhsNumbers(expectedCount))
         .Returns(nhsNumbers);
        _classToTest = new GeneratorController(_mockReferralService.Object);
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
        };
        //Act
        var response = _classToTest.Get(expectedCount);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public void SingleNumberGeneratedFromNull()
      {
        Random random = new Random();
        int expected = 200;
        int expectedCount = 1;
        string[] nhsNumbers = new[] { "12345678901" };
        _mockReferralService.Setup(t => t.GetNhsNumbers(expectedCount))
         .Returns(nhsNumbers);
        _classToTest = new GeneratorController(_mockReferralService.Object);
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
        };
        //Act
        var response = _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
        ((string[])result.Value).Should().HaveCount(expectedCount);
      }

      [Fact]
      public void ExceptionCapturedRetuns500()
      {
        Random random = new Random();
        int expected = 500;
        string[] nhsNumbers = new[] { "12345678901" };
        _mockReferralService.Setup(t => t.GetNhsNumbers(It.IsAny<int>()))
         .Throws(new ArgumentException("Test"));
        _classToTest = new GeneratorController(_mockReferralService.Object);
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
        };
        //Act
        var response = _classToTest.Get();
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