namespace WmsHub.Business.Models.AuthService;

public class AuthOptions
{
  /// <summary>
  /// Guid associated with reply-to email address.
  /// </summary>
  public string EmailReplyToId { get; set; }
  /// <summary>
  /// Gov.UK Notify template id for SmsKey email message.
  /// </summary>
  public string EmailTemplateId { get; set; }
  /// <summary>
  /// Expires in number of seconds
  /// </summary>
  public int Expires => TokenExpiry * 60;
  public string NotifyLink { get; set; }
  public bool OverrideIssuePreviousToken { get; set; }
  /// <summary>
  /// Refresh Token Expire in number of days
  /// Default is 30
  /// </summary>
  public int RefreshExpireDays { get; set; } = 30;
  public static string SectionKey => "AuthOptions";
  public string SmsApiKey { get; set; }
  public string SmsSenderId { get; set; }
  public string SmsTemplateId { get; set; }
  public static string TextOptionsSectionKey => 
    "WmsHub_TextMessage_Api_TextSettings";
  /// <summary>
  /// Number of minutes for the token to expire
  /// recorded in seconds in bearer token
  /// as minutes x 24 x 60
  /// Default is 1440 = 1 day
  /// </summary>
  public int TokenExpiry { get; set; } = 60;
}
