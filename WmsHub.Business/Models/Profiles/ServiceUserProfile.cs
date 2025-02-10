using AutoMapper;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Profiles
{
  public class ServiceUserProfile : Profile
  {
    public ServiceUserProfile()
    {
      CreateMap<Entities.Referral, ServiceUser>()
        .ForMember(d => d.Age,
          o => o.MapFrom(s => s.DateOfBirth.GetAge()))
        .ForMember(d => d.SexAtBirth,
          o => o.MapFrom(s => s.Sex))
        .ForMember(d => d.Height,
          o => o.MapFrom(s => s.HeightCm))
        .ForMember(d => d.Bmi,
          o => o.MapFrom(s => s.CalculatedBmiAtRegistration))
        .ForMember(d => d.BmiDate,
          o => o.MapFrom(s => s.DateOfBmiAtRegistration))
        .ForMember(d => d.ProviderSelectedDate,
          o => o.MapFrom(s => s.DateOfProviderSelection))
        // Provider should only see the OfferedCompletionLevel
        .ForMember(d => d.TriagedLevel,
          o => o.MapFrom(s => int.Parse(s.OfferedCompletionLevel ?? "0")))
        .ForMember(d => d.HasPhysicalDisability,
          o => o.MapFrom(s => s.HasAPhysicalDisability))
        .ForMember(d => d.HasLearningDisability,
          o => o.MapFrom(s => s.HasALearningDisability));
    }
  }
}