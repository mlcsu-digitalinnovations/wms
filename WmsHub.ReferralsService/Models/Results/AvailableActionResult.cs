using WmsHub.ReferralsService.Models.BaseClasses;
using static WmsHub.ReferralsService.Enums;

namespace WmsHub.ReferralsService.Models.Results;

public class AvailableActionResult : ReferralsResult
{
  public AvailableActions Actions { get; set; }

  public bool HasAction(ReferralAction referralAction)
  {
    return Actions != null && Actions.Contains(referralAction);
  }
}
