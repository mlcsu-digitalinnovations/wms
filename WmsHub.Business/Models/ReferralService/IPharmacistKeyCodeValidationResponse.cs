using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IPharmacistKeyCodeValidationResponse
  {
    Guid Id { get; set; }
    DateTimeOffset Expires { get; set; }
    List<string> Errors { get; set; }
    string ReferringPharmacyEmail { get; set; }
    string KeyCode { get; set; }
    int ExpireMinutes { get; set; }
    bool ValidCode { get; set; }
    string GetErrorMessage();
  }
}