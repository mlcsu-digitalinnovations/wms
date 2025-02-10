using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WmsHub.Provider.Api.Filters
{
  [ExcludeFromCodeCoverage]
  public class DevelopmentEnvironmentOnlyAttribute :  ActionFilterAttribute
  {

    public override void OnActionExecuting(ActionExecutingContext context)
    {
      IWebHostEnvironment env = context.HttpContext.RequestServices
       .GetService<IWebHostEnvironment>();
      if (!env.IsDevelopment())
      {
        context.Result = new NotFoundResult();
      }

      base.OnActionExecuting(context);
    }
  }
}
