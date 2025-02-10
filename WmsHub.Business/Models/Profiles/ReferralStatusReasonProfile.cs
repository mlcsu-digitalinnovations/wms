using AutoMapper;
using WmsHub.Business.Models.ReferralStatusReason;

namespace WmsHub.Business.Models.Profiles;

public class ReferralStatusReasonProfile : Profile
{
  public ReferralStatusReasonProfile()
  {
    CreateMap<Entities.ReferralStatusReason, 
      ReferralStatusReason.ReferralStatusReason>()
      .ReverseMap();

    CreateMap<
      ReferralStatusReasonRequest,
      Entities.ReferralStatusReason>()
      .ReverseMap();
  }
}