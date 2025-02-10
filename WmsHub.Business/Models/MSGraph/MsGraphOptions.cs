using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.MSGraph;
public class MsGraphOptions: IValidatableObject
{
  public const string SECTION_KEY = "MsGraphSettings";

  public string ApiVersion { get; set; }
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
  public string Endpoint { get; set; }
  public string Scope { get; set; }
  public string TenantId { get; set; }
  public string TokenEndpoint { get; set; }
  public object UserSearchObjects { get; set; }
  public string TokenEndpointUrl => 
    TokenEndpoint.Replace("{tenantId}", TenantId);

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(ApiVersion))
    {
      yield return new ValidationResult(
        $"{nameof(ApiVersion)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(ClientId))
    {
      yield return new ValidationResult(
        $"{nameof(ClientId)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(ClientSecret))
    {
      yield return new ValidationResult(
        $"{nameof(ClientSecret)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(Endpoint))
    {
      yield return new ValidationResult(
        $"{nameof(Endpoint)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(Scope))
    {
      yield return new ValidationResult(
        $"{nameof(Scope)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(TenantId))
    {
      yield return new ValidationResult(
        $"{nameof(TenantId)} cannot be null or empty.");
    }
    if (string.IsNullOrWhiteSpace(TokenEndpoint))
    {
      yield return new ValidationResult(
        $"{nameof(TokenEndpoint)} cannot be null or empty.");
    }
  }
}
