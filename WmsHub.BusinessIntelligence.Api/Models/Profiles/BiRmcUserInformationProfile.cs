using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles;

public class BiRmcUserInformationProfile : Profile
{
  public BiRmcUserInformationProfile()
  {
    CreateMap<Business.Models.BusinessIntelligence.BiRmcUserInformation, 
      BiRmcUserInformation>().ReverseMap();
  }
}