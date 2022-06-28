using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  public class StaffRoleProfile : Profile
  {
    public StaffRoleProfile()
    {
      CreateMap<StaffRole, Entities.StaffRole>().ReverseMap();
    }
  }
}