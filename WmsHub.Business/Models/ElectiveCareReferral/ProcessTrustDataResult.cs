using System.Collections.Generic;

namespace WmsHub.Business.Models.ElectiveCareReferral;

public class ProcessTrustDataResult
{
  public Dictionary<int, List<string>> Errors { get; set; } = new();
  public bool IsValid { get; set; } = true;
  public int NoOfReferralsCreated { get; set; } = 0;
  public int QuotaTotal { get; set; }
  public int QuotaRemaining { get; set; }
}