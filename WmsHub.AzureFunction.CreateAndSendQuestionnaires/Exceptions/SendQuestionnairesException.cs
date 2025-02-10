namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Exceptions;

public class SendQuestionnairesException : Exception
{
  public SendQuestionnairesException()
  { }

  public SendQuestionnairesException(string message) : base(message)
  { }

  public SendQuestionnairesException(string message, Exception inner) : base(message, inner)
  { }
}
