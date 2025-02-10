using System.Collections.Generic;

namespace WmsHub.ReferralsService.Models
{
  public class AvailableActions
  {
    public List<AvailableActionEntry> Entry { get; set; }

    public virtual bool Contains(Enums.ReferralAction action)
    {
      string actionToCheck = $"{action}";
      if (Entry == null)
      {
        return false;
      }

      foreach (AvailableActionEntry e in Entry)
      {
        foreach(AvailableActionCodingItem item in e.Resource.Code.Coding)
        {
          if (item.Code == actionToCheck)
          {
            return true;
          }
        }
      }
      return false;
    }

  }

  public class AvailableActionEntry
  {
    public AvailableActionResource Resource { get; set; }
  }

  public class AvailableActionResource
  {
    public AvailableActionCode Code { get; set; }
  }

  public class AvailableActionCode
  {
    public List<AvailableActionCodingItem> Coding { get; set; }
  }

  public class AvailableActionCodingItem
  {
    public string Code { get; set; }
  }

}
