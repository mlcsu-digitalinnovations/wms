using System.Collections.Generic;
using WmsHub.Common.Api.Interfaces;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results;

public class GetDischargeListResult : ReferralsResult, IGetDischargeListResult
{
  public List<IGetDischargeUbrnResponse> DischargeList { get; set; }
}
