namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;
public class Timer
{
  public bool IsPastDue { get; set; }
  public ScheduleStatus ScheduleStatus { get; set; }
}
