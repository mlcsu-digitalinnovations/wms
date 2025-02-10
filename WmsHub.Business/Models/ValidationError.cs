using System.Collections.Generic;

namespace WmsHub.Business.Models;
public class ValidationError
{
  public string Title { get; set; }
  public List<string> Errors { get; set; } = new();
}
