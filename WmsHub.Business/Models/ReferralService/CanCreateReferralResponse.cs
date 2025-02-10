using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ReferralService;

public class CanCreateReferralResponse
{
  public CanCreateReferralResponse(
    CanCreateReferralResult canCreateResult,
    string reason)
  {
    CanCreateResult = canCreateResult;
    Reason = reason;
  }

  public CanCreateReferralResponse(
    CanCreateReferralResult canCreateResult,
    string reason,
    IReferral referal)
  {
    CanCreateResult = canCreateResult;
    Reason = reason;
    Referral = referal;
  }

  public CanCreateReferralResult CanCreateResult { get; }

  public IReferral Referral { get; }

  public string Reason { get; }
}
