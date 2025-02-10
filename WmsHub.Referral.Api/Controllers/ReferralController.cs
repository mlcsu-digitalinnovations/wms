using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Extensions;
using WmsHub.Common.Models;
using WmsHub.Referral.Api.Models;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class ReferralController : BaseController
  {
    private readonly IMapper _mapper;
    private readonly IProcessStatusService _processStatusService;
    private readonly ProcessStatusOptions _processStatusOptions;

    public ReferralController(
      IReferralService referralService,
      IMapper mapper,
      IProcessStatusService processStatusService,
      IOptions<ProcessStatusOptions> processStatusOptions)
      : base(referralService)
    {
      _mapper = mapper;
      _processStatusService = processStatusService;
      _processStatusOptions = processStatusOptions.Value;
    }

    /// <summary>
    /// Gets all active GP referral UBRNs for the provided service id
    /// </summary>
    /// <param name="serviceId">The Service Id of the desired Ubrn list</param>
    /// <response code="200">Ubrn list returned</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet]
    [Route("ubrns/{serviceId}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveUbrns(string serviceId)
    {
      if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
      {
        return BaseReturnResponse(StatusType.NotAuthorised,
          null,
          "Access has not been granted for this endpoint.");
      }

      List<ActiveReferralAndExceptionUbrn> activeReferralAndExceptionUbrns = await Service
        .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId);

      List<GetActiveUbrnResponse> ubrns = _mapper
        .Map<List<GetActiveUbrnResponse>>(activeReferralAndExceptionUbrns);

      return Ok(ubrns);
    }

    /// <summary>
    /// Gets all active GP referral UBRNs
    /// </summary>
    /// <response code="200">Ubrn list returned</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet]
    [Route("ubrns")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveUbrns()
    {
      if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
      {
        return BaseReturnResponse(StatusType.NotAuthorised,
          null,
          "Access has not been granted for this endpoint.");
      }

      List<GetActiveUbrnResponse> ubrns = _mapper
        .Map<List<GetActiveUbrnResponse>>(
          await Service.GetOpenErsGpReferralsThatAreNotCancelledByEreferals());

      return Ok(ubrns);
    }

    /// <summary>
    /// Posts a referral to the WmsHub
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
    public async Task<IActionResult> Post([FromBody] ReferralPost referralPost)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        referralPost.InjectionRemover();

        IReferralCreate referralCreate =
           _mapper.Map<ReferralCreate>(referralPost);

        IReferral response = await Service.CreateReferral(referralCreate);


        // TODO - Respond with exceptions when eReferral API is available.
        return Ok(response);
      }
      catch (ReferralInProgressException ex)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
      }
      catch (ReferralNotUniqueException ex)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
      }
      catch (Exception ex)
      {
        LogException(ex, referralPost);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Updates a referral
    /// </summary>
    /// <param name="referralPut"></param>
    /// <param name="ubrn">The UBRN of the referral to update</param>
    /// <response code="200">Referral updated</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">Referral already exists</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPut]
    [Consumes("application/json")]
    [Route("{ubrn:length(12)}")]
    public async Task<IActionResult> Put(
      [FromBody] ReferralPut referralPut, string ubrn)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        referralPut.InjectionRemover();

        IReferralUpdate referralUpdate = _mapper.Map<ReferralUpdate>(referralPut);
        referralUpdate.Ubrn = ubrn;

        IReferral response = await Service.UpdateGpReferral(referralUpdate);

        // TODO - Respond with exceptions when eReferral API is available.
        return Ok(response);
      }
      catch (Exception ex)
      {
        if (ex is ReferralNotFoundException
          || ex is ReferralNotUniqueException
          || ex is ReferralInvalidStatusException)
        {
          LogException(ex, referralPut);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
          LogException(ex, referralPut);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Updates the referral to CancelledByEReferral
    /// </summary>
    /// <param name="ubrn">The UBRN of the referral to remove</param>
    /// <returns>Success or Failed Response</returns>
    [HttpDelete]
    [Route("{ubrn:length(12)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteReferral(string ubrn)
    {

      try
      {
        using (FunctionTimer timer = new FunctionTimer(
          $"UpdateReferralCancelledByEReferralAsync({ubrn})", 
          Log.Logger,
          10))
        {
          await Service.UpdateReferralCancelledByEReferralAsync(ubrn);

          return Ok();
        }
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

    /// <summary>
    /// Completes the discharge of a referral
    /// </summary>
    /// <param name="id">The id of the referral to remove</param>
    /// <returns>Success or Failed Response</returns>
    [HttpPatch]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DischargeReferral(Guid id)
    {
      try
      {
        await Service.DischargeReferralAsync(id);

        return Ok();
      }
      catch (ReferralNotFoundException ex)
      {
        LogException(ex);
        return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
      }
      catch (ReferralInvalidStatusException ex)
      {
        LogException(ex);
        return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    [HttpPut]
    [Route("CriPut/{ubrn:length(12)}")]
    public async Task<IActionResult>
      CriPut([FromBody] CriUpdateRequest request, string ubrn)
    {
      ReferralClinicalInfo criDocumentBytes =
        _mapper.Map<ReferralClinicalInfo>(source: request);

      criDocumentBytes.Ubrn = ubrn;

      CriCrudResponse response = await Service.CriCrudAsync(criDocumentBytes);
      if (response.ResponseStatus != StatusType.Valid)
      {
        return Problem(response.GetErrorMessage(),
          statusCode: StatusCodes.Status400BadRequest);
      }

      return Ok();
    }

    [HttpGet]
    [Route("GetCriDocument/{ubrn:length(12)}")]
    public async Task<IActionResult> GetCriPdfBytes(string ubrn)
    {
      byte[] result = await Service.GetCriDocumentAsync(ubrn);
      var test = Encoding.UTF8.GetString(result);
      byte[] bytes = Convert.FromBase64String(test);

      string mimeType = "application/pdf";
      return new FileContentResult(bytes, mimeType)
      {
        FileDownloadName = $"{Guid.NewGuid()}.pdf"
      };
    }


    [HttpGet]
    [Route("GetCriDate/{ubrn:length(12)}")]
    public async Task<IActionResult> GetCriDate(string ubrn)
    {
      DateTimeOffset? result = await Service.GetLastCriUpdatedAsync(ubrn);

      return Ok(result);
    }

    [HttpGet]
    [Route("Discharge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDischarges()
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        List<Business.Models.ReferralDischarge> discharges = await Service
          .GetDischarges();

        if (discharges.Any())
        {
          return Ok(
            discharges.Select(d => new Common.Api.Models.ReferralDischarge
            {
              DischargeMessage = d.ToString(),
              Id = d.Id,
              Ubrn = d.Ubrn,
              NhsNumber = d.NhsNumber
            })
            .ToArray());
        }
        else
        {
          return NoContent();
        }
      }
      catch (Exception e)
      {
        LogException(e);
        return Problem(e.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Posts discharged referrals to GpDocumentProxy
    /// </summary>
    /// <returns>Summary of posted discharges</returns>
    [HttpGet]
    [Route("GpDocumentProxy/Discharge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostDischarges()
    {
      try
      {
        _processStatusService.AppName = _processStatusOptions.PostDischargesAppName;
        await _processStatusService.StartedAsync();
      }
      catch (Exception e)
      {
        LogException(e);
      }

      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge> 
          discharges = await Service.GetDischargesForGpDocumentProxy();

        if (discharges.Any())
        {
          List<Guid> successfullyProcessedIds = await Service.PostDischarges(discharges);

          GpDocumentProxyReferralDischarges response = new()
          {
            AllReferralsProcessedSuccessfully = discharges.Count == successfullyProcessedIds.Count,
            Discharges = discharges
              .Select(d => 
                new Common.Api.Models.GpDocumentProxyReferralDischarge
                {
                  Ubrn = d.Ubrn,
                  IsSuccessfullyDischarged = successfullyProcessedIds.Any(i => i == d.Id)
                })
              .ToArray()
          };

          await _processStatusService.SuccessAsync();
          return Ok(response);
        }
        else
        {
          await _processStatusService.SuccessAsync();
          return NoContent();
        }
      }
      catch (Exception e)
      {
        await _processStatusService.FailureAsync(e.Message);

        if (e is PostDischargesException)
        {
          return Ok(e.Message);
        }
        else
        {
          LogException(e);
          return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Updates status of active GP referrals that been discharged
    /// </summary>
    /// <returns>Summary of updated referrals</returns>
    [HttpGet]
    [Route("GpDocumentProxy/UpdateDischarges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDischarges()
    {
      try
      {
        _processStatusService.AppName = _processStatusOptions.UpdateDischargesAppName;
        await _processStatusService.StartedAsync();
      }
      catch (Exception e)
      {
        LogException(e);
      }

      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        GpDocumentProxyUpdateResponse update = await Service.UpdateDischarges();

        await _processStatusService.SuccessAsync();
        return Ok(update);
      }
      catch (Exception e)
      {
        await _processStatusService.FailureAsync(e.Message);

        if (e is UpdateDischargesException)
        {
          return Ok(e.Message);
        }
        else
        {
          LogException(e);
          return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    [HttpPatch]
    [Route("GpDocumentProxy/UpdateWithRejection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateWithRejection(
      [FromBody] RejectionPatch model)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        GpDocumentProxySetRejection response = await Service
          .UpdateDischargedReferralWithRejection(model.Id, model.Information);

        if (response.StatusCode == StatusCodes.Status200OK)
        {
          return Ok();
        }

        return Problem(response.Message, statusCode: response.StatusCode);
      }
      catch (Exception e)
      {
        LogException(e);
        return Problem(e.Message,
         statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <inheritdoc/>
    [HttpPost]
    [Route("{id:guid}/ersclosed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CloseErsReferral(Guid id)
    {
      try
      {
        await Service.CloseErsReferral(id: id);

        return Ok();
      }
      catch (Exception ex)
      {
        if (ex is ArgumentOutOfRangeException)
        {
          LogInformation(ex.Message);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else if (ex is ReferralNotFoundException){
          LogInformation(ex.Message);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else if (ex is ReferralInvalidStatusException
          || ex is ReferralInvalidReferralSourceException)
        {
          LogInformation(ex.Message);
          return Problem(
            ex.Message, 
            statusCode: StatusCodes.Status409Conflict);
        }
        else
        {
          LogException(ex);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <inheritdoc/>
    [HttpPost]
    [Route("{ubrn:length(12)}/ersclosed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CloseErsReferralByUbrn(string ubrn)
    {
      try
      {
        await Service.CloseErsReferral(ubrn: ubrn);

        return Ok();
      }
      catch (Exception ex)
      {
        if (ex is ArgumentOutOfRangeException)
        {
          LogException(ex);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else if (ex is ReferralNotFoundException)
        {
          LogInformation(ex.Message);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else if (ex is ReferralInvalidStatusException
          || ex is ReferralInvalidReferralSourceException)
        {
          LogInformation(ex.Message);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else
        {
          LogException(ex);
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    [HttpPost]
    [Route("terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Terminate([FromQuery] string reason)
    {
      try
      {
        _processStatusService.AppName = _processStatusOptions
          .TerminateNotStartedProgrammeReferralsAppName;
        await _processStatusService.StartedAsync();
      }
      catch (Exception e)
      {
        LogException(e);
      }

      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Referral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        if (reason.TryParseToEnumName(out TerminationReason reasonValue) &&
          reasonValue == TerminationReason.ProgrammeNotStarted)
        {
          int terminatedReferrals = await Service.TerminateNotStartedProgrammeReferralsAsync();
          await _processStatusService.SuccessAsync();
          return Ok(terminatedReferrals);
        }

        await _processStatusService.FailureAsync(ReferralApiConstants.INVALIDTERMINATIONREASON);
        return Problem(
          ReferralApiConstants.INVALIDTERMINATIONREASON,
          statusCode: StatusCodes.Status400BadRequest);
      }
      catch (Exception ex)
      {
        LogException(ex);
        await _processStatusService.FailureAsync(ex.Message);
        return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
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
