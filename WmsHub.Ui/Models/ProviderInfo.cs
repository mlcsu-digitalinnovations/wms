namespace WmsHub.Ui.Models;

public class ProviderInfo
{
  public string Name { get; set; }
  public bool IsActive { get; set; }
  public bool IsLevel1Active { get; set; }
  public bool IsLevel2Active { get; set; }
  public bool IsLevel3Active { get; set; }
  public int NoOfElectiveCareReferrals { get; set; }
  public int NoOfGeneralReferrals { get; set; } // public
  public int NoOfGpReferrals { get; set; }
  public int NoOfMskReferrals { get; set; }
  public int NoOfPharmacyReferrals { get; set; }
  public int NoOfSelfReferrals { get; set; } // staff
}
