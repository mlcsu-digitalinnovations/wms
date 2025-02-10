using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Entities
{
  public interface IProviderAuth
  {
    Provider Provider { get; set; }
    string SmsKey { get; set; }
    DateTimeOffset? SmsKeyExpiry { get; set; }
    bool KeyViaSms { get; set; }
    bool KeyViaEmail { get; set; }
    string MobileNumber { get; set; }
    string EmailContact { get; set; }
  }
}
