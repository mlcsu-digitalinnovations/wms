using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.PatientTriage
{
  public interface IPatientTriageItemsResponse
  {
    StatusType Status { get; set; }
    List<string> Errors { get; set; }
    Dictionary<string, PatientTriage> AgeGroupCompletionData { get; set; }
    Dictionary<string, PatientTriage> AgeGroupWeightData { get; set; }
    Dictionary<string, PatientTriage> SexCompletionData { get; set; }
    Dictionary<string, PatientTriage> SexWeightData { get; set; }
    Dictionary<string, PatientTriage> EthnicityCompletionData { get; set; }
    Dictionary<string, PatientTriage> EthnicityWeightData { get; set; }
    Dictionary<string, PatientTriage> DeprivationCompletionData { get; set; }
    Dictionary<string, PatientTriage> DeprivationWeightData { get; set; }
    string GetErrorMessage();
  }
}