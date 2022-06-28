using AutoMapper;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class PharmacyProfile : Profile
  {
    public PharmacyProfile()
    {
      CreateMap<PharmacyPost, Business.Models.Pharmacy>().ReverseMap();
      CreateMap<PharmacyPut, Business.Models.Pharmacy>().ReverseMap();
    }
  }
}