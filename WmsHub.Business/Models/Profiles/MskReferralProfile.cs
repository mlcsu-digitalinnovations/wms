using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class MskReferralProfile : Profile
  {
    public MskReferralProfile()
    {
      CreateMap<MskReferral, Entities.MskReferral>().ReverseMap();
    }
  }
}