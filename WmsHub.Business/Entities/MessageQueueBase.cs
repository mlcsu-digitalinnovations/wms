using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class MessageQueueBase: BaseEntity
{
  public ApiKeyType ApiKeyType { get; set; }
  /// <inheritdoc/>
  public string PersonalisationJson { get; set; }
  public Guid ReferralId { get; set; }
  public string SendResult { get; set; }
  public DateTime? SentDate { get; set; }
  public string SendTo { get; set; }
  public string ServiceUserLinkId { get; set; }
  public Guid TemplateId { get; set; }
  public MessageType Type { get; set; }
}
