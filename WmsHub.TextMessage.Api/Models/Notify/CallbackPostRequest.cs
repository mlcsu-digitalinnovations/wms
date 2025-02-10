using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.TextMessage.Api.Interfaces;

namespace WmsHub.TextMessage.Api.Models.Notify
{
  public class CallbackPostRequest : ICallbackPostRequest
  {

    public CallbackPostRequest() { }

    [Required]
    public string Id { get; set; }
    public string Reference { get; set; }
    public string To { get; set; }
    public string Status { get; set; } = CallbackStatus.None.ToString();
    public DateTimeOffset Created_at { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? Completed_at { get; set; }
    public DateTimeOffset? Sent_at { get; set; }
    public string Notification_type { get; set; } =
      CallbackNotification.Sms.ToString();
    public string Source_number { get; set; }
    public string Destination_number { get; set; }
    public string Message { get; set; }
    public DateTimeOffset? Date_received { get; set; }
  }
}

