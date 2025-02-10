using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models.ReferralQuestionnaire;
using CompleteQuestionnaireRequest =
  WmsHub.Referral.Api.Models.ReferralQuestionnaire.CompleteQuestionnaireRequest;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Policy = AuthPolicies.Questionnaire.POLICY_NAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class QuestionnaireController : BaseController
{
  private readonly IMapper _mapper;
  private readonly ILogger _logger;

  public QuestionnaireController(
    IMapper mapper,
    ILogger logger,
    IReferralQuestionnaireService referralQuestionnaireService)
    : base(referralQuestionnaireService)
  {
    if (referralQuestionnaireService == null)
    {
      throw new ArgumentNullException(
        $"{nameof(referralQuestionnaireService)} is null");
    }
    if (mapper == null)
    {
      throw new ArgumentNullException(
        $"{nameof(mapper)} is null");
    }
    if (logger == null)
    {
      throw new ArgumentNullException(
        $"{nameof(logger)} is null");
    }
    _mapper = mapper;
    _logger = logger.ForContext<QuestionnaireController>();
  }

  /// <summary>
  /// Creates questionnaires for referrals.
  /// </summary>
  /// <remarks>
  /// Creates questionnaires for referrals 
  /// that have consented for future contact for evaluation
  /// based on referral's programme outcome, triage level and referral source
  /// where date of referral > 01/04/2022 upto the provided to date or today.
  /// <param name="request.ToDate">To date: default today.</param>
  /// </remarks>
  /// <response code="200">
  /// On successful creation of referral questionnaires:
  /// NumberOfQuestionnairesCreated: count of questionnaires created.
  /// NumberOfErrors: count of referrals for whom questionnaire creation failed.
  /// Errors: Error messages if any.
  /// </response>
  /// <response code="400">ToDate property is missing, or before 01/04/2022.
  /// </response>
  /// <response code="401">Api key is missing or invalid.</response>
  /// <response code="500">Internal server error.</response>
  [HttpPost]
  [Route("create")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> CreateAsync(
    CreateQuestionnaireRequest request)
  {
    try
    {
      // Set toDate to have the time of 23:59:59
      DateTimeOffset toDate = (request.ToDate ?? DateTimeOffset.Now)
        .Date.AddDays(1).AddMilliseconds(-1);

      CreateReferralQuestionnaireResponse response = await Service.CreateAsync(
        request.FromDate,
        request.MaxNumberToCreate,
        toDate);

      if (response.Status == CreateQuestionnaireStatus.BadRequest)
      {
        return Problem(
          detail: string.Join(", ", response.Errors),
          statusCode: StatusCodes.Status400BadRequest,
          type: CreateQuestionnaireStatus.BadRequest.ToString());
      }

      if (response.Status == CreateQuestionnaireStatus.Conflict)
      {
        return Problem(
          detail: string.Join(", ", response.Errors),
          statusCode: StatusCodes.Status409Conflict,
          type: CreateQuestionnaireStatus.Conflict.ToString());
      }

      if (response.HasNoContent)
      {
        return NoContent();
      }

      return Ok(response);
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
  /// Sends text messages to mobile number of 
  /// newly created questionnaire referral
  /// </summary>
  /// <response code="200">
  /// NumberOfReferralQuestionnairesSent: count of questionnaires sent
  /// NumberOfReferralQuestionnairesFailed: count of questionnaires failed
  /// NoQuestionnairesToSend: true when no questionnaires to send
  /// </response>
  /// <response code="204">No Content: when 0 questionnaires to send</response>
  /// <response code="401">Unauthorized</response>
  /// <response code="500">Internal server error</response>
  [HttpPost]
  [Route("send")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> SendAsync()
  {
    try
    {
      SendReferralQuestionnaireResponse response =
        await Service.SendAsync();

      if (response.NoQuestionnairesToSend)
      {
        return NoContent();
      }
      else
      {
        return Ok(response);
      }
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
  /// Sets the questionnaire status to 'Started'.
  /// </summary>
  /// <response code="200">Returns referral details</response>
  /// <response code="400">BadRequest: invalid request</response>
  /// <response code="404">
  /// NotFound: when questionnaire not found for notification key
  /// </response>
  /// <response code="409">
  /// Conflict with following type
  ///  type: NotDelivered
  ///  type: Completed
  ///  type: Expired
  /// </response>
  /// <response code="401">Unauthorized</response>
  /// <response code="500">Internal server error</response>
  [HttpPost]
  [Route("start")]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(StartReferralQuestionnaire))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> StartAsync(
    StartReferralQuestionnaireRequest startQuestionnaireRequest)
  {
    try
    {
      StartReferralQuestionnaire startReferralQuestionnaire =
        await Service.StartAsync(
          startQuestionnaireRequest.NotificationKey);

      switch (startReferralQuestionnaire.ValidationState)
      {
        case ReferralQuestionnaireValidationState.NotificationKeyNotFound:
          _logger.Warning("NotificationKey {notificationKey} cannot be found.",
            startQuestionnaireRequest.NotificationKey);
        
        return Problem(
            detail: "The notification key could not be found.",
            statusCode: StatusCodes.Status404NotFound,
            type: ReferralQuestionnaireValidationState
              .NotificationKeyNotFound.ToString());

        case ReferralQuestionnaireValidationState.Completed:
          _logger.Warning("Questionnaire status in invalid, expected: " +
            "'Delivered' or 'Started', actual: {actual}.",
              startReferralQuestionnaire.Status.ToString());

          return Problem(
            detail: "The questionnaire has already been completed.",
            statusCode: StatusCodes.Status409Conflict,
            type: ReferralQuestionnaireValidationState.Completed.ToString());

        case ReferralQuestionnaireValidationState.NotDelivered:
          _logger.Warning("Questionnaire status in invalid, expected: " +
            "'Delivered' or 'Started', actual: {actual}.",
              startReferralQuestionnaire.Status.ToString());

          return Problem(
            detail: "The questionnaire has not been delivered.",
            statusCode: StatusCodes.Status409Conflict,
            type: ReferralQuestionnaireValidationState.NotDelivered.ToString());

        case ReferralQuestionnaireValidationState.Expired:
          return Problem(
            detail: "The questionnaire response time has expired.",
            statusCode: StatusCodes.Status409Conflict,
            type: ReferralQuestionnaireValidationState.Expired.ToString());

        default:
          return Ok(startReferralQuestionnaire);
      }
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
  /// Sets the questionnaire status to 'Completed'.
  /// </summary>
  /// <response code="200">Ok</response>
  /// <response code="400">BadRequest: invalid request</response>
  /// <response code="404">
  /// NotFound: when questionnaire not found for notification key
  /// </response>
  /// <response code="409">
  /// Conflict when status not 'Delivered'
  /// </response>
  /// <response code="401">Unauthorized</response>
  /// <response code="500">Internal server error</response>
  [HttpPost]
  [Route("complete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> CompleteAsync(
    CompleteQuestionnaireRequest request)
  {
    try
    {
      CompleteQuestionnaireResponse response = 
        await Service.CompleteAsync(
          _mapper.Map<CompleteQuestionnaire>(request));

      switch (response.ValidationState)
      {
        case ReferralQuestionnaireValidationState.NotificationKeyNotFound:
          _logger.Warning("NotificationKey {notificationKey} cannot be found.",
            request.NotificationKey);

          return Problem(
            detail: "The notification key could not be found.",
            statusCode: StatusCodes.Status404NotFound,
            type: ReferralQuestionnaireValidationState
              .NotificationKeyNotFound.ToString());

        case ReferralQuestionnaireValidationState.QuestionnaireTypeIncorrect:
          _logger.Warning(
            "Questionnaire type is invalid, expected: {expected}" +
              ", actual: {actual}.",
            response.QuestionnaireType.ToString(),
            request.QuestionnaireType);

          return Problem(
            detail: "QuestionnaireType is incorrect.",
            statusCode: StatusCodes.Status400BadRequest,
            type: ReferralQuestionnaireValidationState
              .QuestionnaireTypeIncorrect.ToString());

        case ReferralQuestionnaireValidationState.BadRequest:
          string errorMessage = string.Join(
              ", ",
              response.GetQuestionnaireTypeErrors);
          _logger.Warning(errorMessage);

          return Problem(
            detail: errorMessage,
            statusCode: StatusCodes.Status400BadRequest,
            type: ReferralQuestionnaireValidationState.BadRequest.ToString());

        case ReferralQuestionnaireValidationState.IncorrectStatus:
          _logger.Warning(
            "Questionnaire status in invalid, expected: 'Started' actual: " +
              "{actual}.",
            response.Status.ToString());

          return Problem(
            detail: "The questionnaire does not have the correct status.",
            statusCode: StatusCodes.Status409Conflict,
            type: ReferralQuestionnaireValidationState
              .IncorrectStatus.ToString());

        default:
          return Ok();
      }
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
  /// Callback for notification proxy with text message sent status.
  /// Sets the status and status datetime of questionnaire from request.
  /// </summary>
  /// <response code="200">Ok</response>
  /// <response code="400">BadRequest: invalid request</response>
  /// <response code="404">
  /// NotFound: when questionnaire not found for client reference or
  ///  Status not in 
  ///  'Delivered',
  ///  'PermanentFailure',
  ///  'TechnicalFailure',
  ///  'TemporaryFailure'
  /// </response>
  /// <response code="401">Unauthorized</response>
  /// <response code="500">Internal server error</response>
  [HttpPost]
  [Route("callback")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> CallbackAsync(
    NotificationProxyCallbackRequest notificationProxyCallbackRequest)
  {
    try
    {
      NotificationCallbackStatus status =
        await Service.CallbackAsync(
          _mapper.Map<NotificationProxyCallback>(
              notificationProxyCallbackRequest));

      switch (status)
      {
        case NotificationCallbackStatus.NotFound:
          return Problem(statusCode: StatusCodes.Status404NotFound);
        case NotificationCallbackStatus.BadRequest:
          return Problem(detail: "Request is null",
            statusCode: StatusCodes.Status400BadRequest);
        case NotificationCallbackStatus.Unknown:
          return Problem(detail: "Unknown status",
            statusCode: StatusCodes.Status400BadRequest);
        default:
          return Ok();
      }
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private IReferralQuestionnaireService Service
  {
    get
    {
      var service = _service as IReferralQuestionnaireService;
      service.User = User;
      return service;
    }
  }
}
