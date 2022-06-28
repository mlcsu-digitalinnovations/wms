namespace WmsHub.Business.Models
{
  public class BusinessIntelligenceOptions
  {
    public const string SectionKey = "BusinessIntelligenceOptions";

    public string[] TraceIpWhitelist { get; set; } = { "127.0.0.1" };
    public bool IsTraceIpWhitelistEnabled { get; set; } = true;
  }
}
