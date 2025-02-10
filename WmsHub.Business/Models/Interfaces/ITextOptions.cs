using System;
using System.Collections.Generic;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.Notify;

public interface ITextOptions : INumberWhiteListOptions
{
  public string Audience { get; set; }
  public Guid GetTemplateIdFor(string templateName);
  public string Issuer { get; set; }
  public string NotifyLink { get; set; }
  public int PrepareMessageDelayMinutes { get; set; }
  public Func<Entities.TextMessage, bool> SearchPredicate { get; set; }
  public string SmsApiKey { get; set; }
  public string SmsBearerToken { get; set; }
  public string SmsSenderId { get; set; }
  public List<SmsTemplate> SmsTemplates { get; set; }
  public string TokenSecret { get; set; }
  public string TokenUserCheck { get; set; }
  public List<string> ValidUsers { get; set; }  
}
