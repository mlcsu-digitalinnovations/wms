using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.BusinessIntelligence.Api.AuthPolicies;
public static class AuthorizationPolicies
{
  public static List<IAuthPolicy> AuthPolicies { get; private set; } = new();

  public static void AddPolicies(
    AuthorizationOptions options,
    IConfiguration config)
  {
    AuthPolicies.Add(new DefaultAuthPolicy(config));
    AuthPolicies.Add(new RmcAuthPolicy(config));
    AuthPolicies.Add(new ReferralCountsAuthPolicy(config));

    foreach (IAuthPolicy policy in AuthPolicies)
    {
      options.AddPolicy(policy.Name, policy.GetAuthorizationPolicy());
    }
  }
}