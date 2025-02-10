namespace WmsHub.Business.Entities;

public interface IOrganisation
{
  string OdsCode { get; set; }
  int QuotaRemaining { get; set; }
  int QuotaTotal { get; set; }
}