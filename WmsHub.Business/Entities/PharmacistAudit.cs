using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Entities
{
  public class PharmacistAudit : PharmacistBase, IAudit, IPharmacist
  {
    public string AuditAction { get; set; }
    public int AuditDuration { get; set; }
    public string AuditErrorMessage { get; set; }
    public int AuditId { get; set; }
    public int AuditResult { get; set; }
    public bool AuditSuccess { get; set; }
  }
}