using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models.ElectiveCareReferral;

public class ElectiveCareReferralOptions
{
  private const string DEFAULT_OPCS_CODES = "G23, G31, G32, G33, G61, J18, " +
    "O18, Q07, Q08, R25, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, " +
    "T32, T97, T98, V25, V26, V60, V61, V67, V68, W31, W37, W38, W39, W40, " +
    "W41, W42, W46, W47, W48, W58, W85, W93, W94, W95, Y75, Z78, Z84";

  public const string SectionKey = "ElectiveCareReferralOptions";
  
  private string _opcsCodes;
  private List<string> _eligibleOpcsCodes;

  public bool EnableEnglishOnlyPostcodes { get; set; } = true;

  public bool IgnorePostcodeValidation { get; set; } = false;

  public string Issuer { get; set; }

  public string OpcsCodes
  {
    get => _opcsCodes;
    set
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        _opcsCodes = DEFAULT_OPCS_CODES;
      }
      else
      {
        _opcsCodes = value;
      }

      EligibleOpcsCodes = _opcsCodes
        .Split(',', Constants.SPLIT_TRIM_AND_REMOVE)
        .ToList();
    }
  }

  public string PrincipalName { get; set; }

  private List<string> EligibleOpcsCodes
  {
    get
    {
      if (_eligibleOpcsCodes == null || !_eligibleOpcsCodes.Any())
      {
        throw new Exception(
          $"{SectionKey}:{nameof(OpcsCodes)} has not been configured.");
      }
      return _eligibleOpcsCodes;
    }

    set => _eligibleOpcsCodes = value;
  }

  public bool ValidateOpcsCodeList(List<string> opcsCodes)
  {
    if (opcsCodes != null)
    {
      foreach (string opcsCode in opcsCodes)
      {
        string trimmedOpcsCode = opcsCode.Trim();

        if (trimmedOpcsCode.Length >= 3
          && EligibleOpcsCodes.Any(x => x == opcsCode[..3]))
        {
          return true;
        }
      }
    }
    return false;
  }
}
