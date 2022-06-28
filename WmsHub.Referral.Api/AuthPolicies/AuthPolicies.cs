using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace WmsHub.Referral.Api.AuthPolicies
{
  public static class AuthorizationPolicies
  {
    public static List<IAuthPolicy> AuthPolicies { get; private set; } = new ();
    
    public static void AddPolicies(
      AuthorizationOptions options,
      IConfiguration config)
    {
      AuthPolicies.Add(new Msk(config));

      foreach (IAuthPolicy policy in AuthPolicies)
      {
        options.AddPolicy(policy.Name, policy.GetAuthorizationPolicy());
      }
    }
  }
}
