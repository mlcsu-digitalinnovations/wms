using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models
{
  public interface ICallbackRequest
  {
    DateTimeOffset? CompletedAt { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? DateReceived { get; set; }
    string DestinationNumber { get; set; }
    string Id { get; set; }
    bool IsCallback { get; }
    string Message { get; set; }
    string NotificationType { get; set; }
    CallbackNotification NotificationTypeValue { get; }
    string Reference { get; set; }
    DateTimeOffset? SentAt { get; set; }
    string SourceNumber { get; set; }
    string Status { get; set; }
    CallbackStatus StatusValue { get; }
    string To { get; set; }
    bool ValidMessage { get; }
  }
}