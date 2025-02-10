using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WmsHub.Ui.Models;
using static System.Net.WebRequestMethods;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Controllers
{
  [AllowAnonymous]
  public class HomeController : BaseController
  {

    public HomeController(IOptions<WebUiSettings> options)
      : base(options.Value)
    {
    }

    public IActionResult Index(string u)
    {
      if (string.IsNullOrEmpty(u))
      {
        return RedirectToAction("Index", "ServiceUser");
      }
      else
      {
        return RedirectToAction("Welcome", "ServiceUser", new { textId = u });
      }
    }

    [Route("Accessibility")]
    public IActionResult AccessibilityPolicy() => View();

    [Route("ContactUs")]
    public IActionResult ContactUs() => View();

    [Route("Cookies")]
    public IActionResult CookiesPolicy() => View();

    [Route("TermsAndConditions")]
    public IActionResult TermsAndConditions() => View();

    [Route("PrivacyPolicy")]
    public IActionResult PrivacyPolicy() => View();

    [Route("Help")]
    public IActionResult Help() => View();
  }
}