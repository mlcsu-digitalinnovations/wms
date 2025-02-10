using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WmsHub.Ui.Models;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Controllers;

public abstract class BaseController : Controller
{
  protected readonly WebUiSettings _settings;

  public BaseController(WebUiSettings settings)
  {
    _settings = settings;
  }

  public override void OnActionExecuting(ActionExecutingContext context)
  {
    base.OnActionExecuting(context);

    ViewBag.Environment = _settings.Environment;
    ViewBag.LiveSiteUrl = _settings.ServiceUserLive;
    if(context.RouteData.Values["controller"].ToString() == "Rmc")
    {
      ViewBag.LiveSiteUrl = _settings.RmcLive;
    }

    ViewBag.ShowEnvironmentAlert =
      _settings.Environment == WebUi.ENV_DEVELOPMENT
      || _settings.Environment == WebUi.ENV_STAGING;
  }
}
