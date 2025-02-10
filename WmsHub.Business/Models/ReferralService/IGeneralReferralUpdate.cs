using System;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IGeneralReferralUpdate : IGeneralReferralCreate
  {
    Guid Id { get; set; }
  }
}