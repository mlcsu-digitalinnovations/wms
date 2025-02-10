using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Referral.Api.Models.ReferralQuestionnaire;

public class NotificationProxyCallbackRequest
{
  [Required]
  public string Id { get; set; }
  [Required]
  public string ClientReference { get; set; }
  [Required]
  public NotificationProxyCallbackRequestStatus Status { get; set; }
  [Required]
  public DateTime StatusAt { get; set; }
}
