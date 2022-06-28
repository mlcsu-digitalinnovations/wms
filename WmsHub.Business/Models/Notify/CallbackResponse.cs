using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Notify
{
  public class CallbackResponse : CallbackRequest
  {
    public CallbackResponse() { }
    public CallbackResponse(ICallbackRequest model)
      : base(model)
    {
    }

    public virtual StatusType ResponseStatus { get; set; }

    public virtual List<string> Errors { get; set; } = new List<string>();

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
        case StatusType.StatusIsUnknown:
          Errors.Add($"An Status of {Status} is unknown.");
          break;
        case StatusType.UnableToFindReferral:
          Errors.Add($"unable to find referral for Text Message ID of {Id} ");
          break;
        case StatusType.TelephoneNumberMismatch:
          Errors.Add($"Call Id {Id} does not have a telephone number of " +
            $"{To}.");
          break;
      }
    }

    public void SetStatus(StatusType status, string errorMessage)
    {
      ResponseStatus = status;
      Errors.Add(errorMessage);
    }
  }
}