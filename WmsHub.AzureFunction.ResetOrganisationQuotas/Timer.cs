namespace WmsHub.AzureFunction.ResetOrganisationQuotas;

public class Timer
{
  public ScheduleStatus ScheduleStatus { get; set; }

  public bool IsPastDue { get; set; }
}
