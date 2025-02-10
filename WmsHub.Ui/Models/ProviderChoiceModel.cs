using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;

namespace WmsHub.Ui.Models
{
  public class ProviderChoiceModel : BaseModel
  {
    public bool DisplayError { get; set; }
    [NotEmpty(ErrorMessage = "A service must be selected")]
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; }
    public List<Provider> Providers { get; set; }
    public string TriagedCompletionLevel { get; set; }

    public bool ShowCoachingSection => TriagedCompletionLevel != $"{TriageLevel.Low:D}";
  }
}