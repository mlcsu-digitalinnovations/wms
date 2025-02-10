using System.Collections.Generic;

namespace WmsHub.Business.Models
{
  public class Provider : BaseModel, IProvider
  {
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Summary2 { get; set; }
    public string Summary3 { get; set; }
    public string Website { get; set; }
    public string Logo { get; set; }
    public bool Level1 { get; set; }
    public bool Level2 { get; set; }
    public bool Level3 { get; set; }

    public List<ProviderDetail> Details { get; set; }

    public ProviderAuth ProviderAuth { get; set; }
  }
}
