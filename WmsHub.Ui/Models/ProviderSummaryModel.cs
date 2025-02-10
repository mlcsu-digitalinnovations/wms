using System;
using WmsHub.Business.Enums;
using WmsHub.Common.Helpers;

namespace WmsHub.Ui.Models;

public class ProviderSummaryModel : BaseModel
{
  public string Logo { get; set; }
  public Guid ProviderId { get; set; }
  public string ProviderName { get; set; }
  public TriageLevel TriagedCompletionLevel { get; set; }

  public bool IsLiva => ProviderId == new Guid(Constants.ProviderConstants.ID_LIVA)
    && TriagedCompletionLevel == TriageLevel.High;

  public bool IsMoreLife => ProviderId == new Guid(Constants.ProviderConstants.ID_MORELIFE)
    && TriagedCompletionLevel == TriageLevel.Medium;

  public bool IsOviva => ProviderId == new Guid(Constants.ProviderConstants.ID_OVIVA)
    && TriagedCompletionLevel == TriageLevel.Medium;

  public bool IsSecondNatureLevel1 => 
    ProviderId == new Guid(Constants.ProviderConstants.ID_SECONDNATURE) 
    && TriagedCompletionLevel == TriageLevel.Low;

  public bool IsSecondNatureLevel3 => 
    ProviderId == new Guid(Constants.ProviderConstants.ID_SECONDNATURE)
    && TriagedCompletionLevel == TriageLevel.High;

  public bool IsSlimmingWorld => 
    ProviderId == new Guid(Constants.ProviderConstants.ID_SLIMMINGWORLD)
    && TriagedCompletionLevel == TriageLevel.Low;

  public bool IsXylaLevel1 => ProviderId == new Guid(Constants.ProviderConstants.ID_XYLA)
    && TriagedCompletionLevel == TriageLevel.Low;

  public bool IsXylaLevel2 => ProviderId == new Guid(Constants.ProviderConstants.ID_XYLA)
    && TriagedCompletionLevel == TriageLevel.Medium;

  public bool IsXylaLevel3 => ProviderId == new Guid(Constants.ProviderConstants.ID_XYLA)
    && TriagedCompletionLevel == TriageLevel.High;
}
