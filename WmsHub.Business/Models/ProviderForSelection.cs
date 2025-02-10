using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models
{
  public class ProviderForSelection
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Website { get; set; }
    public string Logo { get; set; }

  }
}
