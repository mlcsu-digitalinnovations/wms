using System.Diagnostics.CodeAnalysis;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results
{
  [ExcludeFromCodeCoverage]
  public class RegistrationListResult : ReferralsResult
  {
    public virtual RegistrationList ReferralUbrnList { get; set; }
  }
}
