using System;
using System.Text.Json.Serialization;

namespace WmsHub.Business.Models
{
  public class ServiceUser
  {
    public DateTimeOffset DateOfReferral { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public string Ubrn { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }    
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    
    private string _telephone;
    [JsonIgnore]
    public bool? IsTelephoneValid { get;set; }
    public string Telephone 
    { 
      get
      {
        if (IsTelephoneValid == false)
        {
          return null;
        } 
        else
        {
          return _telephone;
        }
      }
      set
      {
        _telephone = value;
      }
    }
    
    private string _mobile;
    [JsonIgnore]
    public bool? IsMobileValid { get; set; }
    public string Mobile
    {
      get
      {
        if (IsMobileValid == false)
        {
          return null;
        }
        else
        {
          return _mobile;
        }
      }
      set
      {
        _mobile = value;
      }
    }
    public string Email { get; set; }
    public int Age { get; set; }
    public string SexAtBirth { get; set; }
    public bool? IsVulnerable { get; set; }
    public string Ethnicity { get; set; }
    public bool? HasPhysicalDisability { get; set; }
    public bool? HasLearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public decimal Height { get; set; }
    public decimal Bmi { get; set; }
    public DateTimeOffset BmiDate { get; set; }
    public DateTimeOffset ProviderSelectedDate { get; set; }
    public int TriagedLevel { get; set; }
    public string ReferralSource{ get; set; }
  }
}
