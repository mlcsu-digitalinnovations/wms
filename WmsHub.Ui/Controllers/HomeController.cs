using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Net.WebRequestMethods;

namespace WmsHub.Ui.Controllers
{
  [AllowAnonymous]
  public class HomeController : Controller
  {
    public IActionResult Index(string u)
    {
      if (string.IsNullOrEmpty(u))
      {
        if (Request.Method == Http.Head)
        {
          return Ok();
        }
        else
        {
          return RedirectToAction("Index", "ServiceUser");
        }
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