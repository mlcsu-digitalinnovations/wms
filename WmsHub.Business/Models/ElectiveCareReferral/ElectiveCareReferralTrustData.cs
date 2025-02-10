using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models.ElectiveCareReferral;

public class ElectiveCareReferralTrustData
{
  private string _opcsCodes;
  private string _mobile;
  private string _nhsNumber;

  [AgeRange(ErrorMessage =
    "The field 'Date of Birth' must equate to an age between 18 and 110.")]
  [DateInFuture(ErrorMessage =
    "The field 'Date of Birth' cannot be in the future.")]
  public DateTimeOffset DateOfBirth { get; set; }

  [MaxSecondsAhead(ErrorMessage =
    "The field 'Date of Trust Reported BMI' cannot be in the future.")]
  public DateTimeOffset? DateOfTrustReportedBmi { get; set; }

  [MaxSecondsAhead(ErrorMessage =
    "The field 'Date Placed On Waiting List' cannot be in the future.")]
  [MaxDaysBehind(Constants.MAX_EC_DAYS_ON_WAITING_LIST,
    ErrorMessage = "The field 'Date Placed On Waiting List' cannot be more " +
      "than 3 years ago.")]
  public DateTimeOffset DatePlacedOnWaitingList { get; set; }

  public DateTimeOffset DateOfReferral => DateTimeOffset.Now.Date;

  public string Ethnicity { get; set; }

  [Required]
  [RegularExpression(Constants.REGEX_FAMILYNAME,
    ErrorMessage = "The field 'Family Name' contains invalid characters.")]
  public string FamilyName { get; set; }

  [Required]
  [RegularExpression(Constants.REGEX_GIVENNAME,
    ErrorMessage = "The field 'Given Name' contains invalid characters.")]
  public string GivenName { get; set; }

  public bool IsValid => !ValidationErrors.Any();

  [Required]
  [UkMobile(13, true, "The field 'Mobile' must be a valid UK mobile number.")]
  public string Mobile 
  { 
    get => _mobile; 
    set => _mobile = value.StartsWith("07")
      ? $"+44{value[1..]}"
      : value;
  }

  [Required]
  [NhsNumber(allowNulls: false,
     ErrorMessage = "The field 'NHS Number' is invalid.")]
  public string NhsNumber
  {
    get => _nhsNumber;
    set
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        _nhsNumber = null;
      }
      else
      {
        _nhsNumber = value.RemoveSpaces();
      }
    }
  }

  [Required]
  public string OpcsCodes
  {
    get => _opcsCodes;
    set
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        _opcsCodes = null;
      }
      else
      {
        _opcsCodes = value;
        OpcsCodesList = _opcsCodes
          .Split(Constants.SPLITCHARS, Constants.SPLIT_TRIM_AND_REMOVE)
          .ToList();
      }
    }
  }

  public List<string> OpcsCodesList { get; private set; }

  [Required]
  public string Postcode { get; set; }

  [Required]
  public int RowNumber { get; set; }

  [Required]
  public ReferralSource ReferralSource => ReferralSource.ElectiveCare;

  public string ServiceUserEthnicity { get; set; }

  public string ServiceUserEthnicityGroup { get; set; }

  public Sex Sex => SexHelper.TryParseSex(SexAtBirth, out Sex sex) ? sex : default;

  [Required]
  [SexAtBirth]
  public string SexAtBirth { get; set; }

  public string SourceEthnicity { get; set; }

  [MaxLength(20, ErrorMessage = 
    "The field 'Spell Identifier' must not exceed 20 characters.")]
  public string SpellIdentifier { get; set; }

  public bool? SurgeryInLessThanEighteenWeeks { get; set; }

  [Required]
  public string TrustOdsCode { get; set; }

  [Range(Constants.MIN_BMI, Constants.MAX_BMI,
    ErrorMessage = "The field 'Trust Reported BMI' must be between " +
      $"27.5 and 90.")]
  public decimal TrustReportedBmi { get; set; }

  public List<string> ValidationErrors { get; set; } = new();

  public int WeeksOnWaitingList => (int)Math.Round(
    (double)((DateOfReferral - DatePlacedOnWaitingList).Days / 7),
    MidpointRounding.ToZero);

  public void Validate()
  {
    ValidationContext context = new(this);

    List<ValidationResult> results = new();

    Validator.TryValidateObject(
      this,
      context,
      results,
      validateAllProperties: true);

    ValidationErrors.AddRange(results.Select(x => x.ErrorMessage).ToList());

    if (Regex.IsMatch(FamilyName, Constants.REGEX_ALL_NON_ALPHA))
    {
      ValidationErrors.Add(
        "The field 'Family Name' does not contain any alpha characters.");
    }

    if (Regex.IsMatch(GivenName, Constants.REGEX_ALL_NON_ALPHA))
    {
      ValidationErrors.Add(
        "The field 'Given Name' does not contain any alpha characters.");
    }

    if (SurgeryInLessThanEighteenWeeks != false)
    {
      ValidationErrors.Add("The field 'Surgery in less than 18 weeks?' must " +
        "be N, No or False to be eligible.");
    }
  }
}
