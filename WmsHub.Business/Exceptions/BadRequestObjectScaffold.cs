using System.Collections.Generic;

namespace WmsHub.Business.Exceptions
{
  public class BadRequestObjectScaffold
  {
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
  }
}
