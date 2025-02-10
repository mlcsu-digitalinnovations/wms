using System;
using System.Collections.Generic;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralInProgressException : Exception
{
  public List<string> MatchingReferralUbrns { get; }
  public ReferralInProgressException() : base() { }
  public ReferralInProgressException(string message) : base(message) { }
  public ReferralInProgressException(
    string message,
    List<string> matchingReferralUbrns) : base(message)
  {
    MatchingReferralUbrns = matchingReferralUbrns;
  }
  public ReferralInProgressException(string message, Exception inner)
    : base(message, inner)
  { }
}
