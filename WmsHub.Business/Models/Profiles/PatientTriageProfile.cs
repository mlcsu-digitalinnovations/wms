using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class PatientTriageProfile : Profile
  {
    public PatientTriageProfile()
    {
      CreateMap<Entities.PatientTriage, PatientTriage.PatientTriage>();
    }
  }
}