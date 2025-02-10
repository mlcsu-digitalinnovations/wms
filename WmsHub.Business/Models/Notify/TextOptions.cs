using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.Notify;

public class TextOptions : NumberWhiteListOptions, ITextOptions
{
  private string _generalReferralNotifyLink;
  private string _notifyLink;

  public static DomainAccess Access => DomainAccess.TextMessageApi;
  public string Audience { get; set; } = "http://gov.uk";
  [Required]
  public string GeneralReferralNotifyLink
  {
    get => _generalReferralNotifyLink;
    set => _generalReferralNotifyLink = value.EnsureEndsWithForwardSlash();
  }
  public string Issuer { get; set; }
  public MessageTimelineOptions MessageTimelineOptions { get; set; }
  [Required]
  public string NotifyLink
  {
    get => _notifyLink;
    set => _notifyLink = value.EnsureEndsWithForwardSlash();
  }
  public int PrepareMessageDelayMinutes { get; set; } = 5;
  public Func<Entities.TextMessage, bool> SearchPredicate { get; set; } =
    t => t.IsActive
    && t.Sent > DateTime.UtcNow.AddDays(-1)
    && string.IsNullOrWhiteSpace(t.Outcome);
  public static string SectionKey => "TextSettings";
  [Required]
  public string SmsApiKey { get; set; }
  public string SmsBearerToken { get; set; }
  [Required]
  public string SmsSenderId { get; set; }
  public List<SmsTemplate> SmsTemplates { get; set; } = new();
  public bool TokenEnabled { get; set; }
  public string TokenPassword { get; set; }
  [Required]
  public string TokenSecret { get; set; }
  public string TokenUserCheck { get; set; }
  public List<string> ValidUsers { get; set; }

  public virtual Guid GetTemplateIdFor(string templateName)
  {
    if (!SmsTemplates.Any())
    {
      throw new ArgumentOutOfRangeException("Template Id list is empty.");
    }

    SmsTemplate found = SmsTemplates
      .SingleOrDefault(t => t.Name == templateName);

    return found == null
      ? throw new ArgumentNullException(
          $"Template Id not found using name '{templateName}'.")
      : found.Id;
  }   
}
