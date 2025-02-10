namespace WmsHub.AzureFunctions.Exceptions;

public  class SendTextMessagesException : Exception
{
  public SendTextMessagesException()
  { }

  public SendTextMessagesException(string message) : base(message)
  { }

  public SendTextMessagesException(string message, Exception inner) : base(message, inner)
  { }
}
