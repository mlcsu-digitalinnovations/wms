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
  public class PrepareDelayedCallsController : BaseController
  {

    public PrepareDelayedCallsController(IReferralService service)
      : base(service)
    { }

    /// <summary>
    /// Prepares referrals' status to be set to 'RmcCall' 
    /// if the referral DateToDelayUntil date has passed.
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
        string refsPrepared = await Service.PrepareDelayedCallsAsync();

        return Ok(refsPrepared);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    private IReferralService Service
    {
      get
      {
        IReferralService service = _service as IReferralService;
        service.User = User;
        return service;
      }
    }
  }
}
