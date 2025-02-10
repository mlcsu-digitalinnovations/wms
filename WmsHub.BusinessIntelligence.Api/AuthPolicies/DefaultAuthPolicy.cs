using Microsoft.Extensions.Configuration;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.BusinessIntelligence.Api.AuthPolicies;

public class DefaultAuthPolicy : AuthPolicyBase, IAuthPolicy
{
  private const string ROOT_NAME = "Default";

  public const string POLICYNAME = $"{ROOT_NAME}AuthorizationPolicy";

  public override string ApiKeyId => $"ApiKey";

  public override string Name => POLICYNAME;

  public override string Owner => $"{ROOT_NAME}";

  public override string Role => ROOT_NAME;

  public override string Sid => "032dcbfe-377e-43ff-988d-110f3185808a";  

  public DefaultAuthPolicy(IConfiguration config) : base(config)
  { }
}