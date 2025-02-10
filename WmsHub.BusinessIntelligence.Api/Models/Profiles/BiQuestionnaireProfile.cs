using AutoMapper;
using BI = WmsHub.Business.Models.BusinessIntelligence;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles;

public class BiQuestionnaireProfile : Profile
{
  public BiQuestionnaireProfile()
  {
    CreateMap<BI.BiQuestionnaire,
      BiQuestionnaire>();
  }
}
