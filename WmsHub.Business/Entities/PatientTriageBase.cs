using System.Collections.Generic;

namespace WmsHub.Business.Entities
{
  public class PatientTriageBase : BaseEntity
  {
    public string TriageSection { get; set; }
    public string Key { get; set; }
    public string Descriptions { get; set; }
    public int Value { get; set; }
    public int CheckSum { get; set; }
  }
}