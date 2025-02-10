using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Interfaces;

public interface IReferralStatusReason : IBaseModel
{
  string Description { get; set; }
  public ReferralStatusReasonGroup Groups { get; set; }
}