using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.ReferralQuestionnaire;

public class StartReferralQuestionnaireRequest
{
  [Required]
  [MaxLength(200)]
  public string NotificationKey { get; set; }
}
