using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.Referral.Api.AuthPolicies;

public class Questionnaire : AuthPolicyBase, IAuthPolicy
{
  private const string _ROOT_NAME = "Questionnaire";

  public const string POLICY_NAME = $"{_ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"{_ROOT_NAME}ApiKey";

  public override string Name => POLICY_NAME;

  public override string Owner => $"{_ROOT_NAME}.Ui";

  public override string Role => _ROOT_NAME;

  public override string Sid => "30b73ffd-f5b5-4508-8bb7-83ddd860bac9";

  public Questionnaire(IConfiguration config) : base(config)
  { }
}
