using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.SmartCard.Models.Configuration
{
  public class SmartCardConfiguration
  {
    /// <summary>
    /// The number of attempts to find the first control of the login window
    /// </summary>
    public int NumberOfConnectionAttempts { get; set; } = 3;
    public string IsosecIoIdentityAgentName { get; set; }
  }
}
