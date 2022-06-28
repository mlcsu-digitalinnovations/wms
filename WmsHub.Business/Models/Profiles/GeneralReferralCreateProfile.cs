using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  class GeneralReferralCreateProfile : Profile
  {
    public GeneralReferralCreateProfile()
    {
      CreateMap<GeneralReferralCreate, Entities.Referral>().ReverseMap();
    }
  }
}