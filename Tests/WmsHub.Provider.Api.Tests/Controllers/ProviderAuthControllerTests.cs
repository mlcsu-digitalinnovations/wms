using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.ProviderApi.Tests;

namespace WmsHub.Provider.Api.Controllers.Tests
{
  public class ProviderAuthControllerTests : TestSetup
  {
    private readonly ProviderAuthController _classToTest;

    private IEnumerable<Business.Models.Provider> _emptyProviders =
      new List<Business.Models.Provider>();

    private IEnumerable<Business.Models.Provider> _validProviders;

    public ProviderAuthControllerTests()
    {
      _classToTest = new ProviderAuthController(_mockAuthService.Object);
      var provider = new Business.Models.Provider
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
      _validProviders =
        new List<Business.Models.Provider>
        {
          provider
        };
    }

    public class GetTests : ProviderAuthControllerTests
    {
      [Fact]
      public async Task ValidTest_returns_200()
      {
        //Arrange
        int expected = 200;
        _mockAuthService.Setup(x => x.SendNewKeyAsync())
         .Returns(Task.FromResult(true));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
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
      public async Task InvalidTest_Returns_500()
      {
        //Arrange
        int expected = 500;
        _mockAuthService.Setup(x => x.SendNewKeyAsync())
         .Returns(Task.FromResult(false));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task ProviderAuthCredentialsNotFoundException_Returns_401()
      {
        //Arrange
        int expected = 400;
        string error = 
          "Provider authentication credentials are not currently available.";
        _mockAuthService.Setup(x => x.SendNewKeyAsync())
         .Throws(new ProviderAuthCredentialsNotFoundException("test"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Get();
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be(error);
        }
      }

      [Fact]
      public async Task Exception_Throws()
      {
        //Arrange
        _mockAuthService.Setup(x => x.SendNewKeyAsync())
         .Throws(new Exception("test"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        try
        {
          var response = await _classToTest.Get();
          //Assert
          Assert.True(false, "Exception expected");
        }
        catch (Exception ex)
        {
          Assert.True(true, ex.Message);
        }
      }
    }

    public class GetKeyTests : ProviderAuthControllerTests
    {
      [Fact]
      public async Task ValidTest_returns_200_token()
      {
        //Arrange
        string key = "12345678";
        var validationResponse = new KeyValidationResponse
        {
          ValidationStatus = ValidationType.ValidKey
        };
        var tokenResponse = new AccessTokenResponse
        {
          ValidationStatus = ValidationType.Valid,
          AccessToken = "new access token",
          RefreshToken = "new refresh token",
          Expires = 1440,
          TokenType = "Bearer"
        };
        int expected = 200;
        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Returns(Task.FromResult(validationResponse));
        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Returns(Task.FromResult(tokenResponse));
        _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
         .Returns(Task.FromResult(true));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Get(key);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Theory]
      [InlineData(ValidationType.KeyApiKeyMismatch)]
      [InlineData(ValidationType.KeyOutOfDate)]
      [InlineData(ValidationType.KeyIncorrectLength)]
      [InlineData(ValidationType.KeyIsNotNumeric)]
      [InlineData(ValidationType.KeyNotFound)]
      [InlineData(ValidationType.KeyNotRecognised)]
      [InlineData(ValidationType.KeyNotSet)]
      public async Task Invalid_KeyValidationResponse_Returns_400(
        ValidationType validationType)
      {
        //Arrange
        string key = "12345678";
        var validationResponse =
          new KeyValidationResponse(validationType, "Test");

        int expected = 400;
        string error = "Test";
        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Returns(Task.FromResult(validationResponse));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Get(key);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be(error);
        }
      }

      [Fact]
      public async Task ValidateKey_Exception()
      {
        //Arrange
        string key = "12345678";

        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Throws(new Exception("Test"));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        try
        {
          var response = await _classToTest.Get(key);
          //Assert
          Assert.True(false, "Exception expected");
        }
        catch (Exception ex)
        {
          Assert.True(true, ex.Message);
        }
      }

      [Fact]
      public async Task Invalid_GenerateTokenAsync_Returns_400()
      {
        //Arrange 
        string key = "12345678";
        var validationResponse = new KeyValidationResponse
        {
          ValidationStatus = ValidationType.ValidKey
        };
        var tokenResponse = new AccessTokenResponse();
        tokenResponse.SetStatus(ValidationType.Invalid, "Test");
        int expected = 400;
        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Returns(Task.FromResult(validationResponse));
        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Returns(Task.FromResult(tokenResponse));


        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext {User = _providerUser};
        //Act
        var response = await _classToTest.Get(key);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be("Test");
        }
      }

      [Fact]
      public async Task NoProvider_ArgumentNullException()
      {
        //Arrange
        string key = "12345678";
        var validationResponse = new KeyValidationResponse
        {
          ValidationStatus = ValidationType.ValidKey
        };
        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Returns(Task.FromResult(validationResponse));
        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Throws(new ArgumentNullException("Test Provider not found"));


        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext {User = _providerUser};
        //Act
        try
        {
          var response = await _classToTest.Get(key);
          //Assert
          Assert.True(false, "ArgumentNullException expected");
        }
        catch (ArgumentNullException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
      }

      [Fact]
      public async Task NoValidGuidId_ArgumentException()
      {
        //Arrange
        string key = "12345678";
        var validationResponse = new KeyValidationResponse
        {
          ValidationStatus = ValidationType.ValidKey
        };
        _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
         .Returns(Task.FromResult(validationResponse));
        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Throws(new ArgumentException("Not valid test id"));


        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        try
        {
          var response = await _classToTest.Get(key);
          //Assert
          Assert.True(false, "ArgumentException expected");
        }
        catch (ArgumentException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
      }

    }

    public class PostTests : ProviderAuthControllerTests
    {
      [Fact]
      public async Task ValidTest_returns_200_token()
      {
        //Arrange
        string grant_type = "refresh_token";
        string refresh_token = "TestToken";
        RefreshTokenValidationResponse tokenValidationResponse =
          new RefreshTokenValidationResponse(ValidationType.Valid);

        var tokenResponse = new AccessTokenResponse
        {
          ValidationStatus = ValidationType.Valid,
          AccessToken = "new access token",
          RefreshToken = "new refresh token",
          Expires = 1440,
          TokenType = "Bearer"
        };
        int expected = 200;
        _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
         .Returns(Task.FromResult(tokenValidationResponse));

        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Returns(Task.FromResult(tokenResponse));
        _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
         .Returns(Task.FromResult(true));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Post(grant_type, refresh_token);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_MissingRefreshKey_Returns_400()
      {
        //Arrange
        int expected = 400;
        string error = "Refresh token not supplied.";
        //Act
        var response = await _classToTest.Post(null, null);
        //Assert
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be(error);
        }
      }

      [Fact]
      public async Task Invalid_MissingGrantType_Returns_400()
      {
        //Arrange
        int expected = 400;
        string error = "Grant Type must be supplied.";
        //Act
        var response = await _classToTest.Post(null, "Test Key");
        //Assert
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be(error);
        }
      }

      [Fact]
      public async Task Invalid_MissingGrantTypeMismatch_Returns_400()
      {
        //Arrange
        int expected = 400;
        string error = "Only grant type of refresh_token allowed.";
        //Act
        var response = await _classToTest.Post("wrong_type", "Test Key");
        //Assert
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
          details.Detail.Should().Be(error);
        }
      }

      [Fact]
      public async Task Invalid_validationResponse_Returns_400()
      {
        //Arrange
        string grant_type = "refresh_token";
        string refresh_token = "TestToken";
        RefreshTokenValidationResponse tokenValidationResponse =
          new RefreshTokenValidationResponse(ValidationType.Invalid, "Error");

        int expected = 400;
        _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
         .Returns(Task.FromResult(tokenValidationResponse));


        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Post(grant_type, refresh_token);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
        }
      }

      [Fact]
      public async Task Invalid_TokenGeneration_Return_400()
      {
        //Arrange
        string grant_type = "refresh_token";
        string refresh_token = "TestToken";
        RefreshTokenValidationResponse tokenValidationResponse =
          new RefreshTokenValidationResponse(ValidationType.Valid);

        var tokenResponse = new AccessTokenResponse();
        tokenResponse.SetStatus(ValidationType.Invalid,"Error Test");

        int expected = 400;
        _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
         .Returns(Task.FromResult(tokenValidationResponse));

        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Returns(Task.FromResult(tokenResponse));

        _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
         .Returns(Task.FromResult(true));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Post(grant_type, refresh_token);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result = response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        if (result.Value.GetType().Name == "ProblemDetails")
        {
          ProblemDetails details = result.Value as ProblemDetails;
          details.Status.Should().Be(expected);
        }
      }

      [Fact]
      public async Task NoProvider_ArgumentNullException()
      {
        //Arrange
        string grant_type = "refresh_token";
        string refresh_token = "TestToken";

        RefreshTokenValidationResponse tokenValidationResponse =
          new RefreshTokenValidationResponse(ValidationType.Valid);
        _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
         .Returns(Task.FromResult(tokenValidationResponse));

        _mockAuthService.Setup(x => x.GenerateTokensAsync())
         .Throws(new ArgumentNullException("Test Provider not found"));


        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        try
        {
          var response = await _classToTest.Post(grant_type, refresh_token);
          //Assert
          Assert.True(false, "ArgumentNullException expected");
        }
        catch (ArgumentNullException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
      }

    }
  }
}