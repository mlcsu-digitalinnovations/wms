using System;

namespace WmsHub.Business.Exceptions;
public class UpdateDischargesException : Exception
{
  private const string ErrorMessage = "Update Discharges ran with errors, latest error";
  public UpdateDischargesException() : base() { }
  public UpdateDischargesException(string error) : base($"{ErrorMessage}: {error}") { }
  public UpdateDischargesException(string message, Exception inner) : base(message, inner) { }
}
