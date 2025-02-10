using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IServiceUserSubmissionResponse
  {
    List<string> Errors { get; }
    StatusType ResponseStatus { get; set; }

    string GetErrorMessage();
    void SetStatus(StatusType status, string errorMessage);
  }
}