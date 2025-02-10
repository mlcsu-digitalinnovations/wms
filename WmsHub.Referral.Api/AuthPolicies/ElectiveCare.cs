using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public class ElectiveCare : AuthPolicyBase, IAuthPolicy
{
  private const string _ROOT_NAME = "ElectiveCare";

  public const string POLICY_NAME = $"{_ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{_ROOT_NAME}ApiKey";

  public override string Name => POLICY_NAME;

  public override string Owner => $"{_ROOT_NAME}.Ui";

  public override string Role => _ROOT_NAME;

  public override string Sid => "e2aedbd0-123c-47c7-8701-ac24388ed3f8";

  public ElectiveCare(IConfiguration config) : base(config)
  { }
}