using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities
{
  // Additional config in OnModelCreating
  [Table("CallsAudit")]
  public class CallAudit : CallBase, ICall, IAudit
  {
    public int AuditId { get; set; }
    public string AuditAction { get; set; }
    public int AuditDuration { get; set; }
    public string AuditErrorMessage { get; set; }
    public int AuditResult { get; set; }
    public bool AuditSuccess { get; set; }
  }
}
