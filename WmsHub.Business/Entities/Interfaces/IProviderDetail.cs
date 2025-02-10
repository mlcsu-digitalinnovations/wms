using System;

namespace WmsHub.Business.Entities.Interfaces;

public interface IProviderDetail
{
  Guid ProviderId { get; set; }
  string Section { get; set; }
  int TriageLevel { get; set; }
  string Value { get; set; }

  Provider Provider { get; set; }
}