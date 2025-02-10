using System.Collections.Generic;
using WmsHub.ReferralsService.Interfaces;

namespace WmsHub.ReferralsService.Models.BaseClasses;

public abstract class ReferralsResult : IReferralsResult
{
  public string AggregateErrors
  {
    get
    {
      string value = null;

      if (Errors != null && Errors.Count > 0)
      {
        value = string.Join(":", Errors);
      }

      return value ?? "No AggregateErrors";
    }
  }
  public List<string> Errors { get; } = new();
  public bool HasErrors => Errors.Count > 0;
  public virtual bool Success { get; set; }
  public virtual bool WasRetrievedFromErs { get; set; }
}
