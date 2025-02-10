namespace WmsHub.Business.Entities;

public class OrganisationBase : BaseEntity, IOrganisation
{
  public string OdsCode { get; set; }
  public int QuotaTotal { get; set; }
  public int QuotaRemaining { get; set; }
}
