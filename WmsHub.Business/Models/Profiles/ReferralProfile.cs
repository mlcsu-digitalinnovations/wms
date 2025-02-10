using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class ReferralProfile : Profile
  {
    public ReferralProfile()
    {
      CreateMap<Referral, Entities.Referral>().ReverseMap();
    }
  }
}