using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Provider.Api.Controllers;

[ExcludeFromCodeCoverage]
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
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

    IExceptionHandlerFeature context = HttpContext.Features.Get<IExceptionHandlerFeature>();

    return Problem(
        detail: context.Error.StackTrace,
        title: context.Error.Message);
  }

  [Route("/error")]
  public IActionResult Error() => Problem();
}