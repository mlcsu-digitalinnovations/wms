using System.Collections.Generic;

namespace WmsHub.Ui.Models
{
  public class ProviderInfoModel
  {
    public IEnumerable<ProviderInfo> Providers { get; set; }
    public IEnumerable<ReferralSourceInfo> Sources { get; set; }
  }
}





