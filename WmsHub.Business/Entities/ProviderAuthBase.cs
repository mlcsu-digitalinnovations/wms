using System;

namespace WmsHub.Business.Entities
{
  public class ProviderAuthBase: BaseEntity
  {
    public string SmsKey { get; set; }
    public DateTimeOffset? SmsKeyExpiry { get; set; }
    public bool KeyViaSms { get; set; }
    public bool KeyViaEmail { get; set; }
    public string MobileNumber { get; set; }
    public string EmailContact { get; set; }
    public string IpWhitelist { get; set; }
    public string AccessToken { get; set; }
  }
}