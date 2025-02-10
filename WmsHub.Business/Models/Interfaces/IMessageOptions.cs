using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.MessageService;

namespace WmsHub.Business.Models.Interfaces;

public interface IMessageOptions
{
  MessageTemplate GetTemplate(
    MessageType messageType,
    ReferralStatus status,
    ReferralSource source);
  MessageTemplate GetTemplateById(Guid templateId);
  MessageTemplate GetTemplateByName(string templateName);

  /// <summary>
  /// This is the Email reply to ID on gov.uk/notify
  /// </summary>
  string ReplyToId { get; set; }
  string SenderId { get; set; }
  /// <summary>
  /// Json string of templates
  /// </summary>
  string TemplateJson { get; set; }
  string ServiceUserRmcEndpoint { get; set; }
}
