using System;

namespace WmsHub.TextMessage.Api.Interfaces
{
  public interface ICallbackPostRequest
  {
    DateTimeOffset? Completed_at { get; set; }
    DateTimeOffset Created_at { get; set; }
    DateTimeOffset? Date_received { get; set; }
    string Destination_number { get; set; }
    string Id { get; set; }
    string Message { get; set; }
    string Notification_type { get; set; }
    string Reference { get; set; }
    DateTimeOffset? Sent_at { get; set; }
    string Source_number { get; set; }
    string Status { get; set; }
    string To { get; set; }
  }
}