using Xunit;
using WmsHub.Referral.Api;
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

namespace WmsHub.Referral.Api.Tests
{
  public class ApiKeyProviderTests
  {
    private readonly Mock<IProviderService> _mockService =
      new Mock<IProviderService>();

    private readonly Mock<IConfiguration> _mockConfiguration =
      new Mock<IConfiguration>();

    private readonly Mock<IHttpContextAccessor> _mockAccessor =
      new Mock<IHttpContextAccessor>();

    private ApiKeyProvider _classToTest;

    private Business.Models.Provider _provider;

    private Guid _providerId = Guid.NewGuid();

    public ApiKeyProviderTests()
    {
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
      DefaultHttpContext context = new DefaultHttpContext();
      _mockAccessor.Setup(t => t.HttpContext).Returns(context);
      AuthServiceHelper.Configure(string.Empty, _mockAccessor.Object);
      _classToTest = new ApiKeyProvider(_mockConfiguration.Object);
    }

    [Fact()]
    public async Task Valid_Referral_Service_Returns_ApiKey()
    {
      //Arrange
      string expectedClaim = "Referral.Service";
      string key = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["ApiKey"]).Returns(key);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

      //Assert
      result.Key.Should().Be(key);
      result.Claims.Should().Contain(t => t.Value == key);
      result.OwnerName.Should().Be(expectedClaim);
    }

    [Fact()]
    public async Task Valid_SelfReferral_Service_Returns_ApiKey()
    {
      //Arrange
      string expectedClaim = "SelfReferral.Service";
      string key = Guid.NewGuid().ToString();
      string apiKey = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["SelfReferralApiKey"]).Returns(key);
      _mockConfiguration.Setup(t => t["ApiKey"]).Returns(apiKey);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

      //Assert
      result.Key.Should().Be(key);
      result.Claims.Should().Contain(t => t.Value == apiKey);
      result.OwnerName.Should().Be(expectedClaim);
    }

    [Fact()]
    public async Task InValid_Configuration_Return_null()
    {
      //Arrange
      string key = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["NotExist"]).Returns(key);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

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
        Assert.True(false, "Expected Exception");
      }
      catch (Exception ex)
      {
        Assert.True(true, ex.Message);
      }
    }

  }
}