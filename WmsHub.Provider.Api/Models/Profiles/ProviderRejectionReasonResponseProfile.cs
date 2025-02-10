using AutoMapper;
using WmsHub.Business.Models.ReferralStatusReason;

namespace WmsHub.Provider.Api.Models.Profiles
{
  public class ProviderRejectionReasonResponseProfile:Profile
  {
    public ProviderRejectionReasonResponseProfile()
    {
      CreateMap<ProviderRejectionReasonResult, ReferralStatusReason>()
        .ReverseMap();

    }
  }
}