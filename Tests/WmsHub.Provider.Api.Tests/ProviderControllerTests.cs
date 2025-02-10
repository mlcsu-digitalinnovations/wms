using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Provider.Api.Controllers;
using Xunit;

namespace WmsHub.ProviderApi.Tests
{
  public class ProviderControllerTests : TestSetup
  {
    private Business.Models.Provider _provider;
    private ProviderController _classToTest;

    public ProviderControllerTests()
    {
      _classToTest = new ProviderController(_mockProviderService.Object);
      _provider = new Business.Models.Provider
      {
        Id = _providerId,
        IsActive = true,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Name = "Test",
        Summary = "Test",
        Website = "Test",
        Logo = "Test",
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = _providerId
      };
    }

    public class GetTests : ProviderControllerTests
    {
      [Fact]
      public async Task Valid_Returns_200()
      {
        //Arrange
        int expected = 200;
        ProviderResponse _providerResponse =
          new ProviderResponse() {ResponseStatus = StatusType.Valid};

        _mockProviderService.Setup(t => t.GetProviderAsync(It.IsAny<Guid>()))
         .Returns(Task.FromResult(_providerResponse));
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext {User = _providerUser}
        };
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_No_Sid_Guid_Returns_500()
      {
        //Arrange
        int expected = 500;
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext { User = _providerUserNoSid }
        };
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
        }
      }


      [Fact]
      public async Task Invalid_ProviderResponse_Exception_Returns_500()
      {
        //Arrange
        int expected = 500;
        _mockProviderService.Setup(t => t.GetProviderAsync(It.IsAny<Guid>()))
         .Throws(new Exception("Test any exception"));
        _classToTest.ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext { User = _providerUser }
        };
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
        }
      }
    }

  }
}
