using System;

namespace WmsHub.Business.Exceptions;

public class ProcessDocumentException : Exception
{
  public ProcessDocumentException() : base() { }
  public ProcessDocumentException(string message) : base(message) { }
  public ProcessDocumentException(string message, Exception inner)
    : base(message, inner)
  { }
}