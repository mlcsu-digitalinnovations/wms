using System;

namespace WmsHub.Business.Models;

public class ProviderDetail : BaseModel
{
  public Guid ProviderId { get; set; }

  public string Section { get; set; }

  public int TriageLevel { get; set; }

  public string Value { get; set; }
}
