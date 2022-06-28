using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities
{
  public class EthnicityOverrideBase : BaseEntity
  {
    public Guid EthnicityId { get; set; }
    public ReferralSource ReferralSource { get; set; }
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
  }
}
