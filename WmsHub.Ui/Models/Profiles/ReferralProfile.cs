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
            CreateMap<ReferralListItemModel, Referral>().ReverseMap();
            CreateMap<Referral, Entities.Referral>().ReverseMap();

            CreateMap<ReferralListItemModel, IReferral>().ReverseMap();

            CreateMap<Entities.Call, Call>().ReverseMap();
            CreateMap<Entities.TextMessage, TextMessage>().ReverseMap();
        }
    }
}