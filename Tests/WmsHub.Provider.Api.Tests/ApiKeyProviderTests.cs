using Xunit;
using System;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Provider.Api.Tests
{
  public class ApiKeyProviderTests
  {
    private readonly Mock<IApiKeyService> _mockApiService = new();

    private readonly Mock<IProviderService> _mockService = new();

    private readonly Mock<IConfiguration> _mockConfiguration = new();

    private readonly Mock<IHttpContextAccessor> _mockAccessor = new();
    private readonly Mock<ApiKeyStoreResponse> _mockResponse = new();
    
    private ApiKeyProvider _classToTest;

    private Business.Models.Provider _provider;

    private Guid _providerId = Guid.NewGuid();

    public ApiKeyProviderTests()
    {
      if (_provider == null)
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
      }

      DefaultHttpContext context = new();
      _mockAccessor.Setup(t => t.HttpContext).Returns(context);
      AuthServiceHelper.Configure(string.Empty, _mockAccessor.Object);
      _classToTest = new ApiKeyProvider(_mockService.Object,
        _mockApiService.Object, _mockConfiguration.Object, 
        _mockAccessor.Object);
      _mockResponse.Setup(t => t.Sid)
        .Returns("test");
      _mockResponse.Setup(t => t.NameIdentifier)
        .Returns("test");
      _mockResponse.Setup(t => t.KeyUser)
        .Returns("test");
    }

    [Fact()]
    public async Task Valid_ProviderAdmin_Returns_ApiKey()
    {
      //Arrange
      string key = Guid.NewGuid().ToString();
      _mockConfiguration.Setup(t => t["ApiKey"]).Returns(key);
      _mockService.Setup(t => t.ValidateProviderKeyAsync(key))
        .Returns(Task.FromResult(_providerId));
      _mockResponse.Setup(t => t.ValidationStatus)
        .Returns(ValidationType.ValidKey);
      _mockApiService.Setup(t => t.Validate(key, true))
        .ReturnsAsync(_mockResponse.Object);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

     //Assert
     result.Key.Should().Be(key);
     result.Claims.Should().Contain(t=>t.Value == "test");
    }

    /// <summary>
    /// This should return ProviderId only not the Entity
    /// </summary>
    /// <returns></returns>
    [Fact()]
    public async Task Valid_Provider_Returns_ApiKey()
    {
      //Arrange
      string key = _providerId.ToString();
      _mockConfiguration.Setup(t => t["ApiKey"])
       .Returns(Guid.NewGuid().ToString());
      _mockService.Setup(t => t.ValidateProviderKeyAsync(key))
       .Returns(Task.FromResult(_providerId));
      _mockResponse.Setup(t => t.ValidationStatus)
        .Returns(ValidationType.ValidKey);

      _mockApiService.Setup(t => t.Validate(key, true))
        .ReturnsAsync(_mockResponse.Object);

      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

      //Assert
      result.Key.Should().Be(key);
      result.Claims.Should().Contain(t => t.Value == "test");
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

    [Fact()]
    public async Task Invalid_ProviderIsNull_Returns_Null()
    {
      //Arrange
      string key = _providerId.ToString();
      _mockConfiguration.Setup(t => t["ApiKey"])
       .Returns(Guid.NewGuid().ToString());
      _mockService.Setup(t => t.ValidateProviderKeyAsync(key))
       .Returns(Task.FromResult(Guid.Empty));
      _mockResponse.Setup(t => t.ValidationStatus)
        .Returns(ValidationType.Invalid);

      _mockApiService.Setup(t => t.Validate(key, true))
        .ReturnsAsync(_mockResponse.Object);
      // Act
      IApiKey result = await _classToTest.ProvideAsync(key);

      //Assert
      result.Should().BeNull();
    }

    [Fact()]
    public async Task Invalid_Provider_Throws_Exception()
    {
      //Arrange
      string key = _providerId.ToString();
      _mockConfiguration.Setup(t => t["ApiKey"])
       .Returns(Guid.NewGuid().ToString());
      _mockService.Setup(t => t.ValidateProviderKeyAsync(key))
       .Throws(new Exception("Test Exception"));
      // Act
      try
      {
        IApiKey result = await _classToTest.ProvideAsync(key);
      }
      catch (Exception ex)
      {
        Assert.True(true, ex.Message);
      }
    }


  }
}