namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;

public class ScheduleStatus
{
  public DateTime Last { get; set; }
  public DateTime LastUpdated { get; set; }
  public DateTime Next { get; set; }
}
