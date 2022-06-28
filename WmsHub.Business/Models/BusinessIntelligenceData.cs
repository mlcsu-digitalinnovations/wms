using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Business.Models
{
  [ExcludeFromCodeCoverage]
  public class BusinessIntelligenceData
  {
    public string UserId { get; set; }
    public int UserAge { get; set; }
    public string ProviderId { get; set; }
    public string ProviderName { get; set; }
    public int TextsSent { get; set; }
    public int PhoneCallsMade { get; set; }
    public bool RejectedStatus { get; set; }
    public bool RejectedLetterSent { get; set; }
  }
}
