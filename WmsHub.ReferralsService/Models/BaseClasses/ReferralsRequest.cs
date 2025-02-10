using System.Diagnostics.CodeAnalysis;
using WmsHub.Common.Models;

namespace WmsHub.ReferralsService.Models.BaseClasses
{
  [ExcludeFromCodeCoverage]
  public abstract class ReferralsRequest
    {
        protected ErsSession CurrentSession {get; set;}
        
        public ReferralsRequest(ErsSession session) {
          CurrentSession = session;
        }
    }
}