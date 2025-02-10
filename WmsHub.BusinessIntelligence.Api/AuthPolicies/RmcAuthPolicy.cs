using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.BusinessIntelligence.Api.AuthPolicies;

public class RmcAuthPolicy : AuthPolicyBase, IAuthPolicy
{
  private const string ROOT_NAME = "Rmc";

  public const string POLICYNAME = $"{ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{ROOT_NAME}ApiKey";

  public override string Name => POLICYNAME;

  public override string Owner => $"{ROOT_NAME}.Controller";

  public override string Role => ROOT_NAME;

  public override string Sid => "4998fdad-a28e-4b8f-9294-a2d2bf7ec4e0";

  public RmcAuthPolicy(IConfiguration config) : base(config)
  { }
}