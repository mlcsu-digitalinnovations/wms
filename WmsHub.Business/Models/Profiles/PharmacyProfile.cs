using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class PharmacyProfile : Profile
  {
    public PharmacyProfile()
    {
      CreateMap<Pharmacy, Entities.Pharmacy>().ReverseMap();
    }
  }
}