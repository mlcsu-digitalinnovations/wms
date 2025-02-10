using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles;

public class TraceIssueReferralProfile : Profile
{
  public TraceIssueReferralProfile()
  {
    CreateMap<Business.Models.Tracing.TraceIssueReferral,
      TraceIssueReferral>();
  }
}
