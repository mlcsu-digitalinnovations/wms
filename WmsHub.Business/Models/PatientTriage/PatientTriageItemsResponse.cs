using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.PatientTriage
{
  public class PatientTriageItemsResponse : IPatientTriageItemsResponse
  {
    public virtual StatusType Status { get; set; }

    public virtual List<string> Errors { get; set; } = new List<string>();

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

    public Dictionary<string, PatientTriage> AgeGroupCompletionData { get; set; }
    public Dictionary<string, PatientTriage> AgeGroupWeightData { get; set; }
    public Dictionary<string, PatientTriage> SexCompletionData { get; set; }
    public Dictionary<string, PatientTriage> SexWeightData { get; set; }
    public Dictionary<string, PatientTriage> EthnicityCompletionData { get; set; }
    public Dictionary<string, PatientTriage> EthnicityWeightData { get; set; }
    public Dictionary<string, PatientTriage> DeprivationCompletionData { get; set; }
    public Dictionary<string, PatientTriage> DeprivationWeightData { get; set; }
  }
}
