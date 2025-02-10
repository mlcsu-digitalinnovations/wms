using System.Collections.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ProviderService;

namespace WmsHub.Business.Models.Profiles
{
	public class ProviderProfile : Profile
	{
		public ProviderProfile()
		{
			CreateMap<Provider, Entities.Provider>().ReverseMap();
      CreateMap<ProviderAuth, Entities.ProviderAuth>().ReverseMap();
      CreateMap<Provider, ProviderRequest>().ReverseMap();
      CreateMap<Entities.Provider, ProviderRequest>().ReverseMap();
			CreateMap<Entities.Provider, ProviderResponse>().ReverseMap();
      CreateMap<Entities.Provider, NewProviderApiKeyResponse>().ReverseMap();
			CreateMap<Models.Provider, ProviderForSelection>();
    }
	}
}