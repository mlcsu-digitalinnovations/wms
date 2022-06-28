using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  class ReferralUpdateProfile : Profile
  {
    public ReferralUpdateProfile()
    {
      CreateMap<Entities.Referral, ReferralUpdate>().ReverseMap();
      CreateMap<ReferralUpdate, IReferralCreate>().ReverseMap();
    }
  }
}