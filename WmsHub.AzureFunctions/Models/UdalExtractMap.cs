using CsvHelper.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.AzureFunctions.Models;

public class UdalExtractMap : ClassMap<UdalExtract>
{
  public UdalExtractMap()
  {
    // NHS Number is first to make is easier to encrypt with PIPE
    Map(m => m.NhsNumber).Index(0).Name("NhsNumber");
    Map(m => m.Age).Index(1).Name("Age");
    Map(m => m.CalculatedBmiAtRegistration).Index(2).Name("CalculatedBmiAtRegistration");
    Map(m => m.Coaching0007).Index(3).Name("Coaching00-07");
    Map(m => m.Coaching0814).Index(4).Name("Coaching08-14");
    Map(m => m.Coaching1521).Index(5).Name("Coaching15-21");
    Map(m => m.Coaching2228).Index(6).Name("Coaching22-28");
    Map(m => m.Coaching2935).Index(7).Name("Coaching29-35");
    Map(m => m.Coaching3642).Index(8).Name("Coaching36-42");
    Map(m => m.Coaching4349).Index(9).Name("Coaching43-49");
    Map(m => m.Coaching5056).Index(10).Name("Coaching50-56");
    Map(m => m.Coaching5763).Index(11).Name("Coaching57-63");
    Map(m => m.Coaching6470).Index(12).Name("Coaching64-70");
    Map(m => m.Coaching7177).Index(13).Name("Coaching71-77");
    Map(m => m.Coaching7884).Index(14).Name("Coaching78-84");
    Map(m => m.ConsentForFutureContactForEvaluation).Index(15).Name("ConsentForFutureContactForEvaluation");
    Map(m => m.DateCompletedProgramme).Index(16).Name("DateCompletedProgramme");
    Map(m => m.DateOfBmiAtRegistration).Index(17).Name("DateOfBmiAtRegistration");
    Map(m => m.DateOfProviderContactedServiceUser).Index(18).Name("DateOfProviderContactedServiceUser");
    Map(m => m.DateOfProviderSelection).Index(19).Name("DateOfProviderSelection");
    Map(m => m.DateOfReferral).Index(20).Name("DateOfReferral");
    Map(m => m.DatePlacedOnWaitingListForElectiveCare).Index(21).Name("DatePlacedOnWaitingListForElectiveCare");
    Map(m => m.DateStartedProgramme).Index(22).Name("DateStartedProgramme");
    Map(m => m.DateToDelayUntil).Index(23).Name("DateToDelayUntil");
    Map(m => m.DeprivationQuintile).Index(24).Name("DeprivationQuintile");
    Map(m => m.DocumentVersion).Index(25).Name("DocumentVersion");
    Map(m => m.Ethnicity).Index(26).Name("Ethnicity");
    Map(m => m.EthnicityGroup).Index(27).Name("EthnicityGroup");
    Map(m => m.EthnicitySubGroup).Index(28).Name("EthnicitySubGroup");
    Map(m => m.GpRecordedWeight).Index(29).Name("GpRecordedWeight");
    Map(m => m.GpSourceSystem).Index(30).Name("GpSourceSystem");
    Map(m => m.HasALearningDisability).Index(31).Name("HasALearningDisability");
    Map(m => m.HasAPhysicalDisability).Index(32).Name("HasAPhysicalDisability");
    Map(m => m.HasDiabetesType1).Index(33).Name("HasDiabetesType1");
    Map(m => m.HasDiabetesType2).Index(34).Name("HasDiabetesType2");
    Map(m => m.HasHypertension).Index(35).Name("HasHypertension");
    Map(m => m.HasRegisteredSeriousMentalIllness).Index(36).Name("HasRegisteredSeriousMentalIllness");
    Map(m => m.HeightCm).Index(37).Name("HeightCm");
    Map(m => m.IsVulnerable).Index(38).Name("IsVulnerable");
    Map(m => m.MethodOfContact).Index(39).Name("MethodOfContact");
    Map(m => m.NumberOfContacts).Index(40).Name("NumberOfContacts");
    Map(m => m.OpcsCodesForElectiveCare).Index(41).Name("OPCSCodesForElectiveCare");
    Map(m => m.ProviderEngagement0007).Index(42).Name("ProviderEngagement00-07");
    Map(m => m.ProviderEngagement0814).Index(43).Name("ProviderEngagement08-14");
    Map(m => m.ProviderEngagement1521).Index(44).Name("ProviderEngagement15-21");
    Map(m => m.ProviderEngagement2228).Index(45).Name("ProviderEngagement22-28");
    Map(m => m.ProviderEngagement2935).Index(46).Name("ProviderEngagement29-35");
    Map(m => m.ProviderEngagement3642).Index(47).Name("ProviderEngagement36-42");
    Map(m => m.ProviderEngagement4349).Index(48).Name("ProviderEngagement43-49");
    Map(m => m.ProviderEngagement5056).Index(49).Name("ProviderEngagement50-56");
    Map(m => m.ProviderEngagement5763).Index(50).Name("ProviderEngagement57-63");
    Map(m => m.ProviderEngagement6470).Index(51).Name("ProviderEngagement64-70");
    Map(m => m.ProviderEngagement7177).Index(52).Name("ProviderEngagement71-77");
    Map(m => m.ProviderEngagement7884).Index(53).Name("ProviderEngagement78-84");
    Map(m => m.ProviderName).Index(54).Name("ProviderName");
    Map(m => m.ProviderUbrn).Index(55).Name("ProviderUbrn");
    Map(m => m.ReferralSource).Index(56).Name("ReferralSource");
    Map(m => m.ReferringGpPracticeNumber).Index(57).Name("ReferringGpPracticeNumber");
    Map(m => m.ReferringOrganisationOdsCode).Index(58).Name("ReferringOrganisationOdsCode");
    Map(m => m.Sex).Index(59).Name("Sex");
    Map(m => m.StaffRole).Index(60).Name("StaffRole");
    Map(m => m.Status).Index(61).Name("Status");
    Map(m => m.TriagedCompletionLevel).Index(62).Name("TriagedCompletionLevel");
    Map(m => m.VulnerableDescription).Index(63).Name("VulnerableDescription");
    Map(m => m.WeightMeasurement0007).Index(64).Name("WeightMeasurement00-07");
    Map(m => m.WeightMeasurement0814).Index(65).Name("WeightMeasurement08-14");
    Map(m => m.WeightMeasurement1521).Index(66).Name("WeightMeasurement15-21");
    Map(m => m.WeightMeasurement2228).Index(67).Name("WeightMeasurement22-28");
    Map(m => m.WeightMeasurement2935).Index(68).Name("WeightMeasurement29-35");
    Map(m => m.WeightMeasurement3642).Index(69).Name("WeightMeasurement36-42");
    Map(m => m.WeightMeasurement4349).Index(70).Name("WeightMeasurement43-49");
    Map(m => m.WeightMeasurement5056).Index(71).Name("WeightMeasurement50-56");
    Map(m => m.WeightMeasurement5763).Index(72).Name("WeightMeasurement57-63");
    Map(m => m.WeightMeasurement6470).Index(73).Name("WeightMeasurement64-70");
    Map(m => m.WeightMeasurement7177).Index(74).Name("WeightMeasurement71-77");
    Map(m => m.WeightMeasurement7884).Index(75).Name("WeightMeasurement78-84");
    Map(m => m.WeightMeasurement8500).Index(76).Name("WeightMeasurement85+");
  }
}
