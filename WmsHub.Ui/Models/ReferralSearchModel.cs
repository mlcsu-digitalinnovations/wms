using WmsHub.Business.Enums;

namespace WmsHub.Ui.Models
{
  public class ReferralSearchModel
  {
    public string NhsNumber { get; set; }
    public string Ubrn { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Postcode { get; set; }
    public string TelephoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public string EmailAddress { get; set; }
    public bool? IsVulnerable { get; set; }
    public string[] Statuses { get; set; }
    public int? Limit { get; set; }
    /// <summary>
    /// true
    /// </summary>
    public SearchFilter DelayedReferralsFilter { get; set; } 
      = SearchFilter.Include;

    public bool HasUserSearchCriteria
    {
      get
      {
        return !string.IsNullOrWhiteSpace(NhsNumber)
          || !string.IsNullOrWhiteSpace(Ubrn)
          || !string.IsNullOrWhiteSpace(FamilyName)
          || !string.IsNullOrWhiteSpace(GivenName)
          || !string.IsNullOrWhiteSpace(Postcode)
          || !string.IsNullOrWhiteSpace(TelephoneNumber)
          || !string.IsNullOrWhiteSpace(MobileNumber)
          || !string.IsNullOrWhiteSpace(EmailAddress);
      }
    }    
  }
}