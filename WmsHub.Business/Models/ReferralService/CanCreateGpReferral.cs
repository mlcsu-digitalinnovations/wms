using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ReferralService
{
  public class CanCreateGpReferral
  {
    public Guid ExistingReferralId { get; set; } = Guid.Empty;
    public bool IsUpdatingCancelledReferral => ExistingReferralId != Guid.Empty;
    public ReferralStatus Status { get; set; } = ReferralStatus.New;
    public string StatusReason => string.Join(", ", StatusReasons);
    public List<string> StatusReasons { get; private set; } = new List<string>();
  }

}
