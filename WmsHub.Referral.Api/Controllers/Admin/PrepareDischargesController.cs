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
  public class PrepareDischargesController : BaseController
  {

    public PrepareDischargesController(IReferralDischargeService service)
      : base(service)
    { }

    /// <summary>
    /// Prepares GP referrals ready for discharge
    /// </summary>
    /// <returns>The number of referrals prepared for discharge</returns>
    /// <response code="200">Referrals prepared</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
      try
      {
        return Ok(await Service.PrepareDischarges());
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    private ReferralDischargeService Service
    {
      get
      {
        ReferralDischargeService service = _service as ReferralDischargeService;
        service.User = User;
        return service;
      }
    }
  }
}
