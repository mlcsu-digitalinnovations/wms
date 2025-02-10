using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
  public class ProviderBiDataProfile : Profile
  {
    public ProviderBiDataProfile()
    {
      CreateMap<Business.Models.ProviderBiData, ProviderBiData>();
    }
  }
}