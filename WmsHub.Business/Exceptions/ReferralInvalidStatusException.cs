using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralInvalidStatusException : Exception
{
  public ReferralInvalidStatusException(
    Guid id,
    ReferralStatus referralstatus)
    : base(
        $"Referral {id} has an invalid referral source of {referralstatus}.")
  { }

  public ReferralInvalidStatusException(
    Guid id,
    string referralSource)
    : base(
        $"Referral {id} has an invalid referral source of {referralSource}.")
  { }

  public ReferralInvalidStatusException() : base() { }
  public ReferralInvalidStatusException(string message) : base(message) { }
  public ReferralInvalidStatusException(string message, Exception inner)
    : base(message, inner)
  { }
}
