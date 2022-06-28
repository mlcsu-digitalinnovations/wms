using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models.AuthService
{
  public class AuthOptions
  {
    public const string SectionKey = "AuthOptions";

    public const string TextOptionsSectionKey =
      "WmsHub_TextMessage_Api_TextSettings";
    public string SmsApiKey { get; set; }
    public string NotifyLink { get; set; }
    public string SmsTemplateId { get; set; }
    public string SmsSenderId { get; set; }

    /// <summary>
    /// Number of minutes for the token to expire
    /// recorded in seconds in bearer token
    /// as minutes x 24 x 60
    /// Default is 1440 = 1 day
    /// </summary>
    public int TokenExpiry { get; set; } = 60;


    /// <summary>
    /// Expires in number of seconds
    /// </summary>
    public int Expires => TokenExpiry * 60 ;

    /// <summary>
    /// Refresh Token Expire in number of days
    /// Default is 30
    /// </summary>
    public int RefreshExpireDays { get; set; } = 30;
  }
}
