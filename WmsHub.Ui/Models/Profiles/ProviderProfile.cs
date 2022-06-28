using AutoMapper;
using WmsHub.Business.Models;
using Entities = WmsHub.Business.Entities;

namespace WmsHub.Ui.Models.Profiles
{
  public class ProviderProfile : Profile
  {
    public ProviderProfile()
    {
      CreateMap<Entities.Provider, Business.Models.Provider>().ReverseMap();
      CreateMap<Provider, Business.Models.Provider>().ReverseMap();
      CreateMap<ProviderAuth, Entities.ProviderAuth>().ReverseMap();
    }

  }
}