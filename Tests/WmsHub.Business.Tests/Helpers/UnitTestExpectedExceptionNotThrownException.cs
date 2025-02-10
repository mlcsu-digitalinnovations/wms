using System;

namespace WmsHub.Business.Tests.Helpers;
public class UnitTestExpectedExceptionNotThrownException : Exception
{
  public UnitTestExpectedExceptionNotThrownException() : base() { }
  public UnitTestExpectedExceptionNotThrownException(string message) : base(message) { }
  public UnitTestExpectedExceptionNotThrownException(string message, Exception inner)
    : base(message, inner)
  { }
}
