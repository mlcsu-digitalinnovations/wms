using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models;
public class MskOrganisation : IValidatableObject
{
  private string _odsCode;
  [Required]
  public string OdsCode
  {
    get => _odsCode;
    set => _odsCode = value.ToUpperInvariant();
  }
  public bool SendDischargeLetters { get; set; }
  [Required]
  public string SiteName { get; set; }

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(OdsCode))
    {
      yield return new RequiredValidationResult(nameof(OdsCode));
    }
    else if (OdsCode.Length != 5)
    {
      yield return new InvalidValidationResult(nameof(OdsCode), OdsCode);
    }

    if (string.IsNullOrWhiteSpace(SiteName))
    {
      yield return new RequiredValidationResult(nameof(SiteName));
    }
    else if (SiteName.Length > 200)
    {
      yield return new InvalidValidationResult(nameof(SiteName), SiteName);
    }


  }
}
