using System;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models
{
  public class PharmacyReferralPostRequest
  {
    private string _email;
    public string ReferringPharmacyOdsCode { get; set; }
    public string ReferringPharmacyEmail { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public string NhsNumber { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    public string Email
    {
      get => _email;
      set
      {
        _email = value.EmailCleaner(Constants.INVALID_EMAIL_TERMS);
        if (!RegexUtilities.IsValidEmail(_email))
        {
          _email = string.Empty;
        }
      }
    }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Sex { get; set; }
    public string Ethnicity { get; set; }
    public string ServiceUserEthnicity { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    public decimal? CalculatedBmiAtRegistration { get; set; }
    public bool? ConsentForGpAndNhsNumberLookup { get; set; }
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  }
}