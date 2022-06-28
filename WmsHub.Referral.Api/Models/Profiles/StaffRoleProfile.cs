using AutoMapper;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class StaffRoleProfile : Profile
  {
    public StaffRoleProfile()
    {
      CreateMap<Business.Models.ReferralService.StaffRole, StaffRole>();
    }
  }
}