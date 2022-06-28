using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class PrepareRmcCallsController : BaseController
  {

    public PrepareRmcCallsController(IReferralService service)
      : base(service)
    { }

    /// <summary>
    /// Prepares referrals so they are in a state to be called by RMC
    /// </summary>
    /// <returns>The number of referrals prepared</returns>
    /// <response code="200">Referrals prepared</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
      try
      {
        string callsPrepared = await Service.PrepareRmcCallsAsync();

        return Ok(callsPrepared);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    private ReferralService Service
    {
      get
      {
        ReferralService service = _service as ReferralService;
        service.User = User;
        return service;
      }
    }
  }
}
