using System;

namespace WmsHub.Business.Entities
{
  public interface IPharmacyReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
  }
}