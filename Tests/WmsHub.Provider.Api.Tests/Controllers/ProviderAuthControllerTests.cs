using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.AuthService;
using WmsHub.ProviderApi.Tests;
using WmsHub.Provider.Api.Models;
using FluentAssertions.Execution;
using WmsHub.Business.Models.ProviderService;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UglyToad.PdfPig.Graphics.Colors;

namespace WmsHub.Provider.Api.Controllers.Tests;

public class ProviderAuthControllerTests : TestSetup
{
  private readonly ProviderAuthController _providerAuthController;

  private IEnumerable<Business.Models.Provider> _emptyProviders =
    new List<Business.Models.Provider>();

  private IEnumerable<Business.Models.Provider> _validProviders;

  public ProviderAuthControllerTests()
  {
    _providerAuthController = new ProviderAuthController(_mockAuthService.Object);
    Business.Models.Provider provider = new ()
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
    [Theory]
    [InlineData(new[] {MessageType.Email})]
    [InlineData(new[] {MessageType.SMS})]
    [InlineData(new[] {MessageType.Email, MessageType.SMS})]
    public async Task KeySentSuccessfully_NoErrors_Returns200(MessageType[] messageTypes)
    {
      // Arrange.
      string expectedMessage = "A new authentication key has been sent to your registered " +
        $"contact via {string.Join(", ", messageTypes)}.";

      ProviderAuthNewKeyResponse serviceResponse = new()
      {
        MessageTypesSent = messageTypes.ToList()
      };

      _mockAuthService.Setup(x => x.SendNewKeyAsync()).ReturnsAsync(serviceResponse).Verifiable();

      _providerAuthController.ControllerContext = new() 
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get();

      // Assert.
      _mockAuthService.Verify();
      response.Should().NotBeNull().And.BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(MessageType.Email)]
    [InlineData(MessageType.SMS)]
    public async Task KeySentSuccessfully_WithErrors_Returns200(MessageType messageTypeSent)
    {
      // Arrange.
      List<string> errorList = new() { "Error." };
      string expectedMessage = "A new authentication key has been sent to your registered " +
        $"contact via {messageTypeSent}. The following errors occurred during the process: " +
        string.Join(" ", errorList);

      ProviderAuthNewKeyResponse serviceResponse = new()
      {
        MessageTypesSent = new() { messageTypeSent },
        Errors = errorList
      };

      _mockAuthService.Setup(x => x.SendNewKeyAsync()).ReturnsAsync(serviceResponse).Verifiable();

      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get();

      // Assert.
      _mockAuthService.Verify();
      response.Should().NotBeNull().And.BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task KeysNotSent_WithErrors_Returns500()
    {
      // Arrange.
      List<string> errorList = new() { "First error.", "Second error." };
      string expectedMessage = "There was a problem sending a new key. The following errors " +
        "occurred: " + string.Join(" ", errorList);

      ProviderAuthNewKeyResponse serviceResponse = new()
      {
        MessageTypesSent = new(),
        Errors = errorList
      };

      _mockAuthService.Setup(x => x.SendNewKeyAsync()).ReturnsAsync(serviceResponse).Verifiable();

      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get();

      // Assert.
      _mockAuthService.Verify();
      ObjectResult result = response.Should().NotBeNull().And.BeOfType<ObjectResult>().Subject;
      result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
      result.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task Exception_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message.";
      string expectedResponseMessage = "There was a problem sending a new key. " + exceptionMessage;

      _mockAuthService.Setup(x => x.SendNewKeyAsync())
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();
      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get();

      // Assert.
      _mockAuthService.Verify();
      ObjectResult result = response.Should().NotBeNull().And.BeOfType<ObjectResult>().Subject;
      result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
      result.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedResponseMessage);
    }

    [Fact]
    public async Task ProviderAuthCredentialsNotFoundException_Returns400()
    {
      // Arrange.
      string expectedMessage = 
        "Provider authentication credentials are not currently available.";
      _mockAuthService.Setup(x => x.SendNewKeyAsync())
       .Throws(new ProviderAuthCredentialsNotFoundException("Exception message."))
       .Verifiable();
      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get();

      // Assert.
      _mockAuthService.Verify();
      ObjectResult result = response.Should().NotBeNull().And.BeOfType<ObjectResult>().Subject;
      result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
      result.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedMessage);
    }
  }

  public class GetKeyTests : ProviderAuthControllerTests
  {
    [Fact]
    public async Task ValidTest_returns_200_token()
    {
      // Arrange.
      string key = "12345678";
      int expected = 200;
      KeyValidationResponse validationResponse = new ()
      {
        ValidationStatus = ValidationType.ValidKey
      };
      AccessTokenResponse tokenResponse = new ()
      {
        ValidationStatus = ValidationType.Valid,
        AccessToken = "new access token",
        RefreshToken = "new refresh token",
        Expires = 1440,
        TokenType = "Bearer"
      };
      
      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Returns(Task.FromResult(validationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.Get(key);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }
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
      // Arrange.
      string key = "12345678";
      int expected = 400;
      string error = "Test";

      KeyValidationResponse validationResponse =
        new (validationType, "Test");
      
      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Returns(Task.FromResult(validationResponse));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response = await _providerAuthController.Get(key);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task ValidateKey_Exception()
    {
      // Arrange.
      string key = "12345678";
      string expectedError = "Test";

      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Throws(new Exception("Test"));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      Func<Task> act = async () => await _providerAuthController.Get(key);

      // Assert.
      await act.Should().ThrowAsync<Exception>()
        .WithMessage(expectedError);
    }

    [Fact]
    public async Task Invalid_GenerateTokenAsync_Returns_400()
    {
      // Arrange.
      string key = "12345678";
      int expected = 400;

      KeyValidationResponse validationResponse = new ()
      {
        ValidationStatus = ValidationType.ValidKey
      };

      AccessTokenResponse tokenResponse = new ();
      tokenResponse.SetStatus(ValidationType.Invalid, "Test");
      
      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Returns(Task.FromResult(validationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext {User = _providerUser};

      // Act.
      IActionResult response = await _providerAuthController.Get(key);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task NoProvider_ArgumentNullException()
    {
      // Arrange.
      string key = "12345678";
      string expectedErrorMessage =
        "Value cannot be null. (Parameter 'Test Provider not found')";

      KeyValidationResponse validationResponse = new ()
      {
        ValidationStatus = ValidationType.ValidKey
      };

      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Returns(Task.FromResult(validationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Throws(new ArgumentNullException("Test Provider not found"));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      Func<Task> act = async () => await _providerAuthController.Get(key);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>()
        .WithMessage(expectedErrorMessage);
    }

    [Fact]
    public async Task NoValidGuidId_ArgumentException()
    {
      // Arrange.
      string key = "12345678";
      string expectedErrorMessage = "Not valid test id";

      KeyValidationResponse validationResponse = new ()
      {
        ValidationStatus = ValidationType.ValidKey
      };

      _mockAuthService.Setup(x => x.ValidateKeyAsync(key))
       .Returns(Task.FromResult(validationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Throws(new ArgumentException("Not valid test id"));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      Func<Task> act = async () => await _providerAuthController.Get(key);

      // Assert.
      await act.Should().ThrowAsync<ArgumentException>()
        .WithMessage(expectedErrorMessage);
    }
  }

  public class PostTests : ProviderAuthControllerTests
  {
    [Fact]
    public async Task ValidTest_returns_200_token()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 200;

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);

      AccessTokenResponse tokenResponse = new ()
      {
        ValidationStatus = ValidationType.Valid,
        AccessToken = "new access token",
        RefreshToken = "new refresh token",
        Expires = 1440,
        TokenType = "Bearer"
      };

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }

    [Fact]
    public async Task Invalid_MissingRefreshKey_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Refresh token not supplied.";

      // Act.
      IActionResult response = await _providerAuthController.Post(null, null);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantType_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Grant Type must be supplied.";

      // Act.
      IActionResult response = await _providerAuthController.Post(null, "Test Key");

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantTypeMismatch_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Only grant type of refresh_token allowed.";

      // Act.
      IActionResult response =
        await _providerAuthController.Post("wrong_type", "Test Key");

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_validationResponse_Returns_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Invalid, "Error");

      int expected = 400;
      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_TokenGeneration_Return_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 400;

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);
      AccessTokenResponse tokenResponse = new ();
      tokenResponse.SetStatus(ValidationType.Invalid,"Error Test");
      
      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task NoProvider_ArgumentNullException()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      string expectedErrorMessage =
        "Value cannot be null. (Parameter 'Test Provider not found')";

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Throws(new ArgumentNullException("Test Provider not found"));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      Func<Task> act = async () => 
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>()
        .WithMessage(expectedErrorMessage);
    }
  }

  public class RefreshTokenTests : ProviderAuthControllerTests
  {
    [Fact]
    public async Task ValidTest_returns_200_token()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 200;

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);
      AccessTokenResponse tokenResponse = new ()
      {
        ValidationStatus = ValidationType.Valid,
        AccessToken = "new access token",
        RefreshToken = "new refresh token",
        Expires = 1440,
        TokenType = "Bearer"
      };

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest {
          GrantType = grant_type,
          RefreshToken = refresh_token
        });

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }

    [Fact]
    public async Task Invalid_MissingRefreshKey_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Refresh token not supplied.";

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest());

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantType_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Grant Type must be supplied.";

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest
        {
          RefreshToken = "Test Key"
        });

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantTypeMismatch_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string error = "Only grant type of refresh_token allowed.";

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest
        {
          GrantType = "wrong_type",
          RefreshToken = "Test Key"
        });

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_validationResponse_Returns_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 400;

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Invalid, "Error");
      
      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest
        {
          GrantType = grant_type,
          RefreshToken = refresh_token
        });

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_TokenGeneration_Return_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 400;

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);

      AccessTokenResponse tokenResponse = new ();
      tokenResponse.SetStatus(ValidationType.Invalid, "Error Test");
      
      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = await _providerAuthController.RefreshToken(
        new RefreshTokenRequest
        {
          GrantType = grant_type,
          RefreshToken = refresh_token
        });

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task NoProvider_ArgumentNullException()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      string expectedErrorMessage =
        "Value cannot be null. (Parameter 'Test Provider not found')";

      RefreshTokenValidationResponse tokenValidationResponse =
        new (ValidationType.Valid);

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Throws(new ArgumentNullException("Test Provider not found"));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      Func<Task> act = async () => await _providerAuthController.RefreshToken(
          new RefreshTokenRequest
          {
            GrantType = grant_type,
            RefreshToken = refresh_token
          });

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>()
        .WithMessage(expectedErrorMessage);
    }
  }

  public class TokenTests: ProviderAuthControllerTests
  {
    [Fact]
    public async Task ValidTest_returns_200_token()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "refresh_token_21";
      int expected = 200;

      RefreshTokenValidationResponse tokenValidationResponse =
        new(ValidationType.Valid);
      AccessTokenResponse tokenResponse = new()
      {
        ValidationStatus = ValidationType.Valid,
        AccessToken = "new access token",
        RefreshToken = "new refresh token",
        Expires = 1440,
        TokenType = "Bearer"
      };

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new()
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response = 
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }

    [Fact]
    public async Task Invalid_MissingRefreshKey_Returns_400()
    {
      // Arrange.
      int expected = 400;
      string grant_type = "refresh_token";
      string error = "Refresh token not supplied.";

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, null);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantType_Returns_400()
    {
      // Arrange.
      string refresh_token = "refresh_token_21";
      int expected = 400;
      string error = "Grant Type must be supplied.";

      // Act.
      IActionResult response =
        await _providerAuthController.Post(null, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_MissingGrantTypeMismatch_Returns_400()
    {
      // Arrange.
      string grant_type = "access_token";
      string refresh_token = "refresh_token_21";
      int expected = 400;
      string error = "Only grant type of refresh_token allowed.";

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_validationResponse_Returns_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 400;

      RefreshTokenValidationResponse tokenValidationResponse =
        new(ValidationType.Invalid, "Error");

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task Invalid_TokenGeneration_Return_400()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      int expected = 400;

      RefreshTokenValidationResponse tokenValidationResponse =
        new(ValidationType.Valid);

      AccessTokenResponse tokenResponse = new();
      tokenResponse.SetStatus(ValidationType.Invalid, "Error Test");

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Returns(Task.FromResult(tokenResponse));
      _mockAuthService.Setup(t => t.SaveTokenAsync(It.IsAny<string>()))
       .Returns(Task.FromResult(true));

      _providerAuthController.ControllerContext = new ControllerContext
      {
        HttpContext =
        new DefaultHttpContext { User = _providerUser }
      };

      // Act.
      IActionResult response =
        await _providerAuthController.Post(grant_type, refresh_token);

      // Assert.
      using (new AssertionScope())
      {
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
    }

    [Fact]
    public async Task NoProvider_ArgumentNullException()
    {
      // Arrange.
      string grant_type = "refresh_token";
      string refresh_token = "TestToken";
      string expectedErrorMessage =
        "Value cannot be null. (Parameter 'Test Provider not found')";

      RefreshTokenValidationResponse tokenValidationResponse =
        new(ValidationType.Valid);

      _mockAuthService.Setup(x => x.ValidateRefreshKeyAsync(refresh_token))
       .Returns(Task.FromResult(tokenValidationResponse));
      _mockAuthService.Setup(x => x.GenerateTokensAsync())
       .Throws(new ArgumentNullException("Test Provider not found"));

      _providerAuthController.ControllerContext = new ControllerContext();
      _providerAuthController.ControllerContext.HttpContext =
        new DefaultHttpContext { User = _providerUser };

      // Act.
      Func<Task> act = async () => await _providerAuthController.RefreshToken(
          new RefreshTokenRequest
          {
            GrantType = grant_type,
            RefreshToken = refresh_token
          });

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>()
        .WithMessage(expectedErrorMessage);
    }
  }
}