using System;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class NhsNumberTrace
  {
    public Guid Id { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Postcode { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
  }
}
