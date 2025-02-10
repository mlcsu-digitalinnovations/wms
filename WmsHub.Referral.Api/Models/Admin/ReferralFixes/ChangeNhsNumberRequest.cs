using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes;

public class ChangeNhsNumberRequest
{
  [NhsNumber]
  public string OriginalNhsNumber { get; set; }
  [NhsNumber]
  public string UpdatedNhsNumber { get; set; }
}