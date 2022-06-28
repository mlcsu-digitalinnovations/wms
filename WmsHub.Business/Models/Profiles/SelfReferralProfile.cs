using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class SelfReferralProfile : Profile
  {
    public SelfReferralProfile()
    {
      CreateMap<SelfReferral, Entities.SelfReferral>().ReverseMap();
    }
  }
}