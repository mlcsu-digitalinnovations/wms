using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities;

[Table("OrganisationsAudit")]
public class OrganisationAudit : OrganisationBase, IAudit, IOrganisation
{  
  public string AuditAction { get; set; }
  public int AuditDuration { get; set; }
  public string AuditErrorMessage { get; set; }
  public int AuditId { get; set; }
  public int AuditResult { get; set; }
  public bool AuditSuccess { get; set; }
}
