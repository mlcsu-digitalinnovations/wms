using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models.Interfaces
{
  public interface IReferralSearchResponse
  {
    IEnumerable<IReferral> Referrals { get; set; }
    int Count { get; set; }
  }
}
