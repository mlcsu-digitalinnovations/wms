using AutoMapper;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Models.MSGraph;

namespace WmsHub.Business.Models.Profiles
{
  public class MsGraphProfile : Profile
  {
    public MsGraphProfile()
    {
      CreateMap<DeleteUser, ElectiveCareUserBase>().ReverseMap();
      CreateMap<DeleteUser, FilteredUser>().ReverseMap();
    }
  }
}
