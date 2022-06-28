#nullable enable
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Services
{
  public class WmsAuthService : ServiceBase<Entities.Referral>,
    IWmsAuthService
  {
    private readonly IMapper _mapper;
    private readonly AuthOptions _options;
    private readonly INotificationClientService _notificationClient;

    public WmsAuthService(DatabaseContext context,
      IOptions<AuthOptions> options,
      IMapper mapper,
      INotificationClientService notificationClient) : base(context)
    {
      _options = options.Value;
      _mapper = mapper;
      _notificationClient = notificationClient;
    }

    public virtual async Task<AccessTokenResponse> GenerateTokensAsync()
    {
      Entities.Provider? entity = await _context
        .Providers
        .Where(p => p.IsActive)
        .Include(t => t.RefreshTokens.Where(t => t.IsActive))
        .SingleOrDefaultAsync(t => t.Id == User.GetUserId());

      if (entity == null)
      {
        throw new ArgumentNullException(
          $"Provider not found with Id {User.GetUserId()}.");
      }

      string token = NotifyTokenHandler.GenerateToken(entity.Name,
        entity.Id.ToString(), _options.TokenExpiry);

      //Only generate new refresh token if previous within 7 days of
      // becoming out of date
      string rToken = string.Empty;
      Entities.RefreshToken? currentToken =
        entity.RefreshTokens.FirstOrDefault(t => t.IsActive);
      if (currentToken != null &&
          currentToken.Expires > DateTimeOffset.Now.AddDays(+7))
      {
        rToken = currentToken.Token;
      }
      else
      {

        RefreshToken refreshToken =
          NotifyTokenHandler.GenerateRefreshToken(entity.Id.ToString(),
            _options.RefreshExpireDays);
        refreshToken.IsActive = true;
        rToken = refreshToken.Token;

        Entities.RefreshToken entityRefreshToken =
          _mapper.Map<RefreshToken, Entities.RefreshToken>(refreshToken);

        UpdateModified(entityRefreshToken);
        entity.RefreshTokens ??= new List<Entities.RefreshToken>();

        entity.RefreshTokens.ForEach(t => t.IsActive = false);

        entity.RefreshTokens.Add(entityRefreshToken);
      }

      AccessTokenResponse response = new AccessTokenResponse()
      {
        AccessToken = token,
        RefreshToken = rToken,
        TokenType = "bearer",
        Expires = _options.Expires,
        ValidationStatus = ValidationType.Valid
      };

      entity.ProviderAuth!.AccessToken = token;
      entity.ProviderAuth.SmsKey = string.Empty;
      entity.ProviderAuth.SmsKeyExpiry = null;
      UpdateModified(entity);


      await _context.SaveChangesAsync();

      return response;

    }

    public virtual async Task<Provider> GetProviderAsync()
    {
      Entities.Provider? entity = await _context
        .Providers
        .Where(p => p.IsActive)
        .Include(t => t.ProviderAuth)
        .SingleOrDefaultAsync(t => t.Id == User.GetUserId());

      Provider model = _mapper.Map<Entities.Provider, Provider>(entity!);

      return model;
    }

    public virtual async Task<bool> SendNewKeyAsync()
    {
      Provider model = await GetProviderAsync();
      if (model is null)
      {
        throw new ProviderNotFoundException(
        $"Provider not found with id {User.GetUserId()}");
      }

      Random random = new Random();
      if (model.ProviderAuth == null)
      {
        throw new ProviderAuthCredentialsNotFoundException(
          $"ProviderAuth not found for {model.Id}");
      }

      model.ProviderAuth.SmsKey = Generators.GenerateKey(random);

      await UpdateProviderAuthKeyAsync(model);

      if (model.ProviderAuth.KeyViaSms)
      {
        return await _notificationClient.SendKeyUsingSmsAsync(model,
         _options.SmsApiKey, _options.SmsTemplateId,
         _options.SmsSenderId);
      }

      if (model.ProviderAuth.KeyViaEmail)
      {
        return await SendKeyUsingEmailAsync(model);
      }

      return false;

    }

    public virtual async Task<Provider> UpdateProviderAuthKeyAsync(
      Provider model)
    {
      if (model == null)
      {
        throw new ArgumentNullException(nameof(model));
      }

      Entities.Provider? entity = await _context
        .Providers
        .Include(t => t.ProviderAuth)
        .Where(p => p.IsActive)
        .SingleOrDefaultAsync(t => t.Id == model.Id);

      if (entity == null || entity.ProviderAuth == null)
      {
        throw new ProviderNotFoundException(
          $"Provider not found with ID {model.Id}");
      }

      if (!string.IsNullOrWhiteSpace(model.ProviderAuth.SmsKey))
      {
        entity.ProviderAuth.SmsKey = model.ProviderAuth.SmsKey;
        entity.ProviderAuth.SmsKeyExpiry = DateTimeOffset.Now.AddMinutes(15);
      }

      UpdateModified(entity);

      await _context.SaveChangesAsync();

      Provider provider = _mapper.Map<Entities.Provider, Provider>(entity);

      return provider;

    }

    public virtual async Task<RefreshTokenValidationResponse>
      ValidateRefreshKeyAsync(string refresh_token)
    {
      Entities.Provider? entity = await _context
        .Providers
        .Include(t => t.ProviderAuth)
        .Include(t => t.RefreshTokens.Where(r => r.IsActive))
        .Where(p => p.IsActive)
        .SingleOrDefaultAsync(t => t.Id == User.GetUserId() && t.IsActive);

      if (entity == null)
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidClient,
          "Active provider not found");
      }

      if (entity.ProviderAuth == null)
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidRequest,
          "No active credentials found");
      }

      if (entity.RefreshTokens == null || !entity.RefreshTokens.Any())
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidRequest,
          "No active Refresh Tokens found");
      }

      RefreshTokenValidationResponse response =
        new RefreshTokenValidationResponse(
          ValidationType.NotSet);

      Entities.RefreshToken? token = entity.RefreshTokens
       .OrderByDescending(t => t.ModifiedAt)
       .FirstOrDefault();


      if (token!.Token != refresh_token)
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidRequest,
          "Refresh Token provided is not up to date");
      }

      if (token.Expires < DateTimeOffset.Now)
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidRequest,
          "Token has expired");
      }

      if (token.Revoked != null && token.Revoked.Value < DateTimeOffset.Now)
      {
        return new RefreshTokenValidationResponse(
          ValidationType.InvalidRequest,
          "Token has been revoked");
      }

      return new RefreshTokenValidationResponse(ValidationType.Valid);
    }

    public virtual async Task<bool> SaveTokenAsync(string accessToken)
    {
      Entities.Provider? entity = await _context
        .Providers
        .Include(p => p.ProviderAuth)
        .Where(p => p.IsActive)
        .SingleOrDefaultAsync(p => p.Id == User.GetUserId());

      if (entity == null || entity.ProviderAuth == null)
      {
        return false;
      }

      Entities.ProviderAuth auth = entity.ProviderAuth;
      auth.AccessToken = accessToken;
      UpdateModified(auth);

      return await _context.SaveChangesAsync() > 0;
    }

    public virtual async Task<KeyValidationResponse> ValidateKeyAsync(
      string key)
    {
      Regex regex = new Regex(Constants.REGEX_NUMERIC_STRING);

      if (string.IsNullOrWhiteSpace(key))
      {
        return new KeyValidationResponse(ValidationType.KeyNotSet,
          "No Key provided");
      }

      if (key.Length != 8)
      {
        return new KeyValidationResponse(ValidationType.KeyIncorrectLength,
          "Key is not valid");
      }

      if (!regex.IsMatch(key))
      {
        return new KeyValidationResponse(ValidationType.KeyIsNotNumeric,
          "Key is not valid");
      }

      if (await IsApiKeyMismatch(key))
      {
        return new KeyValidationResponse(ValidationType.KeyApiKeyMismatch,
          "Key is not valid. You will need a new key");
      }

      if (await IsApiKeyOutOfDate(key))
      {
        return new KeyValidationResponse(ValidationType.KeyOutOfDate,
          "Key is out of date. You will need a new key");
      }

      return new KeyValidationResponse(ValidationType.ValidKey);

    }

    private async Task<bool> SendKeyUsingEmailAsync(Provider model)
    {
      throw new NotImplementedException();
    }

    private async Task<bool> IsApiKeyMismatch(string key)
    {
      Provider model = await GetProviderAsync();
      if (model is null)
      {
        throw new ProviderNotFoundException(
          $"Provider not found with {User.GetUserId()}");
      }

      if (model.ProviderAuth.SmsKey.Equals(key))
      {
        return false;
      }

      await RemoveProvidersSmsKey();

      return true;

    }

    private async Task<bool> IsApiKeyOutOfDate(string key)
    {
      Provider model = await GetProviderAsync();
      if (model is null)
      {
        throw new ArgumentNullException("Provider not found");
      }

      if (model.ProviderAuth.SmsKeyExpiry > DateTimeOffset.Now)
      {
        return false;
      }

      await RemoveProvidersSmsKey();

      return true;

    }

    private async Task RemoveProvidersSmsKey()
    {
      Entities.Provider? entity =await _context
        .Providers
        .Include(p => p.ProviderAuth)
        .Where(p => p.IsActive)
        .SingleOrDefaultAsync(p => p.Id == User.GetUserId());

      if (entity == null || entity.ProviderAuth == null)
      {
        throw new ProviderNotFoundException(
          $"Provider not found with {User.GetUserId()}");
      }

      entity.ProviderAuth.SmsKey = string.Empty;
      entity.ProviderAuth.SmsKeyExpiry = null;
      UpdateModified(entity);
      await _context.SaveChangesAsync();
    }

  }
}
