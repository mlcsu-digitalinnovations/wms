using Asp.Versioning;
using Microsoft.AspNetCore.Http;
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
public class SendMessagesController : BaseController
{
  public SendMessagesController(IMessageService service) : base(service)
  { }

  /// <summary>
  /// Reviews the queued list of messages to send, then sends them, recording 
  /// any exceptions or reasons for failure.
  /// </summary>
  /// <returns>Total numbers of sent and exceptions</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPost]
  public async Task<IActionResult> Post()
  {
    try
    {
      Dictionary<string, string> result = 
        await Service.SendQueuedMessagesAsync();

      return Ok(result);
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
