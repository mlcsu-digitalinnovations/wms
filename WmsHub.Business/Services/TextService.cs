using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using static WmsHub.Business.Enums.ReferralStatus;
using ValidationContext =
  System.ComponentModel.DataAnnotations.ValidationContext;

namespace WmsHub.Business.Services
{
  public class TextService
    : ServiceBase<Entities.Referral>, IDisposable, ITextService
  {
    private readonly ITextNotificationHelper _helper;
    private readonly ITextOptions _settings;
    private readonly IMapper _mapper;

    public TextService() : base(null)
    { }

    public TextService(IOptions<TextOptions> settings,
      ITextNotificationHelper helper,
      IMapper mapper,
      DatabaseContext context) : base(context)
    {
      _settings = settings.Value;
      _helper = helper;
      _mapper = mapper;
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

      List<ValidationResult> results = new ();

      bool isValid = Validator.TryValidateObject(
        smsMessage, context, results, validateAllProperties: true);

      if (!isValid)
      {
        throw new ValidationException(
          string.Join(" ", results.Select(s => s.ErrorMessage).ToArray()));
      }

      // SmsNotificationResponse or NotifyClientException
      Object response = await _helper.TextClient.SendSmsAsync(
          mobileNumber: smsMessage.MobileNumber,
          templateId: smsMessage.TemplateId,
          personalisation: smsMessage.Personalisation,
          clientReference: smsMessage.ClientReference,
          smsSenderId: _settings.SmsSenderId
        ) as Object;

      if (response.GetType() == typeof(NotifyClientException))
      {
        throw (NotifyClientException)response;
      }

      return response as SmsNotificationResponse;
    }

    public virtual async Task<int> PrepareMessagesToSend(
      Guid referralId = default)
    {
      DateTime after =
        DateTimeOffset.Now.AddHours(-Constants.HOURS_BEFORE_NEXT_STAGE).Date;

      var query = _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r =>
          r.Status == FailedToContactTextMessage.ToString()
          || r.Status == ProviderDeclinedTextMessage.ToString()
          || r.Status == ProviderRejectedTextMessage.ToString()
          || r.Status == ProviderTerminatedTextMessage.ToString()
          || r.Status == CancelledDuplicateTextMessage.ToString()
          || ((r.Status == New.ToString()
              || r.Status == TextMessage1.ToString())
            && r.ProviderId == null
            && !r.TextMessages.Any(tm =>
              tm.IsActive
              && tm.Outcome != "FAILED"
              && (tm.Sent.Date > after || tm.Sent == default))));

      if (referralId != default)
      {
        query = query.Where(r => r.Id == referralId);
      }

      List<Entities.Referral> referrals = await query.ToListAsync();

      List<Entities.TextMessage> textMessages = new(referrals.Count);

      int createdTextMessages = 0;
      foreach (var referral in referrals)
      {
        if (referral.Mobile.IsUkMobile())
        {
          createdTextMessages++;

          Entities.TextMessage textMessage = new()
          {
            Base36DateSent = Base36Converter
              .ConvertDateTimeOffsetToBase36(DateTimeOffset.Now),
            IsActive = true,
            Number = referral.Mobile,
            ReferralId = referral.Id
          };
          UpdateModified(textMessage);
          textMessages.Add(textMessage);

          _context.TextMessages.Add(textMessage);

          if (referral.Status == New.ToString())
          {
            referral.Status = TextMessage1.ToString();
          }
          else if (referral.Status == TextMessage1.ToString())
          {
            referral.Status = TextMessage2.ToString();
          }
          // Don't change the status reason
          else if (referral.Status != FailedToContactTextMessage.ToString()
            && referral.Status != ProviderDeclinedTextMessage.ToString()
            && referral.Status != ProviderRejectedTextMessage.ToString() 
            && referral.Status != ProviderTerminatedTextMessage.ToString() 
            && referral.Status != CancelledDuplicateTextMessage.ToString())
          {
            throw new InvalidOperationException(
              $"Referral {referral.Id} unexpected status of {referral.Status}");
          }
        }
        else
        {
          referral.Status = TextMessage2.ToString();
          referral.StatusReason = "Mobile number is not a valid mobile number";
          referral.IsMobileValid = false;
        }
        UpdateModified(referral);
      }

      await _context.SaveChangesAsync();

      return createdTextMessages;
    }

    /// <summary>
    /// this updates the individual request. This needs to be run before any 
    /// callback is generated by the notify, or the Id and Referal may not
    /// be found in the WmsHub.  
    /// Outcome of Text Messager must be set to SENT
    /// TextBessageId is in Reference
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
    public virtual async Task UpdateMessageRequestAsync(
      ISmsMessage smsMessage, 
      string outcome = Constants.TEXT_MESSAGE_SENT)
    {
      Entities.Referral referral = await GetReferralWithTextMessage(
        smsMessage.LinkedTextMessage.Value);

      if (referral == null)
      {
        throw new ArgumentNullException($"Referral not found using the Text " +
          $"Message Id {smsMessage.LinkedTextMessage}");
      }

      Entities.TextMessage textMessage = referral.TextMessages
        .SingleOrDefault(c => c.Id == smsMessage.LinkedTextMessage.Value);

      if (textMessage == null)
      {
        throw new ArgumentException($"Unable to find a TextMessage id " +
           $"of {smsMessage.LinkedTextMessage}");
      }

      textMessage.Outcome = outcome;
      textMessage.Sent = smsMessage.Sent;

      referral.MethodOfContact = (int)MethodOfContact.TextMessage;
      referral.NumberOfContacts++;

      if (referral.Status.Is(CancelledDuplicateTextMessage))
      {
        referral.Status = CancelledDuplicate.ToString();
      }
      else if (referral.Status.Is(FailedToContactTextMessage))
      {
        referral.Status = FailedToContact.ToString();
      }
      else if (referral.Status.Is(ProviderDeclinedTextMessage)
        || referral.Status.Is(ProviderRejectedTextMessage)
        || referral.Status.Is(ProviderTerminatedTextMessage))
      {
        // Status from a terminated text message should be updated to:
        //  Complete if the referral is a non-GP referral
        //  ProviderCompleted if the referral is a GP referral
        if (referral.ReferralSource.Is(ReferralSource.GpReferral))
        {
          referral.Status = ProviderCompleted.ToString();          
        }
        else
        {
          referral.Status = Complete.ToString();
        }
      }

      UpdateModified(textMessage);

      UpdateModified(referral);

      await _context.SaveChangesAsync();

    }

    public virtual async Task<bool> AddNewMessageAsync(TextMessageRequest
        message)
    {
      Entities.TextMessage tm = new Entities.TextMessage(message);

      UpdateModified(tm);

      Entities.Referral referral = await _context.Referrals
        .Include(r => r.TextMessages)
        .Where(r => r.IsActive && r.Id == message.ReferralId)
        .FirstOrDefaultAsync();

      if (referral == null) throw new ArgumentNullException(
        $"Referral not found with ID of {message.ReferralId}");

      if (referral.TextMessages == null)
        referral.TextMessages = new List<Entities.TextMessage>();

      referral.TextMessages.Add(tm);

      return await _context.SaveChangesAsync() > 0;
    }

    public virtual async Task<IEnumerable<ISmsMessage>> GetMessagesToSendAsync(
      int? limit = null)
    {
      string[] statusFilter = new string[]
      {
        TextMessage1.ToString(),
        TextMessage2.ToString(),
        FailedToContactTextMessage.ToString(),
        ProviderDeclinedTextMessage.ToString(),
        ProviderRejectedTextMessage.ToString(),
        ProviderTerminatedTextMessage.ToString(),
        CancelledDuplicateTextMessage.ToString()
      };

      List<SmsMessage> smsMessages = await _context
        .TextMessages
        .AsNoTracking()
        .Where(t => t.IsActive)
        .Where(t => statusFilter.Contains(t.Referral.Status))
        .Where(t => t.Sent == default)
        .Take(limit ?? 10000)
        .Select(t => new SmsMessage()
        {
          Base36DateSent = t.Base36DateSent,
          ClientReference = t.Id.ToString(),
          LinkedTextMessage = t.Id,
          MobileNumber = t.Number,
          Personalisation = new Dictionary<string, dynamic>
            { {"givenName", t.Referral.GivenName} },
          ReferralSource = t.Referral.ReferralSource,
          ReferralStatus = t.Referral.Status
        })
        .ToListAsync();

      foreach (SmsMessage smsMessage in smsMessages)
      {
        smsMessage.TemplateId = GetReferralTemplateId(
          smsMessage.ReferralStatus, smsMessage.ReferralSource);
      }

      return smsMessages;
    }

    public virtual async Task<ISmsMessage>
        GetMessageByReferralIdToSendAsync(Guid referralId)
    {
      Entities.Referral referral = await
        GetReferralWithTextMessageByReferralId(referralId);

      SmsMessage smsMessage = new SmsMessage();
      try
      {
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
      return GetReferralTemplateId(referral.Status, referral.ReferralSource);
    }

    protected string GetReferralTemplateId(
      string referralStatus, 
      string referralSource)
    {
      string templateId;
      if (referralStatus.Is(TextMessage1))
      {
        if (referralSource.Is(ReferralSource.GeneralReferral))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_GENERAL_FIRST).ToString();
        }
        else if (referralSource.Is(ReferralSource.Msk))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_MSK_FIRST).ToString();
        }
        else if (referralSource.Is(ReferralSource.Pharmacy))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_PHARMACY_FIRST).ToString();
        }
        else if (referralSource.Is(ReferralSource.SelfReferral))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_SELF_FIRST).ToString();
        }
        else
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_GP_FIRST).ToString();
        }
      }
      else if (referralStatus.Is(TextMessage2))
      {
        if (referralSource.Is(ReferralSource.GeneralReferral))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_GENERAL_SECOND).ToString();
        }
        else if (referralSource.Is(ReferralSource.Msk))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_MSK_SECOND).ToString();
        }
        else if (referralSource.Is(ReferralSource.Pharmacy))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_PHARMACY_SECOND).ToString();
        }
        else if (referralSource.Is(ReferralSource.SelfReferral))
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_SELF_SECOND).ToString();
        }
        else
        {
          templateId = _settings.GetTemplateIdFor(
            TextOptions.TEMPLATE_GP_SECOND).ToString();
        }
      }
      else if (referralStatus.Is(FailedToContactTextMessage))
      {
        templateId = _settings.GetTemplateIdFor(
          TextOptions.TEMPLATE_FAILEDTOCONTACT).ToString();
      }
      else if (referralStatus.Is(ProviderDeclinedTextMessage))
      {
        templateId = _settings.GetTemplateIdFor(
          TextOptions.TEMPLATE_NONGP_DECLINED).ToString();
      }
      else if (referralStatus.Is(ProviderRejectedTextMessage))
      {
        templateId = _settings.GetTemplateIdFor(
          TextOptions.TEMPLATE_NONGP_REJECTED).ToString();
      }
      else if (referralStatus.Is(ProviderTerminatedTextMessage))
      {
        templateId = _settings.GetTemplateIdFor(
          TextOptions.TEMPLATE_NONGP_TERMINATED).ToString();
      }
      else if (referralStatus.Is(CancelledDuplicateTextMessage))
      {
        templateId = _settings.GetTemplateIdFor(
          TextOptions.TEMPLATE_SELF_CANCELLEDDUPLICATE).ToString();
      }
      else
      {
        throw new ReferralInvalidStatusException("Expected a referral status " +
          $"of {TextMessage1}, {TextMessage2}, {FailedToContactTextMessage}, " +
          $"{ProviderRejectedTextMessage} or {ProviderTerminatedTextMessage} " +
          $"but found {referralStatus}.");
      }

      return templateId;
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
        throw new ArgumentNullException(nameof(request));

      ValidateModelResult result = ValidateModel(request);

      CallbackResponse response = new CallbackResponse(request);

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
        throw new ArgumentNullException(nameof(request));

      ValidateModelResult result = ValidateModel(request);

      CallbackResponse response = new CallbackResponse(request);

      if (result.IsValid)
      {
        if (Guid.TryParse(request.Id, out Guid referralId))
        {
          Entities.Referral referral = await _context
            .Referrals
            .Where(r => r.Id == referralId)
            .FirstOrDefaultAsync();

          if (referral == null) throw new ReferralNotFoundException(
            $"Unable to find a Referral that has an Id of {referralId}");

          //set status and validmobile fields for referral
          referral.IsMobileValid = false;
          if (referral.Status == TextMessage1.ToString())
            referral.Status = TextMessage2.ToString();

          UpdateModified(referral);
          await _context.SaveChangesAsync();
        }
        else
        {
          response.SetStatus(StatusType.Invalid, "Referral Id is not a Guid.");
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
        throw new ArgumentNullException(nameof(referral));
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      Guid.TryParse(request.Reference, out Guid textMessageId);

      Entities.TextMessage requestedText =
        referral.TextMessages.FirstOrDefault(c => c.Id == textMessageId);

      if (requestedText == null) throw new ReferralNotFoundException(
           $"Unable to find a referral that has a TextMessage id " +
           $"of {request.Reference}");

      if (request.StatusValue == CallbackStatus.Delivered)
        referral.IsMobileValid = true;


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
        throw new ArgumentNullException(nameof(request));
      if (response == null)
        throw new ArgumentNullException(nameof(response));

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
        throw new ArgumentNullException(nameof(request));

      if (!Guid.TryParse(request.Reference, out Guid id))
        throw new ArgumentException(nameof(request.Reference));

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

    private bool _disposed;

    public void Dispose()
    {
      if (_disposed)
        return;

      _disposed = true;
    }

  }
}