using AutoMapper;
using WmsHub.Business.Models.ElectiveCareReferral;

namespace WmsHub.Business.Models.Profiles;
public class ElectiveCarePostErrorsProfile: Profile
{
  public ElectiveCarePostErrorsProfile()
  {
    CreateMap<ElectiveCarePostError, Entities.ElectiveCarePostError>()
      .ReverseMap();
  }
}
