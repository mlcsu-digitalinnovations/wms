namespace WmsHub.Business.Models.ReferralService;

public class ReferralTimelineOptions
{
  public int MaxDaysToStartProgrammeAfterProviderSelection { get; set; } = 42;

  public const string SectionKey = "ReferralTimelineOptions";
}
