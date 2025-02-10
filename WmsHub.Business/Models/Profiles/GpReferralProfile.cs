using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class GpReferralProfile : Profile
  {
    public GpReferralProfile()
    {
      CreateMap<GpReferral, Entities.GpReferral>().ReverseMap();
    }
  }
}
