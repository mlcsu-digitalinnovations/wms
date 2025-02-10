using System.Collections.Generic;

namespace WmsHub.Business.Models;

public class SpineTraceResponse : SpineTraceResult
{
  public SpineTraceResponse() { }
  public SpineTraceResponse(string error)
  {
    Errors.Add(error);
  }

  public List<string> Errors { get; set; } = new();
}