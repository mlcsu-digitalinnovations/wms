using AutoMapper;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Referral.Api.Models.ElectiveCareReferral;

namespace WmsHub.Referral.Api.Models.Profiles.ElectiveCareReferral;

public class PostRequestDataRowProfile : Profile
{
  public PostRequestDataRowProfile()
  {
    CreateMap<PostRequest.TrustDataRow, ElectiveCareReferralTrustData>();
    CreateMap<UserManagementPostRequest.UserManagement, ElectiveCareUserData>();
  }
}
