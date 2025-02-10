using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class PharmacyReferralProfile:Profile
  {
    public PharmacyReferralProfile()
    {
      CreateMap<PharmacyReferral, Entities.PharmacyReferral>().ReverseMap();
    }
  }
}