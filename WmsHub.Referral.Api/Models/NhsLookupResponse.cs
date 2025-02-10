using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Referral.Api.Models
{
  public class NhsLookupResponse
  {
    public NhsLookupReferralResponse Referral { get; set; }
    public List<Provider> Providers { get; set; }
    public string Error { get; set; }
  }
}
