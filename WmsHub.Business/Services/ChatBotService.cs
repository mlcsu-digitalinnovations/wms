using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.SignalR;
using WmsHub.Common.Validation;
using Call = WmsHub.Business.Entities.Call;

using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Services
{
  public class ChatBotService : ServiceBase<Entities.Referral>, IChatBotService
  {
    private readonly IMapper _mapper;
    private readonly SignalRMessenger _signalRMessenger;
    private readonly ArcusOptions _options;

    public ChatBotService(
      DatabaseContext context,
      IMapper mapper,
      IOptions<ArcusOptions> options,
      IHubContext<SignalRHub> signalRHubContext) : base(context)
    {
      _mapper = mapper;
      _options = options.Value;

      _options.Validate();
      _signalRMessenger = new SignalRMessenger(signalRHubContext);
    }

    public virtual async Task<UpdateReferralWithCallResponse>
      UpdateReferralWithCall(UpdateReferralWithCallRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      ValidateModelResult result = ValidateModel(request);

      UpdateReferralWithCallResponse response =
        new UpdateReferralWithCallResponse(request);

      if (result.IsValid)
      {

        Entities.Referral referral = await GetReferralWithCalls(request);

        if (ValidateReferralCanBeUpdated(referral, request, response))
        {
          await UpdateReferral(referral, request);
          response.SetStatus(StatusType.Valid);
        }
      }
      else
      {
        response.SetStatus(StatusType.Invalid, result.GetErrorMessage());
      }

      return response;
    }

    public async Task<string> RemoveReferrals(List<Entities.Referral> referrals)
    {
      var sb = new StringBuilder();
      foreach (var referral in referrals)
      {
        //Check if fakes already in the system using the status reason
        var found = await _context.Referrals
          .FirstOrDefaultAsync(t => t.StatusReason == referral.StatusReason);
        if (found == null)
        {
          sb.AppendLine("Fake entry using StatusReason " +
            $"{referral.StatusReason} is not loaded");
          continue;
        }

        // Check if fakes in the system using the UBRN as 
        // the reason could have changed
        found = await _context.Referrals
          .FirstOrDefaultAsync(t => t.Ubrn == referral.Ubrn);
        if (found == null)
        {
          sb.AppendLine($"Fake entry using UBRN {referral.Ubrn} is not loaded");
          continue;
        }
        _context.Referrals.Remove(found);

      }
      await _context.SaveChangesAsync();

      return sb.ToString();
    }

    /// <summary>
    /// This is a fake intergration referrals and should not be used
    /// outside the scope of testing
    /// </summary>
    /// <param name="referrals"></param>
    /// <returns></returns>
    public async Task<bool> AddReferrals(List<Entities.Referral> referrals)
    {
      foreach (var referral in referrals)
      {
        // Check if fakes already in the system using the status reason
        var found = await _context.Referrals
          .FirstOrDefaultAsync(t => t.StatusReason == referral.StatusReason);
        if (found != null)
          throw new ArgumentException("Fake entires are already loaded");

        // Check if fakes in the system using the UBRN as the reason could have 
        // changed
        found = await _context.Referrals
          .FirstOrDefaultAsync(t => t.Ubrn == referral.Ubrn);
        if (found != null)
          throw new ArgumentException("Fake entires are already loaded");

        _context.Referrals.Add(referral);

      }
      return await _context.SaveChangesAsync() > 0;
    }

    private async Task<Entities.Referral> GetReferralWithCalls(
      UpdateReferralWithCallRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      Entities.Referral referral = await _context
        .Referrals
        .Include(r => r.Calls)
        .Where(r => r.Calls.Any(c => c.Id == request.Id))
        .Where(r => r.IsActive)
        .FirstOrDefaultAsync();

      return referral;
    }

    /// <summary>
    /// Create calls for all referrals whose status is:
    ///   TextMessage2 and last text message was sent at least 48 hours ago
    ///     -> Update Referral status to ChatBotCall1
    ///   ChatBotCall1 and last chat bot call was sent at least 48 hours ago
    ///     -> Update Referral status to ChatBotCall2
    /// </summary>
    /// <returns></returns>
    public virtual async Task<PrepareCallsForTodayResponse> PrepareCallsAsync()
    {
      DateTimeOffset after =
        DateTimeOffset.Now.AddHours(-Constants.HOURS_BEFORE_NEXT_STAGE).Date;

      List<Referral> textReferrals = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Status == TextMessage2.ToString())
        .Where(r => !r.TextMessages.Any(t => t.IsActive
          && (t.Sent.Date > after || t.Sent == default)))
        .ProjectTo<Referral>(_mapper.ConfigurationProvider)
        .ToListAsync();

      List<Referral> callReferrals = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Status == ChatBotCall1.ToString())
        .Where(r => !r.Calls.Any(c => c.IsActive
          && (c.Sent.Date > after || c.Sent == default)))
        .ProjectTo<Referral>(_mapper.ConfigurationProvider)
        .ToListAsync();

      IEnumerable<Referral> referrals = callReferrals.Union(textReferrals);

      int callsPrepared = 0;
      foreach (var referral in referrals)
      {
        Call call = referral.CreateNewChatBotCall(User);
        bool isCallNumberValid = !string.IsNullOrWhiteSpace(call.Number);

        Entities.Referral entity =
          await _context.Referrals.FindAsync(referral.Id);

        if (referral.Status == TextMessage2.ToString())
        {
          if (isCallNumberValid)
          {
            entity.Status = ChatBotCall1.ToString();
          }
          else
          {
            entity.Status = ChatBotCall2.ToString();
          }
        }
        else if (referral.Status == ChatBotCall1.ToString())
        {
          entity.Status = ChatBotCall2.ToString();
        }
        else
        {
          throw new InvalidOperationException(
            $"Referral {referral.Id} unexpected status of {referral.Status}");
        }

        UpdateModified(entity);

        if (isCallNumberValid)
        {
          _context.Calls.Add(call);
          callsPrepared++;
        }
      }

      await _context.SaveChangesAsync();

      return new PrepareCallsForTodayResponse()
      { 
        CallsPrepared = callsPrepared 
      };
    }

    public virtual async Task UpdateReferralCallListSent(
      IEnumerable<ICallee> callees)
    {
      if (callees is null)
        throw new ArgumentNullException(nameof(callees));

      List<Guid> callIds = callees.Select(c => Guid.Parse(c.Id)).ToList();

      List<Entities.Call> calls = await _context
        .Calls
        .Where(c => c.IsActive)
        .Where(c => callIds.Contains(c.Id))
        .ToListAsync();

      calls.ForEach(call => { call.Sent = DateTimeOffset.Now; });

      await _context.SaveChangesAsync();
    }

    public async Task<List<Entities.Referral>> GetReferralsWithCalls(
      Expression<Func<Entities.Referral, bool>> predicate)
    {

      var referrals = await _context
        .Referrals
        .Include(r => r.Calls)
        .Where(r => r.Calls
                    .Any(c => c.ModifiedAt > DateTimeOffset.Now.AddHours(-1)))
        .Where(r => r.IsActive)
        .Where(predicate)
        .ToListAsync();

      return referrals;
    }

    private async Task<IList<Callee>> GetCalleeListAsync()
    {

      List<Call> orderedList = await _context.Calls
        .AsNoTracking()
        .Include(c => c.Referral)
        .Where(c => c.IsActive)
        .Where(c => c.Referral.IsActive)
        .Where(c => c.Sent == default)
        .Where(c => c.Outcome != Constants.OUTCOME_EXPIRED)
        .Where(c => c.Referral.Status == ChatBotCall1.ToString()
                    || c.Referral.Status == ChatBotCall2.ToString())
        .OrderBy(t => t.ModifiedAt).Take(_options.ReturnLimit).ToListAsync();


      List<Callee> callList = orderedList
        .Select(c => new Callee
        {
          CallAttempt = c.Referral.Status == ChatBotCall1.ToString()
            ? Constants.CALL_ATTEMPT_1
            : Constants.CALL_ATTEMPT_2,
          Id = c.Id.ToString(),
          PrimaryPhone = c.Number,
          ServiceUserName = $"{c.Referral.GivenName} {c.Referral.FamilyName}"
        })
        .ToList();

      return callList;
    }

    private async Task UpdateReferral(
      Entities.Referral referral,
      UpdateReferralWithCallRequest request)
    {
      if (referral == null)
        throw new ArgumentNullException(nameof(referral));
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      Entities.Call requestedCall = referral
        .Calls
        .OrderByDescending(c => c.Called)
        .FirstOrDefault(c => c.Id == request.Id);

      if (requestedCall == null)
        throw new ArgumentException(
          $"Unable to find a referral that has a call id of {request.Id}");

      await UpdateReferralStatusForCallOutcomeAsync(
        referral, requestedCall, request.Outcome);

      requestedCall.Outcome = request.Outcome;
      requestedCall.Called = request.Timestamp;
      UpdateModified(requestedCall);

      UpdateModified(referral);

      await _context.SaveChangesAsync();

      if (referral.Status == ChatBotTransfer.ToString())
      {
        await _signalRMessenger.SendChatBotTransfer();
      }
    }

    private static async Task UpdateReferralStatusForCallOutcomeAsync(
      Entities.Referral referral, Entities.Call requestedCall, string outcome)
    {
      if (referral == null)
        throw new ArgumentNullException(nameof(referral));
      if (referral.Calls == null)
        throw new ArgumentNullException(nameof(referral),
          $"{nameof(referral.Calls)} is null");
      if (requestedCall == null)
        throw new ArgumentNullException(nameof(requestedCall));
      if (string.IsNullOrWhiteSpace(outcome))
        throw new ArgumentNullOrWhiteSpaceException(nameof(outcome));

      if (outcome.TryParseToEnumName(out ChatBotCallOutcome parsedOutcome))
      {

        referral.MethodOfContact = (int)MethodOfContact.ChatBot;
        referral.NumberOfContacts++;

        bool numberValid = true;
        if (parsedOutcome == ChatBotCallOutcome.CallGuardian
          || parsedOutcome == ChatBotCallOutcome.InvalidNumber)
        {
          numberValid = false;
        }

        if (referral.Telephone == requestedCall.Number)
        {
          referral.IsTelephoneValid = numberValid;
        }
        else if (referral.Mobile == requestedCall.Number)
        {
          referral.IsMobileValid = numberValid;
        }
        else
        {
          throw new ChatBotNumberNotFoundException("Number " +
            $"{requestedCall.Number} not found in referral {referral.Id}");
        }
      }
      else
      {
        throw new ArgumentException(
          $"Unknown outcome of {outcome}", nameof(outcome));
      }

      // Outcome TransferringToRmc received when call is being transferred 
      // to the RMC team.
      if (parsedOutcome == ChatBotCallOutcome.TransferringToRmc)
      {
        if (referral.Status == ChatBotCall1.ToString()
            || referral.Status == ChatBotCall2.ToString())
        {
          referral.Status = ChatBotTransfer.ToString();
          referral.MethodOfContact = (int)MethodOfContact.RmcCall;
          referral.NumberOfContacts++;
        }
        else
        {
          throw new ReferralInvalidStatusException(
            $"ChatBot received an outcome of {parsedOutcome} for referral " +
            $"{referral.Id}. Expected a status of {ChatBotCall1} or " +
            $"{ChatBotCall2} but found {referral.Status}.");
        }
      }
    }

    private static bool ValidateReferralCanBeUpdated(
      Entities.Referral referral,
      UpdateReferralWithCallRequest request,
      UpdateReferralWithCallResponse response)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));
      if (response == null)
        throw new ArgumentNullException(nameof(response));

      bool validationResult = false;

      if (referral == null)
      {
        response.SetStatus(StatusType.CallIdDoesNotExist);
      }
      else
      {
        if (referral.Calls.Any(c => c.Number == request.Number))
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

    public virtual async Task<GetReferralCallListResponse> GetReferralCallList(
      GetReferralCallListRequest request)
    {
      GetReferralCallListResponse response = new GetReferralCallListResponse(
        request, _options);

      response.InvalidNumberCount = await UpdateNullNumbersAsync();
      response.DuplicateCount = await RemoveCalleeDuplicatesAsync();
      response.Arcus.Callees = await GetCalleeListAsync();
      response.SetStatus(StatusType.Valid);

      return response;
    }

    /// <summary>
    /// Updates the Outcome to InvalidNumber and Sent to 01/01/1900
    /// of all active calls that have a null number.
    /// </summary>
    /// <remarks>This should only be a temporary method due to other
    /// changes that are being implemented to stop calls with null numbers
    /// being added in the first place.</remarks>
    /// <returns>The number of updated null numbers</returns>
    public async Task<int> UpdateNullNumbersAsync()
    {
      Call[] calls = await _context.Calls
        .Where(c => c.IsActive)
        .Where(c => c.Number == null)
        .ToArrayAsync();

      foreach(Call call in calls)
      {
        call.Outcome = ChatBotCallOutcome.InvalidNumber.ToString();
        call.Sent = new DateTime(1900,1,1);
        UpdateModified(call);
      }

      if (calls.Any())
      {
        await _context.SaveChangesAsync();
      }
      return calls.Count();
    }

    public virtual async Task<UpdateReferralTransferResponse>
      UpdateReferralTransferRequestAsync(UpdateReferralTransferRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      ValidateModelResult result = ValidateModel(request);

      UpdateReferralTransferResponse response =
        new UpdateReferralTransferResponse(request);

      if (result.IsValid)
      {
        try
        {
          UpdateResult updateResult = await UpdateReferralTransfer(request);
          if (updateResult.UpdateOutcome)
            response.SetStatus(StatusType.Valid);
          else
            response.SetStatus(StatusType.Invalid, updateResult.Message);
        }
        catch (Exception)
        {
          throw;
        }
      }
      else
      {
        response.SetStatus(StatusType.Invalid, result.GetErrorMessage());
      }

      return response;
    }

    private async Task<UpdateResult> UpdateReferralTransfer(
      UpdateReferralTransferRequest request)
    {
      UpdateResult response = new UpdateResult();
      response.UpdateOutcome = false;

      string[] statuses = new string[]
      {
        ChatBotCall1.ToString(),
        ChatBotCall2.ToString()
      };

      List<Entities.Referral> referrals = await _context
        .Referrals
        .Where(r => r.IsActive)
        .Where(r => statuses.Contains(r.Status))
        .Where(r => r.Telephone == request.Number || r.Mobile == request.Number)
        .ToListAsync();

      if (referrals.Count == 1)
      {
        Entities.Referral referral = referrals.SingleOrDefault();
        referral.Status = ReferralStatus.ChatBotTransfer.ToString();
        UpdateModified(referral);
        await _context.SaveChangesAsync();
        await _signalRMessenger.SendChatBotTransfer();
        response.UpdateOutcome = true;
      }
      else if (referrals.Count > 1)
      {
        response.Message = "No distinct referral found " +
          $"with associated phone number {request.Number}.";
      }
      else if (referrals.Count == 0)
      {
        response.Message = "No referral found " +
          $"with associated phone number {request.Number}.";
      }

      return response;
    }

    protected async Task<int> RemoveCalleeDuplicatesAsync()
    {
      List<Call> query = await _context.Calls
        .Include(c => c.Referral)
        .Where(c => c.IsActive)
        .Where(c => c.Referral.IsActive)
        .Where(c => c.Sent == default)
        .Where(c => c.Outcome != Constants.OUTCOME_EXPIRED)
        .Where(c => c.Referral.Status == ChatBotCall1.ToString()
                    || c.Referral.Status == ChatBotCall2.ToString())
        .ToListAsync();
      IEnumerable<IGrouping<Guid, Call>> grouped =
        query.GroupBy(t => t.ReferralId);
      foreach (IGrouping<Guid, Call> messages in grouped)
      {
        if (messages.Count() > 1)
        {
          Call latestTextMessage =
            messages.OrderByDescending(t => t.ModifiedAt).First();
          foreach (Call tm in messages)
          {
            if (tm.Id != latestTextMessage.Id)
            {
              tm.Outcome = Constants.OUTCOME_EXPIRED;
              UpdateModified(tm);
            }
          }
        }
      }
      return await _context.SaveChangesAsync();
    }
  }
}