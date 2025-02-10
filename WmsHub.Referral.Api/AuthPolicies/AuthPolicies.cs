using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public static class AuthorizationPolicies
{
  public static List<IAuthPolicy> AuthPolicies { get; private set; } = new();

  public static void AddPolicies(
    AuthorizationOptions options,
    IConfiguration config)
  {
    AuthPolicies.Add(new Admin(config));
    AuthPolicies.Add(new ElectiveCare(config));
    AuthPolicies.Add(new GeneralReferral(config));
    AuthPolicies.Add(new Msk(config));
    AuthPolicies.Add(new Questionnaire(config));

    foreach (IAuthPolicy policy in AuthPolicies)
    {
      options.AddPolicy(policy.Name, policy.GetAuthorizationPolicy());
    }
  }

  public static void AddPolicies(
    AuthorizationOptions options,
    IConfiguration config,
    string policies
    )
  {
    if (string.IsNullOrWhiteSpace(policies))
    {
      AddPolicies(options, config);
    }
    else
    {
      foreach (string policy in policies.Split(','))
      {
        try
        {
          switch (policy)
          {
            case "Admin":
              Admin policyToAdd1 = new(config);
              if (options.GetPolicy(policyToAdd1.Name) == null)
              {
                AuthPolicies.Add(policyToAdd1);
                options.AddPolicy(
                policyToAdd1.Name,
                policyToAdd1.GetAuthorizationPolicy());
              }
              break;
            case "Msk":
              Msk policyToAdd2 = new(config);
              if (options.GetPolicy(policyToAdd2.Name) == null)
              {
                AuthPolicies.Add(policyToAdd2);
                options.AddPolicy(
                policyToAdd2.Name,
                policyToAdd2.GetAuthorizationPolicy());
              }

              break;
          }
        }
        catch (Exception)
        {
          throw new ArgumentException("AddPolicies keeps breaking");
        }
      }
    }
  }
}