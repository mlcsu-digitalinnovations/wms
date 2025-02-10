using System;

namespace WmsHub.Business.Exceptions;
public class PostDischargesException : Exception
{
  private const string ErrorMessage = "Post Discharges ran with errors, latest error";
  public PostDischargesException() : base() { }
  public PostDischargesException(string error): base($"{ErrorMessage}: {error}") { }
  public PostDischargesException(string message, Exception inner) : base(message, inner) { }
}
