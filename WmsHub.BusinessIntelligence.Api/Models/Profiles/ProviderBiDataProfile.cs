using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
  public class ProviderBiDataRequestErrorProfile : Profile
  {
    public ProviderBiDataRequestErrorProfile()
    {
      CreateMap<Business.Models.ProviderBiDataRequestError, 
        ProviderBiDataRequestError>();
    }
  }
}