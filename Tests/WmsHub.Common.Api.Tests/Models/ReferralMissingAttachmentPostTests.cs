using WmsHub.Common.Api.Models;

namespace WmsHub.Common.Api.Tests.Models
{
  public class ReferralMissingAttachmentPostTests : ReferralPostBaseTests
  {
    protected override ReferralPostBase CreateBaseModel(
      string ubrn = VALID_UBRN, string serviceId = VALID_SERVICEID)
    {
      return new ReferralMissingAttachmentPost
      {
        Ubrn = ubrn,
        ServiceId = serviceId
      };
    }
  }
}

