using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Models
{
  [ExcludeFromCodeCoverage]
  public class Analytics: IAnalytics
  {
    public Guid Id { get; set; }
    public Guid LinkId { get; set; }
    public string LinkDescription { get; set; }
    public string Value { get; set; }
    public int PropertyLookup { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}
