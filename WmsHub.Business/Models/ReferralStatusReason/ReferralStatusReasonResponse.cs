using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ReferralStatusReason;

public class ReferralStatusReasonResponse : ReferralStatusReason
{
  public ReferralStatusReasonResponse()
  {
    ResponseStatus = StatusType.Valid;
  }

  public virtual StatusType ResponseStatus { get; set; }

  public List<string> Errors { get; private set; } = new List<string>();

  public string GetErrorMessage()
  {
    string msg = string.Join(" ", Errors);
    return msg;
  }

  public void SetStatus(StatusType status, string errorMessage)
  {
    ResponseStatus = status;
    Errors.Add(errorMessage);
  }
}
