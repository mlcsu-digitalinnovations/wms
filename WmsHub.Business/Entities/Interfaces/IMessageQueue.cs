using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public interface IMessageQueue
{
  /// <summary>
  /// Used for links to the RMC portal.  This is currently just for the 
  /// statuses of TexMessage1 and TextMessage2.  LinkId should
  /// be unique as it can then be used to test the date sent.
  /// </summary>
  string ServiceUserLinkId { get; set; }
  Guid Id { get; set; }
  bool IsActive { get; set; }
  DateTimeOffset ModifiedAt { get; set; }
  Guid ModifiedByUserId { get; set; }
  MessageType Type { get; set; }
  string PersonalisationJson { get; set; }
  Guid ReferralId { get; set; }
  DateTime? SentDate { get; set; }
  string SendTo { get; set; }
  Guid TemplateId { get; set; }
}