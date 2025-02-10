using System.ComponentModel.DataAnnotations;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;
public class CreateQuestionnaireRequest
{
  public DateTimeOffset? FromDate { get; set; }
  [Range(1, 250)]
  public int MaxNumberToCreate { get; set; }
  public DateTimeOffset? ToDate { get; set; }
}