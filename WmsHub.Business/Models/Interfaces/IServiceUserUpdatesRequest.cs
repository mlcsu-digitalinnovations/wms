using System;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IServiceUserUpdatesRequest
  {
    int? Coaching { get; set; }
    DateTime? Date { get; set; }
    int? Measure { get; set; }
    decimal? Weight { get; set; }
  }
}