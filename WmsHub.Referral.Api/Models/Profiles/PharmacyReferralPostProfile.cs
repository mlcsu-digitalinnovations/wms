using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class PharmacyReferralPostProfile : Profile
  {
    public PharmacyReferralPostProfile()
    {
      CreateMap<PharmacyReferralPostRequest, PharmacyReferralCreate>();
    }
  }
}