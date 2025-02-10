using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class ReferralExceptionCreateProfile : Profile
  {
    public ReferralExceptionCreateProfile()
    {
      CreateMap<ReferralExceptionCreate, Entities.Referral>();
    }

  }
}