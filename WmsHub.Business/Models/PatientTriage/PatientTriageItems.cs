using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.PatientTriage
{
  public class PatientTriageItems
  {
    private Dictionary<string, PatientTriage> _courseConstants;
    public List<PatientTriage> All { get; set; }

    public Dictionary<string, PatientTriage> AgeGroupCompletionData => All
      .Where(t =>
        t.TriageSection == TriageSection.AgeGroupCompletionData.ToString())
      .ToDictionary(_ => _.Key, _ => _);

    public Dictionary<string, PatientTriage> AgeGroupWeightData => All
      .Where(t =>
        t.TriageSection == TriageSection.AgeGroupWeightData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> SexCompletionData => All
      .Where(t =>
        t.TriageSection == TriageSection.SexCompletionData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> SexWeightData => All
      .Where(t =>
        t.TriageSection == TriageSection.SexWeightData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> EthnicityCompletionData => All
      .Where(t =>
        t.TriageSection == TriageSection.EthnicityCompletionData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> EthnicityWeightData => All
      .Where(t =>
        t.TriageSection == TriageSection.EthnicityWeightData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> DeprivationCompletionData => All
      .Where(t =>
        t.TriageSection == TriageSection.DeprivationCompletionData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> DeprivationWeightData => All
      .Where(t =>
        t.TriageSection == TriageSection.DeprivationWeightData.ToString())
      .ToDictionary(_ => _.Key, _ => _);
    public Dictionary<string, PatientTriage> CourseConstants => _courseConstants ??= All
      .Where(t => t.TriageSection == "CompletionScores")
      .ToDictionary(_ => _.Key, _ => _);
  }
}