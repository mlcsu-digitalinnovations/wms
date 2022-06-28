using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WmsHub.Business.Services;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ExcludeFromCodeCoverage]
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class DeprivationController : ControllerBase
  {
    private readonly IDeprivationService _service;

    public DeprivationController(IDeprivationService service)
    {
      _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> EtlImdFile()
    {
      _service.User = User;
      await _service.EtlImdFile();
      return Ok();
    }
  }
}