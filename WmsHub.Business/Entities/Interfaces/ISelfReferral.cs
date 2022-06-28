using System;

namespace WmsHub.Business.Entities
{
  public interface ISelfReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
  }
}