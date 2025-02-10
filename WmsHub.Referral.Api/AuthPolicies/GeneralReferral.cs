using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public class GeneralReferral : AuthPolicyBase, IAuthPolicy
{
  private const string _ROOT_NAME = "GeneralReferral";

  public const string POLICY_NAME = $"{_ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{_ROOT_NAME}ApiKey";

  public override string Name => POLICY_NAME;

  public override string Owner => $"{_ROOT_NAME}.Ui";

  public override string Role => _ROOT_NAME;

  public override string Sid => "2e9f8e10-6f60-49ee-a76f-301acfd66fd2";

  public GeneralReferral(IConfiguration config) : base(config)
  { }
}