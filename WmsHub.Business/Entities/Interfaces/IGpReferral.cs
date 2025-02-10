using System;

namespace WmsHub.Business.Entities
{
  public interface IGpReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
  }
}
