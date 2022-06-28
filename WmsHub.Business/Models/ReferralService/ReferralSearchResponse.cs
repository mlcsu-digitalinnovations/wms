using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService
{
  [ExcludeFromCodeCoverage]
  public class ReferralSearchResponse: IReferralSearchResponse
  {
    public IEnumerable<IReferral> Referrals { get; set; }
    public int Count { get; set; }
  }
}
