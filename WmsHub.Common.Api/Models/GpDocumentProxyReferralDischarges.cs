namespace WmsHub.Common.Api.Models;

public class GpDocumentProxyReferralDischarges
{
  public bool AllReferralsProcessedSuccessfully { get; set; }
  public GpDocumentProxyReferralDischarge[] Discharges { get; set; }
}