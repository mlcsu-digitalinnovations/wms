using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WmsHub.TextMessage.Api.Controllers
{
  [ExcludeFromCodeCoverage]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class HealthProbeController : ControllerBase
  {
    [Route("")]
    [HttpHead]
    [AllowAnonymous]
    public IActionResult HealthProbe()
    {
      return Ok();
    }
  }
}