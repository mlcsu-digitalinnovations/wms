using System;

namespace WmsHub.Business.Models
{
  public class GpReferral : IGpReferral
  {
    public int Id { get; set; }

    public Guid ReferralId { get; set; }

    public string ProviderUbrn => $"GP{Id:0000000000}";
  }
}
