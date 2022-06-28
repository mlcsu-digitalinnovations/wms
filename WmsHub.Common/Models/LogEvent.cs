using System;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Common.Models
{
  [ExcludeFromCodeCoverage]
  public class LogEvent
  {
    public DateTimeOffset Timestamp { get; set; }

    public string Level { get; set; }

    public string RenderedMessage { get; set; }
  }
}
