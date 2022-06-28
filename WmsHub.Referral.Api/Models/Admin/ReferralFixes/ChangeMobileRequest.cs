using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes
{
  public class ChangeMobileRequest
  {
    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK)]
    public string OriginalMobile { get; set; }
    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK)]
    public string UpdatedMobile { get; set; }
  }
}