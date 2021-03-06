using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace WmsHub.TextMessage.Api.Controllers
{
  [ExcludeFromCodeCoverage]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class ErrorController : ControllerBase
  {
    [Route("/error-development")]
    public IActionResult ErrorDevelopment(
        [FromServices] IWebHostEnvironment webHostEnvironment)
    {
      if (webHostEnvironment.EnvironmentName != "Development")
      {
        throw new InvalidOperationException(
          "This shouldn't be invoked in non-development environments.");
      }

      IExceptionHandlerFeature context = 
        HttpContext.Features.Get<IExceptionHandlerFeature>();

      return Problem(
          detail: context.Error.StackTrace,
          title: context.Error.Message);
    }

    [Route("/error")]
    public IActionResult Error() => Problem();
  }
}