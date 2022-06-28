using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WmsHub.ChatBot.Api.Controllers
{
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