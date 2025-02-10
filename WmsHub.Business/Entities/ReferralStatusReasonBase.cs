using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class ReferralStatusReasonBase : BaseEntity
{
  public string Description { get; set; }
  public ReferralStatusReasonGroup Groups { get; set; }
}
