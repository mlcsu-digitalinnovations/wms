using System;

namespace WmsHub.Common.Api.Models;
public class DateRange(DateTimeOffset from, DateTimeOffset to)
{
  public DateTimeOffset From { get; set; } = from;
  public DateTimeOffset To { get; set; } = to;
}
