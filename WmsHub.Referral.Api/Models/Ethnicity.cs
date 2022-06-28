using System;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Referral.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class Ethnicity
  {
    public string DisplayName { get; set; }
    public int DisplayOrder { get; set; }
    public string GroupName { get; set; }
    public int GroupOrder { get; set; }
    public string TriageName { get; set; }
  }
}
