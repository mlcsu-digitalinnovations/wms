using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class ReferralPostProfile : Profile
  {
    public ReferralPostProfile()
    {
      CreateMap<ReferralPost, ReferralCreate>()
        .ForMember(d => d.Address1,
          opt => opt.MapFrom(s => s.Address1.Trim()))
        .ForMember(d => d.Address2,
          opt => opt.MapFrom(s => s.Address2.Trim()))
        .ForMember(d => d.Address3,
          opt => opt.MapFrom(s => s.Address3.Trim()))
        .ForMember(d => d.Email,
          opt => opt.MapFrom(s => s.Email.Trim()))
        .ForMember(d => d.Ethnicity,
          opt => opt.MapFrom(s => s.Ethnicity.Trim()))
        .ForMember(d => d.FamilyName,
          opt => opt.MapFrom(s => s.FamilyName.Trim()))
        .ForMember(d => d.GivenName,
          opt => opt.MapFrom(s => s.GivenName.Trim()))
        .ForMember(d => d.Postcode, opt =>
          opt.MapFrom(s => s.Postcode.Trim()))
        .ForMember(d => d.ReferringGpPracticeName,
          opt => opt.MapFrom(s => s.ReferringGpPracticeName.Trim()))
        .ForMember(d => d.ReferringGpPracticeNumber,
          opt => opt.MapFrom(s => s.ReferringGpPracticeNumber.Trim()))
        .ForMember(d => d.Sex, opt =>
          opt.MapFrom(s => s.Sex.Trim()))
        .ForMember(d => d.VulnerableDescription,
          opt => opt.MapFrom(s => s.VulnerableDescription.Trim()));
    }
  }
}