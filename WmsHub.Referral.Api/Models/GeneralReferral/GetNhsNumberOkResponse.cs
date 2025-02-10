using System;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.GeneralReferral;

public class GetNhsNumberOkResponse : PostRequest
{
  public Guid Id { get; set; }

  public bool IsDateOfBmiAtRegistrationValid
  {
    get
    {
      MaxDaysBehindAttribute maxDaysBehindAttribute = 
        new(Constants.MAX_DAYS_BMI_DATE_AT_REGISTRATION);

      return maxDaysBehindAttribute.IsValid(DateOfBmiAtRegistration);
    }
  }
    
  public string ReferralSource { get; set; }
}
