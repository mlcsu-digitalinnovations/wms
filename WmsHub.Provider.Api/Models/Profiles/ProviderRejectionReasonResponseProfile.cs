using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Provider.Api.Models.Profiles
{
  public class ProviderRejectionReasonResponseProfile:Profile
  {
    public ProviderRejectionReasonResponseProfile()
    {
      CreateMap<ProviderRejectionReasonResult, ProviderRejectionReason>();
    }
  }
}