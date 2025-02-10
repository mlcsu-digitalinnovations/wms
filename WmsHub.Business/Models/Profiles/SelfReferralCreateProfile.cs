using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  class SelfReferralCreateProfile : Profile
  {
    public SelfReferralCreateProfile()
    {
      CreateMap<SelfReferralCreate, Entities.Referral>().ReverseMap();
    }
  }
}
