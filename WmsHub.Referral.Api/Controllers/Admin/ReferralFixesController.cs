using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Exceptions;
using WmsHub.Referral.Api.Models.Admin.ReferralFixes;

namespace WmsHub.Referral.Api.Controllers.Admin;

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
    string ubrn,
    ChangeDateOfBirthRequest changeDateOfBirthRequest)
  {
    try
    {
      return Ok(await Service.ChangeDateOfBirthAsync(
        ubrn,
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
  /// <response code="200">Referral updated successfully.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="403">Unprocessable values passed to method</response>
  /// <response code="404">A referral was not found for the provided UBRN or 
  /// it was found but the original mobile provided does not match.
  /// </response>
  /// <response code="409">The referral does not currently have the provided
  /// original mobile.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again.</response>
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
        ex is ArgumentOutOfRangeException)
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
  /// Changes a referral's NHS number.
  /// </summary>
  /// <remarks>
  /// The existing referral's NHS number must match the provided 
  /// original NHS number.
  /// </remarks>
  /// <returns>The referral object once it has been updated.</returns>
  /// <response code="200">Referral updated successfully.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="403">Unprocessable values passed to method.</response>
  /// <response code="404">A referral was not found for the provided UBRN or 
  /// it was found but the original NHS number provided does not match.
  /// </response>
  /// <response code="409">The referral does not currently have the provided
  /// original mobile.</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPatch("ChangeNhsNumber/{ubrn:length(12)}")]
  public async Task<IActionResult> ChangeNhsNumber(
    string Ubrn,
    ChangeNhsNumberRequest changeNhsNumberRequest)
  {
    try
    {
      return Ok(await Service.ChangeNhsNumberAsync(
        Ubrn,
        changeNhsNumberRequest.OriginalNhsNumber,
        changeNhsNumberRequest.UpdatedNhsNumber));
    }
    catch (Exception ex)
    {
      if (ex is ArgumentException ||
        ex is ArgumentOutOfRangeException)
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
  /// Updates a referrals's Sex property.
  /// </summary>
  /// <remarks>
  /// Id, Ubrn and OriginalSex properties must all match the current state of the referral.
  /// </remarks>
  /// <returns>A string confirming the changes made.</returns>
  /// <response code="200">Referral updated successfully.</response>
  /// <response code="400">Missing or invalid paramaters.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="404">A referral was not found that matched Id, Ubrn and OriginalSex.</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPatch("ChangeSex/{id:guid}")]
  public async Task<IActionResult> ChangeSex(Guid id, ChangeSexRequest changeSexRequest)
  {
    try
    {
      return Ok(await Service.ChangeSexAsync(
        id,
        changeSexRequest.OriginalSex,
        changeSexRequest.Ubrn,
        changeSexRequest.UpdatedSex));
    }
    catch (Exception ex)
    {
      if (ex is ArgumentException or ArgumentOutOfRangeException)
      {
        return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
      }

      if (ex is ReferralNotFoundException)
      {
        return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
      }

      LogException(ex);
      return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Fixes Mobile and Telephone Numbers
  /// </summary>
  /// <remarks>
  /// There are some referrals that have their Telephone and Mobiles numbers
  /// in the incorrect fields.  There are also some that have invalid numbers
  /// </remarks>
  /// <returns>A status message on the fix about any changes made.</returns>
  /// <response code="200">Referral updated successfully</response>    
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Unprocessable values passed to method</response>
  /// <response code="404">A referral was not found for the provided UBRN
  /// </response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPatch("FixNumbers/{ubrn:length(12)}")]
  public async Task<IActionResult> FixNumbers(string ubrn)
  {
    try
    {
      return Ok(await Service.FixNumbersAsync(ubrn));
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
  /// Updates referrals that have their eRS UBRN in the ProviderUbrn field
  /// and adds the system generated UBRN into the ProviderUbrn field
  /// </summary>
  /// <returns>The number of referrals that have been updated</returns>
  /// <response code="200">Referrals updated successfully</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  [HttpGet("FixReferralProviderUbrn")]
  public async Task<IActionResult> FixReferralProviderUbrn()
  {
    try
    {
      return (Ok(await Service.FixReferralProviderUbrnAsync()));
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

  /// <summary>
  /// Corrects GP referrals stuck with a status of Letter or LetterSent
  /// </summary>
  /// <remarks>
  /// For all GP referrals that have a status of Letter or LetterSent:<br/>
  /// Status = RejectedToEreferrals
  /// ProgrammeOutcome = DidNotCommence<br/>
  /// </remarks>
  /// <returns>The number of referrals updated.</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet("GPReferralsLetterOrLetterSent")]
  public async Task<IActionResult> GPReferralsLetterOrLetterSent()
  {
    try
    {
      return Ok(await Service.FixGPReferralsWithStatusLetterOrLetterSent());
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
  /// Corrects MSK referrals stuck with a status of RejectedToEreferrals
  /// </summary>
  /// <remarks>
  /// For MSK referrals that have a status of RejectedToEreferrals:<br/>
  /// Status = CancelledByEreferrals
  /// </remarks>
  /// <returns>The number of referrals updated.</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet("MSKReferralsRejectedToEreferrals")]
  public async Task<IActionResult> MSKReferralsRejectedToEreferrals()
  {
    try
    {
      return Ok(await Service.FixMSKReferralsWithStatusRejectedToEreferrals());
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
  /// Corrects Pharmacy referrals stuck with an invalid status
  /// </summary>
  /// <remarks>
  /// For all Pharmacy referrals that have a status of Letter, LetterSent<br/>
  /// or RejectedToEreferrals:<br/>
  /// Status = CancelledByEreferrals
  /// ProgrammeOutcome = DidNotCommence<br/>
  /// </remarks>
  /// <returns>The number of referrals updated.</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet("PharmacyReferralsWithInvalidStatus")]
  public async Task<IActionResult> PharmacyReferralsWithInvalidStatus()
  {
    try
    {
      return Ok(await Service.FixPharmacyReferralsWithInvalidStatus());
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
  /// Corrects Self referrals stuck with a status of Letter, LetterSent,
  /// RejectedToEreferrals or FailedToContact
  /// </summary>
  /// <remarks>
  /// For Self referrals that have a status of Letter or LetterSent:<br/>
  /// ProgrammeOutcome = DidNotCommence<br/>
  /// Status = CancelledByEreferrals<br/>
  /// For Self referrals that have a status of RejectedToEreferrals:<br/>
  /// Status = CancelledByEreferrals<br/>
  /// For Self referrals that have a status of FailedToContact:<br/>
  /// Status = CancelledDueToNonContact<br/>
  /// </remarks>
  /// <returns>The number of referrals updated.</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet("SelfReferralsWithInvalidStatus")]
  public async Task<IActionResult> SelfReferralsWithInvalidStatus()
  {
    try
    {
      return Ok(await Service.FixSelfReferralsWithInvalidStatus());
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
  /// Updates the ProviderUbrn property of referrals where the value is null.
  /// </summary>
  /// <returns>A list of outcomes for referrals without a provider ubrn.
  /// </returns>
  [HttpGet("FixReferralsWithNullProviderUbrn")]
  public async Task<IActionResult> FixReferralsWithNullProviderUbrn()
  {
    try
    {
      return (Ok(await Service.FixReferralsWithNullProviderUbrn()));
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
  /// Updates referrals with invalid statuses to Complete.
  /// </summary>
  /// <returns>
  /// A list of referrals whose status has been updated.
  /// </returns>
  /// <remarks>
  /// For general, msk, pharmacy and self referrals invalid statuses are:
  /// <br/>- CancelledByEreferrals
  /// <br/>- CancelledDueToNonContact
  /// <br/>- CancelledDuplicate
  /// <br/>- FailedToContact
  /// <br/>- Letter
  /// <br/>- LetterSent
  /// <br/>- RejectedToEreferrals<br/>
  /// For GP referrals invalid statuses are:
  /// <br/>- CancelledDueToNonContact
  /// <br/>- CancelledDuplicate
  /// <br/>- DischargeOnHold
  /// <br/>- Letter
  /// <br/>- LetterSent
  /// </remarks>
  [HttpGet("FixReferralsWithInvalidStatuses")]
  public async Task<IActionResult> FixReferralsWithInvalidStatuses()
  {
    try
    {
      return (Ok(await Service.FixReferralsWithInvalidStatuses()));
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
  /// Updates the DateStartedProgramme property of referrals where the value is
  /// null to be equal to the earliest linked ProviderSubmission Date.
  /// </summary>
  /// <returns>
  /// A list of referrals whose status has been updated, including their
  /// updated DateSelectedProgramme value.
  /// </returns>
  [HttpGet("FixReferralsWithMissingDateStartedProgramme")]
  public async Task<IActionResult>
    FixReferralsWithMissingDateStartedProgramme()
  {
    try
    {
      return Ok(await Service.FixReferralsWithMissingDateStartedProgramme());
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
  /// Sets IsErsClosed to false (i.e. reopens the eRS referral) for a GP referral matching the 
  /// provided Id and Ubrn, with a Status of RejectedToEreferrals and IsErsClosed = true.
  /// </summary>
  /// <param name="id">Required. The Guid Id of the referral to be updated.</param>
  /// <param name="request">Required. Request body object containing a single Ubrn property, to
  /// match the Ubrn of the referral to be updated.</param>
  /// <returns>
  /// The updated Referral model.
  /// </returns>
  /// <response code="200">Referral updated.</response>
  /// <response code="400">Invalid or missing Id or Ubrn parameters.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="404">Referral not found matching parameters.</response>
  /// <response code="500">Internal server error.</response>
  [HttpPatch("SetIsErsClosedToFalse/{id:guid}")]
  public async Task<IActionResult> SetIsErsClosedToFalse(
    Guid id,
    SetErsIsClosedToFalseRequest request)
  {
    try
    {
      Business.Models.Referral updatedReferral =
        await Service.SetIsErsClosedToFalse(id, request.Ubrn);

      return Ok(updatedReferral);
    }
    catch (Exception ex)
    {
      LogException(ex);
      int statusCode = StatusCodes.Status500InternalServerError;
      if (ex is ArgumentOutOfRangeException or ArgumentNullOrWhiteSpaceException)
      {
        statusCode = StatusCodes.Status400BadRequest;
      }

      if (ex is ReferralNotFoundException)
      {
        statusCode = StatusCodes.Status404NotFound;
      }

      return Problem(ex.Message, statusCode: statusCode);
    }
  }

  ///<summary>
  /// Set Ethnicity to null for referrals with specified Ids where ServiceUserEthnicity and
  /// ServiceUserEthnicityGroup do not correspond correctly to Ethnicity.
  ///</summary>
  ///<param name="request">Request model containing Ids property.</param>
  ///<returns>A list of Ids of referrals which have been updated.</returns>
  /// <response code="200">Referrals updated.</response>
  /// <response code="400">Malformed, null or empty Ids array.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPost("SetMismatchedEthnicityToNull")]
  public async Task<IActionResult> SetMismatchedEthnicityToNull(
    SetMismatchedEthnicityToNullRequest request)
  {
    try
    {
      return Ok(await Service.SetMismatchedEthnicityToNull(request.Ids));
    }
    catch (Exception ex)
    {
      if (ex is ArgumentNullException or ArgumentOutOfRangeException)
      {
        return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
      }

      LogException(ex);
      return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Update ReferringOrganisationOdsCode for MSK referrals to a different 
  /// code present in the MskOrganisations table.
  /// </summary>
  /// <param name="currentOdsCode">Required. The ReferringOrganisationOdsCode 
  /// to be changed. Must be present in MskOrganisations table.</param>
  /// <param name="request">Required. The value ReferringOrganisationOds
  /// be updated to. Must be present in the MskOrganistions table.</param>
  /// <returns>
  /// A list of Ids of referrals which have been updated.
  /// </returns>
  /// <response code="200">Referrals updated.</response>
  /// <response code="400">Invalid or missing ODS codes</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="404">MskOrganisation not found</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpPatch("UpdateMskReferringOrganisationOdsCode/{currentOdsCode}")]
  public async Task<IActionResult> UpdateMskReferringOrganisationOdsCode(
    string currentOdsCode,
    UpdateMskReferringOrganisationOdsCodeRequest request)
  {
    try
    {
      return Ok(await Service.UpdateMskReferringOrganisationOdsCode(
        currentOdsCode,
        request.NewOdsCode));
    }
    catch (Exception ex)
    {
      LogException(ex);
      if (ex is ArgumentException)
      {
        return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
      }
      else if (ex is MskOrganisationNotFoundException)
      {
        return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status404NotFound);
      }
      else
      {
        return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  protected IReferralAdminService Service
  {
    get
    {
      IReferralAdminService service = _service as IReferralAdminService;
      service.User = User;
      return service;
    }
  }
}
