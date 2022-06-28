using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;

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

    public ReferralController(IReferralService referralService, IMapper mapper)
      : base(referralService)
    {
      _mapper = mapper;
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

      List<GetActiveUbrnResponse> ubrns =
        _mapper.Map<List<GetActiveUbrnResponse>>(
          await Service.GetActiveReferralAndExceptionUbrns(serviceId));

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

      List<GetActiveUbrnResponse> ubrns =
        _mapper.Map<List<GetActiveUbrnResponse>>(
          await Service.GetActiveReferralAndExceptionUbrns(null));

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

        IReferralUpdate referralUpdate =
           _mapper.Map<ReferralUpdate>(referralPut);

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
        await Service.UpdateReferralCancelledByEReferralAsync(ubrn);

        return Ok();
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
        return Problem(e.Message,
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