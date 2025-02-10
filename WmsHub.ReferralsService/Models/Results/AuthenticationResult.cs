using System.Diagnostics.CodeAnalysis;
using  WmsHub.ReferralsService.Models.BaseClasses;
using WmsHub.Common.Models;

namespace WmsHub.ReferralsService.Models.Results
{
  [ExcludeFromCodeCoverage]
  public class AuthenticationResult :ReferralsResult
    {
        public ErsSession Session {get; set;}
    }
}