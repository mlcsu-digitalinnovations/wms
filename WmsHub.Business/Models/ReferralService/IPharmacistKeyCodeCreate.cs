namespace WmsHub.Business.Models.ReferralService
{
  public interface IPharmacistKeyCodeCreate
  {
    public string ReferringPharmacyEmail { get; set; }
    public string KeyCode { get; set; }
    public int ExpireMinutes { get; set; }
  }
}