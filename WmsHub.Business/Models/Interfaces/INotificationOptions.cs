namespace WmsHub.Business.Models.Notify;

public interface INotificationOptions
{
  string Endpoint { get; set; }
  string FailedContactApiKey { get; set; }
  string NotificationApiKey { get; set; }
  string NotificationApiUrl { get; set; }
  string NotificationEmailLink { get; set; }
  string NotificationQuestionnaireLink { get; set; }
  string NotificationSenderId { get; set; }
  string ProviderListApiKey { get; set; }
  string QuestionnaireApiKey { get; set; }
}