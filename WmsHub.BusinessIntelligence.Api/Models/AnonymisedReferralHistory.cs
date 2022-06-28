using System.Collections.Generic;
using WmsHub.Business.Models;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class AnonymisedReferralHistory: AnonymisedReferral
  {
    public List<AnonymisedTextMessage> TextMessageHistory { get; set; }
  }
}