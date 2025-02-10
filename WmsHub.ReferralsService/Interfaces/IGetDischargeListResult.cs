using System.Collections.Generic;
using WmsHub.Common.Api.Interfaces;

namespace WmsHub.ReferralsService.Interfaces;
public interface IGetDischargeListResult: IReferralsResult
{
  List<IGetDischargeUbrnResponse> DischargeList { get; set; }
}
