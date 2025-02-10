using System.Collections.Generic;

namespace WmsHub.Business.Models;

public class SmsPostErrorResponse
{
  public string Title { get; set; }
  public string Status { get; set; }
  public string TraceId { get; set; }
  public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}
