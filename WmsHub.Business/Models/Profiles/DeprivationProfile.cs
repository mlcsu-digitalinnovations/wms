using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class DeprivationProfile : Profile
  {
    public DeprivationProfile()
    {
      CreateMap<Deprivation, Entities.Deprivation>().ReverseMap();
    }
  }
}