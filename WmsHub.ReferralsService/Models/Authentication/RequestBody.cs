using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Referrals.Models.Authentication
{
  [ExcludeFromCodeCoverage]
  public class RequestBody
  {
    public string typeInfo { get; set; }
    public string token { get; set; }
    public Permission permission { get; set; }
  }
}
