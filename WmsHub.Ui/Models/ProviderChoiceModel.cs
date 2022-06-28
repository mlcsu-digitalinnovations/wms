using System;
using System.Collections.Generic;
using WmsHub.Common.Attributes;

namespace WmsHub.Ui.Models
{
  public class ProviderChoiceModel : BaseModel
  {
    [NotEmpty(ErrorMessage = "A service must be selected")]
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; }
    public List<Provider> Providers { get; set; }
  }
}