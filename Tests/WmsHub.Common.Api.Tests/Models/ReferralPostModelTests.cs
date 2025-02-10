using WmsHub.Common.Api.Models;

namespace WmsHub.Common.Api.Tests.Models
{
  public class ReferralPostModelTests : ReferralPostBaseTests
  {
    protected override ReferralPostBase CreateBaseModel(
      string ubrn = "123456789012", string serviceId = "1234567")
    {
      return new ReferralPost
      {
        Ubrn = ubrn, 
        ServiceId = serviceId
      };
    }
  }
}
