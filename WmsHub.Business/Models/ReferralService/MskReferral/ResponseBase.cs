using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Business.Models.ReferralService;

public abstract class ResponseBase : IResponseBase
{
  public enum ErrorTypes
  {
    None,
    Validation,
    NotFound,
    Expired,
    TooManyAttempts,
    Incorrect,
    Unknown,
    Whitelist,
    MaxActiveAccessKeys
  }

  private List<string> _errors;

  public bool HasErrors => _errors != null && _errors.Any();

  public ErrorTypes ErrorType { get; private set; }

  public void AddErrorMessage(ErrorTypes errorType, string error)
  {
    if (!string.IsNullOrWhiteSpace(error))
    {
      _errors ??= new();
      _errors.Add(error);
    }
    ErrorType = errorType;
  }

  public virtual string GetErrorMessage()
  {
    if (_errors == null)
    {
      return string.Empty;
    }
    else
    {
      return string.Join(' ', _errors);
    }
  }
}