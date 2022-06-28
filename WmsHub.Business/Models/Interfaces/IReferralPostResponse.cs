using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models.Interfaces
{
  public interface IReferralPostResponse
  {
    Guid Id { get; set; }
    IEnumerable<ProviderForSelection> ProviderChoices { get; set; }
  }
}
