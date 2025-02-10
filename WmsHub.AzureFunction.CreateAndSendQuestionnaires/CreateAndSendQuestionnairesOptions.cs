using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Extensions;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires;

public class CreateAndSendQuestionnairesOptions
{
  private string _referralApiBaseUrl;

  [Required]
  public DateTime CreateQuestionnairesFromDate { get; set; } = new DateTime(2022, 4, 1, 0, 0, 0);

  [Required]
  public string CreateQuestionnairesPath { get; set; } = "questionnaire/create";

  [Required]
  public int MaximumIterations { get; set; }

  [Required]
  public int MaximumQuestionnairesToCreate { get; set; } = 250;

  [Required]
  [Url]
  public string ReferralApiBaseUrl
  {
    get => _referralApiBaseUrl;
    set => _referralApiBaseUrl = value.EnsureEndsWithForwardSlash();
  }

  [Required]
  public string ReferralApiQuestionnaireKey { get; set; }

  [Required]
  public string SendQuestionnairesPath { get; set; } = "questionnaire/create";
}
