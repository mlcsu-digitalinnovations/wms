using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
public class ReferralDataController(IReferralService service) : BaseController(service)
{
  [HttpPost("UbrnToId")]
  public async Task<IActionResult> UbrnToId(IEnumerable<string> ubrns)
  {
    try
    {
      IEnumerable<string> res = await Service.GetIdsFromUbrns(ubrns);

      return Ok(res);
    }
    catch (Exception ex)
    {
      return BadRequest(ex.Message);
    }
  }

  protected IReferralService Service
  {
    get
    {
      IReferralService service = _service as IReferralService;
      service.User = User;
      return service;
    }
  }
}
