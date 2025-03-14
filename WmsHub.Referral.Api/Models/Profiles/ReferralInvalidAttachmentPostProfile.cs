using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class ReferralInvalidAttachmentPostProfile : Profile
  {
    public ReferralInvalidAttachmentPostProfile()
    {
      CreateMap<ReferralInvalidAttachmentPost, ReferralExceptionCreate>();
      CreateMap<ReferralInvalidAttachmentPost, ReferralExceptionUpdate>();
    }
  }
}