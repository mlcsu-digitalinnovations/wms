using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Tests.Services;
using Xunit;
using Provider = WmsHub.Business.Entities.Provider;
using RefreshToken = WmsHub.Business.Models.AuthService.RefreshToken;

#nullable enable

namespace WmsHub.Business.Services.Tests
{
  [Collection("Service collection")]
  public class WmsAuthServiceTests : ServiceTestsBase
  {
    private const string _testSecret = "abcdef%6789£lkjHHY";
    private const string _issuer = "http://mytestsite.com";
    private const string _audience = "http://gov.uk";
    private readonly string _validId = Guid.NewGuid().ToString();
    private const string _validSmsApiKey = "abcdef123456";
    private const string _invlaidSmsApiKey = "123456abcdef";
    private readonly string _validTemplateId = Guid.NewGuid().ToString();
    private readonly string _validSenderId = Guid.NewGuid().ToString();
    private readonly DatabaseContext _context;
    public WmsAuthService _classToTest;

    private readonly Mock<IOptions<TextOptions>> _mockSettings =
      new Mock<IOptions<TextOptions>>();

    private readonly Mock<TextOptions> _options = new Mock<TextOptions>();

    private readonly Mock<NotificationClientService> _mockNotificationService =
      new Mock<NotificationClientService>();

    public readonly AuthOptions _authOptions = new AuthOptions
    {
      SmsTemplateId = Guid.NewGuid().ToString(),
      SmsSenderId = Guid.NewGuid().ToString(),
      SmsApiKey = _validSmsApiKey,
      NotifyLink = "TestLink",
      TokenExpiry = 5
    };

    public readonly Mock<IOptions<AuthOptions>> _mockOptions =
      new Mock<IOptions<AuthOptions>>();


    private void AddFakeProviderUsingId(Guid id)
    {
      Entities.Provider provider = ServiceFixture.CreateProviderWithAuth(id);
      bool found = _context.Providers.Contains(provider);
      if (found)
      {
        return;
      }

      _context.Providers.Add(provider);
      _context.SaveChanges();
    }

    private Guid AddFakeProviderNoAuthUsingId()
    {
      Entities.Provider provider =
        ServiceFixture.CreateProviderWithReferrals();
      Provider? found =
        _context.Providers.SingleOrDefault(t => t.Id == provider.Id);
      if (found != null)
      {
        return found.Id;
      }

      _context.Providers.Add(provider);
      _context.SaveChanges();
      return provider.Id;
    }

    private async Task RemoveFakeProviderUsingId(Guid id)
    {
      Provider? entity = await _context
        .Providers
        .SingleOrDefaultAsync(t => t.Id == id);

      if (entity is null)
      {
        return;
      }

      _context.Providers.Remove(entity);
      await _context.SaveChangesAsync();
    }
    public WmsAuthServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);
      _mockOptions.Setup(x => x.Value).Returns(_authOptions);

      _classToTest = new WmsAuthService(_context, _mockOptions.Object,
        _serviceFixture.Mapper, _mockNotificationService.Object)
      {
        User = GetClaimsPrincipal()
      };
      List<string> validIds = new List<string> { _validId };
      _options.Setup(x => x.TokenSecret)
       .Returns(_testSecret);
      _options.Setup(x => x.Issuer).Returns(_issuer);
      _options.Setup(x => x.Audience).Returns(_audience);
      _mockSettings.Setup(x => x.Value)
       .Returns(_options.Object);

      NotifyTokenHandler.Configure(
        Options.Create(_options.Object), null, null);
    }

    public class GenerateTokensAsyncTests : WmsAuthServiceTests
    {
      private readonly Entities.Provider _provider;
      private readonly Guid _provider_id;

      public GenerateTokensAsyncTests(ServiceFixture fixture) : base(fixture)
      {
        _provider_id = Guid.NewGuid();
        //Add provider to use with tests
        _provider = ServiceFixture.CreateProviderWithAuth(_provider_id);
        bool found = _context.Providers.Contains(_provider);
        if (!found)
        {
          _context.Providers.Add(_provider);
          _context.SaveChanges();
        }

        _options.Setup(t => t.ValidUsers).Returns(new EditableList<string>
          {_provider_id.ToString()});

        NotifyTokenHandler.Configure(
          Options.Create(_options.Object), null, null);
      }

      [Fact()]
      public async Task ValidToken_Test()
      {
        //Arrange
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(_provider_id.ToString())
        };
        int expectedExpiry = 300;
        string expectedType = "bearer";

        //Act
        AccessTokenResponse result = await _classToTest.GenerateTokensAsync();
        //Assert
        result.Expires.Should().Be(expectedExpiry);
        result.TokenType.Should().Be(expectedType);
        result.ValidationStatus.Should().Be(ValidationType.Valid);
        NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)
         .Should().BeTrue();
      }

      [Fact]
      public async Task DoNotReplaceRefreshToken()
      {
        //Arrange
        int expectedExpiry = 300;
        string expectedType = "bearer";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(_provider_id.ToString())
        };

        Provider provider = await _context
          .Providers
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == _provider.Id);

        if (provider.RefreshTokens.Any())
        {
          provider.RefreshTokens.ForEach(t => t.IsActive = false);
        }

        RefreshToken refreshToken =
          NotifyTokenHandler.GenerateRefreshToken(provider.ToString(), 10);

        provider.RefreshTokens.Add(new Entities.RefreshToken
        {
          IsActive = true,
          Token = refreshToken.Token,
          Expires = refreshToken.Expires,
          Created = refreshToken.Created,
          CreatedBy = refreshToken.CreatedBy
        });

        await _context.SaveChangesAsync();

        //Act
        AccessTokenResponse result = await _classToTest.GenerateTokensAsync();

        //Assert
        result.RefreshToken.Should().Be(refreshToken.Token);
        result.Expires.Should().Be(expectedExpiry);
        result.TokenType.Should().Be(expectedType);
        result.ValidationStatus.Should().Be(ValidationType.Valid);
        NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)
         .Should().BeTrue();
      }

      [Fact]
      public async Task ReplaceRefreshTokenUnder7Days()
      {
        //Arrange
        int expectedExpiry = 300;
        string expectedType = "bearer";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(_provider_id.ToString())
        };

        Provider provider = await _context
          .Providers
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == _provider.Id);

        if (provider.RefreshTokens.Any())
        {
          provider.RefreshTokens.ForEach(t => t.IsActive = false);
        }

        RefreshToken refreshToken =
          NotifyTokenHandler.GenerateRefreshToken(provider.ToString(), 6);

        provider.RefreshTokens.Add(new Entities.RefreshToken
        {
          IsActive = true,
          Token = refreshToken.Token,
          Expires = refreshToken.Expires,
          Created = refreshToken.Created,
          CreatedBy = refreshToken.CreatedBy
        });

        await _context.SaveChangesAsync();

        //Act
        AccessTokenResponse result = await _classToTest.GenerateTokensAsync();

        //Assert
        result.RefreshToken.Should().NotBe(refreshToken.Token);
        result.Expires.Should().Be(expectedExpiry);
        result.TokenType.Should().Be(expectedType);
        result.ValidationStatus.Should().Be(ValidationType.Valid);
        NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)
         .Should().BeTrue();
      }

      [Fact]
      public async Task ReplaceRefreshTokenUnderDeactivated()
      {
        //Arrange
        int expectedExpiry = 300;
        string expectedType = "bearer";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(_provider_id.ToString())
        };

        Provider provider = await _context
          .Providers
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == _provider.Id);

        if (provider.RefreshTokens.Any())
        {
          provider.RefreshTokens.ForEach(t => t.IsActive = false);
        }

        RefreshToken refreshToken =
          NotifyTokenHandler.GenerateRefreshToken(provider.ToString(), 6);

        provider.RefreshTokens.Add(new Entities.RefreshToken
        {
          IsActive = false,
          Token = refreshToken.Token,
          Expires = refreshToken.Expires,
          Created = refreshToken.Created,
          CreatedBy = refreshToken.CreatedBy
        });

        await _context.SaveChangesAsync();

        //Act
        AccessTokenResponse result = await _classToTest.GenerateTokensAsync();

        //Assert
        result.RefreshToken.Should().NotBe(refreshToken.Token);
        result.Expires.Should().Be(expectedExpiry);
        result.TokenType.Should().Be(expectedType);
        result.ValidationStatus.Should().Be(ValidationType.Valid);
        NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)
         .Should().BeTrue();
      }

      [Fact]
      public async Task ProviderNotFound_Exception()
      {
        //Arrange
        string expected =
          "Value cannot be null. (Parameter 'Provider not found with Id";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetInvalidClaimsPrincipal()
        };
        //Act
        try
        {
          AccessTokenResponse result =
            await _classToTest.GenerateTokensAsync();
          true.Should().BeFalse("ArgumentNullException expected");
        }
        catch (ArgumentNullException ane)
        {
          //Assert
          ane.Message.Should().StartWith(expected);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }


      }
    }

    public class GetProviderAsyncTests : WmsAuthServiceTests
    {
      public GetProviderAsyncTests(ServiceFixture fixture) : base(fixture)
      {
      }

      [Fact()]
      public async Task ValidProviderReturned()
      {
        //Arrange
        Guid expected = Guid.NewGuid();
        AddFakeProviderUsingId(expected);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(expected.ToString())
        };
        //Act
        Models.Provider model = await _classToTest.GetProviderAsync();

        //Assert
        model.Id.Should().Be(expected);
      }

      [Fact]
      public async Task ProviderNotFoundReturnNull()
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(userId)
        };
        //Act
        Models.Provider model = await _classToTest.GetProviderAsync();

        //Assert
        model.Should().BeNull();
      }
    }

    public class SendNewKeyAsyncTests : WmsAuthServiceTests
    {
      public SendNewKeyAsyncTests(ServiceFixture fixture) : base(fixture)
      {

      }

      [Fact()]
      public async Task ValidProviderViaSms()
      {
        //Arrange
        Guid expected = Guid.NewGuid();
        AddFakeProviderUsingId(expected);

        var provider = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == expected);

        provider.ProviderAuth!.KeyViaSms = true;
        provider.ProviderAuth.MobileNumber = "+447777123456";
        await _context.SaveChangesAsync();

        _mockNotificationService.Setup(t =>
            t.SendKeyUsingSmsAsync(It.IsAny<Models.Provider>(),
              It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .Returns(Task.FromResult(true)).Verifiable();
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(expected.ToString())
        };
        //Act
        bool isValid = await _classToTest.SendNewKeyAsync();

        //Assert
        isValid.Should().BeTrue();
      }

      [Fact]
      public async Task ProviderModelIsNull_Throws_ProviderNotFoundException()
      {
        //Arrange
        string userId = Guid.NewGuid().ToString();
        string expected = $"Provider not found with id {userId}";
        Guid provider_id = Guid.NewGuid();

        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(userId)
        };
        //Act
        try
        {
          bool isValid = await _classToTest.SendNewKeyAsync();
          //Assert
          true.Should().BeFalse("Expected ProviderNotFoundException");
        }
        catch (ProviderNotFoundException pex)
        {
          pex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }

      }

      [Fact]
      public async Task ProviderAuthNull_Throws_ProviderAuthNotFoundException()
      {
        //Arrange
        Guid providerId = AddFakeProviderNoAuthUsingId();
        string expected = $"ProviderAuth not found for {providerId}";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(providerId.ToString())
        };
        //Act
        try
        {
          bool isValid = await _classToTest.SendNewKeyAsync();
          //Assert
          true.Should().BeFalse("Expected ProviderNotFoundException");
        }
        catch (ProviderAuthCredentialsNotFoundException pex)
        {
          pex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }
      }
    }

    public class UpdateProviderAuthKeyAsyncTests : WmsAuthServiceTests
    {
      public UpdateProviderAuthKeyAsyncTests(ServiceFixture fixture) : base(
        fixture)
      {

      }

      [Fact()]
      public async Task ProviderModelIsNull_Throw_ProviderNotFoundException()
      {
        //Arrange
        string expected = "Value cannot be null. (Parameter 'model')";
        //act
        try
        {
          Models.Provider provider =
            await _classToTest.UpdateProviderAuthKeyAsync(null!);

          true.Should().BeFalse("ProviderNotFoundException was expected");
        }
        catch (ArgumentNullException pex)
        {
          pex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }
      }

      [Fact()]
      public async Task ProviderEntityIsNull_Throw_ProviderNotFoundException()
      {
        //Arrange
        Models.Provider model = new Models.Provider
        {
          Id = Guid.NewGuid()
        };
        string expected = $"Provider not found with ID {model.Id}";
        //act
        try
        {
          Models.Provider provider =
            await _classToTest.UpdateProviderAuthKeyAsync(model);

          true.Should().BeFalse("ProviderNotFoundException was expected");
        }
        catch (ProviderNotFoundException pex)
        {
          pex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }
      }

      [Fact]
      public async Task NoProviderAuthSmsKey_RetunsModel()
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Models.Provider model = new Models.Provider
        {
          Id = provider_id,
          ProviderAuth = new ProviderAuth()
        };
        //Act
        Models.Provider provider =
          await _classToTest.UpdateProviderAuthKeyAsync(model);
        //Assert
        provider.ProviderAuth.SmsKeyExpiry.Should().BeNull();
        provider.ProviderAuth.SmsKey.Should().BeNullOrWhiteSpace();
      }

      [Fact]
      public async Task Valid_RetunsModel()
      {
        //Arrange
        string expected = "12345678";
        DateTimeOffset expectedExpiry = DateTimeOffset.Now.AddMinutes(15);
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Models.Provider model = new Models.Provider
        {
          Id = provider_id,
          ProviderAuth = new ProviderAuth
          {
            SmsKey = expected
          }
        };

        //Act
        Models.Provider provider =
          await _classToTest.UpdateProviderAuthKeyAsync(model);
        //Assert
        provider.ProviderAuth.SmsKeyExpiry!.Value.Date.Should()
         .Be(expectedExpiry.Date);
        provider.ProviderAuth.SmsKey.Should().Be(expected);
      }


    }

    public class ValidateRefreshKeyAsyncTest : WmsAuthServiceTests
    {
      public ValidateRefreshKeyAsyncTest(ServiceFixture fixture) : base(
        fixture)
      {
      }

      [Fact]
      public async Task ValidTest()
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Entities.Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == provider_id);

        RefreshToken token = NotifyTokenHandler
          .GenerateRefreshToken(provider_id.ToString(), 10);

        entity.RefreshTokens = new List<Entities.RefreshToken>
        {
          new Entities.RefreshToken
          {
            Token = token.Token,
            IsActive = true,
            Expires = token.Expires
          }
        };
        await _context.SaveChangesAsync();
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(token.Token);
        //Assert
        response.Errors.Should().HaveCount(0);
        response.ValidationStatus.Should().Be(ValidationType.Valid);
        await RemoveFakeProviderUsingId(provider_id);
      }

      [Fact]
      public async Task ProviderNotFound_Return_InvalidClient()
      {
        //Arrange
        string expected = "Active provider not found";
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(Guid.NewGuid().ToString())
        };
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(It.IsAny<string>());
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should()
         .Be(ValidationType.InvalidClient);
      }

      [Fact]
      public async Task ProviderAuthNotFound_Return_InvalidRequest()
      {
        //Arrange
        string expected = "No active credentials found";
        Guid providerId = AddFakeProviderNoAuthUsingId();
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(providerId.ToString())
        };
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(It.IsAny<string>());
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should()
         .Be(ValidationType.InvalidRequest);
        await RemoveFakeProviderUsingId(providerId);
      }

      [Fact]
      public async Task ProviderNoRefreshTokens_Return_InvalidRequest()
      {
        //Arrange
        string expected = "No active Refresh Tokens found";
        Guid providerId = Guid.NewGuid();
        AddFakeProviderUsingId(providerId);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(providerId.ToString())
        };
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(It.IsAny<string>());
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should()
         .Be(ValidationType.InvalidRequest);
        await RemoveFakeProviderUsingId(providerId);
      }

      [Fact]
      public async Task RefreshTokenMismatch_Return_InvalidRequest()
      {
        //Arrange
        string expected = "Refresh Token provided is not up to date";
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };

        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == provider_id);

        RefreshToken token = NotifyTokenHandler
          .GenerateRefreshToken(provider_id.ToString(), 10);

        entity.RefreshTokens = new List<Entities.RefreshToken>
        {
          new Entities.RefreshToken
          {
            Token = token.Token,
            IsActive = true,
            Expires = token.Expires
          }
        };
        await _context.SaveChangesAsync();
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync("BrokenToken");
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
        await RemoveFakeProviderUsingId(provider_id);
      }

      [Fact]
      public async Task RefreshTokenExpired_Return_InvalidRequest()
      {
        //Arrange
        string expected = "Token has expired";
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == provider_id);
        
        RefreshToken token = NotifyTokenHandler
          .GenerateRefreshToken(provider_id.ToString(), 10);

        entity.RefreshTokens = new List<Entities.RefreshToken>
        {
          new Entities.RefreshToken
          {
            Token = token.Token,
            IsActive = true,
            Expires = token.Expires.AddDays(-11)
          }
        };
        await _context.SaveChangesAsync();
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(token.Token);
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
        await RemoveFakeProviderUsingId(provider_id);
      }

      [Fact]
      public async Task RefreshTokenRevoked_Return_InvalidRequest()
      {
        //Arrange
        string expected = "Token has been revoked";
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .Include(t => t.RefreshTokens)
          .SingleAsync(t => t.Id == provider_id);

        RefreshToken token = NotifyTokenHandler
          .GenerateRefreshToken(provider_id.ToString(), 10);

        entity.RefreshTokens = new List<Entities.RefreshToken>
        {
          new Entities.RefreshToken
          {
            Token = token.Token,
            IsActive = true,
            Expires = token.Expires,
            Revoked = DateTimeOffset.Now.AddDays(-1)
          }
        };
        await _context.SaveChangesAsync();
        //Act
        RefreshTokenValidationResponse response =
          await _classToTest.ValidateRefreshKeyAsync(token.Token);
        //Assert
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expected));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
        await RemoveFakeProviderUsingId(provider_id);
      }

    }


    public class SaveTokenAsyncTest : WmsAuthServiceTests
    {
      public SaveTokenAsyncTest(ServiceFixture fixture) : base(
        fixture)
      {

      }

      [Fact()]
      public async Task ValidTest_Return_true()
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        //Act
        bool result = await _classToTest.SaveTokenAsync("new_token");
        //Assert
        result.Should().BeTrue();
      }

      [Fact()]
      public async Task ProviderNotFound_Return_False()
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(Guid.NewGuid().ToString())
        };
        //Act
        bool result = await _classToTest.SaveTokenAsync("new_token");
        //Assert
        result.Should().BeFalse();
      }

      [Fact()]
      public async Task ProviderAuthNotFound_Return_False()
      {
        //Arrange
        Guid provider_id = AddFakeProviderNoAuthUsingId();
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        //Act
        bool result = await _classToTest.SaveTokenAsync("new_token");
        //Assert
        result.Should().BeFalse();
      }
    }

    public class ValidateKeyAsyncTest : WmsAuthServiceTests
    {
      public ValidateKeyAsyncTest(ServiceFixture fixture) : base(
        fixture)
      {

      }

      [Theory]
      [InlineData("12345678")]
      [InlineData("01230123")]
      public async Task ValidTests(string keyToValidate)
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == provider_id);

        entity.ProviderAuth!.SmsKey = keyToValidate;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(5);
        await _context.SaveChangesAsync();
        //Act
        KeyValidationResponse
          response = await _classToTest.ValidateKeyAsync(keyToValidate);
        //Assert

        response.Errors.Should().HaveCount(0);
        response.ValidationStatus.Should().Be(ValidationType.ValidKey);

      }

      [Theory]
      [InlineData("No Key provided", ValidationType.KeyNotSet, "")]
      [InlineData("Key is not valid", ValidationType.KeyIncorrectLength,
        "0123")]
      [InlineData("Key is not valid", ValidationType.KeyIsNotNumeric,
        "ABCD1234")]
      public async Task InValidTests_ReturnsErrors(string error,
        ValidationType validationType,
        string keyToValidate)
      {
        //Arrange
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == provider_id);

        entity.ProviderAuth!.SmsKey = keyToValidate;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(5);
        await _context.SaveChangesAsync();
        //Act
        KeyValidationResponse
          response = await _classToTest.ValidateKeyAsync(keyToValidate);
        //Assert

        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should().Be(validationType);

      }

      [Fact]
      public async Task KeyMismatch_response_errorKeyApiKeyMismatch()
      {
        //Arrange
        string error = "Key is not valid. You will need a new key";
        string wrongKey = "00000000";
        string keyToValidate = "12345678";
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == provider_id);

        entity.ProviderAuth!.SmsKey = keyToValidate;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(5);
        await _context.SaveChangesAsync();
        //Act
        KeyValidationResponse
          response = await _classToTest.ValidateKeyAsync(wrongKey);
        //Assert

        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should()
         .Be(ValidationType.KeyApiKeyMismatch);
      }

      [Fact]
      public async Task KeyMismatch_response_errorProviderNotFoundException()
      {
        //Arrange
        string userid = Guid.NewGuid().ToString();
        string error = $"Provider not found with {userid}";
        string wrongKey = "00000000";
        string keyToValidate = "12345678";
        Guid provider_id = Guid.NewGuid();

        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(userid)
        };
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == provider_id);

        entity.ProviderAuth!.SmsKey = keyToValidate;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(5);
        await _context.SaveChangesAsync();
        //Act
        try
        {
          KeyValidationResponse
            response = await _classToTest.ValidateKeyAsync(wrongKey);
          //Assert

          true.Should().BeFalse("Expected ProviderNotFoundException");
        }
        catch (ProviderNotFoundException pex)
        {
          pex.Message.Should().Be(error);
        }
        catch (Exception ex)
        {
          true.Should().BeFalse(ex.Message);
        }
      }

      [Fact]
      public async Task KeyOutOfDate_response_error_KeyOutOfDate()
      {
        //Arrange
        string error = "Key is out of date. You will need a new key";
        string keyToValidate = "12345678";
        Guid provider_id = Guid.NewGuid();
        AddFakeProviderUsingId(provider_id);
        _classToTest = new WmsAuthService(_context, _mockOptions.Object,
          _serviceFixture.Mapper, _mockNotificationService.Object)
        {
          User = GetClaimsPrincipalWithId(provider_id.ToString())
        };
        Provider entity = await _context
          .Providers
          .Include(t => t.ProviderAuth)
          .SingleAsync(t => t.Id == provider_id);

        entity.ProviderAuth!.SmsKey = keyToValidate;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(-5);
        await _context.SaveChangesAsync();
        //Act
        KeyValidationResponse
          response = await _classToTest.ValidateKeyAsync(keyToValidate);
        //Assert

        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should()
         .Be(ValidationType.KeyOutOfDate);
      }
    }
  }
}