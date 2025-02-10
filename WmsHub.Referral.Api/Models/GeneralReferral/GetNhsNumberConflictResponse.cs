using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Referral.Api.Models.GeneralReferral
{
  public class GetNhsNumberConflictResponse
  {
    public GetNhsNumberConflictResponse(CanCreateReferralResponse response)
    {
      if (response is null)
      {
        throw new ArgumentNullException(nameof(response));
      }

      Error = response.CanCreateResult;
      DateOfReferral = response.Referral?.DateOfReferral;
      ErrorDescription = response.Reason;
      ProviderName = response.Referral?.Provider?.Name;
      ReferralSource = response.Referral?.ReferralSource;
      Ubrn = response.Referral?.Ubrn;
    }

    public DateTimeOffset? DateOfReferral { get; }
    public CanCreateReferralResult Error { get; }
    public string ErrorDescription { get; }
    public string ProviderName { get; }
    public string ReferralSource { get; }
    public string Ubrn { get; }
  }
}
