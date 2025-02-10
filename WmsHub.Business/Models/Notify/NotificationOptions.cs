using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Notify;

public class NotificationOptions : INotificationOptions
{
  public const string SECTION_KEY = "NotificationOptions";

  public string Endpoint { get; set; }
  public string FailedContactApiKey { get; set; }
  public string NotificationApiKey { get; set; }
  public string NotificationApiUrl { get; set; }
  public string NotificationEmailLink { get; set; }
  public string NotificationQuestionnaireLink { get; set; }
  public string NotificationSenderId { get; set; }
  public string NotificationUrl =>
    $"{NotificationApiUrl.EnsureEndsWithForwardSlash()}{Endpoint}";
  public string ProviderListApiKey { get; set; }
  public string QuestionnaireApiKey { get; set; }
}
