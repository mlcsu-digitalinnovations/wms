namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public class FetchWorklistRequest : ARequestBase
{
  public string? ServiceId => Parameter
    ?.FirstOrDefault(x => x.Name == "service")
    ?.ValueIdentifier
    ?.Value;
}
