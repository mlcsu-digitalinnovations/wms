using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralStatusReason;

public class ReferralStatusReason : BaseModel, IReferralStatusReason
{
  public string Description { get; set; }
  public ReferralStatusReasonGroup Groups { get; set; }
}
