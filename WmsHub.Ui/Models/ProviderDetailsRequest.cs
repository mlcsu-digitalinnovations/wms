using System;

namespace WmsHub.Ui.Models;

public class ProviderDetailsRequest
{
  public Guid ReferralId { get; set; }
  public string Ubrn { get; set; }
}
