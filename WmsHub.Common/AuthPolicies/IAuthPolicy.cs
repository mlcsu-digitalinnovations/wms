using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;

namespace WmsHub.Common.AuthPolicies;

public interface IAuthPolicy
{
  string ApiKeyId { get; }
  string ApiKeyValue { get; }
  string Owner { get; }
  string Name { get; }
  string Sid { get; }

  List<Claim> GetClaims(string key);
  AuthorizationPolicy GetAuthorizationPolicy();
  bool HasMatchingApiKey(string key);
}