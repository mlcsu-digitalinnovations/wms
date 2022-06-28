using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using WmsHub.Business.Models;

namespace WmsHub.Provider.Api.Models.Profiles
{
  [ExcludeFromCodeCoverage]
  public class ReferralResponseProfile : Profile
  {
    public ReferralResponseProfile()
    {
      CreateMap<Referral, ReferralResponse>();
      CreateMap<ProviderSubmission, ReferralResponseProviderSubmission>();
    }
  }
}