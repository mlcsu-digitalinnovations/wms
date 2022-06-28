using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models.ReferralService
{
  public class PharmacistKeyCodeGenerationResponse: 
    PharmacistKeyCodeCreate, IPharmacistKeyCodeGenerationResponse
  {
    public DateTimeOffset Expires { get; set; }
    public virtual List<string> Errors { get; set; }
      = new List<string>();

    public Guid Id { get; set; }

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

  }
}