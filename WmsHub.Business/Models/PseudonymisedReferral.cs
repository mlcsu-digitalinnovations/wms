using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models;

public class PseudonymisedReferral : AnonymisedReferral
{
  public bool HasReceivedCoaching => ProviderSubmissions != null
    && ProviderSubmissions.Any(s => s.Coaching > 0);

  public bool IsEligible => 
    !((string.IsNullOrWhiteSpace(StatusReason) 
      || StatusReason == Constants.BusinessIntelligence.INELIGIBLESTATUSREASON)
    && !TriagedCompletionLevel.HasValue
    && NumberOfContacts == 0);

  public string NhsNumber { get; set; }

  [JsonPropertyName("OPCSCodesForElectiveCare")]
  public string OpcsCodesForElectiveCare => OpcsCodes;

  public string ReferralSourceDescription =>
    ReferralSource == Enums.ReferralSource.SelfReferral.ToString()
      ? Enums.ReferralSource.SelfReferral.GetDescriptionAttributeValue()
      : ReferralSource;

  public string ServiceUserEthnicitySubgroup => ServiceUserEthnicity;

  public string TriagedCompletionLevelString
  {
    get
    {
      if (TriagedCompletionLevel == null)
      {
        if (Constants.BusinessIntelligence.AWAITINGTRIAGESTATUSES.Contains(Status))
        {
          return Constants.BusinessIntelligence.AWAITINGTRIAGE;
        }

        return Constants.BusinessIntelligence.NOTTRIAGED;
      }

      return TriagedCompletionLevel.Value.ToString(CultureInfo.InvariantCulture);
    }
  }
}