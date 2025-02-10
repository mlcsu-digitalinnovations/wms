using AutoMapper;
using WmsHub.Business.Models.PatientTriage;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class PatientTriageRequestProfile : Profile
  {
    public PatientTriageRequestProfile()
    {
      CreateMap<PatientTriagePutRequest, PatientTriageUpdateRequest>();
    }
  }
}