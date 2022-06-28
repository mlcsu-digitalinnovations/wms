using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
  public class ReprocessedReferralProfile : Profile
  {
    public ReprocessedReferralProfile()
    {
      CreateMap<Business.Models.ReprocessedReferral, ReprocessedReferral>();
    }
  }
}
