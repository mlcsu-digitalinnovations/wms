using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Business.Services
{
  public class ApiKeyService : ServiceBase<Entities.Referral>, IApiKeyService
  {
    private readonly IMapper _mapper;
    private readonly DomainAccess _access;
    public ApiKeyService(DatabaseContext context,
      IMapper mapper,

      IOptions<ProviderOptions> options) : base(context)
    {
      _mapper = mapper;
      _access = options.Value.Access;
    }

    public virtual async Task<ApiKeyStoreResponse> Validate(string key,
      bool validateProviders = false)
    {
      ApiKeyStoreResponse response = await GetApiKeyStoreByKeyAsync(key);
      if (!validateProviders)
      {
        if (response == null)
        {
          response = new ApiKeyStoreResponse();
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} is not valid.");
          return response;
        }

        if (response.HasExpired)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} has expired.");
          return response;
        }

        if (!response.IsValidDomain)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} is not valid for domain {_access}.");
          return response;
        }

        if (response.UserId == Guid.Empty)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} does not have a valid User ID.");
          return response;
        }

        response.SetStatus(ValidationType.Valid, "");
        return response;

      }

      if (response != null)
      {
        if (response.HasExpired)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} has expired.");
          return response;
        }

        if (!response.IsValidDomain)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} is not valid for domain {_access}.");
          return response;
        }

        if (response.UserId == Guid.Empty)
        {
          response.SetStatus(ValidationType.Invalid,
            $"ApiKey {key} does not have a valid User ID.");
          return response;
        }

        response.SetStatus(ValidationType.Valid, "");
        return response;
      }

      Guid userId = await ValidateProviderKeyAsync(key);
      if (userId == Guid.Empty)
      {
        response = new ApiKeyStoreResponse();
        response.SetStatus(ValidationType.Invalid,
          $"ApiKey {key} is not valid.");
        return response;
      }

      response = new ApiKeyStoreResponse();
      response.Sid = userId.ToString();
      response.Domain = (int)_access;
      response.Key = key;
      response.KeyUser = "provider";
      response.SetStatus(ValidationType.Valid, "");
      return response;
    }


    public virtual async
      Task<ApiKeyStoreResponse> GetApiKeyStoreByKeyAsync(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new KeyNotFoundException("Api Key must be supplied");

      Entities.ApiKeyStore keystore =
        await _context.ApiKeyStore.Where(r => r.IsActive)
          .SingleOrDefaultAsync(t => t.Key == key);

      if (keystore == null)
        return null;

      ApiKeyStoreResponse model =
        _mapper.Map<Entities.ApiKeyStore, ApiKeyStoreResponse>(keystore);

      model.IsValidDomain = model.Access.HasFlag(_access);

      return model;
    }

    public virtual async Task<Guid> ValidateProviderKeyAsync(
      string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentNullException("Provider ApiKey must be provided");

      Entities.Provider provider =
        await _context.Providers
          .FirstOrDefaultAsync(t =>
            t.ApiKey == key && t.ApiKeyExpires.Value > DateTime.UtcNow);
      return provider?.Id ?? Guid.Empty;
    }

  }
}
