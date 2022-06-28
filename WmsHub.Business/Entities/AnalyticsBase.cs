using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Entities
{
  public class AnalyticsBase: BaseEntity
  {
    public Guid LinkId { get; set; }
    /// <summary>
    /// The table the link id was provided for
    /// </summary>
    public string LinkDescription { get; set; }
    /// <summary>
    /// The value of the field to be analised
    /// </summary>
    public string Value { get; set; }
    /// <summary>
    /// Lookup enum from PropertyLookupEnum
    /// </summary>
    public int PropertyLookup { get; set; }

  }
}
