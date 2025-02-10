using System.Collections.Generic;

namespace WmsHub.Business.Models
{
  public class AnonymisedReferralHistory : AnonymisedReferral
  {
    public List<AnonymisedTextMessage> TextMessageHistory { get; set; }
  }
}