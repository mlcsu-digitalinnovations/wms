using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class SmsPostResponse
{
  public string Id { get; set; }
  public string ClientReference { get; set; }
  public string Mobile { get; set; }
  public Dictionary<string, string> Personalisation { get; set; }
  public string TemplateId { get; set; }
  public string SenderId { get; set; }
  public string Status { get; set; }
  public DateTime StatusDateTime { get; set; }
  public List<string> GetNotificationErrors { get; set; } = new();
  public ReferralQuestionnaireStatus ResponseStatus =>
    (Status == "Created")
    ? ReferralQuestionnaireStatus.Sending
    : (ReferralQuestionnaireStatus)Enum
        .Parse(typeof(ReferralQuestionnaireStatus), Status);

  public string GetNotificationErrorsAsString()
  {
    return string.Join(", ", GetNotificationErrors);
  }
}
