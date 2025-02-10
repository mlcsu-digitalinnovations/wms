using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.MSGraph;

/// <summary>
/// https://learn.microsoft.com/en-us/graph/api/user-post-users?view=graph-rest-1.0&tabs=csharp
/// </summary>
public class CreateUser: IValidatableObject
{
  [JsonProperty("accountEnabled")]
  public bool AccountEnabled { get; internal set; }
  [JsonIgnore]
  public string Action { get; set; }
  [JsonProperty("displayName")]
  public string DisplayName { get; internal set; }
  [JsonProperty("givenName")]
  public string GivenName { get; internal set; }
  [JsonProperty("identities")]
  public CreateIdentities[] Identities { get; internal set; }
  [JsonProperty("mail")]
  public string Mail { get; internal set; }
  [JsonProperty("mailNickname")]
  public string MailNickname { get; internal set; }
  [JsonIgnore]
  public string[] OdsCodes { get; internal set; }
  [JsonProperty("extension_7f1f413c66724a3eabceb6cb6d43e063_ODS")]
  public string OdsCode { get; internal set; }
  [JsonProperty("extension_7f1f413c66724a3eabceb6cb6d43e063_OrgName")]
  public string OrgName { get; internal set; }
  [JsonProperty("passwordProfile")]
  public CreateUserPassword PasswordProfile { get; set; }
  [JsonProperty("surname")]
  public string Surname { get; set; }
  [JsonProperty("userPrincipalName")]
  public string UserPrincipalName { get; internal set; }

  /// <summary>
  /// Example https://graph.microsoft.com/v1.0/users
  /// <list type="bullet">
  /// <item><description>{0} string Endpoint</description></item>
  /// <item><description>{1} string ApiVersion</description></item>
  /// </list>
  /// </summary>
  [JsonIgnore]
  public const string ENDPOINT = "{0}/{1}/users";

  public IEnumerable<ValidationResult> Validate(
  ValidationContext validationContext)
  {
    List<(string propertyName, string value, string[] expected)> 
      propertiesToValidate = new()
    {
      (nameof(Action), Action, new [] { 
        Constants.Actions.CREATE, 
        Constants.Actions.DELETE }),
      (nameof(Mail), Mail, null),
      (nameof(GivenName), GivenName, null),
      (nameof(OdsCode), OdsCode, OdsCodes),
      (nameof(Surname), Surname, null)
    };

    foreach ((string propertyName, string value, string[] expected) property
      in propertiesToValidate)
    {
      foreach (InvalidValidationResult validationResult
        in Validators.ValidateStringIsNotNullOrWhiteSpace(
          property.propertyName,
          property.value,
          property.expected))
      {
        yield return validationResult;
      }
    }
  }
}

