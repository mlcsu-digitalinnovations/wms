using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models.Admin.ReferralFixes;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class ReferralFixesController : BaseController
  {
    public ReferralFixesController(IReferralAdminService service)
      : base(service)
    {
    }

    /// <summary>
    /// Changes a referral's date of birth
    /// </summary>
    /// <remarks>
    /// The existing referral's date of birth must match the provided original
    /// date of birth.
    /// </remarks>
    /// <returns>The referral object once it has been updated.</returns>
    /// <response code="200">Referral updated successfully</response>    
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Unprocessable values passed to method</response>
    /// <response code="404">A referral was not found for the provided UBRN or 
    /// it was found but the original date of birth provided does not match
    /// </response>
    /// <response code="409">The referral does not currently have the provided
    /// original date of birth.</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPatch("ChangeDateOfBirth/{ubrn:length(12)}")]
    public async Task<IActionResult> ChangeDateOfBirth(
      string Ubrn,
      ChangeDateOfBirthRequest changeDateOfBirthRequest)
    {
      try
      {
        return Ok(await Service.ChangeDateOfBirthAsync(
          Ubrn,
          changeDateOfBirthRequest.OriginalDateOfBirth,
          changeDateOfBirthRequest.UpdatedDateOfBirth));
      }
      catch (Exception ex)
      {
        if (ex is ArgumentException ||
          ex is AgeOutOfRangeException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        if (ex is ReferralNotFoundException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Changes a referral's mobile phone number
    /// </summary>
    /// <remarks>
    /// The existing referral's mobile phone number must match the provided 
    /// original mobile.
    /// </remarks>
    /// <returns>The referral object once it has been updated.</returns>
    /// <response code="200">Referral updated successfully</response>    
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Unprocessable values passed to method</response>
    /// <response code="404">A referral was not found for the provided UBRN or 
    /// it was found but the original mobile provided does not match
    /// </response>
    /// <response code="409">The referral does not currently have the provided
    /// original mobile.</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPatch("ChangeMobile/{ubrn:length(12)}")]
    public async Task<IActionResult> ChangeMobile(
      string Ubrn,
      ChangeMobileRequest changeMobileRequest)
    {
      try
      {
        return Ok(await Service.ChangeMobileAsync(
          Ubrn,
          changeMobileRequest.OriginalMobile,
          changeMobileRequest.UpdatedMobile));
      }
      catch (Exception ex)
      {
        if (ex is ArgumentException ||
          ex is AgeOutOfRangeException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        if (ex is ReferralNotFoundException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Soft deletes a cancelled GP referral with the provided UBRN
    /// </summary>
    /// <remarks>
    /// The referral MUST have a status of CancelledByEReferrals and MUST have
    /// a referral source of GpReferral for the delete to be successful.
    /// </remarks>
    /// <returns>Details of the action performed.</returns>
    /// <response code="200">Referral deleted.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="404">Referral was not found.</response>
    /// <response code="409">Referral is not a GP referral or does not have a 
    /// status of CancelledByEReferrals.</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("CancelledGpReferral/{ubrn:length(12)}")]
    public async Task<IActionResult> DeleteCancelledGpReferral(
      DeleteCancelledGpReferralRequest request,
      string ubrn)
    {
      try
      {
        return Ok(await Service.DeleteCancelledGpReferralAsync(
          ubrn, request.Reason));
      }
      catch (Exception ex)
      {
        if (ex is ReferralNotFoundException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else if (ex is ReferralInvalidStatusException
          || ex is ReferralInvalidReferralSourceException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Soft deletes any referral via its id, current status and ubrn
    /// </summary>
    /// <returns>Details of the action performed.</returns>
    /// <response code="200">Referral deleted.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="404">Referral was not found by Id, Status and Ubrn.
    /// </response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("DeleteReferral/{id:guid}")]
    public async Task<IActionResult> DeleteReferral(
      DeleteReferralRequest request,
      Guid id)
    {
      try
      {
        return Ok(
          await Service.DeleteReferralAsync(new Business.Models.Referral() 
          {
            Id = id,
            Status = request.CurrentStatus,
            StatusReason = request.Reason,
            Ubrn = request.Ubrn
          }));
      }
      catch (Exception ex)
      {
        if (ex is ReferralNotFoundException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Resets a referral's status to New
    /// </summary>
    /// <remarks>
    /// Resets a referral to the start of the process that has been authorised 
    /// for a status change
    /// </remarks>
    /// <returns>The referral object once it has been reset.</returns>
    /// <response code="200">Referral deleted.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="404">Referral was not found by Id, Status and Ubrn.
    /// </response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("ResetReferralStatusToNew/{id:guid}")]
    public async Task<IActionResult> ResetReferralStatusToNew(
      Guid id,
      ResetReferralRequest resetReferralRequest)
    {
      return await ResetReferralToStatus(
        id,
        ReferralStatus.New,
        resetReferralRequest);
    }

    /// <summary>
    /// Resets a referral's status to RmcCall
    /// </summary>
    /// <remarks>
    /// Resets a referral to the start of the process that has been authorised 
    /// for a status change
    /// </remarks>
    /// <returns>The referral object once it has been reset.</returns>
    /// <response code="200">Referral deleted.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="404">Referral was not found by Id, Status and Ubrn.
    /// </response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("ResetReferralStatusToRmcCall/{id:guid}")]
    public async Task<IActionResult> ResetReferralStatusToRmcCall(
      Guid id,
      ResetReferralRequest resetReferralRequest)
    {
      return await ResetReferralToStatus(
        id, 
        ReferralStatus.RmcCall,
        resetReferralRequest);
    }

    private async Task<IActionResult> ResetReferralToStatus(
      Guid id,
      ReferralStatus referralStatus,
      ReasonStatusUbrnRequestBase resetReferralRequest)
    {
      try
      {
        Business.Models.Referral referral = new()
        {
          Id = id,
          Status = resetReferralRequest.CurrentStatus,
          StatusReason = resetReferralRequest.Reason,
          Ubrn = resetReferralRequest.Ubrn
        };

        return Ok(
          await Service.ResetReferralAsync(referral, referralStatus));
      }
      catch (Exception ex)
      {
        if (ex is ReferralNotFoundException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Corrects non-GP referrals stuck with a status of ProviderCompleted
    /// </summary>
    /// <remarks>
    /// When a provider sends an PUT request to the service user endpoint to 
    /// complete a referral if the referral source is not a GP referral then
    /// the referral's status should be set to Complete and not 
    /// ProviderCompleted so that it doesn't get added to the discharge list.
    /// </remarks>
    /// <returns>The number of non-GP referrals that have had their status
    /// updated from ProviderCompleted to Complete.</returns>
    /// <response code="200">Referrals updated successfully</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet("NonGpProviderCompleted")]
    public async Task<IActionResult> NonGpProviderCompleted()
    {
      try
      {
        return (Ok(await Service
          .FixNonGpReferralsWithStatusProviderCompletedAsync()));
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Corrects referrals stuck with a status of ProviderAwaitingTrace
    /// </summary>
    /// <remarks>
    /// For all referrals that have a status of ProviderAwaitingTrace:<br/>
    /// If NHS number = duplicate, status = CancelledDuplicateTextMessage<br/>
    /// If NHS number != duplicate, status = ProviderAwaitingStart<br/>
    /// If NHS number == null and TraceCount > 1, status = ProviderAwaitingStart
    /// </remarks>
    /// <returns>The number of referrals updated.</returns>
    /// <response code="200">Referrals prepared</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet("ProviderAwaitingTrace")]
    public async Task<IActionResult> ProviderAwaitingTrace()
    {
      try
      {
        return Ok(await Service.FixProviderAwaitingTraceAsync());
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    protected IReferralAdminService Service
    {
      get
      {
        var service = _service as IReferralAdminService;
        service.User = User;
        return service;
      }
    }
  }
}
