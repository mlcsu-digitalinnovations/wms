using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services;

public class ReferralQuestionnaireService
  : ServiceBase<ReferralQuestionnaire>, IReferralQuestionnaireService
{
  private const string SMS_RETRY_ATTEMPTS_KEY = "SmsRetryAttempts";
  private const int SMS_RETRY_ATTEMPTS = 1;
  private const string QUESTIONNAIRE_EXPIRY_DAYS_KEY =
    "QuestionnaireExpiryDays";
  
  private const string CREATE_QUESTIONNAIRE_FROM_DATE_KEY =
    "CreateQuestionnaireFromDate";
  private readonly DateTime CREATE_QUESTIONNAIRE_FROM_DATE = 
    new(2021, 4, 1, 23, 59, 59);

  private const string ISRUNNING_CONFIGURATION_KEY_PREFIX =
    "WmsHub_ReferralQuestionnaireService_IsRunning:";

  private readonly IConfiguration _configuration;
  private readonly ILinkIdService _linkIdService;
  private readonly INotificationService _notificationService;
  private readonly QuestionnaireNotificationOptions _options;
  private readonly ILogger _logger;
  private readonly List<ReferralQuestionnaireStatus> _validStartStatus = new()
  {
    { ReferralQuestionnaireStatus.Delivered },
    { ReferralQuestionnaireStatus.Started },
    { ReferralQuestionnaireStatus.Sending }
  };

  public ReferralQuestionnaireService(
    DatabaseContext context,
    IConfiguration configuration,
    ILinkIdService linkIdService,
    IOptions<QuestionnaireNotificationOptions> options,
    INotificationService notificationService,
    ILogger logger)
    : base(context)
  {

    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(linkIdService);
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(notificationService);
    ArgumentNullException.ThrowIfNull(logger);

    _configuration = configuration;
    _linkIdService = linkIdService;
    _options = options.Value;
    _notificationService = notificationService;
    _logger = logger;

    ValidateModelResult validationResult = ValidateModel(_options);
    if (!validationResult.IsValid)
    {
      string message = string.Join(" ",
        validationResult.Results.Select(s => s.ErrorMessage).ToArray());

      _logger.Warning(message);
      throw new ValidationException(message);      
    }
  }

  /// <summary>
  /// Creates ReferralQuestionnaire for all referrals not existing in 
  /// ReferralQuestionnaire table satisfying predicate
  /// ConsentForFutureContactForEvaluation = true and
  /// isActive = true and
  /// DateOfReferral > 01/04/2022 and
  /// has mobile number and has programe outcome and
  /// referral source is GpReferral or SelfReferral.
  /// </summary>
  /// <returns>CreateReferralQuestionnaireResponse 
  /// containing count of number of referral questionnaires created.</returns>
  public async Task<CreateReferralQuestionnaireResponse> CreateAsync(
    DateTimeOffset? fromDate,
    int maxNumberToCreate,
    DateTimeOffset toDate)
  {
    CreateReferralQuestionnaireResponse response = new();

    if (GetIsRunning(nameof(CreateAsync)))
    {
      response.Status = CreateQuestionnaireStatus.Conflict;
      response.Errors.Add("Process is already running.");
      return response;
    }

    try
    { 
      SetIsRunning(nameof(CreateAsync), true);

      DateTime configurationFromDate = _configuration.GetValue(
        CREATE_QUESTIONNAIRE_FROM_DATE_KEY,
        CREATE_QUESTIONNAIRE_FROM_DATE);

      if (fromDate.HasValue && fromDate < configurationFromDate)
      {
        response.Status = CreateQuestionnaireStatus.BadRequest;
        response.Errors.Add("From date must be after " +
          $"{configurationFromDate}.");
      }

      if (!fromDate.HasValue)
      {
        fromDate = configurationFromDate;
      }

      if (toDate < fromDate)
      {
        _logger.Warning($"ToDate must be after {fromDate}.");
        response.Status = CreateQuestionnaireStatus.BadRequest;
        response.Errors.Add($"ToDate must be after {fromDate}.");
      }

      if (maxNumberToCreate < 1 || maxNumberToCreate > 250)
      {
        response.Status = CreateQuestionnaireStatus.BadRequest;
        response.Errors.Add("MaxNumberToCreate must be in the range 1 to 250.");
      }

      if (response.Status == CreateQuestionnaireStatus.BadRequest)
      {
        return response;
      }

      toDate = toDate.Date
        .AddDays(1)
        .AddMilliseconds(-1);
      fromDate = fromDate.Value.Date;

      List<CreateReferralQuestionnaire> questionnairesToCreate
        = await GetQuestionnairesToCreate(
          fromDate.Value, maxNumberToCreate, toDate);

      if (questionnairesToCreate.Count > 0)
      {
        List<Questionnaire> questionnaires = await GetQuestionnaires();

        DateTimeOffset now = DateTimeOffset.Now;

        foreach (CreateReferralQuestionnaire create in questionnairesToCreate)
        {
          QuestionnaireType? questionnaireType
            = create.GetQuestionnaireType();

          if (questionnaireType == null)
          {
            response.Errors.AddRange(create.GetQuestionnaireTypeErrors);
          }
          else
          {
            Questionnaire questionnaire = questionnaires
              .SingleOrDefault(q => q.Type == questionnaireType);

            if (questionnaire == null)
            {
              response.Errors.Add(
                $"Unknown questionnaire type {questionnaireType} for referral " +
                $"{create.Id}.");
            }
            else
            {
              ReferralQuestionnaire referralQuestionnaire = new()
              {
                Created = now,
                Email = create.Email,
                FailureCount = 0,
                FamilyName = create.FamilyName,
                GivenName = create.GivenName,
                IsActive = true,
                Mobile = create.Mobile,
                NotificationKey = await _linkIdService.GetUnusedLinkIdAsync(3),
                QuestionnaireId = questionnaire.Id,
                ReferralId = create.Id,
                Status = ReferralQuestionnaireStatus.Created
              };

              _context.ReferralQuestionnaires.Add(referralQuestionnaire);
              UpdateModified(referralQuestionnaire);

              response.NumberOfQuestionnairesCreated++;
            }
          }
        }

        await _context.SaveChangesAsync();
      }

      return response;
    }
    finally
    {
      SetIsRunning(nameof(CreateAsync), false);
    }
  }

  /// <summary>
  /// Send sms messages for all ReferralQuestionnaire satisfying predicate
  /// isActive = true and
  /// failure count is less than smsRetryAttempts
  /// status in ('Created', 'TechnicalFailure', 'TemporaryFailure').
  /// </summary>
  /// <returns>SendReferralQuestionnaireResponse 
  /// containing count of number of sent message failures,
  /// sent message success or 
  /// no questionnaires smsMessages to send flag (true/false).</returns>
  public async Task<SendReferralQuestionnaireResponse> SendAsync()
  {
    int smsRetryAttempts = _configuration
      .GetConfigValue<int>(SMS_RETRY_ATTEMPTS_KEY, SMS_RETRY_ATTEMPTS);

    List<SmsPostRequest> smsMessages =
      await GetReferralQuestionnairesToSend(smsRetryAttempts);

    if (smsMessages.Count == 0)
    {
      return new SendReferralQuestionnaireResponse
      {
        NoQuestionnairesToSend = true
      };
    }

    int failureCount = 0;
    int successCount = 0;

    foreach (SmsPostRequest smsMessage in smsMessages)
    {
      try
      {
        SmsPostResponse response =
          await _notificationService.SendNotificationAsync(smsMessage);

        if (response.GetNotificationErrors.Any())
        {
          _logger.Warning("Unable to send SMS for client referance: " +
            $"{response.ClientReference} because: " +
            $"{response.GetNotificationErrorsAsString}.");
        }

        await UpdateRefrefererralQuestionnaireStatus(
        smsMessage.ClientReference,
        response.ResponseStatus,
        smsRetryAttempts);

        if (ReferralQuestionnaireStatus.Sending == response.ResponseStatus)
        {
          successCount++;
        }
        else
        {
          failureCount++;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);

        await UpdateRefrefererralQuestionnaireStatus(
          smsMessage.ClientReference,
          ReferralQuestionnaireStatus.TechnicalFailure,
          smsRetryAttempts);
        failureCount++;
      }
    }

    return new SendReferralQuestionnaireResponse
    {
      NumberOfReferralQuestionnairesFailed = failureCount,
      NumberOfReferralQuestionnairesSent = successCount
    };
  }

  /// <summary>
  /// Starts ReferralQuestionnaire sets staus to 'Started'
  /// </summary>
  /// <returns>StartReferralQuestionnaire</returns>
  public async Task<StartReferralQuestionnaire> StartAsync(
    string notificationKey)
  {
    StartReferralQuestionnaire response = new()
    {
      ValidationState = ReferralQuestionnaireValidationState.Valid
    };

    ReferralQuestionnaire referralQuestionnaire = await _context
      .ReferralQuestionnaires
      .Include(r => r.Questionnaire)
      .FirstOrDefaultAsync(rq => rq.NotificationKey == notificationKey);

    if (referralQuestionnaire == null)
    {
      response.ValidationState =
        ReferralQuestionnaireValidationState.NotificationKeyNotFound;
      return response;
    }

    int questionnaireExpiryDays = _configuration
      .GetConfigValue(
        QUESTIONNAIRE_EXPIRY_DAYS_KEY, 
        Constants.QUESTIONNAIRE_EXPIRY_DAYS);

    if (referralQuestionnaire.Delivered.HasValue 
      && referralQuestionnaire.Delivered.Value
        .AddDays(questionnaireExpiryDays) < DateTimeOffset.Now)
    {
      DateTimeOffset expiryDate = referralQuestionnaire.Delivered.Value
        .AddDays(questionnaireExpiryDays);
      
      _logger.Warning("Questionnaire expired for client referance: " +
        $"{referralQuestionnaire.Id} on: {expiryDate}.");
      
      response.ValidationState = ReferralQuestionnaireValidationState.Expired;

      return response;
    }

    if (!_validStartStatus.Contains(referralQuestionnaire.Status))
    {
      response.Status = referralQuestionnaire.Status;
      response.ValidationState =
        referralQuestionnaire.Status == ReferralQuestionnaireStatus.Completed
        ? ReferralQuestionnaireValidationState.Completed
        : ReferralQuestionnaireValidationState.NotDelivered;

      return response;
    }

    referralQuestionnaire.Status = ReferralQuestionnaireStatus.Started;
    referralQuestionnaire.Started = DateTimeOffset.Now;

    UpdateModified(referralQuestionnaire);
    await _context.SaveChangesAsync();

    response.ProviderName = await _context.Referrals
      .Where(x => x.Id == referralQuestionnaire.ReferralId)
      .Select(x => x.Provider.Name)
      .SingleOrDefaultAsync();

    response.FamilyName = referralQuestionnaire.FamilyName;
    response.GivenName = referralQuestionnaire.GivenName;
    response.QuestionnaireType = referralQuestionnaire.Questionnaire.Type;
    response.Status = referralQuestionnaire.Status;

    return response;
  }

  /// <summary>
  /// Completes ReferralQuestionnaire sets status to 'Completed'
  /// <returns>CompleteQuestionnaireResponse</returns>
  public async Task<CompleteQuestionnaireResponse> CompleteAsync(
    CompleteQuestionnaire request)
  {
    CompleteQuestionnaireResponse response = new()
    {
      ValidationState = ReferralQuestionnaireValidationState.Valid
    };

    if (request == null)
    {
      response.ValidationState = 
        ReferralQuestionnaireValidationState.BadRequest;
      response.GetQuestionnaireTypeErrors.Add("Request is null");
      return response;
    }

    ValidateModelResult validateModelResult = ValidateModel(request);
    if (!validateModelResult.IsValid)
    {
      response.ValidationState = 
        ReferralQuestionnaireValidationState.BadRequest;
      response.GetQuestionnaireTypeErrors.Add(
        validateModelResult.GetErrorMessage());
      return response;
    }

    ReferralQuestionnaire referralQuestionnaire = await _context
      .ReferralQuestionnaires
      .Include(r => r.Questionnaire)
      .Where(rq => rq.NotificationKey == request.NotificationKey)
      .SingleOrDefaultAsync();

    if (referralQuestionnaire == null)
    {
      response.ValidationState =
        ReferralQuestionnaireValidationState.NotificationKeyNotFound;
      return response;
    }

    if (request.QuestionnaireType != referralQuestionnaire.Questionnaire.Type)
    {
      response.QuestionnaireType = referralQuestionnaire.Questionnaire.Type;
      response.ValidationState =
        ReferralQuestionnaireValidationState.QuestionnaireTypeIncorrect;
      return response;
    }

    if (referralQuestionnaire.Status != ReferralQuestionnaireStatus.Started)
    {
      response.Status = referralQuestionnaire.Status;
      response.ValidationState =
        ReferralQuestionnaireValidationState.IncorrectStatus;
      return response;
    }

    referralQuestionnaire.Answers = request.Answers;
    referralQuestionnaire.Completed = DateTimeOffset.Now;
    referralQuestionnaire.ConsentToShare = request.ConsentToShare;
    referralQuestionnaire.Email = request.Email;
    referralQuestionnaire.FamilyName = request.FamilyName;
    referralQuestionnaire.GivenName = request.GivenName;
    referralQuestionnaire.Mobile = request.Mobile;
    referralQuestionnaire.Status = ReferralQuestionnaireStatus.Completed;

    UpdateModified(referralQuestionnaire);
    await _context.SaveChangesAsync();

    return response;
  }

  /// <summary>
  /// Sets ReferralQuestionnaire status and statusAt from 
  /// notification callback
  /// <returns>NotificationCallbackStatus</returns>
  public async Task<NotificationCallbackStatus>
    CallbackAsync(NotificationProxyCallback request)
  {
    if (request == null)
    {
      return NotificationCallbackStatus.BadRequest;
    }

    ReferralQuestionnaire referralQuestionnaire =
      await _context.ReferralQuestionnaires
        .SingleOrDefaultAsync(rq =>
          rq.Id.ToString().ToLower() == request.ClientReference.ToLower());

    if (referralQuestionnaire == null)
    {
      _logger.Warning($"Error: not found, " +
        $"for client reference: {request.ClientReference}.");
      return NotificationCallbackStatus.NotFound;
    }

    switch (request.Status)
    {
      case NotificationProxyCallbackRequestStatus.Delivered:
        referralQuestionnaire.Status = ReferralQuestionnaireStatus.Delivered;
        referralQuestionnaire.Delivered = request.StatusAt;
        break;
      case NotificationProxyCallbackRequestStatus.TemporaryFailure:
        referralQuestionnaire.Status =
          ReferralQuestionnaireStatus.TemporaryFailure;
        referralQuestionnaire.TemporaryFailure = request.StatusAt;
        break;
      case NotificationProxyCallbackRequestStatus.TechnicalFailure:
        referralQuestionnaire.Status =
          ReferralQuestionnaireStatus.TechnicalFailure;
        referralQuestionnaire.TechnicalFailure = request.StatusAt;
        break;
      case NotificationProxyCallbackRequestStatus.PermanentFailure:
        referralQuestionnaire.Status =
          ReferralQuestionnaireStatus.PermanentFailure;
        referralQuestionnaire.PermanentFailure = request.StatusAt;
        break;
      default:
        _logger.Warning($"Error: unknown status, " +
          $"for client reference: {request.ClientReference}.");
        return NotificationCallbackStatus.Unknown;
    }

    UpdateModified(referralQuestionnaire);
    await _context.SaveChangesAsync();

    return NotificationCallbackStatus.Success;
  }

  private bool GetIsRunning(string methodName)
  {
    ConfigurationValue value = _context.ConfigurationValues
      .SingleOrDefault(x =>
        x.Id == ISRUNNING_CONFIGURATION_KEY_PREFIX + methodName);

    if (value == null || !bool.TryParse(value.Value, out bool isRunning))
    {
      return SetIsRunning(methodName, false);
    }

    return isRunning;
  }

  private async Task<List<CreateReferralQuestionnaire>>
    GetQuestionnairesToCreate(
    DateTimeOffset fromDate, 
    int maxNumberToCreate,
    DateTimeOffset toDate)
  {
    return await _context.Referrals
      .Where(r => r.ConsentForFutureContactForEvaluation == true)
      .Where(r => r.IsActive)
      .Where(r => r.Status == ReferralStatus.Complete.ToString())
      .Where(r => r.Mobile != null)
      .Where(r => r.ProgrammeOutcome != null)
      .Where(r => (r.ReferralSource == ReferralSource.GpReferral.ToString()
        || r.ReferralSource == ReferralSource.SelfReferral.ToString()))
      .Where(r => r.ReferralQuestionnaire == null)
      .Where(r => r.DateOfReferral > fromDate && r.DateOfReferral < toDate)
      .Take(maxNumberToCreate)
      .Select(r => new CreateReferralQuestionnaire
      {
        Email = r.Email,
        FamilyName = r.FamilyName,
        GivenName = r.GivenName,
        Id = r.Id,
        Mobile = r.Mobile,
        ProgrammeOutcome = r.ProgrammeOutcome,
        ReferralSource = r.ReferralSource,
        TriagedCompletionLevel = r.TriagedCompletionLevel
      })
      .ToListAsync();
  }

  private async Task<List<Questionnaire>> GetQuestionnaires()
  {
    return await _context
      .Questionnaires
      .Select(x => new Questionnaire()
      {
        Id = x.Id,
        Type = x.Type
      })
      .ToListAsync();
  }

  private async Task<List<SmsPostRequest>>
     GetReferralQuestionnairesToSend(int smsRetryAttempts)
  {
    return await _context.ReferralQuestionnaires
      .AsNoTracking()
      .Where(rq => rq.IsActive)
      .Where(rq => rq.FailureCount < smsRetryAttempts)
      .Where(rq =>
        rq.Status == ReferralQuestionnaireStatus.Created ||
        rq.Status == ReferralQuestionnaireStatus.TechnicalFailure ||
        rq.Status == ReferralQuestionnaireStatus.TemporaryFailure)
      .Select((rq) => new SmsPostRequest
      {
        ClientReference = rq.Id.ToString(),
        Mobile = rq.Mobile,
        SenderId = _options.NotificationSenderId,
        Personalisation = new Dictionary<string, dynamic>
        {
          { 
            Constants.NotificationPersonalisations.GIVEN_NAME, 
            rq.GivenName 
          },
          { 
            Constants.NotificationPersonalisations.LINK, 
            $"{_options.NotificationQuestionnaireLinkUrl}{rq.NotificationKey}" 
          }
        },
        TemplateId = rq.Questionnaire.NotificationTemplateId.ToString()
      })
      .ToListAsync();
  }

  private bool SetIsRunning(string methodName, bool isRunning)
  {
    string configurationId = ISRUNNING_CONFIGURATION_KEY_PREFIX + methodName;
    ConfigurationValue configurationValue = _context.ConfigurationValues
      .Where(c => c.Id == configurationId)
      .SingleOrDefault();

    if (configurationValue == null)
    {
      configurationValue = new ConfigurationValue()
      {
        Id = configurationId,
        Value = isRunning.ToString()
      };
      _context.ConfigurationValues.Add(configurationValue);
    }
    else
    {
      configurationValue.Value = isRunning.ToString();
    }

    _context.SaveChanges();
    return isRunning;
  }

  private async Task UpdateRefrefererralQuestionnaireStatus(
    string clientReference,
    ReferralQuestionnaireStatus status,
    int smsRetryAttemps)
  {
    ReferralQuestionnaire referralQuestionnaire =
      await _context.ReferralQuestionnaires.SingleOrDefaultAsync(rq =>
        rq.Id == Guid.Parse(clientReference));

    switch (status)
    {
      case ReferralQuestionnaireStatus.Sending:
        referralQuestionnaire.Status = ReferralQuestionnaireStatus.Sending;
        referralQuestionnaire.Sending = DateTimeOffset.Now;
        break;

      case ReferralQuestionnaireStatus.Created:
        referralQuestionnaire.Status = ReferralQuestionnaireStatus.Created;
        referralQuestionnaire.Created = DateTimeOffset.Now;
        break;

      case ReferralQuestionnaireStatus.TemporaryFailure:
        int failureCount = referralQuestionnaire.FailureCount;
        failureCount++;
        referralQuestionnaire.FailureCount = failureCount;
        if (failureCount >= smsRetryAttemps)
        {
          referralQuestionnaire.Status =
            ReferralQuestionnaireStatus.PermanentFailure;
          referralQuestionnaire.PermanentFailure = DateTimeOffset.Now;
        }
        else
        {
          referralQuestionnaire.Status =
            ReferralQuestionnaireStatus.TemporaryFailure;
          referralQuestionnaire.TemporaryFailure = DateTimeOffset.Now;
        }

        break;

      case ReferralQuestionnaireStatus.TechnicalFailure:
        referralQuestionnaire.Status =
        ReferralQuestionnaireStatus.TechnicalFailure;
        referralQuestionnaire.TechnicalFailure = DateTimeOffset.Now;
        break;

      case ReferralQuestionnaireStatus.PermanentFailure:
        referralQuestionnaire.Status =
        ReferralQuestionnaireStatus.PermanentFailure;
        referralQuestionnaire.PermanentFailure = DateTimeOffset.Now;
        break;
    }

    UpdateModified(referralQuestionnaire);

    await _context.SaveChangesAsync();
  }
}
