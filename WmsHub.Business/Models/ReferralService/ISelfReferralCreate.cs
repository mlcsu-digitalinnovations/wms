using System;

namespace WmsHub.Business.Models.ReferralService
{
  public interface ISelfReferralCreate
  {
    string FamilyName { get; set; }
    string GivenName { get; set; }
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    string Postcode { get; set; }
    string Telephone { get; set; }
    string Mobile { get; set; }
    string Email { get; set; }
    DateTimeOffset DateOfBirth { get; set; }
    string Sex { get; set; }
    string Ethnicity { get; set; }
    public string ServiceUserEthnicity { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
    bool? HasAPhysicalDisability { get; set; }
    bool? HasALearningDisability { get; set; }
    bool? HasHypertension { get; set; }
    bool? HasDiabetesType1 { get; set; }
    bool? HasDiabetesType2 { get; set; }
    decimal HeightCm { get; set; }
    decimal WeightKg { get; set; }
    DateTimeOffset DateOfBmiAtRegistration { get; set; }
    string StaffRole { get; set; }
  }
}
