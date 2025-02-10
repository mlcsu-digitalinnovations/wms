using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.BusinessIntelligence.Api.AuthPolicies;

public class ReferralCountsAuthPolicy : AuthPolicyBase, IAuthPolicy
{
  private const string ROOT_NAME = "ReferralCounts";

  public const string POLICYNAME = $"{ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{ROOT_NAME}ApiKey";

  public override string Name => POLICYNAME;

  public override string Owner => $"{ROOT_NAME}.Controller";

  public override string Role => ROOT_NAME;

  public override string Sid => "61c07606-f555-4f45-8893-0f5d4f879732";

  public ReferralCountsAuthPolicy(IConfiguration config) : base(config)
  { }
}