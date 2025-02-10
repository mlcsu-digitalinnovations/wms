using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService
{
  public class SelfReferralPostResponse: IReferralPostResponse
  {
    public Guid Id { get; set; }
    public IEnumerable<ProviderForSelection> ProviderChoices { get; set; }
  }
}
