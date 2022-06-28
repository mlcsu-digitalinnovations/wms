using AutoMapper;
using WmsHub.Business.Models.ProviderService;

namespace WmsHub.Business.Models.Profiles
{
  public class ServiceUserSubmissionRequestProfile:Profile
  {
    public ServiceUserSubmissionRequestProfile()
    {
      CreateMap<ServiceUserSubmissionRequest, ServiceUserSubmissionRequestV2>();
    }
  }
}