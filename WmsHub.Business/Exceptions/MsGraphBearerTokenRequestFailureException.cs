using System;

namespace WmsHub.Business.Exceptions;
public class MsGraphBearerTokenRequestFailureException : Exception
{
  public MsGraphBearerTokenRequestFailureException()
  {
  }

  public MsGraphBearerTokenRequestFailureException(string message) : base(message)
  {
  }

  public MsGraphBearerTokenRequestFailureException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
