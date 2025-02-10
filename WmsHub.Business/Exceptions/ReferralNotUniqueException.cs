using System;
using System.Collections.Generic;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralNotUniqueException : Exception
{
  public List<string> MatchingReferralUbrns { get; }
  public ReferralNotUniqueException() : base() { }
  public ReferralNotUniqueException(string message) : base(message) { }
  public ReferralNotUniqueException(
    string message,
    List<string> matchingReferralUbrns) : base(message)
  {
    MatchingReferralUbrns = matchingReferralUbrns;
  }
  public ReferralNotUniqueException(string message, Exception inner)
    : base(message, inner)
  { }
}
