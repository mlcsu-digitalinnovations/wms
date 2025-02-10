using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Models;

public class WebUiSettings
{
  public const string SectionKey = "RmcUi";
  public int DefaultReferralCallbackDelayDays { get; set; }
  public string ElectiveCareServiceUserHubLink { get; set; }
  public string ElectiveCareServiceUserHubLinkTemplateId { get; set; }
  public string Environment { get; set; } = WebUi.ENV_DEVELOPMENT;
  public int MaxDaysAfterFirstContactToDelay { get; set; } = 30;
  public string ProviderByEmailTemplateId { get; set; }
  public string ProviderLinkEndpoint { get; set; }
  public string ReplyToId { get; set; }
  public string ServiceUserHubLink { get; set; }
  public string RmcLive { get; set; }
  public string ServiceUserLive { get; set; }
  public string SecurityHeaderScriptSrc { get; set; }
  public string SecurityHeaderStyleSrc { get; set; }
}