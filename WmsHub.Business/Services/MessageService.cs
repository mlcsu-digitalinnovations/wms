using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.MessageService;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Business.Enums.ReferralStatus;
using static WmsHub.Common.Helpers.Constants;
using static WmsHub.Common.Helpers.Constants.MessageServiceConstants;

namespace WmsHub.Business.Services;

public class MessageService : ServiceBase<Referral>, IMessageService
{
  private static readonly object _lockObject = new();
  private const string ELECTIVECARECREATEUSER =
    "ElectiveCareUserRegistrations";
  private const string ELECTIVECAREDELETEUSER = "ElectiveCareUserDeletion";
  private const ReferralStatus FAILECONTACTSTATUSES =
    FailedToContactEmailMessage | FailedToContactTextMessage;
  private const ReferralStatus PROVIDERLISTSSTATUSES =
    RmcCall | RmcDelayed;
  private const ReferralStatus TEXT_MESSAGE_STATUSES =
    ProviderDeclinedTextMessage |
    ProviderRejectedTextMessage |
    ProviderTerminatedTextMessage |
    CancelledDuplicateTextMessage |
    TextMessage1 |
    TextMessage2 |
    TextMessage3;

  private readonly ILogger _logger;
  private readonly IMessageOptions _options;
  private readonly INotificationService _notificationService;

  public MessageService(
    DatabaseContext context,
    ILogger logger,
    IOptions<MessageOptions> options,
    INotificationService notificationService) : base(context)
  {
    if (_context == null)
    {
      throw new ArgumentNullException($"{nameof(context)} is null.");
    }

    _logger = logger
      ?? throw new ArgumentNullException($"{nameof(logger)} is null.");
    _notificationService = notificationService
      ?? throw new ArgumentNullException(
        $"{nameof(notificationService)} is null.");
    _options = options == null
      ? throw new ArgumentNullException(
        $"{nameof(IOptions<MessageOptions>)} is null.")
      : options.Value ?? throw new ArgumentNullException(
          $"{nameof(MessageOptions)} is null.");
    ValidateModelResult optionValidationResult =
     Validators.ValidateModel(options.Value);
    if (!optionValidationResult.IsValid)
    {
      throw new ValidationException(
        $"{nameof(IMessageOptions)}: " +
        $"{optionValidationResult.GetErrorMessage()}");
    }
  }

  /// <inheritdoc/>
  public virtual void AddReferralToMessageQueue(
    QueueItem queueItem,
    MessageType messageType)
  {
    if (queueItem == null)
    {
      throw new ArgumentNullException("Referral must be provided.");
    }

    if (messageType == MessageType.Letter)
    {
      _logger.Warning(
        "Message Type {messageType} not yet supported",
        messageType);
    }

    MessageTemplate template = _options.GetTemplate(
      messageType,
      queueItem.Status.ToEnum<ReferralStatus>(),
      queueItem.Source.ToEnum<ReferralSource>());

    Dictionary<string, string> personalisation = new();

    if (template != null && template.ExpectedPersonalisationList.Any())
    {
      foreach (string item in template.ExpectedPersonalisationList)
      {
        switch (item)
        {
          case NotificationPersonalisations.GIVEN_NAME:
            personalisation.Add(item, queueItem.GivenName);
            break;
          case NotificationPersonalisations.NHS_NUMBER:
            personalisation.Add(item, queueItem.NhsNumber ?? "");
            break;
          case NotificationPersonalisations.LINK:
            string link = _options.ServiceUserRmcEndpoint;
            personalisation.Add(item, $"{link}?u={queueItem.Link}");
            break;
        }
      }
    }

    ApiKeyType apiKeyType = ApiKeyType.None;

    if (FAILECONTACTSTATUSES.HasFlag(queueItem.Status.ToEnum<ReferralStatus>()))
    {
      apiKeyType = ApiKeyType.FailedToContact;
    }
    else if (PROVIDERLISTSSTATUSES.HasFlag(
      queueItem.Status.ToEnum<ReferralStatus>()))
    {
      apiKeyType = ApiKeyType.ProviderList;
    }
    else if (queueItem.Status.ToEnum<ReferralStatus>() == TextMessage1)
    {
      apiKeyType = ApiKeyType.TextMessage1;
    }
    else if (queueItem.Status.ToEnum<ReferralStatus>() == TextMessage2)
    {
      apiKeyType = ApiKeyType.TextMessage2;
    }
    else
    {
      apiKeyType = ApiKeyType.None;
    }

    MessageQueue messageQueue = new()
    {
      ApiKeyType = apiKeyType,
      ServiceUserLinkId = queueItem.Link,
      ReferralId = queueItem.Id,
      Type = messageType,
      TemplateId = template.Id,
      SendTo = messageType == MessageType.Email
        ? queueItem.Source.ToEnum<ReferralSource>() switch
        {
          ReferralSource.Msk => queueItem.ReferringOrganisationEmail,
          ReferralSource.Pharmacy => queueItem.ReferringOrganisationEmail,
          ReferralSource.SelfReferral => queueItem.EmailAddress,
          _ => queueItem.ReferringClinicianEmail
        }
        : queueItem.MobileNumber,
      PersonalisationJson = personalisation.Any()
        ? JsonConvert.SerializeObject(personalisation)
        : null,
      IsActive = true
    };

    ValidateModelResult validationResult = ValidateModel(messageQueue);

    if (!validationResult.IsValid)
    {
      string errors = string.Join(" ",
        validationResult.Results.Select(s => s.ErrorMessage).ToArray());
      _logger.Error(errors);
      throw new ValidationException(errors);
    }
    UpdateModified(messageQueue);

    if (_context.ChangeTracker.HasChanges())
    {
      if (_context.ChangeTracker.Entries<MessageQueue>()
        .Any(x => x.Entity.ReferralId == messageQueue.ReferralId))
      {
        string message =
          $"{messageType}, Duplicate Referral, " +
          $"ReferralId {messageQueue.ReferralId}.";

        throw new DuplicateException(message);
      }
    }

    if (!_context.MessagesQueue.Any(x =>
        x.ReferralId == messageQueue.ReferralId
        && x.SentDate == null))
    {
      lock (_lockObject)
      {
        if (!_context.MessagesQueue.Any(x =>
          x.ReferralId == messageQueue.ReferralId
          && x.SentDate == null))
        {
          _context.MessagesQueue.Add(messageQueue);
          _context.SaveChanges();
        }
      }
    }
  }

  // <inhericdoc/>
  public async Task<string> AddReferralToTextMessage(
    QueueItem item,
    MessageType type)
  {
    if (item == null)
    {
      throw new ArgumentNullException("Referral must be provided.");
    }

    if (item.Status == TextMessage1.ToString() ||
       item.Status == TextMessage2.ToString())
    {
      if (!item.MobileNumber.IsUkMobile())
      {
        throw new InvalidReferralMobileNumber($"Not a valid UK Number.");
      }

      TextMessage textMessage = new()
      {
        IsActive = true,
        Number = item.MobileNumber,
        ReferralId = item.Id,
        ServiceUserLinkId = Base36Converter
          .ConvertDateTimeOffsetToBase36(DateTimeOffset.Now)
      };
      UpdateModified(textMessage);
      _context.TextMessages.Add(textMessage);

      _context.TextMessages.Add(textMessage);
      await _context.SaveChangesAsync();
      return textMessage.ServiceUserLinkId;
    }

    return null;
  }

  /// <inheritdoc/>
  public virtual MessageQueue CreateElectiveCareUserCreateMessage(
    Guid principalId,
    string odsCode,
    string emailAddress)
  {
    string password = Generators.GenerateKeyCode(new Random(), 12, true, false);
    Dictionary<string, dynamic> personalisation = new()
    {
      { MessageTemplateConstants.ORGANISATION_CODE , odsCode },
      { MessageTemplateConstants.PASSWORD , password }
    };

    try
    {
      MessageTemplate template =
        _options.GetTemplateByName(ELECTIVECARECREATEUSER);

      MessageQueue message = new(ApiKeyType.ElectiveCareNewUser,
        password,
        personalisation,
        principalId,
        emailAddress,
        template.Id,
        MessageType.Email);

      ValidateModelResult validationResult = Validators.ValidateModel(message);

      if (!validationResult.IsValid)
      {
        throw new ValidationException(
          $"{nameof(IMessageOptions)}: {validationResult.GetErrorMessage()}");
      }

      return message;
    }
    catch (Exception)
    {
      throw;
    }
  }

  /// <inheritdoc/>
  public virtual MessageQueue CreateElectiveCareUserDeleteMessage(
    Guid principalId,
    string odsCode,
    string emailAddress)
  {
    Dictionary<string, dynamic> personalisation =
       new()
       {
         { MessageTemplateConstants.ORGANISATION_CODE, odsCode }
       };
    MessageTemplate template =
      _options.GetTemplateByName(ELECTIVECAREDELETEUSER);
    MessageQueue message = new()
    {
      ApiKeyType = ApiKeyType.ElectiveCareDeleteUser,
      IsActive = true,
      PersonalisationJson = JsonConvert.SerializeObject(personalisation),
      ReferralId = principalId,
      SendTo = emailAddress,
      TemplateId = template.Id,
      Type = MessageType.Email
    };

    return message;
  }

  /// <inheritdoc/>
  public async Task<string[]> PrepareFailedToContactAsync()
  {
    List<Referral> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == FailedToContact.ToString())
      .Where(r => r.ReferralSource == ReferralSource.SelfReferral.ToString() ||
        r.ReferralSource == ReferralSource.Pharmacy.ToString() ||
        r.ReferralSource == ReferralSource.Msk.ToString()
        )
      .ToListAsync();

    if (!referrals.Any())
    {
      return Array.Empty<string>();
    }

    List<Referral> textMessageReferrals = referrals
       .Where(t => t.ReferralSource == ReferralSource.SelfReferral.ToString())
       .ToList();

    textMessageReferrals
      .ForEach(t =>
        t.SetReferralStatusFailedToContactAndUpdate(User.GetUserId()));

    List<Referral> emailMessageReferrals = referrals
      .Where(t => t.ReferralSource == ReferralSource.Pharmacy.ToString()
               || t.ReferralSource == ReferralSource.Msk.ToString())
      .ToList();
    emailMessageReferrals
      .ForEach(t => t.SetReferralStatusAndUpdateForEmail(
        FailedToContactEmailMessage,
        User.GetUserId()));

    string[] ids = textMessageReferrals.Select(t => t.Id.ToString())
      .Concat(emailMessageReferrals.Select(t => t.Id.ToString()))
      .ToArray();

    await _context.SaveChangesAsync();

    return ids;
  }

  /// <inheritdoc/>
  public async Task<string[]> PrepareNewReferralsToContactAsync()
  {
    IQueryable<Referral> referralsQuery = _context.Referrals
        .Where(r => r.IsActive && r.Status == New.ToString())
        .Where(r => !_context.MessagesQueue
            .Where(t => t.IsActive)
            .Select(t => t.ReferralId)
            .Contains(r.Id));

    List<Referral> referrals = await referralsQuery.ToListAsync();

    List<Referral> textMessage1Referrals = referrals
      .Where(t => t.Mobile.IsUkMobile())
      .ToList();

    textMessage1Referrals
      .ForEach(t =>
        t.SetReferralStatusAndUpdateForSms(TextMessage1, User.GetUserId()));

    string[] ids = textMessage1Referrals
      .Select(t => t.Id.ToString())
      .ToArray();

    await _context.SaveChangesAsync();

    return ids;
  }

  /// <inheritdoc/>
  public async Task<string[]> PrepareTextMessage1ReferralsToContactAsync()
  {
    DateTime after =
      DateTimeOffset.Now.AddHours(-HOURS_BEFORE_NEXT_STAGE).Date;

    IQueryable<Referral> allReferrals =
      from r in _context.Referrals
      join m in _context.MessagesQueue on r.Id equals m.ReferralId
      where r.IsActive &&
        r.Status == TextMessage1.ToString() &&
        m.IsActive &&
        m.SentDate < after &&
        m.SendResult == "success" &&
        m.ApiKeyType == ApiKeyType.TextMessage1
      select r;

    List<Referral> referrals = await allReferrals.ToListAsync();

    referrals.ForEach(t =>
      t.SetReferralStatusAndUpdateForSms(TextMessage2, User.GetUserId()));

    string[] ids = referrals
      .Select(t => t.Id.ToString())
      .ToArray();

    await _context.SaveChangesAsync();

    return ids;
  }

  /// <inheritdoc/>
  public async Task<Dictionary<string, string>> QueueMessagesAsync(
    bool sendFailedOnly = false)
  {
    List<QueueItem> queueItems =
      await GetReferralIdsForMessagesQueue(sendFailedOnly);

    int smsCount = 0;
    int emailCount = 0;
    int exceptionCount = 0;
    List<string> messages = new();


    foreach (QueueItem item in queueItems)
    {
      MessageType type =
        item.Status.ToEnum<ReferralStatus>()
          .GetAttributeOfType<ReferralStatusMessageTypeAttribute>()
          .Type;
      try
      {
        AddReferralToMessageQueue(item, type);
      }
      catch (ValidationException ex)
      {
        exceptionCount++;
        messages.Add(ex.Message);
        continue;
      }
      catch (TemplateNotFoundException ex)
      {
        exceptionCount++;
        messages.Add(ex.Message);
        continue;
      }

      if (type == MessageType.Email)
      {
        emailCount++;
      }
      else if (type == MessageType.SMS)
      {
        smsCount++;
      }
    }

    return new Dictionary<string, string> {
      { KEY_EMAILS_QUEUED, emailCount.ToString()},
      { KEY_TEXT_QUEUED, smsCount.ToString()},
      { KEY_VALIDATION_COUNT, exceptionCount.ToString()},
      { KEY_VALIDATION, string.Join("; ", messages) }
    };
  }

  /// <inheritdoc/>
  public async Task SaveElectiveCareMessage(MessageQueue message)
  {
    UpdateModified(message);
    _context.MessagesQueue.Add(message);
    await _context.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task<Dictionary<string, string>> SendQueuedMessagesAsync()
  {
    int messageCount = 0;
    int exceptionCount = 0;
    StringBuilder errors = new();

    List<MessageQueue> messagesToSend = await _context.MessagesQueue
      .Where(t => t.IsActive)
      .Where(t => t.SentDate == null)
      .ToListAsync();

    if (!messagesToSend.Any())
    {
      return new Dictionary<string, string> {
        {
          KEY_INFORMATION, "No messages queued to send."
        } };
    }

    foreach (MessageQueue message in messagesToSend)
    {
      Models.MessageQueue queueItem = new(
        apiKeyType: message.ApiKeyType,
        clientReference: message.Id.ToString(),
        emailTo: message.SendTo,
        emailReplyToId: _options.ReplyToId,
        mobile: message.SendTo,
        personalisationList: _options.GetTemplateById(
          message.TemplateId).ExpectedPersonalisationList,
        personalisations: message.Personalisations,
        templateId: message.TemplateId,
        type: message.Type);

      try
      {
        HttpResponseMessage response =
          await _notificationService.SendMessageAsync(queueItem);
        message.SendResult = NotificationCallbackStatus.Success.ToString();
        messageCount++;
      }
      catch (NotificationProxyException ex)
      {
        errors.AppendLine(ex.Message);
        message.SendResult = ex.Message;
        exceptionCount++;
      }

      message.SentDate = DateTime.UtcNow;
    }
    await _context.SaveChangesAsync();

    return new Dictionary<string, string> {
      {KEY_TOTAL_TO_SEND, messagesToSend.Count.ToString()},
      {KEY_TOTAL_SENT, messageCount.ToString() },
      {KEY_EXCEPTIONS, exceptionCount.ToString() },
      {KEY_EXCEPTIONS_MESSAGE, errors.ToString() }
    };
  }

  private async Task<List<QueueItem>> GetReferralIdsForMessagesQueue(
  bool sendFailedOnly = false)
  {
    List<QueueItem> queueItems = new();

    DateTime after =
      DateTimeOffset.Now.AddHours(-HOURS_BEFORE_NEXT_STAGE).Date;

    // Add FailedToContactEmailMessage, FailedToContactTextMessage,
    // and CancelledDuplicateTextMessage where they have never been sent
    // before.
    queueItems.AddRange(await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r =>
        r.Status == FailedToContactEmailMessage.ToString() ||
        r.Status == FailedToContactTextMessage.ToString())
      .Where(r => !_context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id))
      .Select(t => new QueueItem
      {
        Id = t.Id,
        GivenName = t.GivenName,
        NhsNumber = t.NhsNumber,
        Ubrn = t.Ubrn,
        EmailAddress = t.Email,
        MobileNumber = t.Mobile,
        Status = t.Status,
        Source = t.ReferralSource,
        ReferringOrganisationEmail = t.ReferringOrganisationEmail,
        ReferringClinicianEmail = t.ReferringClinicianEmail
      }).ToListAsync());

    if (sendFailedOnly)
    {
      return queueItems.Distinct(new IdComparer()).ToList();
    }
    // and CancelledDuplicateTextMessage where they have never been sent
    // before.
    queueItems.AddRange(await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == CancelledDuplicateTextMessage.ToString())
      .Where(r => r.ReferralSource == ReferralSource.SelfReferral.ToString())
      .Where(r => !_context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id))
      .Select(t => new QueueItem
      {
        Id = t.Id,
        GivenName = t.GivenName,
        NhsNumber = t.NhsNumber,
        Ubrn = t.Ubrn,
        EmailAddress = t.Email,
        MobileNumber = t.Mobile,
        Status = t.Status,
        Source = t.ReferralSource,
        ReferringOrganisationEmail = t.ReferringOrganisationEmail,
        ReferringClinicianEmail = t.ReferringClinicianEmail
      }).ToListAsync());

    // Add the following only if they have never been allocated a provider
    // ProviderDeclinedTextMessage, ProviderRejectedTextMessage
    // and ProviderTerminatedTextMessage.
    queueItems.AddRange(await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r =>
        r.Status == ProviderDeclinedTextMessage.ToString() ||
        r.Status == ProviderRejectedTextMessage.ToString() ||
        r.Status == ProviderTerminatedTextMessage.ToString())
      .Where(r => r.Provider != null)
       .Where(r => !_context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id))
      .Select(t => new QueueItem
      {
        Id = t.Id,
        GivenName = t.GivenName,
        NhsNumber = t.NhsNumber,
        Ubrn = t.Ubrn,
        EmailAddress = t.Email,
        MobileNumber = t.Mobile,
        Status = t.Status,
        Source = t.ReferralSource,
        ReferringOrganisationEmail = t.ReferringOrganisationEmail,
        ReferringClinicianEmail = t.ReferringClinicianEmail
      }).ToListAsync());

    // Add TextMessage1 where provider has not been selected and
    // they have not already received the First TextMessage
    queueItems.AddRange(await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == TextMessage1.ToString())
      .Where(r => r.Provider == null)
      .Where(r => !_context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id &&
          t.IsActive &&
          t.ApiKeyType == ApiKeyType.TextMessage1))
      .Select(t => new QueueItem
      {
        Id = t.Id,
        GivenName = t.GivenName,
        NhsNumber = t.NhsNumber,
        Ubrn = t.Ubrn,
        EmailAddress = t.Email,
        MobileNumber = t.Mobile,
        Status = t.Status,
        Source = t.ReferralSource,
        ReferringOrganisationEmail = t.ReferringOrganisationEmail,
        ReferringClinicianEmail = t.ReferringClinicianEmail,
        Link = Base36Converter
        .ConvertDateTimeOffsetToBase36(DateTimeOffset.Now)
      }).ToListAsync());

    // Add TextMessage2 to queue if there has been a text message 1 previously
    // sent and provider still not set.
    queueItems.AddRange(await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == TextMessage2.ToString())
      .Where(r => r.Provider == null)
      .Where(r => !_context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id &&
          t.IsActive &&
          t.ApiKeyType == ApiKeyType.TextMessage2))
      .Where(r => _context.MessagesQueue
        .Any(t =>
          t.ReferralId == r.Id &&
          t.IsActive &&
          t.SentDate != null &&
          t.SentDate < after &&
          t.ApiKeyType == ApiKeyType.TextMessage1))
      .Select(t => new QueueItem
      {
        Id = t.Id,
        GivenName = t.GivenName,
        NhsNumber = t.NhsNumber,
        Ubrn = t.Ubrn,
        EmailAddress = t.Email,
        MobileNumber = t.Mobile,
        Status = t.Status,
        Source = t.ReferralSource,
        ReferringOrganisationEmail = t.ReferringOrganisationEmail,
        ReferringClinicianEmail = t.ReferringClinicianEmail,
        Link = Base36Converter
        .ConvertDateTimeOffsetToBase36(DateTimeOffset.Now)
      }).ToListAsync());

    return queueItems.Distinct(new IdComparer()).ToList();
  }

  private async Task UpdateTextMessage(Guid referralId)
  {
    await _context.TextMessages
      .Where(t => t.ReferralId == referralId)
      .Where(t => t.IsActive)
      .Where(t => t.Received == null)
      .Where(t => string.IsNullOrWhiteSpace(t.Outcome))
      .ForEachAsync(t => t.SetRecievedAndOutcome(User.GetUserId()));
  }
}