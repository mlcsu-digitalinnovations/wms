using AutoMapper;
using WmsHub.Business.Models.ReferralService.MskReferral;

namespace WmsHub.Referral.Api.Models.MskReferral
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      CreateMap<PostRequest, MskReferralCreate>();
    }
  }
}
