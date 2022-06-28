using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results
{
  public class GetDischargeListResult : ReferralsResult
  {
    public List<GetDischargeUbrnResponse> DischargeList { get; set; }
  }
}
