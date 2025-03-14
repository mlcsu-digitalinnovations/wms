﻿using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralNHSNumberNotUniqueException : Exception
{
  public ReferralNHSNumberNotUniqueException() : base() { }
  public ReferralNHSNumberNotUniqueException(string message)
    : base(message)
  { }
  public ReferralNHSNumberNotUniqueException(string message, Exception inner)
    : base(message, inner)
  { }
}
