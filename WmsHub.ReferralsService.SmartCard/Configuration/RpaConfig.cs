using System;

namespace WmsHub.ReferralsService.SmartCard.Configuration
{
  public class RpaConfig
  {
    public string IsosecIoIdentityAgentName { get; set; }
    public string SmartCardEmailAddress { get; set; }
    public string SmartCardPassword { get; set; }
    public float TimeDelayMultiplier { get; set; }
    public float NumberOfConnectionAttempts { get; set; }

  }
}
