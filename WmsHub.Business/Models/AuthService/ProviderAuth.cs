using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models
{
  [ExcludeFromCodeCoverage]
  public class ProviderAuth: BaseModel, IProviderAuth
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
