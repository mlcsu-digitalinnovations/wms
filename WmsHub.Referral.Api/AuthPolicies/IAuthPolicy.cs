using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;

namespace WmsHub.Referral.Api.AuthPolicies
{
  public interface IAuthPolicy
  {
    string ApiKeyId { get; }
    string ApiKeyValue { get; }
    string ClaimTypeId { get; }
    public string ClaimValue { get; }
    string Owner { get; }
    string Name { get; }
    string Sid { get; }

    List<Claim> GetClaims();
    AuthorizationPolicy GetAuthorizationPolicy();
    bool HasMatchingApiKey(string key);
  }
}