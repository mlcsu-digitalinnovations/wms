using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Extensions;

public class ReferralStatusMessageTypeAttribute : Attribute
{
  public ReferralStatusMessageTypeAttribute(MessageType messageType) 
  { 
    Type= messageType;
  }
  public MessageType Type { get; set; }
}
