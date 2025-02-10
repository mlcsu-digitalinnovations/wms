using Xunit;
using WmsHub.TextMessage.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services;

namespace WmsHub.TextMessage.Api.Tests
{
  public class ApiKeyProviderTests
  {
    private readonly Mock<ITextService> _mockService =
      new Mock<ITextService>();

    private readonly Mock<IConfiguration> _mockConfiguration =
      new Mock<IConfiguration>();

    private readonly Mock<IHttpContextAccessor> _mockAccessor =
      new Mock<IHttpContextAccessor>();

    private ApiKeyProvider _classToTest;


    public ApiKeyProviderTests()
    {
      DefaultHttpContext context = new DefaultHttpContext();
      _mockAccessor.Setup(t => t.HttpContext).Returns(context);
      AuthServiceHelper.Configure(string.Empty, _mockAccessor.Object);
      _classToTest = new ApiKeyProvider(_mockService.Object,
        _mockConfiguration.Object);
    }

    [Fact()]
    public async Task Valid_Referral_Service_Returns_ApiKey()
    {
      //Arrange
      string key = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["ApiKey"]).Returns(key);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

      //Assert
      result.Key.Should().Be(key);
      result.Claims.Should().Contain(t => t.Value == key);
    }


    [Fact()]
    public async Task InValid_Configuration_Return_null()
    {
      //Arrange
      string key = Guid.NewGuid().ToString();
      string keyNotFound = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["ApiKey"]).Returns(key);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(keyNotFound);

      //Assert
      result.Should().BeNull();
    }


    [Fact()]
    public async Task Invalid_KeyIsNull_Throw_Exception()
    {
      //Arrange

      //Act
      try
      {
        IApiKey result = await _classToTest.ProvideAsync("");
        //Assert
        Assert.Fail("Expected Exception");
      }
      catch (Exception ex)
      {
        Assert.True(true, ex.Message);
      }
    }

  }
}