using System.Collections.Generic;

namespace WmsHub.ReferralsService.Interfaces;
public interface IReferralsResult
{
  string AggregateErrors { get; }
  List<string> Errors { get; }
  bool HasErrors { get; }
  bool Success { get; set; }
}
