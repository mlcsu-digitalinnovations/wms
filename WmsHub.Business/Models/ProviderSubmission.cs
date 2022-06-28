using System;

namespace WmsHub.Business.Models
{
  public class ProviderSubmission
  {
    public int Coaching { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset SubmissionDate { get; set; }
    public int Measure { get; set; }
    public decimal Weight { get; set; }
  }
}
