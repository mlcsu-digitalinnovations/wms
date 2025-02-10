using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Controllers;

[ExcludeFromCodeCoverage]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class TestController : BaseController
{
  private readonly IMapper _mapper;

  public TestController(IReferralService referralService, IMapper mapper)
    : base(referralService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Posts a referral to the WmsHub ready for the first chat bot call
  /// creating two associated text messages 48 and 96 hours in the past
  /// </summary>
  /// <param name="referralPost"></param>
  /// <response code="200">Referral created</response>
  /// <response code="400">Missing/invalid values</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="409">Referral already exists</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPost]
  [Consumes("application/json")]
  [Route("Referral/Chatbot")]
  public async Task<IActionResult> PostChatBotStatus
    ([FromBody] ReferralPost referralPost)
  {
    try
    {
      IReferralCreate referralCreate =
         _mapper.Map<ReferralCreate>(referralPost);
      IReferral response =
        await Service.TestCreateWithChatBotStatus(referralCreate);

      return Ok(response);
    }
    catch (ReferralInvalidCreationException ex)
    {
      LogException(ex, referralPost);
      return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (ReferralNotUniqueException ex)
    {
      LogException(ex, referralPost);
      return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      LogException(ex, referralPost);
      return Problem(ex.Message,
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
