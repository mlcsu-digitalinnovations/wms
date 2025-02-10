using static WmsHub.Business.Models.ReferralService.ResponseBase;

namespace WmsHub.Business.Models.ReferralService;

public interface IResponseBase
{
  ErrorTypes ErrorType { get; }

  bool HasErrors { get; }

  void AddErrorMessage(ErrorTypes errorType, string error);

  string GetErrorMessage();
}