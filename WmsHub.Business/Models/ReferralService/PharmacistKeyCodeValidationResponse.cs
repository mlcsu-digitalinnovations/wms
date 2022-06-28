using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models.ReferralService
{
  public class PharmacistKeyCodeValidationResponse : 
    IPharmacistKeyCodeValidationResponse
  {
    public Guid Id { get; set; }
    public DateTimeOffset Expires { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public string ReferringPharmacyEmail { get; set; }
    public string KeyCode { get; set; }
    public int ExpireMinutes { get; set; }
    public bool ValidCode { get; set; }
    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }
  }
}