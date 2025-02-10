using System.Collections.Generic;

namespace WmsHub.Business.Entities
{
  public class Ethnicity : EthnicityBase, IEthnicity
  {
    public List<EthnicityOverride> Overrides { get; set; } = new();
  }
}