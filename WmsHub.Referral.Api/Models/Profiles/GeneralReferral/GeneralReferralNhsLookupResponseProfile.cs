using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Referral.Api.Models.Profiles.GeneralReferral
{
  public class GeneralReferralNhsLookupResponseProfile : Profile
  {
    public GeneralReferralNhsLookupResponseProfile()
    {
      CreateMap<IReferral,  NhsLookupReferralResponse>();
    }
  }
}