using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  class GeneralReferralUpdateProfile : Profile
  {
    public GeneralReferralUpdateProfile()
    {
      CreateMap<GeneralReferralUpdate, Entities.Referral>().ReverseMap();
    }
  }
}