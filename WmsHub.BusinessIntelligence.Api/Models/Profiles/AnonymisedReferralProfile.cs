using AutoMapper;
using System;
using WmsHub.Business.Enums;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles;

public class AnonymisedReferralProfile : Profile
{
  public AnonymisedReferralProfile()
  {
    CreateMap<Business.Models.AnonymisedReferral, AnonymisedReferral>()
      .ForMember(dst => dst.DateCompletedProgramme, opt => opt.MapFrom(src =>
        ResolveDate(src.DateCompletedProgramme)))
      .ForMember(dst => dst.DateOfProviderSelection, opt => opt.MapFrom(src =>
        ResolveDate(src.DateOfProviderSelection)))
      .ForMember(dst => dst.DatePlacedOnWaitingList, opt =>
        opt.MapFrom(src => ResolveDate(src.DatePlacedOnWaitingList)))
      .ForMember(dst => dst.DateStartedProgramme, opt => opt.MapFrom(src =>
        ResolveDate(src.DateStartedProgramme)))
      .ForMember(dst => dst.DateToDelayUntil, opt => opt.MapFrom(src =>
        ResolveDate(src.DateToDelayUntil)))
      .ForMember(dest => dest.ReferralSource,
        opt => opt.MapFrom(src =>
          string.IsNullOrWhiteSpace(src.ReferralSource)
            ? ReferralSource.GpReferral.ToString()
            : src.ReferralSource));
  }

  private static DateTimeOffset? ResolveDate(DateTimeOffset? date) => 
    (date == DateTimeOffset.MinValue) ? null : date;
}