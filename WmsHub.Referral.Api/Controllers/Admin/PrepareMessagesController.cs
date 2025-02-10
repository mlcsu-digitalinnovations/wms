using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
public class PrepareMessagesController : BaseController
{
  public PrepareMessagesController(IMessageService service) : base(service)
  {
  }

  /// <summary>
  /// Reviews all referrals with status FailedToContact and sets their status
  /// FailedToContactTextMessage or FailedToContactEmailMessage.
  /// </summary>
  /// <returns>Returns list of ID's</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPost]
  public async Task<IActionResult> Post()
  {
    try
    {
      string[] refsUpdated = await Service.PrepareFailedToContactAsync();

      //string[] newRefs = await Service.PrepareNewReferralsToContactAsync();

      //string[] tm1Refs = 
       // await Service.PrepareTextMessage1ReferralsToContactAsync();

      //System.Collections.Generic.IEnumerable<string> referrals = 
        //refsUpdated.Union(newRefs).Union(tm1Refs);

      return Ok(refsUpdated);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  protected IMessageService Service
  {
    get
    {
      IMessageService service = _service as IMessageService;
      service.User = User;
      return service;
    }
  }
}
