using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class ReferralMissingAttachmentPostProfile : Profile
  {
    public ReferralMissingAttachmentPostProfile()
    {
      CreateMap<ReferralMissingAttachmentPost, ReferralExceptionCreate>();
    }
  }
}