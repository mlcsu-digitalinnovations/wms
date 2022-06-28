using System.Collections.Generic;

namespace WmsHub.ReferralsService.Models.BaseClasses
{
  public abstract class ReferralsResult
  {
    public virtual bool Success { get; set; }
    public List<string> Errors { get; } = new();

    public bool HasErrors
    {
      get
      {
        return (Errors.Count > 0);
      }
    }

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
  }
}