using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Business.Enums.ReferralStatus;
using static WmsHub.Common.Helpers.Constants;
using static WmsHub.Common.Helpers.Constants.MessageTemplateConstants;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace WmsHub.Business.Services;

public class TextService
  : ServiceBase<Entities.Referral>, IDisposable, ITextService
{  
  private readonly ITextNotificationHelper _helper;
  private readonly TextOptions _settings;
  private readonly IDateTimeProvider _dateTimeProvider;
  private readonly ILinkIdService _linkIdService;

  public TextService() : base(null)
  { }

  public TextService(IOptions<TextOptions> settings,
    ITextNotificationHelper helper,
    DatabaseContext context,
    IDateTimeProvider dateTimeProvider,
    ILinkIdService linkIdService) : base(context)
  {
    _settings = settings.Value;
    _helper = helper;
    _dateTimeProvider = dateTimeProvider;
    _linkIdService = linkIdService;
  }

  /// <summary>
  /// Async method to send the SMS message to the Notify Service
  /// </summary>
  /// <param name="smsMessage"></param>
  /// <returns>Notify.SmsNotificationResponse</returns>
  public virtual async Task<SmsNotificationResponse> SendSmsMessageAsync(
    ISmsMessage smsMessage)
  {
    ValidationContext context = new(instance: smsMessage);

    List<ValidationResult> results = new();

    bool isValid = Validator.TryValidateObject(
      smsMessage, context, results, validateAllProperties: true);

    if (!isValid)
    {
      throw new ValidationException(
        string.Join(" ", results.Select(s => s.ErrorMessage).ToArray()));
    }

    SmsNotificationResponse response = await _helper.TextClient.SendSmsAsync(
      mobileNumber: smsMessage.MobileNumber,
      templateId: smsMessage.TemplateId,
      personalisation: smsMessage.Personalisation,
      clientReference: smsMessage.ClientReference,
      smsSenderId: _settings.SmsSenderId);

    return response;
  }

  public virtual async Task<int> PrepareMessagesToSend(
    Guid referralId = default)
  {
    bool canRunPrepare = GetCanRun(MessageServiceConstants.CONFIG_TEXT_TIME);
    if (!canRunPrepare)
    {
      DateTimeOffset lastRunTime = GetLastRunTime();
      TimeSpan lastRunOffset = _dateTimeProvider.UtcNow - lastRunTime;
      DateTimeOffset nextValidRun =
        lastRunTime.AddMinutes(_settings.PrepareMessageDelayMinutes);

      throw new ProcessAlreadyRunningException(
        $"The prepare text messages process was run {lastRunOffset.Minutes} minutes ago " +
        $"and can't be re-run until {nextValidRun}.");
    }

    SetLastRun(MessageServiceConstants.CONFIG_TEXT_TIME);

    IEnumerable<Guid> referralIds = await GetPreSelectionReferralIdQuery()
      .Union(GetPostRmcReferralIdQuery())
      .Union(GetPostSelectionReferralIdQuery())
      .ToListAsync();

    if (referralId != default)
    {
      referralIds = referralIds.Where(r => r == referralId);
    }

    List<Entities.Referral> referrals = await _context
      .Referrals
      .Include(r => r.TextMessages)
      .Where(r => referralIds.Contains(r.Id))
      .ToListAsync();

    List<Entities.TextMessage> textMessages = new(referrals.Count);

    IEnumerable<Guid> referralIdsRequiringNewLinkId = referrals
      .Where(r => !r.TextMessages
        .Where(t => t.IsActive)
        .Where(t => t.ServiceUserLinkId != null)
        .Any())
      .Select(r => r.Id);

    Queue<string> linkIds = null;

    if (referralIdsRequiringNewLinkId.Any())
    {
      try
      {
        linkIds = new Queue<string>(await _linkIdService
          .GetUnusedLinkIdBatchAsync(referralIdsRequiringNewLinkId.Count(), 3));
      }
      catch (Exception ex)
      {
        if (ex is ProcessAlreadyRunningException)
        {
          DateTimeOffset nextRun = _dateTimeProvider.UtcNow
            .AddMinutes(_settings.PrepareMessageDelayMinutes);
          throw new ProcessAlreadyRunningException("The prepare text messages process cannot be" +
            "completed due to a conflict with the link id generation process. " +
            $"Try again after {nextRun}.");
        }
        else
        {
          throw;
        }
      }
    }

    int createdTextMessages = 0;
    foreach (Entities.Referral referral in referrals)
    {
      if (!referral.Mobile.IsUkMobile()
        || (referral.IsMobileValid.HasValue && referral.IsMobileValid.Value == false))
      {
        referral.StatusReason = "Mobile number is not a valid mobile number";
        referral.IsMobileValid = false;

        if (referral.Status == New.ToString() || referral.Status == TextMessage1.ToString())
        {
          // Cannot send text messages due to invalid mobile, so advance status to TextMessage2
          // to be picked up by ChatBotService PrepareCallsAsync().
          referral.Status = TextMessage2.ToString();
        }
      }
      else
      {
        createdTextMessages++;
        AdvanceReferralStatus(referral);

        string serviceUserLinkId;

        if (referralIdsRequiringNewLinkId.Contains(referral.Id))
        {
          serviceUserLinkId = linkIds.Dequeue();
        }
        else
        {
          Entities.TextMessage earliestTextMessage = referral.TextMessages
          .Where(t => t.IsActive)
          .Where(t => t.ServiceUserLinkId != null)
          .OrderBy(t => t.ServiceUserLinkId)
          .First();

          serviceUserLinkId = earliestTextMessage.ServiceUserLinkId;
        }

        Entities.TextMessage textMessage = new()
        {
          ServiceUserLinkId = serviceUserLinkId,
          IsActive = true,
          Number = referral.Mobile,
          ReferralId = referral.Id,
          ReferralStatus = referral.Status
        };
        UpdateModified(textMessage);
        textMessages.Add(textMessage);
        _context.TextMessages.Add(textMessage);
      }

      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return createdTextMessages;
  }

  /// <summary>
  /// GetCanRun is used to get a config value to check to see if method can be 
  /// run.  If the method id does not exist then a value is added and bool
  /// value is set true.
  /// </summary>
  /// <param name="configId"></param>
  /// <returns></returns>
  private bool GetCanRun(string configId)
  {
    ConfigurationValue value = _context.ConfigurationValues
      .SingleOrDefault(x => x.Id == configId);

    if (value == null
      || !DateTimeOffset.TryParse(value.Value, out DateTimeOffset result))
    {
      return SetLastRun(configId);
    }

    return _dateTimeProvider.UtcNow > result.AddMinutes(_settings.PrepareMessageDelayMinutes);
  }

  private IQueryable<Guid> GetPostRmcReferralIdQuery()
  {
    DateTimeOffset latestAllowedCallDate = _dateTimeProvider.UtcNow
      .AddHours(-_settings.MessageTimelineOptions.MinHoursSincePreviousContactToSendTextMessage3);

    string[] chatBotStatuses = [ChatBotCall1.ToString(), ChatBotTransfer.ToString()];

    IQueryable<Entities.Referral> noCallsAfterLatestAllowedCallDateReferrals = _context
      .Referrals
      .Include(r => r.TextMessages)
      .Include(r => r.Calls)
      .Where(r => r.IsActive)
      .Where(r => !r.Calls
        .Any(c => c.IsActive && (c.Sent.Date > latestAllowedCallDate || c.Sent == default)))
      .Where(r => r.ProviderId == null);

    IQueryable<Entities.Referral> chatBotReferrals = noCallsAfterLatestAllowedCallDateReferrals
      .Where(r => chatBotStatuses.Contains(r.Status));

    IQueryable<Entities.Referral> rmcDelayedReferrals = noCallsAfterLatestAllowedCallDateReferrals
      .Where(r => r.Status == RmcDelayed.ToString())
      .Where(r => r.DateToDelayUntil < _dateTimeProvider.UtcNow);

    IQueryable<Entities.Referral> combinedReferrals = chatBotReferrals.Concat(rmcDelayedReferrals);

    DateTimeOffset earliestAllowedDateOfFirstContact = _dateTimeProvider.UtcNow
      .AddDays(-_settings.MessageTimelineOptions.MaxDaysSinceInitialContactToSendTextMessage3);

    IQueryable<Guid> referralIdsWithTextMessage1AfterEarliestAllowed =
      combinedReferrals
        .Where(r => r.TextMessages
          .Where(t => t.IsActive)
          .Where(t => t.ReferralStatus == TextMessage1.ToString())
          .Where(t => t.Sent != default)
          .Where(t => t.Sent >= earliestAllowedDateOfFirstContact)
          .Any())
        .Select(r => r.Id);

    IQueryable<Guid> referralIdsWithInitialCallAfterEarliestAllowed =
      combinedReferrals
        .Where(r => r.TextMessages.Count == 0)
        .Where(r => r.Calls
          .Where(c => c.IsActive)
          .Where(c => c.Sent != default)
          .Where(c => c.Sent >= earliestAllowedDateOfFirstContact)
          .Any())
        .Select(r => r.Id);

    return referralIdsWithInitialCallAfterEarliestAllowed
      .Concat(referralIdsWithTextMessage1AfterEarliestAllowed);
  }

  private IQueryable<Guid> GetPostSelectionReferralIdQuery()
  {
    string[] postSelectionStatuses =
    [
      CancelledDuplicateTextMessage.ToString(),
      FailedToContactTextMessage.ToString(),
      ProviderDeclinedTextMessage.ToString(),
      ProviderRejectedTextMessage.ToString(),
      ProviderTerminatedTextMessage.ToString()
    ];

    return _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => postSelectionStatuses.Contains(r.Status))
      .Where(r => !r.TextMessages
        .Where(tm => tm.IsActive)
        .Where(tm => tm.Outcome != TEXT_MESSAGE_FAILED)
        .Where(tm => tm.ReferralStatus == r.Status)
        .Any())
      .Select(r => r.Id);
  }

  private IQueryable<Guid> GetPreSelectionReferralIdQuery()
  {
    DateTimeOffset newCutoff = _dateTimeProvider.UtcNow
      .AddHours(-_settings.MessageTimelineOptions.MinHoursSincePreviousContactToSendTextMessage1)
      .Date;

    DateTimeOffset textMessage1Cutoff = _dateTimeProvider.UtcNow
      .AddHours(-_settings.MessageTimelineOptions.MinHoursSincePreviousContactToSendTextMessage2)
      .Date;

    IQueryable<Entities.Referral> baseQuery = _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId == null);

    IQueryable<Entities.Referral> newReferralsQuery = baseQuery
      .Where(r => r.Status == New.ToString())
      .Where(r => !r.TextMessages
        .Where(tm => tm.IsActive)
        .Where(tm => tm.Outcome != TEXT_MESSAGE_FAILED)
        .Where(tm => tm.Sent.Date > newCutoff || tm.Sent == default)
        .Any());

    IQueryable<Entities.Referral> textMessage1ReferralsQuery = baseQuery
      .Where(r => r.Status == TextMessage1.ToString())
      .Where(r => !r.TextMessages
        .Where(tm => tm.IsActive)
        .Where(tm => tm.Outcome != TEXT_MESSAGE_FAILED)
        .Where(tm => tm.Sent.Date > textMessage1Cutoff || tm.Sent == default)
        .Any());

    return newReferralsQuery.Concat(textMessage1ReferralsQuery).Select(r => r.Id);
  }

  private bool SetLastRun(string configId)
  {
    ConfigurationValue configurationValue = _context.ConfigurationValues
      .Where(t => t.Id == configId)
      .SingleOrDefault();

    if (configurationValue == null)
    {
      configurationValue = new ConfigurationValue()
      {
        Id = configId,
        Value = _dateTimeProvider.UtcNow.ToString(CultureInfo.CurrentCulture)
      };
      _context.ConfigurationValues.Add(configurationValue);
    }
    else
    {
      configurationValue.Value = _dateTimeProvider.UtcNow
        .ToString(CultureInfo.CurrentCulture);
    }

    _context.SaveChanges();
    return true;
  }

  /// <summary>
  /// SetCanRun sets the method or creates the value if the Id does not exist
  /// to validate if run.
  /// </summary>
  /// <param name="configId"></param>
  /// <param name="canRun"></param>
  public virtual async Task SetCanRun(string configId, bool canRun)
  {
    ConfigurationValue value = await _context.ConfigurationValues
     .SingleOrDefaultAsync(x => x.Id == configId);

    if (value == null)
    {
      value = new ConfigurationValue()
      {
        Id = configId
      };
      _context.ConfigurationValues.Add(value);
    }

    value.Value = canRun ? "true" : "false";
    await _context.SaveChangesAsync();

  }

  /// <summary>
  /// This updates the individual request. This needs to be run before any 
  /// callback is generated by the notify, or the Id and Referal may not
  /// be found in the WmsHub. Outcome of Text Message must be set to SENT. 
  /// </summary>
  /// <param name="request"></param>
  /// <param name="outcome"></param>
  /// <returns></returns>
  public virtual async Task UpdateMessageRequestAsync(
    ISmsMessage request,
    string outcome = Constants.TEXT_MESSAGE_SENT)
  {
    Entities.Referral referral = await GetReferralWithTextMessage(
      request.LinkedTextMessage.Value)
      ?? throw new ArgumentNullException(
        $"Referral not found using the Text Message Id {request.LinkedTextMessage}");

    Entities.TextMessage textMessage = referral.TextMessages
      .SingleOrDefault(c => c.Id == request.LinkedTextMessage.Value)
      ?? throw new ArgumentException("Unable to find a TextMessage id of {request.LinkedTextMessage}");

    textMessage.Outcome = outcome;
    textMessage.Sent = request.Sent;

    referral.MethodOfContact = (int)MethodOfContact.TextMessage;

    if (outcome == TEXT_MESSAGE_SENT)
    {
      referral.NumberOfContacts++;
    }

    if (referral.Status.Is(CancelledDuplicateTextMessage)
      || referral.Status.Is(FailedToContactTextMessage))
    {
      referral.Status = Complete.ToString();
    }
    else if (referral.Status.Is(ProviderDeclinedTextMessage)
      || referral.Status.Is(ProviderRejectedTextMessage))
    {
      // Status from a provider type text message should be updated to:
      // Complete if the referral is a non-GP referral
      // ProviderCompleted if the referral is a GP referral
      if (referral.ReferralSource.Is(ReferralSource.GpReferral))
      {
        referral.Status = ProviderCompleted.ToString();
      }
      else
      {
        referral.Status = Complete.ToString();
      }
    }
    else if (referral.Status.Is(ProviderTerminatedTextMessage))
    {
      referral.Status = ProviderTerminated.ToString();
    }
    else if (!referral.Status.Is(TextMessage1)
      && !referral.Status.Is(TextMessage2)
      && !referral.Status.Is(TextMessage3))
    {
      throw new InvalidOperationException(
        $"When updating text messages, referral {referral.Id} was found " +
        $"with an unexpected status of {referral.Status}.");
    }

    UpdateModified(textMessage);
    UpdateModified(referral);

    await _context.SaveChangesAsync();
  }

  public virtual async Task<bool> AddNewMessageAsync(TextMessageRequest message)
  {

    ArgumentNullException.ThrowIfNull(message);

    try
    {
      Entities.TextMessage tm = new(message)
      {
        ServiceUserLinkId = await _linkIdService.GetUnusedLinkIdAsync(3)
      };

      UpdateModified(tm);

      Entities.Referral referral = await _context.Referrals
        .Include(r => r.TextMessages)
        .Where(r => r.IsActive && r.Id == message.ReferralId)
        .FirstOrDefaultAsync()
        ?? throw new ReferralNotFoundException(message.ReferralId);

      referral.TextMessages ??= new();

      referral.TextMessages.Add(tm);

      return await _context.SaveChangesAsync() > 0;
    }
    catch (Exception ex)
    {
      if (ex is ProcessAlreadyRunningException)
      {
        throw new ProcessAlreadyRunningException("AddNewMessageAsync could not be completed " +
          $"due to a conflict with the link id generation process. Try again shortly.");
      }
      else
      {
        throw;
      }
    }
  }

  public virtual async Task<IEnumerable<ISmsMessage>> GetMessagesToSendAsync(
    int? limit = null)
  {
    string[] statusFilter = new string[]
    {
      CancelledDuplicateTextMessage.ToString(),
      FailedToContactTextMessage.ToString(),
      ProviderDeclinedTextMessage.ToString(),
      ProviderRejectedTextMessage.ToString(),
      ProviderTerminatedTextMessage.ToString(),
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      TextMessage3.ToString()
    };

    List<SmsMessage> smsMessages = await _context
      .TextMessages
      .AsNoTracking()
      .Where(t => t.IsActive)
      .Where(t => statusFilter.Contains(t.Referral.Status))
      .Where(t => t.Referral.IsActive)
      .Where(t => t.Sent == default)
      .Take(limit ?? 10000)
      .Select(t => new SmsMessage()
      {
        ServiceUserLinkId = t.ServiceUserLinkId,
        ClientReference = t.Id.ToString(),
        LinkedTextMessage = t.Id,
        MobileNumber = t.Number,
        Personalisation = new Dictionary<string, dynamic>
        {
          {"givenName", t.Referral.GivenName},
          {
            "referralSourceDescription",
            GetReferralSourceDescriptionPersonalisation(t.Referral.ReferralSource, t.Referral.Status)
          },
          {"expiryDate", GetExpiryDatePersonalisation(t.Referral)}
        },
        ReferralSource = t.Referral.ReferralSource,
        ReferralStatus = t.Referral.Status
      })
      .ToListAsync();

    foreach (SmsMessage smsMessage in smsMessages)
    {
      smsMessage.TemplateId = GetReferralTemplateId(smsMessage.ReferralStatus);
    }

    return smsMessages;
  }

  public virtual async Task<ISmsMessage>
      GetMessageByReferralIdToSendAsync(Guid referralId)
  {
    Entities.Referral referral = await
      GetReferralWithTextMessageByReferralId(referralId);

    SmsMessage smsMessage = new();
    try
    {
      if (referral == null)
      {
        throw new ReferralNotFoundException("No referral with Id " +
          $"{referralId} has text messages to send.");
      }

      Entities.TextMessage textMessage = referral.TextMessages
        .OrderBy(tm => tm.Sent)
        .FirstOrDefault();

      if (textMessage != null)
      {
        //validate Mobile number
        if (!textMessage.Number.IsUkMobile())
        {
          throw new ReferralInvalidStatusException(
            $"Number failed validation");
        }

        smsMessage.ClientReference = textMessage.Id.ToString();
        smsMessage.MobileNumber = textMessage.Number
          .ConvertToUkMobileNumber(false);
        smsMessage.Personalisation = new Dictionary<string, dynamic>
        {
          {"givenName", referral.GivenName},
          {"familyName", referral.FamilyName},
        };
        smsMessage.TemplateId = GetReferralTemplateId(referral);
        smsMessage.LinkedTextMessage = textMessage.Id;
      }
    }
    catch (ReferralInvalidStatusException ex)
    {
      referral.Status = ReferralStatus.Exception.ToString();
      referral.StatusReason = ex.Message;

      await _context.SaveChangesAsync();
    }

    return smsMessage;
  }

  protected string GetReferralTemplateId(Entities.Referral referral)
  {
    return GetReferralTemplateId(referral.Status);
  }

  protected string GetReferralTemplateId(string referralStatus)
  {
    string templateId = null;

    if (Enum.TryParse(referralStatus, out ReferralStatus referralStatusEnum))
    {
      templateId = referralStatusEnum switch
      {
        TextMessage1 =>
          _settings.GetTemplateIdFor(TEMPLATE_DYNAMIC_SOURCE_REFERRAL_FIRST).ToString(),
        TextMessage2 =>
          _settings.GetTemplateIdFor(TEMPLATE_DYNAMIC_SOURCE_REFERRAL_SECOND).ToString(),
        TextMessage3 =>
          _settings.GetTemplateIdFor(TEMPLATE_DYNAMIC_SOURCE_REFERRAL_THIRD).ToString(),
        FailedToContactTextMessage =>
          _settings.GetTemplateIdFor(TEMPLATE_FAILEDTOCONTACT_SMS).ToString(),
        ProviderDeclinedTextMessage =>
          _settings.GetTemplateIdFor(TEMPLATE_NONGP_DECLINED).ToString(),
        ProviderRejectedTextMessage =>
          _settings.GetTemplateIdFor(TEMPLATE_NONGP_REJECTED).ToString(),
        ProviderTerminatedTextMessage =>
          _settings.GetTemplateIdFor(TEMPLATE_NONGP_TERMINATED).ToString(),
        CancelledDuplicateTextMessage =>
          _settings.GetTemplateIdFor(TEMPLATE_SELF_CANCELLEDDUPLICATE).ToString(),
        _ => null
      };
    }

    return templateId
      ?? throw new ReferralInvalidStatusException("Expected a referral status of " +
        $"{TextMessage1}, {TextMessage2}, {TextMessage3}, {FailedToContactTextMessage}, " +
        $"{ProviderRejectedTextMessage} or {ProviderTerminatedTextMessage} but found " +
        $"{referralStatus}.");
  }

  /// <summary>
  /// to recieve a callback text message a there must be a corresponding 
  /// referral from which a message was firts sent.
  /// Send message --> add TextMessage to Referral --> Receive callback 
  /// message --> update textmessage
  /// </summary>

  public virtual async Task<CallbackResponse> CallBackAsync(
    ICallbackRequest request)
  {

    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    ValidateModelResult result = ValidateModel(request);

    CallbackResponse response = new(request);

    if (result.IsValid)
    {

      Entities.Referral referral = await GetReferralWithTextMessage(request);

      if (ValidateCallbackCanBeUpdated(referral, request, response))
      {
        await UpdateReferralAsync(referral, request);
        response.SetStatus(StatusType.Valid);
      }
    }
    else
    {
      response.SetStatus(StatusType.Invalid, result.GetErrorMessage());
    }

    return response;

  }

  /// <summary>
  /// this is called if the callback has a status of 'permanent-failure'.
  /// it updates the individual referral IsMobileValid to false 
  /// and Status to TextMessage2
  /// </summary>
  /// <param name="requests"></param>
  /// <returns>response object</returns>
  public virtual async Task<CallbackResponse>
    ReferralMobileNumberInvalidAsync(ICallbackRequest request)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    ValidateModelResult result = ValidateModel(request);

    CallbackResponse response = new(request);

    if (result.IsValid)
    {
      if (Guid.TryParse(request.Reference, out Guid textMessageId))
      {
        Entities.TextMessage textMessage = await _context
          .TextMessages
          .Where(t => t.IsActive)
          .Where(r => r.Id == textMessageId)
          .SingleOrDefaultAsync()
          ?? throw new ReferralNotFoundException(
            $"Unable to find a Referral that has a TextMessage Id of {textMessageId}");

        Entities.Referral referral = await _context
          .Referrals
          .Where(t => t.IsActive)
          .Where(r => r.Id == textMessage.ReferralId)
          .SingleOrDefaultAsync()
          ?? throw new ReferralNotFoundException(
            $"Unable to find a Referral that has a an Id of {textMessage.ReferralId}");

        //set status and valid mobile fields for referral
        referral.IsMobileValid = false;
        if (referral.Status == TextMessage1.ToString())
        {
          referral.Status = TextMessage2.ToString();
        }

        string[] postChatBotStatuses =
        {
          TextMessage3.ToString(),
          ProviderDeclinedTextMessage.ToString(),
          ProviderRejectedTextMessage.ToString(),
          ProviderTerminatedTextMessage.ToString()
        };

        if (postChatBotStatuses.Contains(referral.Status))
        {
          referral.Status = FailedToContact.ToString();
          referral.ProgrammeOutcome = ProgrammeOutcome.InvalidContactDetails.ToString();
        }

        textMessage.Outcome = CallbackStatus.PermanentFailure.ToString();
        textMessage.Received = DateTimeOffset.UtcNow;

        UpdateModified(textMessage);

        UpdateModified(referral);
        await _context.SaveChangesAsync();
      }
      else
      {
        response.SetStatus(StatusType.Invalid, "TextMessage Id is not a Guid.");
      }
    }
    else
    {
      response.SetStatus(StatusType.Invalid, result.GetErrorMessage());
    }

    return response;
  }

  protected internal async Task UpdateReferralAsync(
    Entities.Referral referral, ICallbackRequest request)
  {
    if (referral == null)
    {
      throw new ArgumentNullException(nameof(referral));
    }

    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    if (!Guid.TryParse(request.Reference, out Guid textMessageId))
    {
      throw new ArgumentException(nameof(request.Reference));
    }

    Entities.TextMessage requestedText =
      referral.TextMessages.FirstOrDefault(c => c.Id == textMessageId)
      ?? throw new ReferralNotFoundException(
         $"Unable to find a referral that has a TextMessage id of {request.Reference}");

    if (request.StatusValue == CallbackStatus.Delivered)
    {
      referral.IsMobileValid = true;
    }

    requestedText.Update(request);
    requestedText.ReferralId = referral.Id;
    UpdateModified(requestedText);
    UpdateModified(referral);

    await _context.SaveChangesAsync();

  }

  protected virtual bool ValidateCallbackCanBeUpdated
    (Entities.Referral referral,
    ICallbackRequest request,
    CallbackResponse response)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    if (response == null)
    {
      throw new ArgumentNullException(nameof(response));
    }

    bool validationResult = false;

    if (referral == null)
    {
      response.SetStatus(StatusType.UnableToFindReferral);
    }
    else
    {
      if (referral.TextMessages.Any(c => c.Number == request.To))
      {
        validationResult = true;
      }
      else
      {
        response.SetStatus(StatusType.TelephoneNumberMismatch);
      }
    }

    return validationResult;
  }

  protected internal async Task<Entities.Referral>
    GetReferralWithTextMessage(ICallbackRequest request)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    if (!Guid.TryParse(request.Reference, out Guid id))
    {
      throw new ArgumentException(nameof(request.Reference));
    }

    return await GetReferralWithTextMessage(id);
  }

  protected internal virtual async Task<Entities.Referral>
    GetReferralWithTextMessage(Guid id)
  {

    Entities.Referral referral = await _context
      .Referrals
      .Include(r => r.TextMessages)
      .Where(r => r.TextMessages.Any(c => c.Id == id))
      .Where(r => r.IsActive)
      .FirstOrDefaultAsync();

    return referral;
  }

  protected internal async Task<Entities.Referral>
    GetReferralWithTextMessageByReferralId(Guid referralId)
  {

    string[] statusFilter = new string[]
    {
      TextMessage1.ToString(),
      TextMessage2.ToString(),
      TextMessage3.ToString()
    };

    Entities.Referral referral = await _context
      .Referrals
      .Include(r => r.TextMessages
         .Where(t => t.IsActive && t.Sent == default))
      .Where(r => r.Id == referralId)
      .Where(r => r.IsActive)
      .Where(r => statusFilter.Contains(r.Status))
      .Where(r => r.TextMessages.Any(t => t.IsActive && t.Sent == default))
      .FirstOrDefaultAsync();

    return referral;
  }

  public async Task<CallbackResponse> IsCallBackAsync(CallbackRequest request)
  {
    try
    {
      if (request.StatusValue == CallbackStatus.PermanentFailure)
      {
        //update referral
        return await ReferralMobileNumberInvalidAsync(request);
      }
      else
      {
        return await CallBackAsync(request);
      }
    }
    catch (Exception)
    {
      throw;
    }
  }

  private static void AdvanceReferralStatus(Entities.Referral referral)
  {
    string[] textMessageInitialStatuses =
    {
      New.ToString(),
      TextMessage1.ToString(),
      ChatBotCall1.ToString(),
      ChatBotTransfer.ToString(),
      CancelledDuplicateTextMessage.ToString(),
      FailedToContactTextMessage.ToString(),
      ProviderDeclinedTextMessage.ToString(),
      ProviderRejectedTextMessage.ToString(),
      ProviderTerminatedTextMessage.ToString(),
      RmcDelayed.ToString()
    };

    if (!textMessageInitialStatuses.Contains(referral.Status))
    {
      throw new InvalidOperationException(
        $"When preparing text messages, referral {referral.Id} was found " +
        $"with an unexpected status of {referral.Status}.");
    }

    referral.Status = referral.Status switch
    {
      nameof(New) => nameof(TextMessage1),
      nameof(TextMessage1) => nameof(TextMessage2),
      nameof(ChatBotCall1) => nameof(TextMessage3),
      nameof(ChatBotTransfer) => nameof(TextMessage3),
      nameof(RmcDelayed) => nameof(TextMessage3),
      _ => referral.Status
    };
  }

  private static string GetExpiryDatePersonalisation(Entities.Referral referral)
  {
    ArgumentNullException.ThrowIfNull(referral);

    if (!referral.DateOfReferral.HasValue)
    {
      throw new ArgumentException("DateOfReferral is empty.", nameof(referral));
    }

    return referral.DateOfReferral.Value
      .AddDays(MAX_DAYS_UNTIL_FAILEDTOCONTACT)
      .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
  }

  private DateTimeOffset GetLastRunTime()
  {
    return DateTimeOffset.Parse(
          _context.ConfigurationValues
            .Single(v => v.Id == MessageServiceConstants.CONFIG_TEXT_TIME).Value,
            CultureInfo.CurrentCulture);
  }

  private static string GetReferralSourceDescriptionPersonalisation(
    string referralSource,
    string referralStatus)
  {
    string personalisation = null;

    if (!Enum.TryParse(referralSource, out ReferralSource source))
    {
      throw new ArgumentException(
        $"{referralSource} is not a valid ReferralSource.",
        nameof(referralSource));
    }

    if (!Enum.TryParse(referralStatus, out ReferralStatus status))
    {
      throw new ArgumentException(
        $"{referralStatus} is not a valid ReferralStatus.",
        nameof(referralStatus));
    }

    if (status == TextMessage1 || status == TextMessage2)
    {
      personalisation = source switch
      {
        ReferralSource.ElectiveCare => "a referral from your hospital",
        ReferralSource.GeneralReferral => "your recent sign up",
        ReferralSource.GpReferral => "your conversation with your GP",
        ReferralSource.Msk => "your conversation with your Physiotherapist",
        ReferralSource.Pharmacy => "your conversation with your Pharmacist",
        ReferralSource.SelfReferral => "your recent sign up",
        _ => null
      };
    }
    else if (status == TextMessage3)
    {
      personalisation = source switch
      {
        ReferralSource.ElectiveCare => "referral from your hospital",
        ReferralSource.GeneralReferral => "recent sign up",
        ReferralSource.GpReferral => "referral from your GP",
        ReferralSource.Msk => "referral from your Physiotherapist",
        ReferralSource.Pharmacy => "referral from your Pharmacist",
        ReferralSource.SelfReferral => "recent sign up",
        _ => null
      };
    }

    return personalisation;
  }

  private bool _disposed;

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    GC.SuppressFinalize(this);
  }
}