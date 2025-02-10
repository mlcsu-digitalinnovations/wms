using AutoMapper;
using WmsHub.Business.Models.PatientTriage;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class CourseCompletionProfile : Profile
  {
    public CourseCompletionProfile()
    {
      CreateMap<CourseCompletionRequest, CourseCompletion>();
    }
  }
}