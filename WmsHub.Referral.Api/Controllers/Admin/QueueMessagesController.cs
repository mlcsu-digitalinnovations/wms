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
public class QueueMessagesController : BaseController
{
  public QueueMessagesController(IMessageService service) : base(service)
  { }

  /// <summary>
  /// Reviews all possible messages to be sent, 
  /// then queues the relevant message to be sent.
  /// </summary>
  /// <returns>Returns totals for Emails Queued, 
  /// Text Queued and validation count.</returns>
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
        await Service.QueueMessagesAsync(true);

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
