using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models
{
  public interface IProviderAuth : IBaseModel
  {
    string SmsKey { get; set; }
    DateTimeOffset? SmsKeyExpiry { get; set; }
    bool KeyViaSms { get; set; }
    bool KeyViaEmail { get; set; }
    string MobileNumber { get; set; }
    string EmailContact { get; set; }
  }
}
