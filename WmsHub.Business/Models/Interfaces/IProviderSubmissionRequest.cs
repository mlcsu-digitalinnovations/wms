using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IProviderSubmissionRequest
  {
    DateTimeOffset Date { get; set; }
    string Reason { get; set; }
    List<Entities.ProviderSubmission> Submissions { get; set; }
    string Ubrn { get; set; }
    UpdateType UpdateType { get; set; }
  }
}