using CsvHelper.Configuration;
using WmsHub.Common.Api.Models;

namespace WmsHub.ReferralsService.Mappings
{
  public class ReferralPostMap : ClassMap<ReferralPost>
  {
    public ReferralPostMap()
    {
      Map(m => m.Address1).Name("Address1");
      Map(m => m.Address2).Name("Address2");
      Map(m => m.Address3).Name("Address3");
      Map(m => m.CalculatedBmiAtRegistration)
        .Name("CalculatedBmiAtRegistration");
      Map(m => m.DateOfBirth).Name("DateOfBirth");
      Map(m => m.DateOfBmiAtRegistration).Name("DateOfBMIAtRegistration");
      Map(m => m.DateOfReferral).Name("DateOfReferral");
      Map(m => m.Email).Name("Email");
      Map(m => m.Ethnicity).Name("Ethnicity");
      Map(m => m.FamilyName).Name("FamilyName");
      Map(m => m.GivenName).Name("GivenName");
      Map(m => m.HasALearningDisability).Name("HasALearningDisability");
      Map(m => m.HasAPhysicalDisability).Name("HasAPhysicalDisability");
      Map(m => m.HasDiabetesType1).Name("HasDiabetesType1");
      Map(m => m.HasDiabetesType2).Name("HasDiabetesType2");
      Map(m => m.HasHypertension).Name("HasHypertension");
      Map(m => m.HasRegisteredSeriousMentalIllness)
        .Name("HasRegisteredSeriousMentalIllness");
      Map(m => m.HeightCm).Name("HeightCm");
      Map(m => m.IsVulnerable).Name("IsVulnerable");
      Map(m => m.Mobile).Name("Mobile");
      Map(m => m.NhsNumber).Name("NHSNumber");
      Map(m => m.Postcode).Name("Postcode");
      Map(m => m.ReferralAttachmentId).Name("ReferralAttachmentId");
      Map(m => m.ReferringGpPracticeName).Name("ReferringGpPracticeName");
      Map(m => m.ReferringGpPracticeNumber).Name("ReferringGpPracticeNumber");
      Map(m => m.Sex).Name("Sex");
      Map(m => m.Telephone).Name("Telephone");
      Map(m => m.Ubrn).Name("UBRN");
      Map(m => m.VulnerableDescription).Name("VulnerableDescription");
      Map(m => m.WeightKg).Name("WeightKg");
    }
  }
}
