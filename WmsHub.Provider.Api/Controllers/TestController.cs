using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement.Mvc;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Models;
using WmsHub.Provider.Api.Filters;

namespace WmsHub.Provider.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  [ApiExplorerSettings(IgnoreApi = true)]
  [Authorize(AuthenticationSchemes = "ApiKey")]
  [FeatureGate(Feature.TestOnlyApi)]
  public class TestController : BaseController
  {
    private readonly IMapper _mapper;
    public TestController(IProviderService providerService, IMapper mapper)
      : base(providerService)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Creates between 1 and 200 test referrals (default 20) with an initial 
    /// provider selected status if they are not already present
    /// </summary>
    /// <response code="200">Test referrals created</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [HttpPost]
    [DevelopmentEnvironmentOnly]
    public async Task<IActionResult> CreateTestReferrals(
      [FromQuery]int num = 20, bool withHistory = false)
    {
      try
      {
        if (num < 1 || num > 200)
          return BadRequest(
            $"Can only created between 1 and 200 test referrals not {num}.");

        if (await Service.CreateTestReferralsAsync(num, withHistory))
          return Ok($"Created {num} test referrals.");
        else
          return BadRequest("Test referrals already exist.");
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Deletes all test referrals if they are present
    /// </summary>
    /// <response code="200">Test referrals deleted</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response> 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [HttpDelete]
    [DevelopmentEnvironmentOnly]
    public async Task<IActionResult> DeleteTestReferrals()
    {
      try
      {
        if (await Service.DeleteTestReferralsAsync())
          return Ok();
        else
          return BadRequest("No test referrals available to delete.");
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    protected internal virtual IProviderService Service
    {
      get
      {
        var service = _service as IProviderService;
        service.User = User;
        return service;
      }
    }

  }
}
