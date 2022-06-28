using System;

namespace WmsHub.Business.Entities
{
  public interface IGeneralReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
  }
}