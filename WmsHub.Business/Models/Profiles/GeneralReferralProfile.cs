using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class GeneralReferralProfile : Profile
  {
    public GeneralReferralProfile()
    {
      CreateMap<GeneralReferral, Entities.GeneralReferral>().ReverseMap();
    }
  }
}