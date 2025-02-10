using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class ArgumentNullOrWhiteSpaceException : ArgumentException
{
  private const string MESSAGE = "Value cannot be null or white space.";

  public ArgumentNullOrWhiteSpaceException()
    : base(MESSAGE)
  {
  }

  public ArgumentNullOrWhiteSpaceException(string paramName)
    : base(MESSAGE, paramName)
  {
  }

  public ArgumentNullOrWhiteSpaceException(string message, string paramName)
    : base(message, paramName)
  {
  }

  public ArgumentNullOrWhiteSpaceException(
    string message, Exception innerException)
    : base(message, innerException)
  {
  }
}