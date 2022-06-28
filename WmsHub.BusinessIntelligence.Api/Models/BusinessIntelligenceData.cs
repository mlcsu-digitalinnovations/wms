using System;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class BusinessIntelligenceData
  {
    public BusinessIntelligenceData(
      string userId,
      int userAge,
      string providerId,
      string providerName,
      int textsSent,
      int phoneCallsMade,
      bool rejectedStatus,
      bool rejectedLetterSent)
    {
      UserId = userId ?? throw new ArgumentNullException(nameof(userId));
      UserAge = userAge;
      ProviderId = providerId ??
        throw new ArgumentNullException(nameof(providerId));
      ProviderName = providerName ??
        throw new ArgumentNullException(nameof(providerName));
      TextsSent = textsSent;
      PhoneCallsMade = phoneCallsMade;
      RejectedStatus = rejectedStatus;
      RejectedLetterSent = rejectedLetterSent;
    }

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

  
