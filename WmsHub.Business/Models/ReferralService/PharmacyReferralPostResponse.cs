using System;
using System.Collections.Generic;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService
{
  public class PharmacyReferralPostResponse : IReferralPostResponse
  {
    public Guid Id { get; set; }
    public IEnumerable<ProviderForSelection> ProviderChoices { get; set; }
  }
}