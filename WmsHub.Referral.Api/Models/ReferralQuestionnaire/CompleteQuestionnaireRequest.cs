using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models.ReferralQuestionnaire;

public class CompleteQuestionnaireRequest
{
  [Required]
  [MaxLength(200)]
  public string NotificationKey { get; set; }
  [Required]
  public QuestionnaireType QuestionnaireType { get; set; }
  [Required]
  [JsonString]
  public string Answers { get; set; }
}
