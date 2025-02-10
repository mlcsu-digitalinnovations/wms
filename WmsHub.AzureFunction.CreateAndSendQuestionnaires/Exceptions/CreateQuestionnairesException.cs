namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Exceptions;

public class CreateQuestionnairesException : Exception
{
  public CreateQuestionnairesException()
  { }

  public CreateQuestionnairesException(string message) : base(message)
  { }

  public CreateQuestionnairesException(string message, Exception inner) : base(message, inner)
  { }
}
