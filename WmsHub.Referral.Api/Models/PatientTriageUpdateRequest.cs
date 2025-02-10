using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Referral.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class PatientTriagePutRequest
  {
    public string TriageArea { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
  }
}