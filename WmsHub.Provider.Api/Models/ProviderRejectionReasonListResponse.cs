using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Provider.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class ProviderRejectionReasonListResponse
  {
    public ProviderRejectionReasonListResponse()
    {
      _comment =
        "The reason for the rejection by a provider must be one of the " +
        "reasons specified in the list.  In the 'Reason' field specify " +
        "either the ID or the Title.";
    }
    public ProviderRejectionReasonResult[] Reasons { get; set; }
    public string _comment { get; set; }
  }
}
