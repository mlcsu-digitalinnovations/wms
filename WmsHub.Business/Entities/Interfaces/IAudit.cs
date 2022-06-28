namespace WmsHub.Business.Entities
{
  public interface IAudit
  {
    string AuditAction { get; set; }
    int AuditDuration { get; set; }
    string AuditErrorMessage { get; set; }
    int AuditId { get; set; }
    int AuditResult { get; set; }
    bool AuditSuccess { get; set; }
  }
}