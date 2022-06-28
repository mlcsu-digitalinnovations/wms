using AutoMapper;
using WmsHub.Business.Models.PatientTriage;

namespace WmsHub.Business.Models.Profiles
{
  public class CourseCompletionResponseProfile : Profile
  {
    public CourseCompletionResponseProfile()
    {
      CreateMap<CourseCompletion, CourseCompletionResponse>();
    }
  }
}