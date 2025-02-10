using AutoMapper;
using WmsHub.Business.Models.ReferralStatusReason;

namespace WmsHub.Ui.Models.Profiles;

public class ReferralStatusReasonProfile : Profile
{
  public ReferralStatusReasonProfile()
  {
    CreateMap<Business.Entities.ReferralStatusReason, ReferralStatusReason>();

  }
}