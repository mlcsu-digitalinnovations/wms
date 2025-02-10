using AutoMapper;

namespace WmsHub.Business.Models.Profiles;

public class ProviderDetailProfile : Profile
{
  public ProviderDetailProfile()
  {
    CreateMap<ProviderDetail, Entities.ProviderDetail>().ReverseMap();
  }
}