using Swashbuckle.AspNetCore.Annotations;
using System;

namespace WmsHub.Business.Models
{
  public class ProviderSubmission
  {
    [SwaggerSchema("Number of coaching minutes.")]
    public int Coaching { get; set; }
    [SwaggerSchema("Date recorded by provider.")]
    public DateTimeOffset Date { get; set; }
    [SwaggerSchema("Date received by API.")]
    public DateTimeOffset SubmissionDate { get; set; }
    [SwaggerSchema("Provider specific measure.")]
    public int Measure { get; set; }
    [SwaggerSchema("Service user weight in kilograms.")]
    public decimal Weight { get; set; }
  }
}
