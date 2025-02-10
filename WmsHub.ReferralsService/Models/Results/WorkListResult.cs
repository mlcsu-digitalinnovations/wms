using System.Diagnostics.CodeAnalysis;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results
{
  [ExcludeFromCodeCoverage]
  public class WorkListResult : ReferralsResult
  {
    public ErsWorkList WorkList { get; set; }

  }
}