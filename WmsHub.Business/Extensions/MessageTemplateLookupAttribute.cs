using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Extensions;

public class MessageTemplateLookupAttribute: Attribute
{
  public MessageTemplateLookupAttribute(
    ReferralSource source, 
    ReferralStatus status,
    MessageType type)
  {
    Status = status;
    Source = source;
    Type = type;
  }

  public MessageType Type { get; set; }
  public ReferralSource Source { get; set; }
  public ReferralStatus Status { get; set;}
}
