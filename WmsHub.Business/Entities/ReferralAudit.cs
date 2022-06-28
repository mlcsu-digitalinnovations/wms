using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities
{
  // Additional config in OnModelCreating
  [Table("ReferralsAudit")]
  public class ReferralAudit : ReferralBase, IAudit, IReferral, IReferralAudit
  {
    public int AuditId { get; set; }
    public string AuditAction { get; set; }
    public int AuditDuration { get; set; }
    public string AuditErrorMessage { get; set; }
    public int AuditResult { get; set; }
    public bool AuditSuccess { get; set; }
    public Guid CriId { get; set; }

    public virtual Referral Referral { get; set; }
    public virtual UserStore User { get; set; }
  }
}
