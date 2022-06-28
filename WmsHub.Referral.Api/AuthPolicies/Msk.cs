using Microsoft.Extensions.Configuration;

namespace WmsHub.Referral.Api.AuthPolicies
{
  public class Msk : AuthPolicyBase, IAuthPolicy
  {    
    public const string POLICY_NAME = "MskAuthorizationPolicy";

    public override string ApiKeyId => "MskApiKey";

    public override string ClaimTypeId => "MskClaimType";

    public override string Name => POLICY_NAME;

    public override string Owner => "Msk.Ui";

    public override string Sid => "7b82e113-a40e-4a8f-87d3-72946b127a32";

    public Msk(IConfiguration config) : base(config)
    { }

  }
}
