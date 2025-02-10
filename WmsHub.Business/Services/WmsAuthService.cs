#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Business.Services;

public class WmsAuthService : ServiceBase<Entities.Referral>,
  IWmsAuthService
{
  private readonly IMapper _mapper;
  private readonly AuthOptions _options;
  private readonly INotificationService _notificationService;
  private readonly ILogger _logger;

  public WmsAuthService(DatabaseContext context,
    IOptions<AuthOptions> options,
    IMapper mapper,
    INotificationService notificationService,
    ILogger logger)
    : base(context)
  {
    _options = options.Value;
    _mapper = mapper;
    _notificationService = notificationService;
    _logger = logger;
  }

  public virtual async Task<AccessTokenResponse> GenerateTokensAsync()
  {
    Entities.ProviderAuth? providerAuth = await _context
      .ProviderAuth
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(t => t.Id == User.GetUserId());

    Claim? nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)
      ?? throw new ArgumentNullException(
       $"Provider not found in NameIdentifier with Id {User.GetUserId()}.");

    if (providerAuth == null)
    {
      throw new ArgumentNullException(
        $"Provider Auth not found with Id {User.GetUserId()}.");
    }

    string token = NotifyTokenHandler.GenerateToken(
      nameIdentifier.Value,
      providerAuth.Id.ToString(),
      _options.TokenExpiry);

    //Only generate new refresh token if previous within 7 days of
    // becoming out of date
    string refreshTokenValue = string.Empty;
    Entities.RefreshToken? currentToken = await _context.RefreshTokens
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId == User.GetUserId())
      .Where(r => r.Expires > DateTimeOffset.Now.AddDays(+7))
      .FirstOrDefaultAsync();

    if (currentToken == null)
    {
      RefreshToken refreshToken =
        NotifyTokenHandler.GenerateRefreshToken(
          providerAuth.Id.ToString(),
          _options.RefreshExpireDays);
      refreshToken.IsActive = true;
      refreshTokenValue = refreshToken.Token;

      Entities.RefreshToken entityRefreshToken =
        _mapper.Map<RefreshToken, Entities.RefreshToken>(refreshToken);
      entityRefreshToken.ProviderId = providerAuth.Id;

      UpdateModified(entityRefreshToken);
      _context.RefreshTokens
        .Where(r => r.ProviderId == User.GetUserId())
        .ToList()
        .ForEach(t => t.IsActive = false);
      _context.RefreshTokens.Add(entityRefreshToken);
    }
    else
    {
      refreshTokenValue = currentToken.Token;
    }

    AccessTokenResponse response = new()
    {
      AccessToken = token,
      RefreshToken = refreshTokenValue,
      TokenType = "bearer",
      Expires = _options.Expires,
      ValidationStatus = ValidationType.Valid
    };
    return response;
  }

  public virtual async Task<Provider> GetProviderAsync()
  {
    Entities.Provider? entity = await _context
      .Providers
      .Include(p => p.ProviderAuth)
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(t => t.Id == User.GetUserId());

    Provider model = _mapper.Map<Entities.Provider, Provider>(entity!);

    return model;
  }

  public virtual async Task<ProviderAuthNewKeyResponse> SendNewKeyAsync()
  {
    Provider model = await GetProviderAsync() 
      ?? throw new ProviderNotFoundException(User.GetUserId());

    if (model.ProviderAuth == null)
    {
      throw new ProviderAuthCredentialsNotFoundException(model.Id);
    }

    Random random = new();
    model.ProviderAuth.SmsKey = Generators.GenerateKey(random);
    await UpdateProviderAuthKeyAsync(model);

    ProviderAuthNewKeyResponse response = new(model);

    if (!model.ProviderAuth.KeyViaSms && !model.ProviderAuth.KeyViaEmail)
    {
      throw new ProviderAuthNoValidContactException(model.Id);
    }

    if (model.ProviderAuth.KeyViaSms)
    {
      await SendKeyUsingSmsAsync(response);
    }

    if (model.ProviderAuth.KeyViaEmail)
    {
      await SendKeyUsingEmailAsync(response);
    }

    return response;
  }

  public virtual async Task UpdateProviderAuthKeyAsync(
    Provider provider)
  {
    if (provider == null)
    {
      throw new ArgumentNullException(nameof(provider));
    }

    Entities.ProviderAuth? providerAuth = await _context
      .ProviderAuth
      .Where(p => p.IsActive)
      .Where(t => t.Id == User.GetUserId())
      .SingleOrDefaultAsync() 
      ?? throw new ProviderNotFoundException(User.GetUserId());

    if (!string.IsNullOrWhiteSpace(provider.ProviderAuth.SmsKey))
    {
      providerAuth.SmsKey = provider.ProviderAuth.SmsKey;
      providerAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(15);
    }

    if (_context.Entry(providerAuth).State == EntityState.Modified)
    {
      UpdateModified(providerAuth);
      await _context.SaveChangesAsync();
    }
  }

  public virtual async Task<RefreshTokenValidationResponse>
    ValidateRefreshKeyAsync(string refresh_token)
  {
    Entities.ProviderAuth? providerAuth = await _context
      .ProviderAuth
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(t => t.Id == User.GetUserId());

    if (providerAuth == null)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InvalidRequest,
        $"ProviderAuth not found with Id {User.GetUserId()}.");
    }

    if (providerAuth.Provider == null || !providerAuth.Provider.IsActive)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InactiveProvider,
        $"The provider associated with the refresh token {refresh_token} " +
        "is currently disabled and unable to obtain access tokens.");
    }

    Entities.RefreshToken? refreshToken = await _context
      .RefreshTokens
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId == User.GetUserId())
      .OrderByDescending(t => t.ModifiedAt)
      .FirstOrDefaultAsync();

    if (refreshToken == null)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InvalidRequest,
        $"No active Refresh Tokens found is Id {User.GetUserId()}.");
    }

    RefreshTokenValidationResponse response = new(ValidationType.NotSet);

    if (refreshToken!.Token != refresh_token)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InvalidRequest,
        $"Refresh Token {refresh_token} for provided id {User.GetUserId()} " +
        "is not up to date.");
    }

    if (refreshToken.Expires < DateTimeOffset.Now)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InvalidRequest,
        $"Refresh token {refresh_token} has expired for " +
        $"provider Id {User.GetUserId()}.");
    }

    if (refreshToken.Revoked != null &&
      refreshToken.Revoked.Value < DateTimeOffset.Now)
    {
      return new RefreshTokenValidationResponse(
        ValidationType.InvalidRequest,
        $"Refresh token {refresh_token} has been revoked for provider Id" +
        $" {User.GetUserId()}.");
    }

    return new RefreshTokenValidationResponse(ValidationType.Valid);
  }

  public virtual async Task<bool> SaveTokenAsync(string accessToken)
  {
    Entities.ProviderAuth? providerAuth = await _context.ProviderAuth
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(p => p.Id == User.GetUserId());

    if (providerAuth == null)
    {
      return false;
    }

    providerAuth!.AccessToken = accessToken;
    providerAuth.SmsKey = string.Empty;
    providerAuth.SmsKeyExpiry = null;
    UpdateModified(providerAuth);

    return await _context.SaveChangesAsync() > 0;
  }

  public virtual async Task<KeyValidationResponse> ValidateKeyAsync(
    string key)
  {
    Regex regex = new(REGEX_NUMERIC_STRING);

    if (string.IsNullOrWhiteSpace(key))
    {
      return new KeyValidationResponse(ValidationType.KeyNotSet, "No Key provided.");
    }

    if (key.Length != 8)
    {
      return new KeyValidationResponse(ValidationType.KeyIncorrectLength, "Key is not valid.");
    }

    if (!regex.IsMatch(key))
    {
      return new KeyValidationResponse(ValidationType.KeyIsNotNumeric, "Key is not valid.");
    }

    if (await IsApiKeyMismatch(key))
    {
      return new KeyValidationResponse(ValidationType.KeyApiKeyMismatch,
        "Key is not valid. You will need a new key.");
    }

    if (await IsApiKeyOutOfDate())
    {
      return new KeyValidationResponse(ValidationType.KeyOutOfDate,
        "Key is out of date. You will need a new key.");
    }

    return new KeyValidationResponse(ValidationType.ValidKey);
  }

  private async Task<bool> IsApiKeyMismatch(string key)
  {
    Provider model = await GetProviderAsync() 
      ?? throw new ProviderNotFoundException($"Provider not found with {User.GetUserId()}.");

    if (model.ProviderAuth.SmsKey.Equals(key))
    {
      return false;
    }

    await RemoveProvidersSmsKey();

    return true;
  }

  private async Task<bool> IsApiKeyOutOfDate()
  {
    Provider model = await GetProviderAsync() 
      ?? throw new ProviderNotFoundException(User.GetUserId());

    if (model.ProviderAuth.SmsKeyExpiry > DateTimeOffset.Now)
    {
      return false;
    }

    await RemoveProvidersSmsKey();

    return true;

  }

  private async Task RemoveProvidersSmsKey()
  {
    Entities.ProviderAuth? providerAuth = await _context.ProviderAuth
      .Where(p => p.IsActive)
      .SingleOrDefaultAsync(p => p.Id == User.GetUserId())
      ?? throw new ProviderNotFoundException(User.GetUserId());

    providerAuth.SmsKey = string.Empty;
    providerAuth.SmsKeyExpiry = null;
    UpdateModified(providerAuth);
    await _context.SaveChangesAsync();
  }

  private async Task SendKeyUsingSmsAsync(ProviderAuthNewKeyResponse response)
  {
    if (response.Provider.ProviderAuth.MobileNumber.IsUkMobile())
    {
      MessageQueue message = new(
        ApiKeyType.None,
        response.Provider.Id.ToString(),
        null,
        null,
        null,
        new[] { NotificationPersonalisations.CODE },
        response.Provider.ProviderAuth.MobileNumber,
        new() { { NotificationPersonalisations.CODE, response.Provider.ProviderAuth.SmsKey } },
        Guid.Parse(_options.SmsTemplateId),
        MessageType.SMS,
        null);

      try
      {
        HttpResponseMessage notificationResponse =
          await _notificationService.SendMessageAsync(message);

        response.MessageTypesSent.Add(MessageType.SMS);
      }
      catch (Exception ex)
      {
        response.Errors.Add("An error occurred while sending the key via SMS.");
        _logger.Error(ex.Message);
      } 
    }
    else
    {
      _logger.Error("ProviderAuth for Provider {ProviderId} has non-UK mobile number.",
        response.Provider.Id);
      response.Errors.Add("Mobile number is not a valid UK number.");
    }
  }

  private async Task SendKeyUsingEmailAsync(ProviderAuthNewKeyResponse response)
  {
    if (RegexUtilities.IsValidEmail(response.Provider.ProviderAuth.EmailContact))
    {
      MessageQueue message = new(
        ApiKeyType.None,
        response.Provider.Id.ToString(),
        response.Provider.ProviderAuth.EmailContact,
        _options.EmailReplyToId,
        null,
        new[] { NotificationPersonalisations.CODE },
        null,
        new() { { NotificationPersonalisations.CODE, response.Provider.ProviderAuth.SmsKey} },
        Guid.Parse(_options.EmailTemplateId),
        MessageType.Email,
        null);

      try
      {
        HttpResponseMessage notificationResponse =
          await _notificationService.SendMessageAsync(message);

        response.MessageTypesSent.Add(MessageType.Email);
      }
      catch (Exception ex)
      {
        response.Errors.Add("An error occurred while sending the key via Email.");
        _logger.Error(ex.Message);
      }
    }
    else
    {
      _logger.Error("ProviderAuth for Provider {ProviderId} has an invalid email address.",
        response.Provider.Id);
      response.Errors.Add("Email address is not valid.");
    }
  }
}
