namespace WmsHub.Referral.Api.Models;

public class ProcessStatusOptions
{
  public string PostDischargesAppName { get; set; }
  public string PrepareDischargesAppName { get; set; }
  public const string SectionKey = "ProcessStatusOptions";
  public string TerminateNotStartedProgrammeReferralsAppName { get; set; }
  public string UpdateDischargesAppName { get; set; }
}
