using System;
using System.Collections.Generic;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.Notify
{
  public interface ITextOptions : INumberWhiteListOptions
  {
    string NotifyLink { get; set; }
    string SmsApiKey { get; set; }
    string SmsBearerToken { get; set; }
    string SmsSenderId { get; set; }
    List<SmsTemplate> SmsTemplates { get; set; }
    string TokenSecret { get; set; }

    Guid GetTemplateIdFor(string templateName);
    Func<Entities.TextMessage, bool> SearchPredicate { get; set; }

    List<string> ValidUsers { get; set; }
    string Audience { get; set; }
    string Issuer { get; set; }
  }
}