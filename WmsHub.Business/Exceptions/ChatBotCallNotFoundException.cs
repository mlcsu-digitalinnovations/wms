using System;

namespace WmsHub.Business.Exceptions;
public class ChatBotCallNotFoundException : Exception
{
  public ChatBotCallNotFoundException()
  {
  }

  public ChatBotCallNotFoundException(Guid callId, Guid referralId)
    : base($"Unable to find a call with an id of {callId} for the referral id {referralId}.")
  {
  }

  public ChatBotCallNotFoundException(string message) : base(message)
  {
  }

  public ChatBotCallNotFoundException(string message, Exception innerException)
    : base(message, innerException)
  {
  }

}