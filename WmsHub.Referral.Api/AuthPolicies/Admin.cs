using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public class Admin : AuthPolicyBase, IAuthPolicy
{
  private const string _ROOT_NAME = "Admin";

  public const string POLICY_NAME = $"{_ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{_ROOT_NAME}ApiKey";

  public override string Name => POLICY_NAME;

  public override string Owner => $"{_ROOT_NAME}.Ui";

  public override string Role => _ROOT_NAME;

  public override string Sid => "00000000-0000-0000-0000-000000000001";

  public Admin(IConfiguration config) : base(config)
  { }
}