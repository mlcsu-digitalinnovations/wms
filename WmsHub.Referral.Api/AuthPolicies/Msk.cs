using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public class Msk : AuthPolicyBase, IAuthPolicy
{
  private const string _ROOT_NAME = "Msk";

  public const string POLICY_NAME = $"{_ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{_ROOT_NAME}ApiKey";

  public override string Name => POLICY_NAME;

  public override string Owner => $"{_ROOT_NAME}.Ui";

  public override string Role => _ROOT_NAME;

  public override string Sid => "7b82e113-a40e-4a8f-87d3-72946b127a32";

  public Msk(IConfiguration config) : base(config)
  { }
}