using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/Referral/Exception")]
  [Route("Referral/Exception")]
  public class ReferralExceptionController : BaseController
  {
    private readonly IMapper _mapper;

    public ReferralExceptionController(
      IReferralService referralService,
      IMapper mapper)
      : base(referralService)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Posts a referral to the WmsHub has an exception of NhsNumberMismatch
    /// </summary>
    /// <param name="postedModel"></param>
    /// <response code="200">Referral created</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">Referral already exists</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPost]
    [Consumes("application/json")]
    [Route("NhsNumberMismatch")]
    public async Task<IActionResult> PostNhsNumberMismatch(
      [FromBody] ReferralNhsNumberMismatchPost postedModel)
    {
      IReferralExceptionCreate referralExceptionCreate =
        _mapper.Map<ReferralExceptionCreate>(postedModel);

      referralExceptionCreate.ExceptionType =
        CreateReferralException.NhsNumberMismatch;

      return await PostException(referralExceptionCreate, postedModel);
    }

    /// <summary>
    /// Posts a referral to the WmsHub has an exception of MissingAttachment
    /// </summary>
    /// <param name="postedModel"></param>
    /// <response code="200">Referral created</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">Referral already exists</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPost]
    [Consumes("application/json")]
    [Route("MissingAttachment")]
    public async Task<IActionResult> PostMissingAttachment(
      [FromBody] ReferralMissingAttachmentPost postedModel)
    {
      IReferralExceptionCreate referralExceptionCreate = 
        _mapper.Map<ReferralExceptionCreate>(postedModel);

      referralExceptionCreate.ExceptionType = CreateReferralException.MissingAttachment;

      return await PostException(referralExceptionCreate, postedModel);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Consumes("application/json")]
    [Route("InvalidAttachment")]
    public async Task<IActionResult> PostInvalidAttachment(
      [FromBody] ReferralInvalidAttachmentPost postedModel)
    {

      IReferralExceptionCreate referralExceptionCreate =
        _mapper.Map<ReferralExceptionCreate>(postedModel);

      referralExceptionCreate.ExceptionType =
        CreateReferralException.InvalidAttachment;

      return await PostException(referralExceptionCreate, postedModel);
    }


    private async Task<IActionResult> PostException(
      IReferralExceptionCreate referralExceptionCreate,
      object postedModel)
    {
      try
      {
        if (User?.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        IReferral response = await Service
          .CreateException(referralExceptionCreate);

        // TODO - Respond with exceptions when eReferral API is available.
        return Ok(response);
      }
      catch (ReferralNotUniqueException ex)
      {
        LogException(ex, postedModel);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      catch (Exception ex)
      {
        LogException(ex, postedModel);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Route("MissingAttachment")]
    public async Task<IActionResult> PutMissingAttachment(ReferralMissingAttachmentPost model)
    {
      IReferralExceptionUpdate update = _mapper.Map<ReferralExceptionUpdate>(model);

      update.ExceptionType = CreateReferralException.MissingAttachment;

      return await PutException(update);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Consumes("application/json")]
    [Route("NhsNumberMismatch")]

    public async Task<IActionResult> PutNhsNumberMismatch(
      ReferralNhsNumberMismatchPost model)
    {
      IReferralExceptionUpdate update = _mapper.Map<ReferralExceptionUpdate>(model);

      update.ExceptionType = CreateReferralException.NhsNumberMismatch;

      return await PutException(update);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Consumes("application/json")]
    [Route("InvalidAttachment")]
    public async Task<IActionResult> PutInvalidAttachment(ReferralInvalidAttachmentPost model)
    {
      IReferralExceptionUpdate update = _mapper.Map<ReferralExceptionUpdate>(model);
      
      update.ExceptionType = CreateReferralException.InvalidAttachment;

      return await PutException(update);
    }

    protected internal virtual async Task<IActionResult> PutException(
      IReferralExceptionUpdate update)
    {
      try
      {
        if (User?.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        IReferral response = await Service.UpdateReferralToStatusExceptionAsync(update);

        // TODO - Respond with exceptions when eReferral API is available.
        return Ok(response);
      }
      catch (ReferralNotFoundException ex)
      {
        LogException(ex);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }
      catch (ReferralInvalidStatusException ex)
      {
        LogException(ex);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status409Conflict);
      }
      catch (Exception ex)
      {
        LogException(ex);
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

}