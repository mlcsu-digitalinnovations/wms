namespace WmsHub.Business.Models.ElectiveCareReferral;

public class GetQuotaDetailsResult
{
  public string Error { get; set; } = string.Empty;
  public bool IsValid => string.IsNullOrWhiteSpace(Error);
  public string OdsCode { get; set; }
  public int QuotaRemaining { get; set; }
  public int QuotaTotal { get; set; }
}
