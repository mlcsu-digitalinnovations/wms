using System;

namespace WmsHub.Business.Models
{
  public class NhsNumberTraceReferral : NhsNumberTrace
  {
    public string Status { get; set; }
    public DateTimeOffset? LastTraceDate { get; set; }
    public int? TraceCount { get; set; }
  }
}