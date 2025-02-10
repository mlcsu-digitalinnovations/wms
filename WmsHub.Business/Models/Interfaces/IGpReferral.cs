using System;

namespace WmsHub.Business.Models
{
  public interface IGpReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
    string ProviderUbrn { get; }
  }
}
