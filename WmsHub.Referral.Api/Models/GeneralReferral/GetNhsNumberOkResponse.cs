using System;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.GeneralReferral
{
  public class GetNhsNumberOkResponse : PostRequest
  {

    public Guid Id { get; set; }

    public bool IsDateOfBmiAtRegistrationValid
    {
      get
      {
        MaxDaysBehindAttribute a = new MaxDaysBehindAttribute(
          Constants.MAX_DAYS_BMI_DATE_AT_REGISTRATION);
        return a.IsValid(DateOfBmiAtRegistration);
      }
    }
  }
}
