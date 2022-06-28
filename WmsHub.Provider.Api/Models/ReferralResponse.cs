using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Provider.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class ReferralResponse
  {
    public DateTimeOffset? DateOfProviderSelection { get; set; }
    public DateTimeOffset? DateStartedProgramme { get; set; }
    public DateTimeOffset? DateCompletedProgramme { get; set; }
    public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }

    public string Status { get; set; }
    public string Ubrn { get; set; }

    public IEnumerable<ReferralResponseProviderSubmission> ProviderSubmissions 
      { get; set; }
  }

  public class ReferralResponseProviderSubmission
  {
    public int Coaching { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset SubmissionDate { get; set; }
    public int Measure { get; set; }
    public decimal Weight { get; set; }
  }
}
