using System;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results
{
  public class CreateReferralResult : ReferralsResult
  {
    public bool CloseErsReferral { get; set; }
    public Guid ReferralId { get; set; }
  }
}
