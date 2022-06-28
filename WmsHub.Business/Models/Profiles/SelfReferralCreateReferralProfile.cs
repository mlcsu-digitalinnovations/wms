using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  class SelfReferralCreateReferralProfile : Profile
  {
    public SelfReferralCreateReferralProfile()
    {
      CreateMap<SelfReferralCreate, ReferralCreate>();
    }
  }
}

