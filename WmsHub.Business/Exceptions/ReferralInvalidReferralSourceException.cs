using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralInvalidReferralSourceException : Exception
{

  public ReferralInvalidReferralSourceException() : base() { }
  public ReferralInvalidReferralSourceException(
    Guid id,
    ReferralSource referralSource)
    : base(
        $"Referral {id} has an invalid referral source of {referralSource}.")
  { }

  public ReferralInvalidReferralSourceException(
    Guid id,
    string referralSource)
    : base(
        $"Referral {id} has an invalid referral source of {referralSource}.")
  { }

  public ReferralInvalidReferralSourceException(
    string ubrn,
    ReferralSource referralSource)
    : base(
        $"Referral with a UBRN of {ubrn} has an invalid referral source " +
        $"of {referralSource}.")
  { }

  public ReferralInvalidReferralSourceException(string message)
    : base(message) { }

  public ReferralInvalidReferralSourceException(
    string message,
    Exception inner)
    : base(message, inner)
  { }
}
