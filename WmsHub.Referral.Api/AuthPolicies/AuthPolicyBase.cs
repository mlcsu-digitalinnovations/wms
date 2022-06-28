using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace WmsHub.Referral.Api.AuthPolicies
{
  public abstract class AuthPolicyBase : IAuthPolicy
  {
    public abstract string ApiKeyId { get; }
    public string ApiKeyValue { get; private set; }
    public abstract string ClaimTypeId { get; }
    public string ClaimValue { get; private set; }
    public abstract string Name { get; }
    public abstract string Owner { get; }
    public abstract string Sid { get; }

    private readonly IConfiguration _config;

    public AuthPolicyBase(IConfiguration config)
    {
      if (config is null)
      {
        throw new ArgumentNullException(nameof(config));
      }
      _config = config;
      ApiKeyValue = GetConfigValue<string>(ApiKeyId);
      ClaimValue = GetConfigValue<string>(ClaimTypeId);
    }

    public AuthorizationPolicy GetAuthorizationPolicy()
    {
      return new AuthorizationPolicyBuilder()
        .RequireClaim(ClaimTypeId, ClaimValue)
        .Build();
    }

    public List<Claim> GetClaims()
    {
      List<Claim> claims = new()
      {
        new Claim(ClaimTypes.Sid, Sid),
        new Claim(ClaimTypeId, ClaimValue)
      };
      return claims;
    }

    public bool HasMatchingApiKey(string key)
    {
      return key
        .Equals(ApiKeyValue, StringComparison.InvariantCultureIgnoreCase);
    }

    private T GetConfigValue<T>(string key)
    {
      T value = _config.GetValue<T>(key);
      if (value == null
        || (value is string && string.IsNullOrWhiteSpace(value.ToString())))
      {
        throw new Exception($"Configuration missing for {key}");
      }
      return value;
    }
  }
}
