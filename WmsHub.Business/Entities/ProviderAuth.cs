using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace WmsHub.Business.Entities
{
  public class ProviderAuth : ProviderAuthBase, IProviderAuth
  {
    public virtual Provider? Provider { get; set; }
  }
}
