using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using WmsHub.Business.Models.Authentication;

namespace WmsHub.Business.Models.Profiles
{
  public class ApiKeyStoreProfile : Profile
  {
    public ApiKeyStoreProfile()
    {
      CreateMap<ApiKeyStore, Entities.ApiKeyStore>().ReverseMap();
      CreateMap<Entities.ApiKeyStore, ApiKeyStoreResponse>();
      CreateMap<ApiKeyStoreRequest, Entities.ApiKeyStore>()
        .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain));
    }
  }
}
