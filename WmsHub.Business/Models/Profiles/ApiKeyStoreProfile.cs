using AutoMapper;
using WmsHub.Business.Models.Authentication;

namespace WmsHub.Business.Models.Profiles;

public class ApiKeyStoreProfile : Profile
{
  public ApiKeyStoreProfile()
  {
    CreateMap<ApiKeyStore, Entities.ApiKeyStore>().ReverseMap();
    CreateMap<Entities.ApiKeyStore, ApiKeyStoreResponse>()
      .ForMember(
        dest => dest.NameIdentifier,
        opt => opt.MapFrom(src => src.Key));
    CreateMap<ApiKeyStoreRequest, Entities.ApiKeyStore>()
      .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain));
  }
}
