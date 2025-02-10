using AutoMapper;
using WmsHub.Business.Models.PatientTriage;

namespace WmsHub.Ui.Models.Profiles
{
  public class PatientTriageProfile : Profile
  {
    public PatientTriageProfile()
    {
      CreateMap<Business.Entities.PatientTriage, PatientTriage>();
    }
  }
}