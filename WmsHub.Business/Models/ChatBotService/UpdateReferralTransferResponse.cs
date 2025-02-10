using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ChatBotService
{
  public class UpdateReferralTransferResponse : UpdateReferralTransferRequest
  {
    public UpdateReferralTransferResponse(UpdateReferralTransferRequest request)
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

    public virtual void SetStatus(StatusType status)
    {
      ResponseStatus = status;

      switch (status)
      {
        case StatusType.OutcomeIsUnknown:
          Errors.Add($"An Outcome of {Outcome} is unknown.");
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
