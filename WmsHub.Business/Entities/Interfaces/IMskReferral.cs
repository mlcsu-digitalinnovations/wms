using System;

namespace WmsHub.Business.Entities
{
  public interface IMskReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
  }
}