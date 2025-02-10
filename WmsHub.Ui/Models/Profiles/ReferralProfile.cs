using System.Collections.Generic;
using AutoMapper;
using WmsHub.Business.Models;
using Entities = WmsHub.Business.Entities ;

namespace WmsHub.Ui.Models.Profiles
{
  public class ReferralProfile : Profile
  {
    public ReferralProfile()
    {
      CreateMap<IReferral, BmiWarningModel>();
      CreateMap<ReferralListItemModel, Referral>()
        .ForMember(
          dest => dest.ReferringOrganisationOdsCode, 
          opt => opt.MapFrom(src => src.ReferringPharmacyOdsCode))
        .ForMember(
          dest => dest.ReferringOrganisationEmail,
          opt => opt.MapFrom(src => src.ReferringPharmacyEmail))
        .ReverseMap();
      CreateMap<Referral, Entities.Referral>().ReverseMap();

      CreateMap<ReferralListItemModel, IReferral>().ReverseMap();

      CreateMap<Entities.Call, Call>().ReverseMap();
      CreateMap<Entities.TextMessage, TextMessage>().ReverseMap();
  }
  }
}