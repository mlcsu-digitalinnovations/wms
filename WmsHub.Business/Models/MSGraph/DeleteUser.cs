using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace WmsHub.Business.Models.MSGraph;

public class DeleteUser : ElectiveCareUserBase, IValidatableObject
{
  /// <summary>
  /// Example 
  /// https://graph.microsoft.com/v1.0/users/
  /// 5b0455f7-ce45-4e21-bb04-8a96111e1b3e
  /// <list type="bullet">
  /// <item><description>{0} string Endpoint</description></item>
  /// <item><description>{1} string ApiVersion</description></item>
  /// <item><description>{2} string User ID</description></item>
  /// </list>
  /// </summary>
  [JsonIgnore]
  public const string ENDPOINT = "{0}/{1}/users/{2}";
  [EmailAddress, JsonIgnore]
  public string EmailAddress { get; internal set; }
  [JsonIgnore] 
  public string Issuer { get; internal set; }
  [JsonIgnore]
  public string ODSCode { get; set; }
  [JsonIgnore]
  public string[] OdsCodes { get; internal set; }
  [JsonIgnore]
  public string OdsCodeCompare { get; internal set; }

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (Id == Guid.Empty)
    {
      yield return new ValidationResult($"{nameof(Id)} cannot have an empty " +
        $"Guid.");
    }

    if (Identities == null || !Identities.Any())
    {
      yield return new ValidationResult($"{nameof(Identities)} contains no" +
        $" email addresses because all supplied email addresses are invalid.");
    }
    else
    {
      bool emailFound = Identities
        .Any(t => t.IssuerAssignedId == EmailAddress && t.Issuer == Issuer);

      if (!emailFound)
      {
        yield return new ValidationResult($"{nameof(EmailAddress)} not found " +
          $"in any identities for the Issuer {Issuer}.");
      }
    }

    if (!OdsCodes.Contains(OdsCodeCompare))
    {
      yield return new ValidationResult($"{nameof(OdsCodeCompare)} is not a " +
        $"valid organisation code.");
    }

    if (ODSCode != OdsCodeCompare)
    {
      yield return new ValidationResult($"{nameof(ODSCode)} must be that of " +
        $"supplied ODS Code of {OdsCodeCompare}.");
    }

  }
}

