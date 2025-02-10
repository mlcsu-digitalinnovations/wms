using AutoMapper;

namespace WmsHub.Ui.Models.Profiles;

public class ProviderDetailProfile : Profile
{
  public ProviderDetailProfile()
  {
    CreateMap<Business.Models.ProviderDetail, ProviderDetail>();
  }
}