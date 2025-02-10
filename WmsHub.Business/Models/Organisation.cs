namespace WmsHub.Business.Models;

public class Organisation
{
  private string _odsCode;

  public string OdsCode { get => _odsCode; set => _odsCode = value.ToUpper(); }
  public int QuotaTotal { get; set; }
  public int QuotaRemaining { get; set; }
}
