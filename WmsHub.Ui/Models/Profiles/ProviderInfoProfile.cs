using AutoMapper;

namespace WmsHub.Ui.Models.Profiles
{
  public class ProviderInfoProfile : Profile
  {
    public ProviderInfoProfile()
    {
      CreateMap<Business.Models.ProviderInfo, ProviderInfo>();
    }
  }
}