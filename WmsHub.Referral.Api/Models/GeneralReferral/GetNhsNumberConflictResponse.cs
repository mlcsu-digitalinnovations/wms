using System;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Referral.Api.Models.GeneralReferral
{
  public class GetNhsNumberConflictResponse
  {
    public GetNhsNumberConflictResponse()
    { }

    public GetNhsNumberConflictResponse(
      ErrorType errorType,
      InUseResponse response)
    {
      Error = errorType;
      DateOfReferral = response.Referral?.DateOfReferral;
      ProviderName = response.Referral?.Provider?.Name;
      ReferralSource = response.Referral?.ReferralSource;
      Ubrn = response.Referral?.Ubrn;
    }

    public DateTimeOffset? DateOfReferral { get; set; }
    public ErrorType Error { get; set; } = ErrorType.None;
    public string ErrorDescription => Error.ToString();
    public string ProviderName { get; set; }
    public string ReferralSource { get; set; }
    public string Ubrn { get; set; }

    public enum ErrorType
    {
      None = 0,
      PreviousReferralCancelled = 1,
      PreviousReferralCompleted = 2,
      OtherReferralSource = 3,
      ProviderPreviouslySelected = 4
    }
  }
}
