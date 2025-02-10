using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Provider.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class ProviderController : BaseController
{
  public ProviderController(IProviderService providerService)
    : base(providerService)
  {
  }

  /// <summary>
  /// Get Provider Details
  /// </summary>
  /// <returns>IActionResult</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Get()
  {
    try
    {

      if (!Guid.TryParse(User.FindFirst(ClaimTypes.Sid)?.Value,
       out Guid userId))
      {
        throw new ArgumentOutOfRangeException(
          "There was a problem getting the user ID");
      }

      ProviderResponse providerResponse =
        await Service.GetProviderAsync(userId);

      return BaseReturnResponse(providerResponse.ResponseStatus,
        providerResponse,
        providerResponse.GetErrorMessage());
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    }
  }

  private ProviderService Service
  {
    get
    {
      ProviderService service = _service as ProviderService;
      service.User = User;
      return service;
    }
  }
}
