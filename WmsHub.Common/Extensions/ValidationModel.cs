using System.Collections.Generic;

namespace WmsHub.Common.Extensions
{
  public class ValidationModel
  {
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public string Error => string.Join("  ", Errors);
  }

}