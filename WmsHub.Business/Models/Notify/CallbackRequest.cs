using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Notify
  {
    public class CallbackRequest : ICallbackRequest
    {
      public CallbackRequest() { }
      public CallbackRequest(ICallbackRequest model)
      {
        Id = model.Id;
        Reference = model.Reference;
        To = model.To;
        Status = model.Status;
        CreatedAt = model.CreatedAt;
        CompletedAt = model.CompletedAt;
        SentAt = model.SentAt;
        NotificationType = model.NotificationType;
        SourceNumber = model.SourceNumber;
        DestinationNumber = model.DestinationNumber;
        Message = model.Message;
        DateReceived = model.DateReceived;
      }

      [Required]
      public string Id { get; set; }
      public string Reference { get; set; }
      public string To { get; set; }
      public string Status { get; set; } = CallbackStatus.None.ToString();

      public CallbackStatus StatusValue
      {
        get
        {
          return Status switch
          {
            "delivered" => CallbackStatus.Delivered,
            "permanent-failure" => CallbackStatus.PermanentFailure,
            "technical-failure" => CallbackStatus.TechnicalFailure,
            "temporary-failure" => CallbackStatus.TemporaryFailure,
            _ => CallbackStatus.None,
          };
        }
      }
      public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
      public DateTimeOffset? CompletedAt { get; set; }
      public DateTimeOffset? SentAt { get; set; }
      public string NotificationType { get; set; } =
        CallbackNotification.Sms.ToString();

      public CallbackNotification NotificationTypeValue =>
        NotificationType == CallbackNotification.Email.ToString() ? 
          CallbackNotification.Email :
          CallbackNotification.Sms;


      public bool IsCallback => StatusValue != CallbackStatus.None;

      #region Recieved Text Message extensions - Currently unused
      public string SourceNumber { get; set; }
      public string DestinationNumber { get; set; }
      public string Message { get; set; }
      public DateTimeOffset? DateReceived { get; set; }

      [JsonIgnore]
      public bool ValidMessage => !string.IsNullOrEmpty(Message)
        && (Message.Length < 160);
      #endregion
    }
  }
