using Xunit;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WmsHub.Referral.Api.Tests;

namespace WmsHub.Referral.Api.Controllers.Admin.Tests
{
  public class PostcodeControllerTests:TestSetup
  {
    private PostcodeController _classToBeTests;
    public class GetLsoaTests : PostcodeControllerTests
    {
      [Fact]
      public async Task Valid()
      {
        //Arrange
        int expected = 200;
        string expectedResult = "test";
        string postcode = "AF1 1AF";
        _mockPostcodeIoService.Setup(t => t.GetLsoaAsync(It.IsAny<string>()))
         .Returns(Task.FromResult(expectedResult));
        _classToBeTests = new PostcodeController(_mockPostcodeIoService.Object);
        //Act
        var response = await _classToBeTests.GetLsoa(postcode);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_PostcodeNull_Returns_400()
      {
        //Arrange
        int expected = 400;
        _classToBeTests = new PostcodeController(_mockPostcodeIoService.Object);
        //Act
        var response = await _classToBeTests.GetLsoa(string.Empty);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult result =
          response as BadRequestObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task ExceptiionHandled_Returns_500()
      {
        //Arrange
        int expected = 500;
        string postcode = "AF1 1AF";
        _mockPostcodeIoService.Setup(t => t.GetLsoaAsync(It.IsAny<string>()))
         .Throws(new ArgumentException("Expected"));
        _classToBeTests = new PostcodeController(_mockPostcodeIoService.Object);
        //Act
        var response = await _classToBeTests.GetLsoa(postcode);
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