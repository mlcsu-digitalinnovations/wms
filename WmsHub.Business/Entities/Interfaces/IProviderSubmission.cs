using System;

namespace WmsHub.Business.Entities
{
  public interface IProviderSubmission
  {
    int Coaching { get; set; }
    DateTimeOffset Date { get; set; }
    int Measure { get; set; }
    decimal Weight { get; set; }
  }
}