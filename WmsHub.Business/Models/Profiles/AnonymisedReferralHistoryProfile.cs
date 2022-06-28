using AutoMapper;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Profiles
{
  public class AnonymisedReferralHistoryProfile : Profile
  {
    public AnonymisedReferralHistoryProfile()
    {
      CreateMap<Entities.ProviderSubmission, ProviderSubmission>()
        .ForMember(dst => dst.SubmissionDate, opt => opt.MapFrom(src =>
          src.ModifiedAt));
      CreateMap<Entities.Referral, AnonymisedReferralHistory>()
        .ForMember(dst => dst.ProviderSubmissions, opt => opt.MapFrom(src =>
          src.Provider.ProviderSubmissions))
        .ForMember(dst => dst.ProviderName, opt => opt.MapFrom(src =>
          src.Provider.Name))
        .ForMember(dst => dst.GpRecordedWeight, opt => opt.MapFrom(src =>
          src.WeightKg))
        .ForMember(dst => dst.TriagedCompletionLevel, opt => opt.MapFrom(src =>
          ResolveTriage(src.TriagedCompletionLevel)))
        .ForMember(dst => dst.Age, opt => opt.MapFrom(src =>
          src.DateOfBirth.GetAge()))
        .ForMember(dst => dst.MethodOfContact,
          opt => opt.MapFrom(src =>
            ((MethodOfContact)(src.MethodOfContact ?? 0)).ToString()));
    }

    static int ResolveTriage(string compLevel)
    {
      return int.TryParse(compLevel, out var triage) ? triage : default(int);
    }
  }
}