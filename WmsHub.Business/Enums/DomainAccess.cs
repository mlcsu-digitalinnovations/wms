using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Enums
{
  [Flags]
  public enum DomainAccess
  {
    None = 0,
    ReferralApi = 1,
    ProviderApi = 2,
    TextMessageApi = 4,
    ChatBotApi = 8,
    BusinessIntelligenceApi = 16,
    PostcodeApi = 32,
    DeprivationServiceApi = 64
  }
}
