using System.Diagnostics.CodeAnalysis;
using System.Security;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;

namespace WmsHub.Provider.Api.Models.Profiles
{
  [ExcludeFromCodeCoverage]
  public class ServiceUserResponseProfile : Profile
  {
    public ServiceUserResponseProfile()
    {
      CreateMap<ServiceUser, ServiceUserResponse>()
        .ForMember(dest => dest.Email,
          opt => opt.MapFrom(src =>
            src.Email.Equals("**DON'T CONTACT BY EMAIL**") ? null : src.Email))
        .ForMember(dest => dest.ReferralSource,
          opt => opt.MapFrom(src =>
            string.IsNullOrWhiteSpace(src.ReferralSource)
              ? ReferralSource.GpReferral.ToString()
              : src.ReferralSource));
    }
  }
}