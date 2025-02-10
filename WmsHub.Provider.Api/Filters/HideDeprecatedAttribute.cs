using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using WmsHub.Common.Api.Models;

namespace WmsHub.Provider.Api.Filters;

[ExcludeFromCodeCoverage]
public class HideDeprecatedAttribute : ActionFilterAttribute
{
  public override void OnActionExecuting(ActionExecutingContext context)
  {
    IOptions<ApiVersionOptions> options = context.HttpContext.RequestServices
      .GetService<IOptions<ApiVersionOptions>>();

    if (options != null)
    {
      ApiVersionOptions apiVersionOptions = options.Value;

      if (apiVersionOptions.HideDeprecated
        && apiVersionOptions.IsVersionDeprecated(context.HttpContext.Request.Path))
      {
        context.Result = new NotFoundResult();
      }
    }

    base.OnActionExecuting(context);
  }
}
