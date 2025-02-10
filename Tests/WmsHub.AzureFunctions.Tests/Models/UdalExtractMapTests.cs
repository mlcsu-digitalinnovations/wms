using CsvHelper.Configuration;
using FluentAssertions;
using WmsHub.AzureFunctions.Models;

namespace WmsHub.AzureFunctions.Tests.Models;
public class UdalExtractMapTests
{

  [Fact]
  public void When_Constructed_Should_MapProperties()
  {
    // Arrange.
    UdalExtractMap udalExtractMap = new();

    // Act.
    MemberMap nhsNumberMap = udalExtractMap.MemberMaps[0];
    MemberMap ageMap = udalExtractMap.MemberMaps[1];
    MemberMap calculatedBmiAtRegistrationMap = udalExtractMap.MemberMaps[2];
    MemberMap coaching0007Map = udalExtractMap.MemberMaps[3];
    MemberMap coaching0814Map = udalExtractMap.MemberMaps[4];
    MemberMap coaching1521Map = udalExtractMap.MemberMaps[5];
    MemberMap coaching2228Map = udalExtractMap.MemberMaps[6];
    MemberMap coaching2935Map = udalExtractMap.MemberMaps[7];
    MemberMap coaching3642Map = udalExtractMap.MemberMaps[8];
    MemberMap coaching4349Map = udalExtractMap.MemberMaps[9];
    MemberMap coaching5056Map = udalExtractMap.MemberMaps[10];
    MemberMap coaching5763Map = udalExtractMap.MemberMaps[11];
    MemberMap coaching6470Map = udalExtractMap.MemberMaps[12];
    MemberMap coaching7177Map = udalExtractMap.MemberMaps[13];
    MemberMap coaching7884Map = udalExtractMap.MemberMaps[14];
    MemberMap consentForFutureContactForEvaluationMap = udalExtractMap.MemberMaps[15];
    MemberMap dateCompletedProgrammeMap = udalExtractMap.MemberMaps[16];
    MemberMap dateOfBmiAtRegistrationMap = udalExtractMap.MemberMaps[17];
    MemberMap dateOfProviderContactedServiceUserMap = udalExtractMap.MemberMaps[18];
    MemberMap dateOfProviderSelectionMap = udalExtractMap.MemberMaps[19];
    MemberMap dateOfReferralMap = udalExtractMap.MemberMaps[20];
    MemberMap datePlacedOnWaitingListForElectiveCareMap = udalExtractMap.MemberMaps[21];
    MemberMap dateStartedProgrammeMap = udalExtractMap.MemberMaps[22];
    MemberMap dateToDelayUntilMap = udalExtractMap.MemberMaps[23];
    MemberMap deprivationQuintileMap = udalExtractMap.MemberMaps[24];
    MemberMap documentVersionMap = udalExtractMap.MemberMaps[25];
    MemberMap ethnicityMap = udalExtractMap.MemberMaps[26];
    MemberMap ethnicityGroupMap = udalExtractMap.MemberMaps[27];
    MemberMap ethnicitySubGroupMap = udalExtractMap.MemberMaps[28];
    MemberMap gpRecordedWeightMap = udalExtractMap.MemberMaps[29];
    MemberMap gpSourceSystemMap = udalExtractMap.MemberMaps[30];
    MemberMap hasALearningDisabilityMap = udalExtractMap.MemberMaps[31];
    MemberMap hasAPhysicalDisabilityMap = udalExtractMap.MemberMaps[32];
    MemberMap hasDiabetesType1Map = udalExtractMap.MemberMaps[33];
    MemberMap hasDiabetesType2Map = udalExtractMap.MemberMaps[34];
    MemberMap hasHypertensionMap = udalExtractMap.MemberMaps[35];
    MemberMap hasRegisteredSeriousMentalIllnessMap = udalExtractMap.MemberMaps[36];
    MemberMap heightCmMap = udalExtractMap.MemberMaps[37];
    MemberMap isVulnerableMap = udalExtractMap.MemberMaps[38];
    MemberMap methodOfContactMap = udalExtractMap.MemberMaps[39];
    MemberMap numberOfContactsMap = udalExtractMap.MemberMaps[40];
    MemberMap opcsCodesForElectiveCareMap = udalExtractMap.MemberMaps[41];
    MemberMap providerEngagement0007Map = udalExtractMap.MemberMaps[42];
    MemberMap providerEngagement0814Map = udalExtractMap.MemberMaps[43];
    MemberMap providerEngagement1521Map = udalExtractMap.MemberMaps[44];
    MemberMap providerEngagement2228Map = udalExtractMap.MemberMaps[45];
    MemberMap providerEngagement2935Map = udalExtractMap.MemberMaps[46];
    MemberMap providerEngagement3642Map = udalExtractMap.MemberMaps[47];
    MemberMap providerEngagement4349Map = udalExtractMap.MemberMaps[48];
    MemberMap providerEngagement5056Map = udalExtractMap.MemberMaps[49];
    MemberMap providerEngagement5763Map = udalExtractMap.MemberMaps[50];
    MemberMap providerEngagement6470Map = udalExtractMap.MemberMaps[51];
    MemberMap providerEngagement7177Map = udalExtractMap.MemberMaps[52];
    MemberMap providerEngagement7884Map = udalExtractMap.MemberMaps[53];
    MemberMap providerNameMap = udalExtractMap.MemberMaps[54];
    MemberMap providerUbrnMap = udalExtractMap.MemberMaps[55];
    MemberMap referralSourceMap = udalExtractMap.MemberMaps[56];
    MemberMap referringGpPracticeNumberMap = udalExtractMap.MemberMaps[57];
    MemberMap referringOrganisationOdsCodeMap = udalExtractMap.MemberMaps[58];
    MemberMap sexMap = udalExtractMap.MemberMaps[59];
    MemberMap staffRoleMap = udalExtractMap.MemberMaps[60];
    MemberMap statusMap = udalExtractMap.MemberMaps[61];
    MemberMap triagedCompletionLevelMap = udalExtractMap.MemberMaps[62];
    MemberMap vulnerableDescriptionMap = udalExtractMap.MemberMaps[63];
    MemberMap weightMeasurement0007Map = udalExtractMap.MemberMaps[64];
    MemberMap weightMeasurement0814Map = udalExtractMap.MemberMaps[65];
    MemberMap weightMeasurement1521Map = udalExtractMap.MemberMaps[66];
    MemberMap weightMeasurement2228Map = udalExtractMap.MemberMaps[67];
    MemberMap weightMeasurement2935Map = udalExtractMap.MemberMaps[68];
    MemberMap weightMeasurement3642Map = udalExtractMap.MemberMaps[69];
    MemberMap weightMeasurement4349Map = udalExtractMap.MemberMaps[70];
    MemberMap weightMeasurement5056Map = udalExtractMap.MemberMaps[71];
    MemberMap weightMeasurement5763Map = udalExtractMap.MemberMaps[72];
    MemberMap weightMeasurement6470Map = udalExtractMap.MemberMaps[73];
    MemberMap weightMeasurement7177Map = udalExtractMap.MemberMaps[74];
    MemberMap weightMeasurement7884Map = udalExtractMap.MemberMaps[75];
    MemberMap weightMeasurement8500Map = udalExtractMap.MemberMaps[76];

    // Assert.
    udalExtractMap.MemberMaps.Should().HaveCount(77);

    nhsNumberMap.Data.Names.Should().ContainSingle().Which.Should().Be("NhsNumber");
    ageMap.Data.Names.Should().ContainSingle().Which.Should().Be("Age");
    calculatedBmiAtRegistrationMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("CalculatedBmiAtRegistration");
    coaching0007Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching00-07");
    coaching0814Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching08-14");
    coaching1521Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching15-21");
    coaching2228Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching22-28");
    coaching2935Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching29-35");
    coaching3642Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching36-42");
    coaching4349Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching43-49");
    coaching5056Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching50-56");
    coaching5763Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching57-63");
    coaching6470Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching64-70");
    coaching7177Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching71-77");
    coaching7884Map.Data.Names.Should().ContainSingle().Which.Should().Be("Coaching78-84");
    consentForFutureContactForEvaluationMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("ConsentForFutureContactForEvaluation");
    dateCompletedProgrammeMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DateCompletedProgramme");
    dateOfBmiAtRegistrationMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DateOfBmiAtRegistration");
    dateOfProviderContactedServiceUserMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DateOfProviderContactedServiceUser");
    dateOfProviderSelectionMap.Data.Names.Should()
      .ContainSingle().Which.Should().Be("DateOfProviderSelection");
    dateOfReferralMap.Data.Names.Should().ContainSingle().Which.Should().Be("DateOfReferral");
    datePlacedOnWaitingListForElectiveCareMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DatePlacedOnWaitingListForElectiveCare");
    dateStartedProgrammeMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DateStartedProgramme");
    dateToDelayUntilMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DateToDelayUntil");
    deprivationQuintileMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("DeprivationQuintile");
    documentVersionMap.Data.Names.Should().ContainSingle().Which.Should().Be("DocumentVersion");
    ethnicityMap.Data.Names.Should().ContainSingle().Which.Should().Be("Ethnicity");
    ethnicityGroupMap.Data.Names.Should().ContainSingle().Which.Should().Be("EthnicityGroup");
    ethnicitySubGroupMap.Data.Names.Should().ContainSingle().Which.Should().Be("EthnicitySubGroup");
    gpRecordedWeightMap.Data.Names.Should().ContainSingle().Which.Should().Be("GpRecordedWeight");
    gpSourceSystemMap.Data.Names.Should().ContainSingle().Which.Should().Be("GpSourceSystem");
    hasALearningDisabilityMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("HasALearningDisability");
    hasAPhysicalDisabilityMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("HasAPhysicalDisability");
    hasDiabetesType1Map.Data.Names.Should().ContainSingle().Which.Should().Be("HasDiabetesType1");
    hasDiabetesType2Map.Data.Names.Should().ContainSingle().Which.Should().Be("HasDiabetesType2");
    hasHypertensionMap.Data.Names.Should().ContainSingle().Which.Should().Be("HasHypertension");
    hasRegisteredSeriousMentalIllnessMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("HasRegisteredSeriousMentalIllness");
    heightCmMap.Data.Names.Should().ContainSingle().Which.Should().Be("HeightCm");
    isVulnerableMap.Data.Names.Should().ContainSingle().Which.Should().Be("IsVulnerable");
    methodOfContactMap.Data.Names.Should().ContainSingle().Which.Should().Be("MethodOfContact");
    numberOfContactsMap.Data.Names.Should().ContainSingle().Which.Should().Be("NumberOfContacts");
    opcsCodesForElectiveCareMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("OPCSCodesForElectiveCare");
    providerEngagement0007Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement00-07");
    providerEngagement0814Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement08-14");
    providerEngagement1521Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement15-21");
    providerEngagement2228Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement22-28");
    providerEngagement2935Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement29-35");
    providerEngagement3642Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement36-42");
    providerEngagement4349Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement43-49");
    providerEngagement5056Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement50-56");
    providerEngagement5763Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement57-63");
    providerEngagement6470Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement64-70");
    providerEngagement7177Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement71-77");
    providerEngagement7884Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("ProviderEngagement78-84");
    providerNameMap.Data.Names.Should().ContainSingle().Which.Should().Be("ProviderName");
    providerUbrnMap.Data.Names.Should().ContainSingle().Which.Should().Be("ProviderUbrn");
    referralSourceMap.Data.Names.Should().ContainSingle().Which.Should().Be("ReferralSource");
    referringGpPracticeNumberMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("ReferringGpPracticeNumber");
    referringOrganisationOdsCodeMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("ReferringOrganisationOdsCode");
    sexMap.Data.Names.Should().ContainSingle().Which.Should().Be("Sex");
    staffRoleMap.Data.Names.Should().ContainSingle().Which.Should().Be("StaffRole");
    statusMap.Data.Names.Should().ContainSingle().Which.Should().Be("Status");
    triagedCompletionLevelMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("TriagedCompletionLevel");
    vulnerableDescriptionMap.Data.Names
      .Should().ContainSingle().Which.Should().Be("VulnerableDescription");
    weightMeasurement0007Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement00-07");
    weightMeasurement0814Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement08-14");
    weightMeasurement1521Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement15-21");
    weightMeasurement2228Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement22-28");
    weightMeasurement2935Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement29-35");
    weightMeasurement3642Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement36-42");
    weightMeasurement4349Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement43-49");
    weightMeasurement5056Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement50-56");
    weightMeasurement5763Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement57-63");
    weightMeasurement6470Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement64-70");
    weightMeasurement7177Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement71-77");
    weightMeasurement7884Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement78-84");
    weightMeasurement8500Map.Data.Names
      .Should().ContainSingle().Which.Should().Be("WeightMeasurement85+");
  }
}
