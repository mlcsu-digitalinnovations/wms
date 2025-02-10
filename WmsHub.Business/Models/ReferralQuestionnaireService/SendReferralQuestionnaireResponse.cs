namespace WmsHub.Business.Models;

public class SendReferralQuestionnaireResponse
{
  public int NumberOfReferralQuestionnairesSent { get; set; }
  public int NumberOfReferralQuestionnairesFailed { get; set; }
  public bool NoQuestionnairesToSend { get; set; }
}
