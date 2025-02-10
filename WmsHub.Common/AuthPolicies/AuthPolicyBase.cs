using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace WmsHub.Common.AuthPolicies;
public abstract class AuthPolicyBase : IAuthPolicy
{
  public abstract string ApiKeyId { get; }
  public string ApiKeyValue { get; private set; }
  public abstract string Name { get; }
  public abstract string Owner { get; }
  public abstract string Role { get; }
  public abstract string Sid { get; }

  private const string IS_AUTHENTICATED = "isAuthenticated";
  private readonly IConfiguration _config;

  public AuthPolicyBase(IConfiguration config)
  {
    if (config is null)
    {
      throw new ArgumentNullException(nameof(config));
    }

    _config = config;

    ApiKeyValue = GetConfigValue<string>(ApiKeyId);
  }

  public AuthorizationPolicy GetAuthorizationPolicy()
  {
    return new AuthorizationPolicyBuilder()
      .RequireClaim(ClaimTypes.Role, Role)
      .RequireClaim(ClaimTypes.Authentication, IS_AUTHENTICATED)
      .Build();
  }

  public List<Claim> GetClaims(string key)
  {
    List<Claim> claims = new();
    if (HasMatchingApiKey(key))
    {
      claims.Add(new Claim(ClaimTypes.Sid, Sid));
      claims.Add(new Claim(ClaimTypes.Role, Role));
      claims.Add(new Claim(ClaimTypes.Authentication, IS_AUTHENTICATED));
    }

    return claims;
  }

  public bool HasMatchingApiKey(string key) =>
    key.Equals(ApiKeyValue, StringComparison.OrdinalIgnoreCase);

  private T GetConfigValue<T>(string key)
  {
    T value = _config.GetValue<T>(key);
    return value == null
      || (value is string && string.IsNullOrWhiteSpace(value.ToString()))
      ? throw new Exception($"Configuration missing for {key}")
      : value;
  }
}