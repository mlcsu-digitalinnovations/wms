using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace WmsHub.Ui.Middleware;

public class ServiceUserInterceptor
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ServiceUserInterceptor> _logger;

  public ServiceUserInterceptor(
    RequestDelegate next,
    ILogger<ServiceUserInterceptor> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task Invoke(HttpContext context)
  {
    string[] unprotectedPages = new string[] 
    {
      "welcome",
      "error",
      "get-started",
      "session-ping",
      "session-ended",
      "VerifyUserDoB"
    };

    Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues =
      context.Request.RouteValues;

    if (routeValues.ContainsKey("controller")
      && routeValues.ContainsKey("action")
      && routeValues.ContainsKey("id"))
    {

      string action = routeValues["action"].ToString();
      string controller = routeValues["controller"].ToString();
      string referralIdFromRoute = routeValues["id"].ToString();

      if (controller == "ServiceUser" && !unprotectedPages.Contains(action))
      {
        // check if session is for correct referral
        string referralIdFromSession = context.Session.GetString("ReferralId");

        if (string.IsNullOrWhiteSpace(referralIdFromSession))
        {
          _logger.LogError(
            ".Wms.Session cookie referral id is null or white space " +
            "for controller {Controller}, action {Action}.",
            controller,
            action);

          context.Response.Redirect("/ServiceUser/GoneWrong");
        }
        else if (referralIdFromSession != referralIdFromRoute)
        {
          _logger.LogError(
            ".Wms.Session cookie referral id {Cookie} does not match referral id in route " +
              "{RouteReferralId} for controller {Controller}, action {Action}.",
            referralIdFromSession,
            referralIdFromRoute,
            controller,
            action);

          context.Response.Redirect("/ServiceUser/GoneWrong");
        }
        else
        {
          await _next.Invoke(context);
        }
      }
      else
      {
        await _next.Invoke(context);
      }
    }
    else
    {
      await _next.Invoke(context);
    }
  }
}

public static class ServiceUserInterceptorExtensions
{
  public static IApplicationBuilder UseServiceUserInterceptor(
    this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<ServiceUserInterceptor>();
  }
}