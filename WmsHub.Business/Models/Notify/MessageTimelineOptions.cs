namespace WmsHub.Business.Models.Notify;
public class MessageTimelineOptions
{
  public const string SectionKey = "MessageTimelineOptions";

  public int MaxDaysSinceInitialContactToSendTextMessage3 { get; set; } = 35;
  public int MinHoursSincePreviousContactToSendTextMessage1 { get; set; } = 48;
  public int MinHoursSincePreviousContactToSendTextMessage2 { get; set; } = 48;
  public int MinHoursSincePreviousContactToSendTextMessage3 { get; set; } = 168;
}
