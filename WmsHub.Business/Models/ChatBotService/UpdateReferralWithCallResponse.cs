using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ChatBotService
{
  public class UpdateReferralWithCallResponse : UpdateReferralWithCallRequest
  {
    public UpdateReferralWithCallResponse(UpdateReferralWithCallRequest request)
      : base(request)
    { }

    public virtual StatusType ResponseStatus { get; private set; }

    public virtual List<string> Errors { get; private set; } 
      = new List<string>();

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

    internal void SetStatus(StatusType status)
    {
      ResponseStatus = status;

      switch (status)
      {
        case StatusType.CallIdDoesNotExist:
          Errors.Add($"Referral with a call Id of {Id} not found.");
          break;
        case StatusType.OutcomeIsUnknown:
          Errors.Add($"An Outcome of {Outcome} is unknown.");
          break;
        case StatusType.TelephoneNumberMismatch:
          Errors.Add($"Call Id {Id} does not have a telephone number of " +
            $"{Number}.");
          break;
      }
    }

    public void SetStatus(StatusType status, string errorMessage)
    {
      ResponseStatus = status;
      Errors.Add(errorMessage);
    }
  }


  /// <summary>
  /// TODO: Using the UpdateReferralWithCallResponse as a skelton to shape the 
  /// model for now
  /// </summary>
  public class GetReferralCallListResponse : BaseResponse
  {
    public GetReferralCallListResponse() { }

    public GetReferralCallListResponse(
      GetReferralCallListRequest request,
      ArcusOptions _options)
    {
      Arcus = new ArcusCall(_options);
    }

    public virtual ArcusCall Arcus { get; set; }
    public virtual int DuplicateCount { get; set; }
    public virtual int InvalidNumberCount { get; set; }
  }

  public class BaseResponse
  {
    public void SetStatus(StatusType status, string errorMessage)
    {
      Status = status;
      Errors.Add(errorMessage);
    }

    public virtual StatusType Status { get; private set; }

    public virtual List<string> Errors { get; private set; } = new List<string>();
    public string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

    public void SetStatus(StatusType status)
    {
      Status = status;

      switch (status)
      {
        case StatusType.CallIdDoesNotExist:
          Errors.Add($"StatusType.CallIdDoesNotExist: TODO");
          break;
        case StatusType.OutcomeIsUnknown:
          Errors.Add($"StatusType.OutcomeIsUnknown: TODO");
          break;
        case StatusType.TelephoneNumberMismatch:
          Errors.Add($"StatusType.TelephoneNumberMismatch: TODO");
          break;
      }
    }
  }
}