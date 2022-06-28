using System.Collections.Generic;

namespace WmsHub.Business.Entities
{
  public class UserStore : UserStoreBase
  {
    public virtual List<ReferralAudit> ReferralAudits { get; set; }
  }
}
