using System;

namespace WmsHub.AzureFunction.ResetOrganisationQuotas;

public class ScheduleStatus
{
  public DateTime Last { get; set; }

  public DateTime Next { get; set; }

  public DateTime LastUpdated { get; set; }
}