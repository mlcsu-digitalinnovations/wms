using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Tests.Services;
using Xunit;
using Provider = WmsHub.Business.Entities.Provider;
using RefreshToken = WmsHub.Business.Models.AuthService.RefreshToken;

namespace WmsHub.Business.Services.Tests;

[Collection("Service collection")]
public class WmsAuthServiceTests : ServiceTestsBase, IDisposable
{
  private const string _testSecret = "abcdef%6789£lkjHHYABCDEFGHIJKLMN";
  private const string _issuer = "http://mytestsite.com";
  private const string _audience = "http://gov.uk";
  private readonly string _validId = Guid.NewGuid().ToString();
  private const string _validSmsApiKey = "abcdef123456";
  private readonly DatabaseContext _context;
  public WmsAuthService _wmsAuthService;

  protected readonly Mock<NotificationService> _mockNotificationService;

  private readonly Mock<ILogger> _mockLogger = new();

  protected readonly AuthOptions _authOptions = new()
  {
    EmailReplyToId = Guid.NewGuid().ToString(),
    EmailTemplateId = Guid.NewGuid().ToString(),
    SmsTemplateId = Guid.NewGuid().ToString(),
    SmsSenderId = Guid.NewGuid().ToString(),
    SmsApiKey = _validSmsApiKey,
    NotifyLink = "TestLink",
    TokenExpiry = 5
  };

  protected readonly TextOptions _textOptions = new()
  {
    Audience = _audience,
    Issuer = _issuer,
    TokenSecret = _testSecret,
  };

  public virtual void Dispose()
  {
    _context.Providers.RemoveRange(_context.Providers);
    _context.ProviderAuth.RemoveRange(_context.ProviderAuth);
    _context.SaveChanges();
    _mockNotificationService.Reset();
    GC.SuppressFinalize(this);
  }

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
    Provider found = _context.Providers.SingleOrDefault(t => t.Id == provider.Id);
    if (found != null)
    {
      return found.Id;
    }

    _context.Providers.Add(provider);
    _context.SaveChanges();
    return provider.Id;
  }

  private Provider AddFakeProviderInactiveUsingId(Guid id)
  {
    Provider provider = ServiceFixture.CreateProviderWithAuth(id);
    bool found = _context.Providers.Contains(provider);
    provider.IsActive = false;
    if (found)
    {
      _context.SaveChanges();
      return provider;
    }

    _context.Providers.Add(provider);
    _context.SaveChanges();
    return provider;
  }

  private async Task RemoveFakeProviderUsingId(Guid id)
  {
    Provider entity = await _context.Providers.SingleOrDefaultAsync(t => t.Id == id);

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

    _mockNotificationService = new(
      _context,
      _mockLogger.Object,
      new HttpClient(),
      Options.Create(new NotificationOptions()));

    _wmsAuthService = new WmsAuthService(
      _context,
      Options.Create(_authOptions),
      _serviceFixture.Mapper,
      _mockNotificationService.Object,
      _mockLogger.Object)
    {
      User = GetClaimsPrincipal()
    };
    List<string> validIds = new List<string> { _validId };

    NotifyTokenHandler.Configure(Options.Create(_textOptions), null, null);
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

      _textOptions.ValidUsers =
        new EditableList<string> { _provider_id.ToString() };

      NotifyTokenHandler.Configure(Options.Create(_textOptions), null, null);
    }

    [Fact]
    public async Task ValidToken_Test()
    {
      // Arrange.
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(_provider_id.ToString())
      };
      int expectedExpiry = 300;
      string expectedType = "bearer";

      // Act.
      AccessTokenResponse result = await _wmsAuthService.GenerateTokensAsync();
      
      // Assert.
      result.Expires.Should().Be(expectedExpiry);
      result.TokenType.Should().Be(expectedType);
      result.ValidationStatus.Should().Be(ValidationType.Valid);
      (await NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)).Should().BeTrue();
    }

    [Fact]
    public async Task DoNotReplaceRefreshToken()
    {
      // Arrange.
      int expectedExpiry = 300;
      string expectedType = "bearer";
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(_provider_id.ToString())
      };

      // Act.
      AccessTokenResponse result = await _wmsAuthService.GenerateTokensAsync();

      // Assert.
      result.RefreshToken.Should().NotBeNullOrWhiteSpace();
      result.Expires.Should().Be(expectedExpiry);
      result.TokenType.Should().Be(expectedType);
      result.ValidationStatus.Should().Be(ValidationType.Valid);
      (await NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)).Should().BeTrue();
    }

    [Fact]
    public async Task ReplaceRefreshTokenUnder7Days()
    {
      // Arrange.
      int expectedExpiry = 300;
      string expectedType = "bearer";
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(_provider_id.ToString())
      };

      Provider provider = await _context
        .Providers
        .Include(t => t.RefreshTokens)
        .SingleAsync(t => t.Id == _provider.Id);

      if (provider.RefreshTokens.Count != 0)
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

      // Act.
      AccessTokenResponse result = await _wmsAuthService.GenerateTokensAsync();

      // Assert.
      result.RefreshToken.Should().NotBe(refreshToken.Token);
      result.Expires.Should().Be(expectedExpiry);
      result.TokenType.Should().Be(expectedType);
      result.ValidationStatus.Should().Be(ValidationType.Valid);
      (await NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)).Should().BeTrue();
    }

    [Fact]
    public async Task ReplaceRefreshTokenUnderDeactivated()
    {
      // Arrange.
      int expectedExpiry = 300;
      string expectedType = "bearer";
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(_provider_id.ToString())
      };

      Provider provider = await _context
        .Providers
        .Include(t => t.RefreshTokens)
        .SingleAsync(t => t.Id == _provider.Id);

      if (provider.RefreshTokens.Count != 0)
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

      // Act.
      AccessTokenResponse result = await _wmsAuthService.GenerateTokensAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.RefreshToken.Should().NotBe(refreshToken.Token);
        result.Expires.Should().Be(expectedExpiry);
        result.TokenType.Should().Be(expectedType);
        result.ValidationStatus.Should().Be(ValidationType.Valid);
        (await NotifyTokenHandler.ValidateCurrentToken(result.AccessToken)).Should().BeTrue();
      }
    }

    [Fact]
    public async Task ProviderNotFound_Exception()
    {
      // Arrange.
      string expected =
        "Value cannot be null. (Parameter 'Provider Auth not found with Id";
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetInvalidClaimsPrincipal()
      };

      // Act.
      try
      {
        AccessTokenResponse result =
          await _wmsAuthService.GenerateTokensAsync();
      }
      catch (ArgumentNullException ane)
      {
        // Assert.
        ane.Message.Should().StartWith(expected);
      }
    }
  }

  public class GetProviderAsyncTests : WmsAuthServiceTests
  {
    public GetProviderAsyncTests(ServiceFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ValidProviderReturned()
    {
      // Arrange.
      Guid expected = Guid.NewGuid();
      AddFakeProviderUsingId(expected);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(expected.ToString())
      };
      // Act.
      Models.Provider model = await _wmsAuthService.GetProviderAsync();

      // Assert.
      model.Id.Should().Be(expected);
    }

    [Fact]
    public async Task ProviderNotFoundReturnNull()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      string userId = Guid.NewGuid().ToString();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context, 
        Options.Create(_authOptions),
        _serviceFixture.Mapper, 
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(userId)
      };
      // Act.
      Models.Provider model = await _wmsAuthService.GetProviderAsync();

      // Assert.
      model.Should().BeNull();
    }
  }

  public class SendNewKeyAsyncTests : WmsAuthServiceTests
  {
    public SendNewKeyAsyncTests(ServiceFixture fixture) : base(fixture)
    { }

    public override void Dispose()
    {
      _mockNotificationService.Reset();
      base.Dispose();
    }

    [Fact]
    public async Task ValidProviderAuth_ViaSms_ReturnsSuccessfulResponseNoErrors()
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: MOBILE_E164);
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService
        .Verify(x => x.SendMessageAsync(It.IsAny<Models.MessageQueue>()),Times.Once);
      response.KeySentSuccessfully.Should().BeTrue();
      response.MessageTypesSent.Should().OnlyContain(m => m == MessageType.SMS);
      response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidProviderAuth_ViaEmail_ReturnsSuccessfulResponseNoErrors()
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: false,
        isKeyViaEmail: true,
        emailContact: "test@nhs.net");
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService
        .Verify(x => x.SendMessageAsync(It.IsAny<Models.MessageQueue>()), Times.Once);
      response.KeySentSuccessfully.Should().BeTrue();
      response.MessageTypesSent.Should().OnlyContain(m => m == MessageType.Email);
      response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidProviderAuth_ViaEmailAndSms_ReturnsSuccessfulResponseNoErrors()
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: MOBILE_E164,
        isKeyViaEmail: true,
        emailContact: "test@nhs.net");
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService
        .Verify(x => x.SendMessageAsync(It.IsAny<Models.MessageQueue>()), Times.Exactly(2));
      response.KeySentSuccessfully.Should().BeTrue();
      response.MessageTypesSent.Should().Contain(MessageType.Email).And.Contain(MessageType.SMS);
      response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ProviderModelIsNull_Throws_ProviderNotFoundException()
    {
      // Arrange.
      string userId = Guid.NewGuid().ToString();
      string expected = $"Unable to find a provider with an id of {userId}.";

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(userId)
      };

      try
      {
        // Act.
        ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType<ProviderNotFoundException>()
          .Subject.Message.Should().Be(expected);
      }
    }

    [Fact]
    public async Task 
      ProviderAuthKeyViaSmsAndKeyViaEmailFalse_Throws_ProviderAuthNoValidContactException()
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: false,
        isKeyViaEmail: false);
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      string expected = $"Provider {provider.Id} has no valid contact details.";

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      Func<Task<ProviderAuthNewKeyResponse>> method = () => _wmsAuthService.SendNewKeyAsync();

      // Assert.
      await method.Should().ThrowAsync<ProviderAuthNoValidContactException>()
        .WithMessage(expected);
    }

    [Fact]
    public async Task ProviderAuthNull_Throws_ProviderAuthNotFoundException()
    {
      // Arrange.
      Guid providerId = AddFakeProviderNoAuthUsingId();
      string expected = $"Unable to find a provider auth with an id of {providerId}.";
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      try
      {
        // Act.
        ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType<ProviderAuthCredentialsNotFoundException>()
          .Subject.Message.Should().Be(expected);
      }
    }

    [Theory]
    [InlineData(MOBILE_INVALID_LONG)]
    [InlineData(MOBILE_INVALID_SHORT)]
    [InlineData("01234567890")]
    [InlineData("+337123456789")]
    public async Task NonUkMobileNumber_SmsOnly_ReturnsUnsuccessfulResponseWithError(
      string mobileNumber)
    {
      // Arrange.
      string expectedError = "Mobile number is not a valid UK number.";

      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: mobileNumber);
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      response.KeySentSuccessfully.Should().BeFalse();
      response.Errors.Should().ContainSingle(expectedError);
      _mockLogger.Verify(l => l.Error(
        "ProviderAuth for Provider {ProviderId} has non-UK mobile number.", provider.Id),
        Times.Once);
    }

    [Fact]
    public async Task InvalidEmail_EmailOnly_ReturnsUnsuccessfulResponseWithError()
    {
      // Arrange.
      string expectedError = "Email address is not valid.";

      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: false,
        isKeyViaEmail: true,
        emailContact: "invalidemail@abcdef");
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      response.KeySentSuccessfully.Should().BeFalse();
      response.Errors.Should().ContainSingle(expectedError);
      _mockLogger.Verify(l => l.Error(
        "ProviderAuth for Provider {ProviderId} has an invalid email address.", provider.Id),
        Times.Once);
    }

    [Theory]
    [InlineData("invalidemail@abcdef", MOBILE_E164, "Email address is not valid")]
    [InlineData("test@nhs.net", MOBILE_INVALID_SHORT, "Mobile number is not a valid UK number.")]
    public async Task OneValidContact_EmailAndSms_ReturnsSuccessfulWithError(
      string email,
      string mobile,
      string expectedError)
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: mobile,
        isKeyViaEmail: true,
        emailContact: email);
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _mockNotificationService.Setup(t =>
        t.SendMessageAsync(It.IsAny<Models.MessageQueue>()))
      .Verifiable();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      response.KeySentSuccessfully.Should().BeTrue();
      response.MessageTypesSent.Should().ContainSingle();
      response.Errors.Should().ContainSingle(expectedError);
      _mockLogger.Verify(l => l.Error(It.IsAny<string>(), provider.Id), Times.Once);
    }

    [Fact]
    public async Task NotificationError_Email_ReturnsUnsuccessfulWithError()
    {
      // Arrange.
      string expectedError = "An error occurred while sending the key via Email.";

      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: false,
        isKeyViaEmail: true,
        emailContact: "test@nhs.net");
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _mockNotificationService.Setup(t =>
        t.SendMessageAsync(It.IsAny<Models.MessageQueue>()))
        .ThrowsAsync(new NotificationProxyException())
        .Verifiable();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService.Verify();
      response.KeySentSuccessfully.Should().BeFalse();
      response.Errors.Should().ContainSingle(expectedError);
    }

    [Fact]
    public async Task NotificationError_Sms_ReturnsUnsuccessfulWithError()
    {
      // Arrange.
      string expectedError = "An error occurred while sending the key via SMS.";

      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: MOBILE_E164);
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _mockNotificationService.Setup(t =>
        t.SendMessageAsync(It.IsAny<Models.MessageQueue>()))
        .ThrowsAsync(new NotificationProxyException())
        .Verifiable();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService.Verify();
      response.KeySentSuccessfully.Should().BeFalse();
      response.Errors.Should().ContainSingle(expectedError);
    }

    [Theory]
    [InlineData(MessageType.Email, MessageType.SMS)]
    [InlineData(MessageType.SMS, MessageType.Email)]
    public async Task OneNotificationError_EmailAndSms_ReturnSuccessfulWithError(
      MessageType failure,
      MessageType success)
    {
      // Arrange.
      ProviderAuth providerAuth = RandomEntityCreator.CreateRandomProviderAuth(
        isKeyViaSms: true,
        mobileNumber: "+447777123456",
        isKeyViaEmail: true,
        emailContact: "test@nhs.net");
      Provider provider = RandomEntityCreator.CreateRandomProvider(providerAuth: providerAuth);
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();

      _mockNotificationService.Setup(t =>
        t.SendMessageAsync(It.Is<Models.MessageQueue>(m => m.Type == success)))
        .Verifiable();

      _mockNotificationService.Setup(t =>
        t.SendMessageAsync(It.Is<Models.MessageQueue>(m => m.Type == failure)))
        .ThrowsAsync(new NotificationProxyException())
        .Verifiable();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider.Id.ToString())
      };

      // Act.
      ProviderAuthNewKeyResponse response = await _wmsAuthService.SendNewKeyAsync();

      // Assert.
      _mockNotificationService.Verify();
      response.KeySentSuccessfully.Should().BeTrue();
      response.MessageTypesSent.Should().Contain(success);
      response.Errors.Should()
        .ContainSingle($"An error occurred while sending the key via {failure}");
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
      // Arrange.
      string expected = "Value cannot be null. (Parameter 'provider')";

      // Act.
      try
      {
        await _wmsAuthService.UpdateProviderAuthKeyAsync(null!);

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
    public async Task ProviderAuthIsNull_Throw_ProviderNotFoundException()
    {
      // Arrange.
      Guid providerId = AddFakeProviderNoAuthUsingId();

      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      Models.Provider model = new Models.Provider
      {
        Id = Guid.NewGuid()
      };
      string expectedMessage = $"Unable to find a provider with an id of {providerId}.";

      // Act.
      try
      {
        await _wmsAuthService.UpdateProviderAuthKeyAsync(model);
      }
      catch (ProviderNotFoundException pex)
      {
        pex.Message.Should().Be(expectedMessage);
      }
    }

    [Fact]
    public async Task NoProviderKeyInModel_NoChangesMadeToAuth()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider_id.ToString())
      };
      Models.Provider model = new Models.Provider
      {
        Id = provider_id,
        ProviderAuth = new Models.ProviderAuth()
      };

      // Act.
      await _wmsAuthService.UpdateProviderAuthKeyAsync(model);

      // Assert.
      Entities.ProviderAuth auth = await _context
        .ProviderAuth
        .SingleAsync(t => t.Id == provider_id);
      using (new AssertionScope())
      {
        auth.SmsKeyExpiry.Should().NotBeNull();
        auth.SmsKey.Should().Be("12345678");
      }
    }

    [Fact]
    public async Task Valid_RetunsModel()
    {
      // Arrange.
      string expected = "12345678";
      DateTimeOffset expectedExpiry = DateTimeOffset.Now.AddMinutes(15);
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider_id.ToString())
      };
      Models.Provider model = new Models.Provider
      {
        Id = provider_id,
        ProviderAuth = new Models.ProviderAuth
        {
          SmsKey = expected
        }
      };

      // Act.
      await _wmsAuthService.UpdateProviderAuthKeyAsync(model);

      // Assert.
      Entities.ProviderAuth auth = await _context
        .ProviderAuth
        .SingleAsync(t => t.Id == provider_id);
      using (new AssertionScope())
      {
        auth.SmsKeyExpiry!.Value.Date.Should().Be(expectedExpiry.Date);
        auth.SmsKey.Should().Be(expected);
      }
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
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(token.Token);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(0);
        response.ValidationStatus.Should().Be(ValidationType.Valid);
      }

      //Clean-up.
      await RemoveFakeProviderUsingId(provider_id);
    }

    [Fact]
    public async Task ProviderAuthNotFound_Return_InvalidClient()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      string expectedMessage =
        $"ProviderAuth not found with Id {providerId}.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(It.IsAny<string>());

      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
      }
    }

    [Fact]
    public async Task ProviderAuthNotFound_Return_InvalidRequest()
    {
      // Arrange.
      Guid providerId = AddFakeProviderNoAuthUsingId();
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      string expectedMessage =
        $"ProviderAuth not found with Id {providerId}.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(It.IsAny<string>());

      // Assert.
      response.Errors.Should().HaveCount(1);
      response.Errors.ForEach(t => t.Should().Be(expectedMessage));
      response.ValidationStatus.Should()
       .Be(ValidationType.InvalidRequest);
      await RemoveFakeProviderUsingId(providerId);
    }

    [Fact]
    public async Task ProviderNoRefreshTokens_Return_InvalidRequest()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      AddFakeProviderUsingId(providerId);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      string expectedMessage =
        $"No active Refresh Tokens found is Id {providerId}.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(It.IsAny<string>());
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should()
         .Be(ValidationType.InvalidRequest);
      }

      //Clean-up.
      await RemoveFakeProviderUsingId(providerId);
    }

    [Fact]
    public async Task ProviderInactive_Return_Forbidden()
    {
      // Arrange.
      Guid providerId = Guid.NewGuid();
      Provider provider = AddFakeProviderInactiveUsingId(providerId);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      string refreshToken = It.IsAny<string>();

      string expectedMessage = $"The provider associated with the refresh" +
        $" token {refreshToken} " +
        $"is currently disabled and unable to obtain access tokens.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(refreshToken);

      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should()
         .Be(ValidationType.InactiveProvider);
      }

      // Clean-up.
      await RemoveFakeProviderUsingId(providerId);

    }

    [Fact]
    public async Task RefreshTokenMismatch_Return_InvalidRequest()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider_id.ToString())
      };

      string expectedMessage =
        $"Refresh Token BrokenToken for provided id {provider_id} " +
        $"is not up to date.";

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

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync("BrokenToken");

      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
      }

      //Clean-up.
      await RemoveFakeProviderUsingId(provider_id);
    }

    [Fact]
    public async Task RefreshTokenExpired_Return_InvalidRequest()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      string expectedMessage =
        $"Refresh token {token.Token} has expired for provider " +
        $"Id {provider_id}.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(token.Token);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
      }

      //Clean-up.
      await RemoveFakeProviderUsingId(provider_id);
    }

    [Fact]
    public async Task RefreshTokenRevoked_Return_InvalidRequest()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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

      string expectedMessage = $"Refresh token {token.Token} has been " +
        $"revoked for provider Id {provider_id}.";

      // Act.
      RefreshTokenValidationResponse response =
        await _wmsAuthService.ValidateRefreshKeyAsync(token.Token);

      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(expectedMessage));
        response.ValidationStatus.Should().Be(ValidationType.InvalidRequest);
      }

      //Clean-up.
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
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider_id.ToString())
      };
      // Act.
      bool result = await _wmsAuthService.SaveTokenAsync("new_token");
      // Assert.
      result.Should().BeTrue();
    }

    [Fact()]
    public async Task ProviderNotFound_Return_False()
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(Guid.NewGuid().ToString())
      };
      // Act.
      bool result = await _wmsAuthService.SaveTokenAsync("new_token");
      // Assert.
      result.Should().BeFalse();
    }

    [Fact()]
    public async Task ProviderAuthNotFound_Return_False()
    {
      // Arrange.
      Guid provider_id = AddFakeProviderNoAuthUsingId();
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipalWithId(provider_id.ToString())
      };
      // Act.
      bool result = await _wmsAuthService.SaveTokenAsync("new_token");
      // Assert.
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
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      // Act.
      KeyValidationResponse
        response = await _wmsAuthService.ValidateKeyAsync(keyToValidate);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(0);
        response.ValidationStatus.Should().Be(ValidationType.ValidKey);
      }
    }

    [Theory]
    [InlineData("No Key provided.", ValidationType.KeyNotSet, "")]
    [InlineData("Key is not valid.", ValidationType.KeyIncorrectLength,
      "0123")]
    [InlineData("Key is not valid.", ValidationType.KeyIsNotNumeric,
      "ABCD1234")]
    public async Task InValidTests_ReturnsErrors(string error,
      ValidationType validationType,
      string keyToValidate)
    {
      // Arrange.
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      // Act.
      KeyValidationResponse
        response = await _wmsAuthService.ValidateKeyAsync(keyToValidate);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should().Be(validationType);
      }
    }

    [Fact]
    public async Task KeyMismatch_response_errorKeyApiKeyMismatch()
    {
      // Arrange.
      string error = "Key is not valid. You will need a new key.";
      string wrongKey = "00000000";
      string keyToValidate = "12345678";
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      // Act.
      KeyValidationResponse
        response = await _wmsAuthService.ValidateKeyAsync(wrongKey);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should()
         .Be(ValidationType.KeyApiKeyMismatch);
      }  
    }

    [Fact]
    public async Task KeyMismatch_response_errorProviderNotFoundException()
    {
      // Arrange.
      string userid = Guid.NewGuid().ToString();
      string error = $"Provider not found with {userid}.";
      string wrongKey = "00000000";
      string keyToValidate = "12345678";
      Guid provider_id = Guid.NewGuid();

      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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

      // Act.
      try
      {
        KeyValidationResponse
          response = await _wmsAuthService.ValidateKeyAsync(wrongKey);
      }
      catch (ProviderNotFoundException pex)
      {
        // Assert.
        pex.Message.Should().Be(error);
      }
    }

    [Fact]
    public async Task KeyOutOfDate_response_error_KeyOutOfDate()
    {
      // Arrange.
      string error = "Key is out of date. You will need a new key.";
      string keyToValidate = "12345678";
      Guid provider_id = Guid.NewGuid();
      AddFakeProviderUsingId(provider_id);
      _wmsAuthService = new WmsAuthService(
        _context,
        Options.Create(_authOptions),
        _serviceFixture.Mapper,
        _mockNotificationService.Object,
        _mockLogger.Object)
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
      // Act.
      KeyValidationResponse
        response = await _wmsAuthService.ValidateKeyAsync(keyToValidate);
      // Assert.
      using (new AssertionScope())
      {
        response.Errors.Should().HaveCount(1);
        response.Errors.ForEach(t => t.Should().Be(error));
        response.ValidationStatus.Should()
         .Be(ValidationType.KeyOutOfDate);
      }
    }
  }
}
